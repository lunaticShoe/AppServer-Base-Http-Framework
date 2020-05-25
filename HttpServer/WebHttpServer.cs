﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace AppServerBase.HttpServer
{
    public class WebHttpServer
    {
        public string[] DefaultDocuments =
        {
            "index.html",
            "index.htm",
            "default.html",
            "default.htm"
        };
        /// <summary>
        /// Mime Type conversion table
        /// </summary>
        private static IDictionary<string, string> _mimeTypeMappings =
            new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
            {
                #region extension to MIME type list
                {".asf", "video/x-ms-asf"},
                {".asx", "video/x-ms-asf"},
                {".avi", "video/x-msvideo"},
                {".bin", "application/octet-stream"},
                {".cco", "application/x-cocoa"},
                {".crt", "application/x-x509-ca-cert"},
                {".css", "text/css"},
                {".deb", "application/octet-stream"},
                {".der", "application/x-x509-ca-cert"},
                {".dll", "application/octet-stream"},
                {".dmg", "application/octet-stream"},
                {".ear", "application/java-archive"},
                {".eot", "application/octet-stream"},
                {".exe", "application/octet-stream"},
                {".flv", "video/x-flv"},
                {".gif", "image/gif"},
                {".hqx", "application/mac-binhex40"},
                {".htc", "text/x-component"},
                {".htm", "text/html"},
                {".html", "text/html"},
                {".ico", "image/x-icon"},
                {".img", "application/octet-stream"},
                {".iso", "application/octet-stream"},
                {".jar", "application/java-archive"},
                {".jardiff", "application/x-java-archive-diff"},
                {".jng", "image/x-jng"},
                {".jnlp", "application/x-java-jnlp-file"},
                {".jpeg", "image/jpeg"},
                {".jpg", "image/jpeg"},
                {".js", "application/x-javascript"},
                {".mml", "text/mathml"},
                {".mng", "video/x-mng"},
                {".mov", "video/quicktime"},
                {".mp3", "audio/mpeg"},
                {".mpeg", "video/mpeg"},
                {".mpg", "video/mpeg"},
                {".msi", "application/octet-stream"},
                {".msm", "application/octet-stream"},
                {".msp", "application/octet-stream"},
                {".pdb", "application/x-pilot"},
                {".pdf", "application/pdf"},
                {".pem", "application/x-x509-ca-cert"},
                {".pl", "application/x-perl"},
                {".pm", "application/x-perl"},
                {".png", "image/png"},
                {".prc", "application/x-pilot"},
                {".ra", "audio/x-realaudio"},
                {".rar", "application/x-rar-compressed"},
                {".rpm", "application/x-redhat-package-manager"},
                {".rss", "text/xml"},
                {".run", "application/x-makeself"},
                {".sea", "application/x-sea"},
                {".shtml", "text/html"},
                {".sit", "application/x-stuffit"},
                {".swf", "application/x-shockwave-flash"},
                {".tcl", "application/x-tcl"},
                {".tk", "application/x-tcl"},
                {".txt", "text/plain"},
                {".war", "application/java-archive"},
                {".wbmp", "image/vnd.wap.wbmp"},
                {".wmv", "video/x-ms-wmv"},
                {".xml", "text/xml"},
                {".xpi", "application/x-xpinstall"},
                {".zip", "application/zip"},
                {".svg", "image/svg+xml"}

                #endregion
            };

        private string _rootDirectory;
        private HttpListener _listener;
        private Thread _serverThread;
        private int Port = 80;


        public static void Send404(HttpListenerResponse Response)
        {
            try
            {
                Response.Headers.Add("Access-Control-Allow-Origin", "*");
                string responseString = "<HTML><BODY><h1>404</h1></BODY></HTML>";
                byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                Response.ContentLength64 = buffer.Length;
                Response.StatusCode = 404;
                Response.OutputStream.Write(buffer, 0, buffer.Length);

                Response.OutputStream.Close();
                Response.OutputStream.Dispose();
            }
            catch { }
        }

        public WebHttpServer(string path, int port = 80)
        {
            _rootDirectory = path;
            Port = port;
        }

        public void Start()
        {
            _serverThread = new Thread(Listen);
            _serverThread.Start();
        }

        private void Listen()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://+:{Port}/");
            _listener.Start();

            for (int i = 0; i < Environment.ProcessorCount * 5; i++)
            {
                void HandleProc()
                {
                    while (true)
                    {
                        var result = _listener.BeginGetContext(Process, _listener);
                        result.AsyncWaitHandle.WaitOne();
                    }
                }

                var procThread = new Thread(HandleProc);
                procThread.IsBackground = true;
                procThread.Priority = ThreadPriority.Normal;
                procThread.Start();
            }
        }


        /// <summary>
        /// Process an individual request. Handles only static file based requests
        /// </summary>
        /// <param name="context"></param>
        private void Process(IAsyncResult ar)
        {
            var Listener = ar.AsyncState as HttpListener;
            var context = Listener.EndGetContext(ar);

            string filename = context.Request.Url.AbsolutePath;
            Console.WriteLine(filename);


            filename = filename.Substring(1);

            if (string.IsNullOrEmpty(filename))
            {
                foreach (string indexFile in DefaultDocuments)
                {
                    if (File.Exists(Path.Combine(_rootDirectory, indexFile)))
                    {
                        filename = indexFile;
                        break;
                    }
                }
            }


            filename = Path.Combine(_rootDirectory, filename);

            if (File.Exists(filename))
            {
                try
                {
                    Stream input = new FileStream(filename, FileMode.Open);

                    //Adding permanent http response headers
                    string mime;
                    context.Response.ContentType = _mimeTypeMappings.TryGetValue(Path.GetExtension(filename), out mime)
                        ? mime
                        : "application/octet-stream";
                    context.Response.ContentLength64 = input.Length;
                    context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                    context.Response.AddHeader("Last-Modified", File.GetLastWriteTime(filename).ToString("r"));

                    byte[] buffer = new byte[1024 * 32];
                    int nbytes;
                    while ((nbytes = input.Read(buffer, 0, buffer.Length)) > 0)
                        context.Response.OutputStream.Write(buffer, 0, nbytes);
                    input.Close();
                    context.Response.OutputStream.Flush();

                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                }
                catch //(Exception ex)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }

            }
            else
            {
                //context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                Send404(context.Response);
                return;
            }

            context.Response.OutputStream.Close();
        }

        

    }
}

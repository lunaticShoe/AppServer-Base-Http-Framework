using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace AppServerBase.HttpServer
{

    public class ServerModuleResponse
    {
        private string StrRes = null;
        private Stream StreamRes = null;
        private string f_name = null;


        public ServerModuleResponse(string response)
        {
            StrRes = response;
        }
        public ServerModuleResponse(JObject responce)
        {
            StrRes = responce.ToString();
        }
        public ServerModuleResponse(Stream responce, string filename)
        {
            f_name = filename;
            StreamRes = responce;
        }

        private void SetJsonResponse(HttpListenerContext context, string JsonResponse)
        {
            try
            {
                context.Response.Headers.Add("Access-Control-Allow-Origin", "*");


                context.Response.ContentType = "application/json; charset=utf-8";
                byte[] buffer = Encoding.UTF8.GetBytes(JsonResponse);
                context.Response.StatusCode = 200;

                context.Response.ContentLength64 = buffer.LongLength;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                // context.Response.OutputStream.wr


                context.Response.OutputStream.Close();
                context.Response.OutputStream.Dispose();
            }
            catch
            {

            }

        }


        private byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[input.Length];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        private bool IsFireFox(string userAgent)
        {
            try
            {
                string browser = userAgent.Substring(
                    userAgent.LastIndexOf(" ") + 1,
                    (userAgent.LastIndexOf("/") - 1) - userAgent.LastIndexOf(" ")
                );

                return (browser.CompareTo("Firefox") == 0);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void SetStreamResponse(HttpListenerContext context, Stream StreamResponse)
        {

            try
            {
                Console.WriteLine("File response. Filename = " + f_name);
                StreamResponse.Position = 0;
                byte[] buffer = ReadFully(StreamResponse);

                context.Response.Headers.Add("Content-Type", "application/octet-stream;   charset=utf-8");
                context.Response.Headers.Add("Content-Type", "binary;   charset=utf-8");
                context.Response.Headers.Add("Content-Type", "application/x-download");

                Console.WriteLine("Common headers set!");
                if ((context.Request.HttpMethod == "GET")
                    && (f_name != null) && (f_name != ""))
                    {

                        if (IsFireFox(context.Request.UserAgent))
                            context.Response.AddHeader("Content-Disposition", "attachment; filename*=\"utf8'ru-ru'" + Uri.EscapeDataString(f_name) + "\"");
                        else
                            context.Response.AddHeader("Content-Disposition", "attachment; filename=" + Uri.EscapeDataString(f_name));
                    }

                context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                context.Response.ContentLength64 = buffer.Length;

                context.Response.StatusCode = 200;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);

                context.Response.OutputStream.Close();
                context.Response.OutputStream.Dispose();
            } 
            catch
            {

            }
        }

        public void Response(HttpListenerContext context)
        {

            if (StrRes != null)
            {
                SetJsonResponse(context, StrRes);
                return;
            }

            if (StreamRes != null)
            {
                SetStreamResponse(context, StreamRes);
                return;
            }
        }
    }

}

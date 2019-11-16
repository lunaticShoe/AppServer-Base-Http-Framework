using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace AppServerBase.HttpServer
{

    public class ServerModuleResponse
    {
        private readonly string StrRes = null;
        private readonly Stream StreamRes = null;
        private readonly string f_name = null;


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
            var response = context.Response;
            try
            {                
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.ContentType = "application/json; charset=utf-8";
                byte[] buffer = Encoding.UTF8.GetBytes(JsonResponse);
                response.StatusCode = 200;

                response.ContentLength64 = buffer.LongLength;
                response.OutputStream.Write(buffer, 0, buffer.Length);      
            }
            catch
            {

            }
            finally
            {
                response.OutputStream.Close();
                response.OutputStream.Dispose();
            }

        }

        private void SetStreamResponse(HttpListenerContext context, Stream StreamResponse)
        {
            var response = context.Response;
            try
            {
                Console.WriteLine("File response. Filename = " + f_name);
                StreamResponse.Position = 0;
                byte[] buffer = ReadFully(StreamResponse);

                response.Headers.Add("Content-Type", "application/octet-stream; charset=utf-8");
                response.Headers.Add("Content-Type", "binary; charset=utf-8");
                response.Headers.Add("Content-Type", "application/x-download");

                Console.WriteLine("Common headers set!");
                if ((context.Request.HttpMethod == "GET")
                    && (f_name != null) && (f_name != ""))
                {

                    if (IsFirefox(context.Request.UserAgent))
                        response.AddHeader(
                            "Content-Disposition", 
                            "attachment; filename*=\"utf8'ru-ru'" 
                            + Uri.EscapeDataString(f_name) + "\"");
                    else
                        response.AddHeader(
                            "Content-Disposition", 
                            "attachment; filename=" 
                            + Uri.EscapeDataString(f_name));
                }

                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.ContentLength64 = buffer.Length;

                response.StatusCode = 200;
                response.OutputStream.Write(buffer, 0, buffer.Length);                
            } 
            catch
            {

            }
            finally
            {
                StreamResponse.Close();
                StreamResponse.Dispose();
                response.OutputStream.Close();
                response.OutputStream.Dispose();
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

        private bool IsFirefox(string userAgent)
        {
            try
            {
                string browser = userAgent.Substring(
                    userAgent.LastIndexOf(" ") + 1,
                    userAgent.LastIndexOf("/") - 1 - userAgent.LastIndexOf(" ")
                );
                return browser.Contains("Firefox");
            }
            catch
            {
                return false;
            }
        }
    }

}

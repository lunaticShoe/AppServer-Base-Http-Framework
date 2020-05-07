using AppServerBase.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;

namespace AppServerBase.HttpServer
{
    public class HTTPServer
    {
        private HttpListener Listener = new HttpListener();

        private Dictionary<string,Type> ModuleTypesCache = new Dictionary<string, Type>();


        private Func<bool> CheckLicenseDelegate;
     //   private Func<string, SessionBase> CheckSessionDelegate;

        public HTTPServer()
        {
            //ServicePointManager.DefaultConnectionLimit = 5000;
            //ServicePointManager.Expect100Continue = false;
            //ServicePointManager.MaxServicePoints = 5000;
            ServicePointManager.MaxServicePoints = int.MaxValue;

            //var assemblies = AppDomain.CurrentDomain
            //    .GetAssemblies().Where(x => x.FullName.Contains("SNMPAgent"));


            var modules = (from t in Assembly.GetCallingAssembly().GetTypes()
                           where t.IsClass && t.CustomAttributes.Count() > 0
                           && t.BaseType == typeof(HttpServerModule)
                           select t);
            
            //ModuleTypesCache = modules.ToDictionary(m =>
            //    (m.GetCustomAttributes()
            //        .FirstOrDefault(ca =>
            //            ca.GetType() == typeof(ServerModuleAttribute)
            //            ) as ServerModuleAttribute).GetModuleName());

            foreach (var module in modules)
            {
                var attributes = module.GetCustomAttributes()
                    .Where(a => a.GetType() == typeof(ServerModuleAttribute));

                foreach (var attr in attributes)
                {
                    var a = attr as ServerModuleAttribute;
                    if (!ModuleTypesCache.ContainsKey(a.GetModuleName()))
                        ModuleTypesCache.Add(a.GetModuleName(), module);
                }
            }

            ModuleTypesCache.OrderByDescending(m => m.Key.Length);
        }

        private void SendAllowCORS(HttpListenerResponse Response)
        {
            try
            {
                Response.StatusCode = 200;
                Response.Headers.Add("Access-Control-Allow-Origin", "*");
                Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
                Response.OutputStream.Close();
                Response.OutputStream.Dispose();
            }
            catch { }
        }

        

        private void Send404(HttpListenerResponse Response)
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
            } catch { }
        }


        private void SendJSONMessage(HttpListenerResponse Response, string json)
        {
            try
            {         
                Response.ContentType = "application/json; charset=utf-8";
                Response.Headers.Add("Access-Control-Allow-Origin", "*");

                byte[] buffer = Encoding.UTF8.GetBytes(json);
                Response.StatusCode = 500;
                Response.ContentLength64 = buffer.Length;
                
                Response.OutputStream.Write(buffer, 0, buffer.Length);

                Response.OutputStream.Close();
                Response.OutputStream.Dispose();
            } 
            catch (Exception ex)
            {
                var error_message_text = $"Server error message: {ex.Message}" +
                    $"\nError stack : {ex.StackTrace}";


                ServerLog.LogError(error_message_text);
            }
        }

        public void SetCheckLicenseMethod(Func<bool> checkLicense)
        {
            CheckLicenseDelegate = checkLicense;
        }

        //public void SetCheckSessionMethod(Func<string, SessionBase> checkSession)
        //{
        //    CheckSessionDelegate = checkSession;
        //}

        public void Start(params string[] address)
        {
            foreach (var addr in address)            
                Listener.Prefixes.Add(addr);
            
           // Listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            Listener.Start();


            for (int i = 0; i < Environment.ProcessorCount * 5; i++)
            {
                void HandleProc()
                {
                    while (true)
                    {
                        var result = Listener.BeginGetContext(ProcessRequest, Listener);
                        result.AsyncWaitHandle.WaitOne();
                    }
                }

                var procThread = new Thread(HandleProc);
                procThread.IsBackground = true;
                procThread.Priority = ThreadPriority.Normal;
                procThread.Start();
            }
        }

        

        private void ProcessRequest(IAsyncResult ar)
        {
            var Listener = ar.AsyncState as HttpListener;
            var context = Listener.EndGetContext(ar);

            
            context.Response.KeepAlive = false;
            var stopwatch = Stopwatch.StartNew();

            try
            {

                if (context.Request.HttpMethod == "OPTIONS")
                {
                    SendAllowCORS(context.Response);
                    return;
                }

                var url = context.Request.Url.AbsolutePath;

                var rng = new int[] { 0, 1 };
                string httpModuleName = ModuleTypesCache.Keys
                    .Where(k => rng.Contains(url.IndexOf(k)))
                    .FirstOrDefault();
              
                var module = GetModule(httpModuleName);

                if (module == null)
                {
                    var msg = $"{context.Request.Url.AbsolutePath}" +
                        $"\nModule {httpModuleName} not found" +
                        $"\n{context.Request.UserAgent}" +
                        $"\n{context.Request.RemoteEndPoint.Address.ToString()}";
                    Console.WriteLine(msg);
                    ServerLog.LogError(msg);
                    Send404(context.Response);
                    return;
                }

                string httpActionName = context.Request.Url.AbsolutePath;
                httpActionName = httpActionName
                    .Substring(httpActionName.IndexOf(httpModuleName) + httpModuleName.Length + 1);

                if (httpActionName.Contains("/"))
                    httpActionName = httpActionName.Substring(0, httpActionName.IndexOf("/"));

                //if (httpActionName[httpActionName.Length - 1] == '/')
                //    httpActionName = httpActionName.Substring(0, httpActionName.Length - 1);

                //httpActionName = httpActionName.Substring(httpActionName.IndexOf(httpModuleName) + httpModuleName.Length + 1);

                string[] allowedMethods = new string[] { "POST","GET" };

                if ((!allowedMethods.Contains(context.Request.HttpMethod)))
                {
                    SendJSONMessage(context.Response, ClientMsg.GetErrorMsgJson("Invalid http method!", "INVALID_HTTP_METHOD"));
                    return;
                }

                var ModuleResponce = module.Execute(httpActionName, context);

                stopwatch.Stop();
                var elapsed_time = stopwatch.ElapsedMilliseconds;

                TimeSpan t = TimeSpan.FromMilliseconds(elapsed_time);
                string answer = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                                        t.Hours,
                                        t.Minutes,
                                        t.Seconds,
                                        t.Milliseconds);
                if (t.Seconds > 1)
                   Console.WriteLine("Total execution time: " + answer);

                if (ModuleResponce == null)
                {
                    Send404(context.Response);
                    return;
                }                
            }
            catch (TargetInvocationException tie)
            {
                HandleException(tie.InnerException, context);
            }
            catch (AggregateException ex)
            {
                HandleException(ex.InnerException, context);
            }
            catch (Exception ex)
            {
                HandleException(ex,context);
            }
        }


        private HttpServerModule GetModule(string module)
        {
            if (module == null)
                return null;

            foreach (var k in ModuleTypesCache.Keys)
            {
                var t = ModuleTypesCache[k];
                if (t.BaseType.Name != typeof(HttpServerModule).Name)
                    continue;


                foreach (var attribute in t.GetCustomAttributes())
                    if (attribute is ServerModuleAttribute &&
                        (attribute as ServerModuleAttribute).GetModuleName() == module)
                    {
                        var serverModule = Activator.CreateInstance(t) as HttpServerModule;
                        serverModule.SetCheckLicenseMethod(CheckLicenseDelegate);
                       // serverModule.SetCheckSessionMethod(CheckSessionDelegate);
                        return serverModule;
                    }
            }
            return null;
        }

        private void HandleException(Exception ex,HttpListenerContext context)
        {
            var body = "";
            //if (context.Request.HttpMethod == "POST")
            //    using (StreamReader reader = new StreamReader(context.Request.InputStream))
            //        body = reader.ReadToEnd();



            var error_message_text = $"{ex.Message}";
            //+
            //    $"\nURL : {context.Request.Url.AbsolutePath}" +
            //    $"\nBody : \n{body}";

            var error_message_text_log = $"Server error message: {ex.Message}" +
                $"\nURL : {context.Request.Url.AbsolutePath}" +
                $"\nBody : \n{body}" +
                $"\nError stack : {ex.StackTrace}";

            ServerLog.LogError(error_message_text_log);

            Console.WriteLine(error_message_text_log);
            if (ex is ServerException)            
                SendJSONMessage(context.Response, ex.Message);
            else
                SendJSONMessage(context.Response, ClientMsg.GetErrorMsgJson(error_message_text, "SERVER_ERROR"));
        }

    }
}
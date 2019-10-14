using AppServerBase.Auth;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;

namespace AppServerBase.HttpServer
{
    public abstract class HttpServerModule
    {
        protected string ModuleName;        
        protected string[] urlParts;
        protected NameValueCollection UrlParams;
        protected string Body;
        protected HttpListenerContext Context;

        private Func<bool> CheckLicenseDelegate;
        private Func<string, SessionBase> CheckSessionDelegate;


        public HttpServerModule()
        {

        }


        public ServerModuleResponse Execute(string action, HttpListenerContext context)
        {
            object result = null;
            Context = context;
            void SetResult(object res)
            {
                if (res != null && res is JObject)
                    res = new ServerModuleResponse(res as JObject);

                (res as ServerModuleResponse).Response(context);
            }

            if (!CheckLicense())
            {
                result = new ServerModuleResponse(
                    new JObject(
                        new JProperty("Message", "Лицензия не активна"),
                        new JProperty("SymbCode", "INVALID_LICENSE")));

                SetResult(result);
                return result as ServerModuleResponse;
            }

            GetURLs(context);
            UrlParams = GetUrlParams(context);
            Body = GetBody(context);

            var tp = GetType();

            foreach (var method in tp.GetMethods())
            {
                var attributes = method.GetCustomAttributes(true);
                //foreach (var attr in attributes)
                //    if ((attr is ServerMethodAttribute)
                //        && (attr as ServerMethodAttribute).URL == action)

                var attr = (from _attr in attributes
                            where (_attr is ServerMethodAttribute)
                            && (_attr as ServerMethodAttribute).URL == action
                            select _attr).FirstOrDefault();
                if (attr == null)
                {
                    result = new ServerModuleResponse(
                    new JObject(
                        new JProperty("Message", "Неверный метод"),
                        new JProperty("SymbCode", "INVALID_METHOD")));
                }
                else
                {
                    if (method.GetParameters().Count() > 0)
                        result = method.Invoke(this, new object[] { Body, context });
                    else
                        result = method.Invoke(this, new object[] { });
                    break;
                }
            }

            SetResult(result);
            return result as ServerModuleResponse;
        }

        protected virtual bool CheckLicense()
        {
            if (CheckLicenseDelegate != null)
                return CheckLicenseDelegate.Invoke();
            return true;//License.License.CheckLicense();
        }
        public void SetCheckLicenseMethod(Func<bool> checkLicense)
        {
            CheckLicenseDelegate = checkLicense;
        }

        public void SetCheckSessionMethod(Func<string,SessionBase> checkSession)
        {
            CheckSessionDelegate = checkSession;
        }


        public HttpServerModule(string moduleName)
        {
            ModuleName = moduleName;
         
        }
        protected void GetURLs(HttpListenerContext context)
        {
            urlParts = context.Request.Url.AbsolutePath.Split('/');
        }

        protected NameValueCollection GetUrlParams(HttpListenerContext context)
        {
            return context.Request.QueryString;
        }

        protected string GetBody(HttpListenerContext context)
        {
            if (context.Request.HttpMethod == "POST")
            {
                using (StreamReader reader = new StreamReader(context.Request.InputStream))
                {
                    return reader.ReadToEnd();                
                }
            }
            return null;
        }     
        

        protected SessionBase ValidateSession(string body)
        {
            string token = "";
            try
            {
                JObject json = JObject.Parse(body);
                token = json["token"].ToString();
            }
            catch
            {
                throw new ServerException(ClientMsg.GetErrorMsgInvalidJSON().ToString());
            }

            if (CheckSessionDelegate != null)
                return CheckSessionDelegate.Invoke(token);

            return null; // SessionCache.GetValidSession(token);            
        }
    }
}

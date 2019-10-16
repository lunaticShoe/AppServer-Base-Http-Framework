using AppServerBase.Auth;
using HttpMultipartParser;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

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
                    result = method.Invoke(this, ProcessPrameters(method.GetParameters()));
                }
            }

            SetResult(result);
            return result as ServerModuleResponse;
        }

        public static object GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        private object[] ProcessPrameters(params ParameterInfo[] parameters)
        {
            var paramValues = new List<object>();
            var multipartParams = new Dictionary<string, object>();

            JObject jsonBody = null;
            if (Context.Request.HttpMethod == "POST"
                && Context.Response.ContentType.Contains("application/json"))
            {
                jsonBody = JObject.Parse(Body);
            }
            StreamingMultipartFormDataParser parser = null;
            if (Context.Response.ContentType.Contains("multipart/form-data"))
            {
                
                parser = new StreamingMultipartFormDataParser(Context.Request.InputStream, Encoding.UTF8);

                parser.ParameterHandler += parameter =>
                {
                    multipartParams.Add(parameter.Name, parameter.Data);
                };

                parser.FileHandler += (name, fileName, type, disposition, buffer, bytes) =>
                {
                    multipartParams.Add(name, new { FileName = fileName, Data = bytes });
                };

                //parser.StreamClosedHandler += () =>
                //{
                //    // Do things when my input stream is closed
                //};
                parser.Run();
            }
            
            foreach (var param in parameters)
            {
                if (param.GetCustomAttributes().Count() == 0)
                {
                    paramValues.Add(GetDefault(param.ParameterType));
                    continue;
                }

                var valueAttribute = param.GetCustomAttributes().ElementAt(0);
                var paramNotation = valueAttribute.GetType();
                var paramName = (valueAttribute as ParamAttribute).ParamName;

                if (paramNotation == typeof(GETParamAttribute)
                    || paramNotation == typeof(POSTParamAttribute))
                {
                    if (UrlParams[paramName] == null)
                        throw new Exception($"Parameter not given: {paramName}");
                    paramValues.Add(UrlParams[paramName]);
                }

                if (paramNotation == typeof(JSONParamAttribute))
                {
                    if (!jsonBody.ContainsKey(paramName))
                        throw new ServerException(ClientMsg.GetErrorMsgInvalidJSON());
                    paramValues.Add(Convert.ChangeType(jsonBody[paramName], paramNotation));
                }
                if ((paramNotation == typeof(JSONObjectParamAttribute))
                    || (paramNotation == typeof(JSONArrayParamAttribute)))
                {
                    if (!jsonBody.ContainsKey(paramName))
                        throw new ServerException(ClientMsg.GetErrorMsgInvalidJSON());
                    paramValues.Add(jsonBody[paramName]);
                }
                if ((paramNotation == typeof(MultiPartOSPParamAttribute))
                    || (paramNotation == typeof(MultiPartAPJParamAttribute)))
                {
                    if (!multipartParams.ContainsKey(paramName))
                        throw new Exception($"Parameter not given: {paramName}");
                    paramValues.Add(multipartParams[paramName]);
                }
            }

            return paramValues.ToArray();
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

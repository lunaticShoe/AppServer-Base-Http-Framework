using AppServerBase.HttpServer.ParamResolver;
using HttpMultipartParser;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AppServerBase.HttpServer
{

    public class MultipartData
    {
        public string Name { get; set; }
        public object Data { get; set; }
    }

    public abstract class HttpServerModule
    {
        protected string ModuleName;
        protected string[] urlParts;
        protected NameValueCollection UrlParams;
        protected string Body;
        protected HttpListenerContext Context;



        private Func<bool> CheckLicenseDelegate;
      //  private Func<string, SessionBase> CheckSessionDelegate;


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
                throw new ServerException(
                    new JObject(
                        new JProperty("Message", "Лицензия не активна"),
                        new JProperty("SymbCode", "INVALID_LICENSE")));
            }
            //var act = GetAction(action);
            GetURLs(action);
            UrlParams = GetUrlParams();
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

                if (attr != null)
                {
                    if (IsAsync(method,attributes))
                    {
                        if (method.ReturnType == typeof(Task<ServerModuleResponse>))
                        {
                            var asyncResult = (Task<ServerModuleResponse>)method.Invoke(this,
                                    ProcessPrameters(method.GetParameters()));
                            asyncResult.Wait();
                            result = asyncResult.Result;
                        }

                        if (method.ReturnType == typeof(Task<JObject>))
                        {
                            var asyncResult = (Task<JObject>)method
                                .Invoke(this, ProcessPrameters(method.GetParameters()));
                            asyncResult.Wait();
                            result = asyncResult.Result;
                        }
                    }
                    else
                    {
                        result = method.Invoke(this, ProcessPrameters(method.GetParameters()));
                    }

                    SetResult(result);
                    return result as ServerModuleResponse;
                }
            }


            throw new ServerException(
                new JObject(
                    new JProperty("Message", "Неверный метод"),
                    new JProperty("SymbCode", "INVALID_METHOD")));
        }

        private bool IsAsync(MethodInfo methodInfo, object[] attributes)
        {
            var attr = (from _attr in attributes
                        where (_attr is AsyncStateMachineAttribute)
                        select _attr).FirstOrDefault();

            return attr != null;
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
            Dictionary<string, object> multipartParams = null;// = GetMultiPartParams();

            JObject jsonBody = null;

            string ContentType = Context.Request.ContentType;
            
            if (Context.Request.HttpMethod == "POST"
               && ContentType != null
               && (ContentType.Contains("application/json")
               || ContentType.Contains("text/plain")
               || ContentType.Contains("text/json")
               || ContentType.Contains("application/x-www-form-urlencoded")))
            {
                try
                {
                    jsonBody = JObject.Parse(Body);
                }
                catch
                {
                    jsonBody = new JObject();
                }
            }

            //if (!Context.Request.ContentType.Contains("application/json"))
            //{
            multipartParams = GetMultiPartParams();
            //}
            
            foreach (var param in parameters)
            {
                if (param.GetCustomAttributes().Count() == 0)
                {
                    paramValues.Add(GetDefault(param.ParameterType));
                    continue;
                }

                var valueAttribute = param.GetCustomAttributes()
                    .Where(a => a.GetType().BaseType == typeof(ParamAttribute))
                    .FirstOrDefault();//.ElementAt(0);

                var requiredAttribute = param.GetCustomAttributes()
                    .Where(a => a.GetType() == typeof(NotRequiredAttribute))
                    .FirstOrDefault();

                var isNotRequired = requiredAttribute != null;

                var resolver = ParamResolverFactory.Create(valueAttribute as ParamAttribute, 
                    param.ParameterType, multipartParams, UrlParams, jsonBody, urlParts, 
                    isNotRequired);

                paramValues.Add(resolver.Resolve());
            }

            return paramValues.ToArray();
        }

        protected virtual Dictionary<string, object> GetMultiPartParams()
        {
            var multipartParams = new Dictionary<string, object>();
                       
            StreamingMultipartFormDataParser parser = null;
            if (Context.Request.ContentType != null
                && Context.Request.ContentType.Contains("multipart/form-data"))
            {

                parser = new StreamingMultipartFormDataParser(Context.Request.InputStream, Encoding.UTF8);

                parser.ParameterHandler += parameter =>
                {
                    var val = new MultipartData { Name = parameter.Name, Data = parameter.Data };
                    //if (!multipartParams.ContainsKey(parameter.Name))
                    //    multipartParams.Add(parameter.Name, parameter.Data);

                    if (!multipartParams.ContainsKey(parameter.Name))
                    {
                        multipartParams.Add(parameter.Name, new List<MultipartData>() { val });
                        return;
                    }

                    (multipartParams[parameter.Name] as List<MultipartData>).Add(val);
                };

                parser.FileHandler += (name, fileName, type, disposition, buffer, bytes) =>
                {
                    var data = new MemoryStream();
                    data.Write(buffer, 0, bytes);

                    var val = new MultipartData { Name = fileName, Data = data };
                    if (!multipartParams.ContainsKey(name))
                    {
                        multipartParams.Add(name, new List<MultipartData>() { val });
                        return;
                    }

                    var item = (multipartParams[name] as List<MultipartData>)
                                .FirstOrDefault(md => md.Name == fileName);

                    if (item != null)
                    {
                        (item.Data as MemoryStream).Write(buffer, 0, bytes);
                    }
                    else
                    {
                        (multipartParams[name] as List<MultipartData>).Add(val);
                    }
                };

                //parser.StreamClosedHandler += () =>
                //{
                //    // Do things when my input stream is closed
                //};
                parser.Run();
            }
            return multipartParams;
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

        //public void SetCheckSessionMethod(Func<string,SessionBase> checkSession)
        //{
        //    CheckSessionDelegate = checkSession;
        //}


        public HttpServerModule(string moduleName)
        {
            ModuleName = moduleName;
         
        }


        protected string GetModuleName()
        {
            var at = GetType()
                .GetCustomAttributes()
                .Where(a => a.GetType() == typeof(ServerModuleAttribute))
                .FirstOrDefault();

            return (at as ServerModuleAttribute)?.GetModuleName();         
        }

        protected void GetURLs(string actionName)
        {
            var url = "/" + GetModuleName() + "/" + actionName;
            var path = Context.Request.Url.AbsolutePath;
            url = path.Substring(url.Length);
            urlParts = url.Split('/');
            urlParts = urlParts.Where(item => item != "").ToArray();
        }

        protected NameValueCollection GetUrlParams()
        {
            return Context.Request.QueryString;
        }

        protected string GetBody(HttpListenerContext context)
        {
            if (context.Request.HttpMethod == "POST" 
                && context.Request.ContentType?.IndexOf("multipart/form-data") < 0)
            {
                using (StreamReader reader = new StreamReader(context.Request.InputStream))
                {
                    return reader.ReadToEnd();                
                }
            }
            return null;
        }

        protected virtual string GetAction(string action)
        {
            //urlParts = action.Split('/');
            return action; //urlParts[0];
        }
    }
}

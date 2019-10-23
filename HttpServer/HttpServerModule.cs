﻿using AppServerBase.Auth;
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
                throw new ServerException(
                    new JObject(
                        new JProperty("Message", "Лицензия не активна"),
                        new JProperty("SymbCode", "INVALID_LICENSE")));
            }

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
                    result = method.Invoke(this, ProcessPrameters(method.GetParameters()));
                    SetResult(result);
                    return result as ServerModuleResponse;
                }
            }


            throw new ServerException(
                new JObject(
                    new JProperty("Message", "Неверный метод"),
                    new JProperty("SymbCode", "INVALID_METHOD")));          
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

            if (Context.Request.HttpMethod == "POST"
               && Context.Request.ContentType != null
               && Context.Request.ContentType.Contains("application/json"))
            {
                jsonBody = JObject.Parse(Body);
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
                    paramValues.Add(Convert.ChangeType(jsonBody[paramName].ToString(), param.ParameterType));
                }
                if ((paramNotation == typeof(JSONObjectParamAttribute))
                    || (paramNotation == typeof(JSONArrayParamAttribute)))
                {
                    if (!jsonBody.ContainsKey(paramName))
                        throw new ServerException(ClientMsg.GetErrorMsgInvalidJSON());
                    paramValues.Add(jsonBody[paramName]);
                }
                if (paramNotation == typeof(MultiPartOSPParamAttribute))
                {
                    if (multipartParams == null || !multipartParams.ContainsKey(paramName))
                        throw new Exception($"Parameter not given: {paramName}");
                    if (param.ParameterType.IsArray)
                        paramValues.Add((multipartParams[paramName] as List<MultipartData>).ToArray());
                    else
                        paramValues.Add(
                            (multipartParams[paramName] as List<MultipartData>)
                            .FirstOrDefault());
                }
                if (paramNotation == typeof(MultiPartTextParamAttribute))
                {
                    if (multipartParams == null || !multipartParams.ContainsKey(paramName))
                        throw new Exception($"Parameter not given: {paramName}");
                    if (param.ParameterType.IsArray)
                        paramValues.Add((multipartParams[paramName] as List<MultipartData>)
                            .Select(md=> md.Data).ToArray());
                    else
                        paramValues.Add(
                            (multipartParams[paramName] as List<MultipartData>)
                            .FirstOrDefault()?.Data);
                }
                if (paramNotation == typeof(URLParamAttribute))
                {
                    var num = (valueAttribute as URLParamAttribute).ParamNumber;
                    if (urlParts.ElementAtOrDefault(num) !=null)
                    {
                        paramValues.Add(num);
                    }
                }
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

        public void SetCheckSessionMethod(Func<string,SessionBase> checkSession)
        {
            CheckSessionDelegate = checkSession;
        }


        public HttpServerModule(string moduleName)
        {
            ModuleName = moduleName;
         
        }


        protected string GetModuleName()
        {
            var at = GetType()
                .GetCustomAttributes()
                .Where(a => a.GetType() == typeof(HttpServerModule))
                .FirstOrDefault();

            return (at as ServerModuleAttribute)?.GetModuleName();         
        }

        protected void GetURLs(string actionName)
        {
            var url = GetModuleName() + "/" + actionName;
            var path = Context.Request.Url.AbsolutePath;
            url = path.Substring(url.Length);
            urlParts = url.Split('/');
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

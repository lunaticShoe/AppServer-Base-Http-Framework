

using AppServerBase.HttpServer;
using AppServerBase.HttpServer.ParamResolver;
using AppServerBase.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;

class JsonParamResolver : IParamResolver 
{
    private readonly Type ParamType;
    private readonly string ParamName;
    private readonly JObject Json;
    private readonly bool IsNotRequired;
    private readonly bool IsSimpleField;
    public JsonParamResolver(string paramName, Type paramType, JObject json, bool isNotRequired, bool isSimpleField)
            => (ParamType, ParamName, Json, IsNotRequired, IsSimpleField)
        = (paramType, paramName, json, isNotRequired, isSimpleField);

    public object Resolve()
    {
        if (IsNotRequired && !Json.ContainsKey(ParamName))
        {
            return IsSimpleField ? ObjectUtils.GetDefault(ParamType) : null;
        }

        if (!Json.ContainsKey(ParamName)) 
        { 
            throw new ServerException(ClientMsg.GetErrorMsgInvalidJSON());
        }

        if (IsSimpleField)
        {
            return Convert.ChangeType(
                Json[ParamName].ToString(),
                ParamType);
        }
        else
        {
            return Json[ParamName];
        }
            
    }
}

class JsonArrayParamResolver : IParamResolver
{
    private readonly Type ParamType;
    private readonly string ParamName;
    private readonly JObject Json;
    private readonly bool IsNotRequired;
    
    public JsonArrayParamResolver(string paramName, Type paramType, JObject json, bool isNotRequired)
            => (ParamType, ParamName, Json, IsNotRequired)
        = (paramType, paramName, json, isNotRequired);

    public object Resolve()
    {
        if (IsNotRequired && !Json.ContainsKey(ParamName))
        {
            return null;
        }

        if (!Json.ContainsKey(ParamName))
        {
            throw new ServerException(ClientMsg.GetErrorMsgInvalidJSON());
        }

        if (ParamType.GetTypeInfo().IsSubclassOf(typeof(IEnumerable<>)))
        {

        }




        return Json[ParamName] as JArray;
        

    }
}

class HttpParamResolver : IParamResolver
{
    private readonly Type ParamType;
    private readonly string ParamName;
    private readonly NameValueCollection UrlParams;
    private readonly bool IsNotRequired;

    public HttpParamResolver(string paramName, Type paramType, NameValueCollection urlParams, bool isNotRequired)
        => (ParamType, ParamName, UrlParams, IsNotRequired) 
        = (paramType, paramName, urlParams, isNotRequired);

    public object Resolve()
    {
        if (IsNotRequired && UrlParams[ParamName] == null)
        {
            return ObjectUtils.GetDefault(ParamType);
        }

        if (UrlParams[ParamName] == null)
            throw new Exception($"Parameter not given: {ParamName}");

        return Convert.ChangeType(UrlParams[ParamName], ParamType);
    }
}

class URLParamResolver : IParamResolver
{
    private readonly Type ParamType;
    private readonly int ParamNumber;
    private readonly string[] UrlParts;
    public URLParamResolver(int paramNumber, Type paramType, string[] urlParts)
        => (ParamType, ParamNumber, UrlParts) = (paramType, paramNumber, urlParts);

    public object Resolve()
    {
        if(UrlParts.ElementAtOrDefault(ParamNumber) != null)
        {
            return Convert.ChangeType(
                    UrlParts.ElementAtOrDefault(ParamNumber),
                    ParamType);
        }
        return null;
    }
}

class MultiPartParamResolver : IParamResolver
{
    private readonly Type ParamType;
    private readonly string ParamName;
    private readonly bool IsNotRequired;
    private readonly Dictionary<string, object> MultipartParams;
    public MultiPartParamResolver(string paramName, Type paramType, Dictionary<string,object> multipartParams, bool isNotRequired)
            => (ParamType, ParamName, MultipartParams, IsNotRequired)
        = (paramType, paramName, multipartParams, isNotRequired);

    public object Resolve()
    {
        if (IsNotRequired && (MultipartParams == null || !MultipartParams.ContainsKey(ParamName)))
        {
            return null;
        }

        if (MultipartParams == null || !MultipartParams.ContainsKey(ParamName))
            throw new Exception($"Parameter not given: {ParamName}");
        if (ParamType.IsArray)
            return (MultipartParams[ParamName] as List<MultipartData>)
                .ToArray();
        else
            return
                (MultipartParams[ParamName] as List<MultipartData>)
                .FirstOrDefault();
    }
}

class MultiPartParamTextResolver : IParamResolver
{
    private readonly Type ParamType;
    private readonly string ParamName;
    private readonly bool IsNotRequired;
    private readonly Dictionary<string, object> MultipartParams;
    public MultiPartParamTextResolver(string paramName, Type paramType, Dictionary<string, object> multipartParams, bool isNotRequired)
            => (ParamType, ParamName, MultipartParams, IsNotRequired)
        = (paramType, paramName, multipartParams, isNotRequired);

    public object Resolve()
    {
        if (IsNotRequired && (MultipartParams == null || !MultipartParams.ContainsKey(ParamName)))
        {
            return null;
        }

        if (MultipartParams == null || !MultipartParams.ContainsKey(ParamName))
            throw new Exception($"Parameter not given: {ParamName}");
        if (ParamType.IsArray)
            return (MultipartParams[ParamName] as List<MultipartData>)
                .Select(md => md.Data).ToArray();
        else
            return
                (MultipartParams[ParamName] as List<MultipartData>)
                .FirstOrDefault()?.Data;
    }
}
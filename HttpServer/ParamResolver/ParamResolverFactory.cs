using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Text;

namespace AppServerBase.HttpServer.ParamResolver
{
    class ParamResolverFactory
    {
        public static IParamResolver Create(ParamAttribute attribute, Type paramType,
            Dictionary<string, object> multipartParams, NameValueCollection UrlParams, JObject json,
            string[] urlParts, bool isNotRequired)
        {
            if (attribute is GETParamAttribute
                    || attribute is POSTParamAttribute)
            {
                return new HttpParamResolver(attribute.ParamName, paramType,
                    UrlParams, isNotRequired);
            }
            if (attribute is URLParamAttribute)
            {
                var num = (attribute as URLParamAttribute).ParamNumber;
                return new URLParamResolver(num, paramType, urlParts);
            }

            if (attribute is JSONParamAttribute)
            {
                return new JsonParamResolver(attribute.ParamName, paramType, 
                    json, isNotRequired,isSimpleField: true);
            }
            if (attribute is JSONObjectParamAttribute
                || attribute is JSONArrayParamAttribute)
            {
                return new JsonParamResolver(attribute.ParamName, paramType,
                    json, isNotRequired, isSimpleField: false);
            }           
            if (attribute is MultiPartOSPParamAttribute)
            {
                return new MultiPartParamResolver(attribute.ParamName, paramType, 
                    multipartParams, isNotRequired);
            } 
            if (attribute is MultiPartTextParamAttribute)
            {
                return new MultiPartParamTextResolver(attribute.ParamName, paramType,
                    multipartParams, isNotRequired);
            } 
            return null;
        }
    }
}

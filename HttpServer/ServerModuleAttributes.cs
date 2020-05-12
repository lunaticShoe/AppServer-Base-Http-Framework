using AppServerBase.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;

namespace AppServerBase.HttpServer
{

    interface IJsonParam
    {
        object ResolveParam(ParameterInfo param, JObject json, bool isNotRequired);
    }

    interface IHttpParam
    {
        object ResolveParam(ParameterInfo param, object paramValue, bool isNotRequired);
    }

    interface IURLParam
    {
        object ResolveParam(ParameterInfo param, string[] urlParts);
    }


    /// <summary>
    /// Обозначить метод
    /// </summary>
    [AttributeUsage(AttributeTargets.Method,
                       AllowMultiple = true)
        
    ]
    public class ServerMethodAttribute : Attribute
    {
        public string URL { get; private set; }
        public bool ValidateSession { get; private set; }

        public ServerMethodAttribute(string URL)
        {
            this.URL = URL;
        }

        public ServerMethodAttribute(string URL, bool ValidateSession)
        {
            this.URL = URL;
            this.ValidateSession = ValidateSession;
        }
    }
    /// <summary>
    /// Обозначить модуль
    /// </summary>
    [AttributeUsage(AttributeTargets.Class,
                       AllowMultiple = true)
    ]
    public class ServerModuleAttribute : Attribute
    {
        private string ModuleName;

        public ServerModuleAttribute(string ModuleName)
        {
            this.ModuleName = ModuleName;
        }

        public string GetModuleName()
        {
            return ModuleName;
        }
    }

    public abstract class ParamAttribute : Attribute
    {
        public string ParamName { get; protected set; }

        public ParamAttribute(string ParamName)
            => this.ParamName = ParamName;        

    }
    [AttributeUsage(AttributeTargets.Parameter,
                   AllowMultiple = false)
  ]
    public class NotRequiredAttribute : Attribute
    {
        public NotRequiredAttribute()
        {

        }
    }
          
    /// <summary>
    /// Обозначить параметр как GET-параметр
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter,
                     AllowMultiple = false)
    ]
    public class GETParamAttribute : ParamAttribute, IHttpParam
    {
        public GETParamAttribute(string ParamName) : base(ParamName)
        {
        }

        public object ResolveParam(ParameterInfo param, object paramValue, bool isNotRequired)
        {
            if (isNotRequired && paramValue == null)
            {
                return ObjectUtils.GetDefault(param.ParameterType);
            }

            if (paramValue == null)
                throw new Exception($"Parameter not given: {ParamName}");

            return Convert.ChangeType(paramValue,param.ParameterType);
        }
    }
    /// <summary>
    /// Обозначить параметр как POST-параметр
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter,
                     AllowMultiple = false)
    ]
    public class POSTParamAttribute : ParamAttribute, IHttpParam
    {
        public POSTParamAttribute(string ParamName) : base(ParamName)
        {
        }

        public object ResolveParam(ParameterInfo param, object paramValue, bool isNotRequired)
        {
            if (isNotRequired && paramValue == null)
            {
                return ObjectUtils.GetDefault(param.ParameterType);
            }

            if (paramValue == null)
                throw new Exception($"Parameter not given: {ParamName}");

            return Convert.ChangeType(paramValue, param.ParameterType);
        }
    }
    /// <summary>
    /// Обозначить параметр как поле JObject
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter,
                     AllowMultiple = false)
    ]
    public class JSONParamAttribute : ParamAttribute, IJsonParam
    {
        public JSONParamAttribute(string ParamName) : base(ParamName)
        {
        }

        public object ResolveParam(ParameterInfo param, JObject json, bool isNotRequired)
        {
            if (isNotRequired && !json.ContainsKey(ParamName))
            {
                return ObjectUtils.GetDefault(param.ParameterType);                
            }

            if (!json.ContainsKey(ParamName))
                throw new ServerException(ClientMsg.GetErrorMsgInvalidJSON());
            return Convert.ChangeType(
                    json[ParamName].ToString(),
                    param.ParameterType);
        }
    }

    /// <summary>
    /// Обозначить параметр как JObject
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter,
                     AllowMultiple = false)
    ]
    public class JSONObjectParamAttribute : ParamAttribute, IJsonParam
    {
        public JSONObjectParamAttribute(string ParamName) : base(ParamName)
        {
        }

        public object ResolveParam(ParameterInfo param, JObject json, bool isNotRequired)
        {
            if (isNotRequired && !json.ContainsKey(ParamName))
            {
                return null;
            }

            if (!json.ContainsKey(ParamName))
                throw new ServerException(ClientMsg.GetErrorMsgInvalidJSON());
            return json[ParamName];
        }
    }

    /// <summary>
    /// Обозначить параметр как JArray
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter,
                     AllowMultiple = false)
    ]
    public class JSONArrayParamAttribute : ParamAttribute, IJsonParam
    {
        private readonly Type ParamType;

        public JSONArrayParamAttribute(string ParamName) 
            : base(ParamName)
        {
        }

        public JSONArrayParamAttribute(string ParamName, Type paramType)
            : this(ParamName) => ParamType = paramType;

        public object ResolveParam(ParameterInfo param, JObject json, bool isNotRequired)
        {
            if (isNotRequired && !json.ContainsKey(ParamName))
            {
                return null;
            }

            if (!json.ContainsKey(ParamName))
                throw new ServerException(ClientMsg.GetErrorMsgInvalidJSON());
            return json[ParamName];
        }
    }

    [AttributeUsage(AttributeTargets.Parameter,
                 AllowMultiple = false)
    ]
    public class MultiPartOSPParamAttribute : ParamAttribute
    {
        public MultiPartOSPParamAttribute(string ParamName) : base(ParamName)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Parameter,
             AllowMultiple = false)
]
    public class MultiPartTextParamAttribute : ParamAttribute
    {
        public MultiPartTextParamAttribute(string ParamName) : base(ParamName)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Parameter,
             AllowMultiple = false)
]
    public class URLParamAttribute : ParamAttribute, IURLParam
    {
        public int ParamNumber { get; private set; }
        public URLParamAttribute(int ParamNumber) : base("")
        {
            this.ParamNumber = ParamNumber;
        }

        public object ResolveParam(ParameterInfo param, string[] urlParts)
        {            
            if (urlParts.ElementAtOrDefault(ParamNumber) != null)
            {
                return Convert.ChangeType(
                        urlParts.ElementAtOrDefault(ParamNumber),
                        param.ParameterType);
            }
            return null;
        }
    }
}

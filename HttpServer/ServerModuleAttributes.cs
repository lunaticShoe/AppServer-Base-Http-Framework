using System;

namespace AppServerBase.HttpServer
{
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

    public class ParamAttribute : Attribute
    {
        public string ParamName { get; protected set; }

        public ParamAttribute(string ParamName)
        {
            this.ParamName = ParamName;
        }
    }
          
    /// <summary>
    /// Обозначить параметр как GET-параметр
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter,
                     AllowMultiple = true)
    ]
    public class GETParamAttribute : ParamAttribute
    {
        public GETParamAttribute(string ParamName) : base(ParamName)
        {
        }
    }
    /// <summary>
    /// Обозначить параметр как POST-параметр
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter,
                     AllowMultiple = true)
    ]
    public class POSTParamAttribute : ParamAttribute
    {
        public POSTParamAttribute(string ParamName) : base(ParamName)
        {
        }
    }
    /// <summary>
    /// Обозначить параметр как поле JObject
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter,
                     AllowMultiple = true)
    ]
    public class JSONParamAttribute : ParamAttribute
    {
        public JSONParamAttribute(string ParamName) : base(ParamName)
        {
        }
    }

    /// <summary>
    /// Обозначить параметр как JObject
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter,
                     AllowMultiple = true)
    ]
    public class JSONObjectParamAttribute : ParamAttribute
    {
        public JSONObjectParamAttribute(string ParamName) : base(ParamName)
        {
        }
    }

    /// <summary>
    /// Обозначить параметр как JArray
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter,
                     AllowMultiple = true)
    ]
    public class JSONArrayParamAttribute : ParamAttribute
    {
        public JSONArrayParamAttribute(string ParamName) : base(ParamName)
        {
        }
    }


}

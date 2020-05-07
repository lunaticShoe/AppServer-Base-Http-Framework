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
                     AllowMultiple = false)
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
                     AllowMultiple = false)
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
                     AllowMultiple = false)
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
                     AllowMultiple = false)
    ]
    public class JSONArrayParamAttribute : ParamAttribute
    {
        private readonly Type ParamType;

        public JSONArrayParamAttribute(string ParamName) 
            : base(ParamName)
        {
        }

        public JSONArrayParamAttribute(string ParamName, Type paramType)
            : this(ParamName) => ParamType = paramType;
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
    public class URLParamAttribute : ParamAttribute
    {
        public int ParamNumber { get; private set; }
        public URLParamAttribute(int ParamNumber) : base("")
        {
            this.ParamNumber = ParamNumber;
        }
    }
}

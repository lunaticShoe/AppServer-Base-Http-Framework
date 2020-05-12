using System;
using System.Collections.Generic;
using System.Text;

namespace AppServerBase.Utils
{
    class ObjectUtils
    {
        public static object GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }
    }
}

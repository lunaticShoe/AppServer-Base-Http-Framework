using System;
using System.Collections.Generic;
using System.Text;

namespace AppServerBase.HttpServer.ParamResolver
{
    interface IParamResolver
    {
        object Resolve();
    }
}

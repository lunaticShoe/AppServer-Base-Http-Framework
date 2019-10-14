using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace AppServerBase.HttpServer
{
    public class ServerException : Exception
    {
        public ServerException(string message) : base(message)
        {

        }

        public ServerException(JObject message) : base(message.ToString())
        {

        }
    }

}

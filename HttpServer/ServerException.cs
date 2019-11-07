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

        private static string GetMessage(string message, string code)
        {
            var msg = new JObject
            {
                { "Message", message },
                { "SymbCode", code }
            };
            return msg.ToString();
        }

        public ServerException(string message, string code) 
            : base(GetMessage(message, code))
        {
            
        }
    }

}

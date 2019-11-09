using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AppServerBase.HttpServer
{
    public class ClientMsg
    {
        public static JObject GetOKMessage()
        {
            return new JObject { { "SymbCode", "OK" } };
        }

        public static JObject GetErrorMsgInvalidJSON()
        {
            return GetErrorMsgJsonJ(
                "Invalid request Json!", 
                "INVALID_JSON");           
        }

        public static string GetErrorMsgJson(string msg,string symbcode)
        {
            return GetErrorMsgJsonJ(msg, symbcode).ToString();
        }

        public static JObject GetErrorMsgJsonJ(string msg, string symbcode)
        {
            return new JObject
            {
                { "Message", msg },
                { "SymbCode", symbcode }
            };
        }
    }
}

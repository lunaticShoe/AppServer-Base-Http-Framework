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

        /*
        
         "INVALID_LOGIN_OR_PASSWORD"
         "INVALID_LICENSE"
         "INVALID_AUTH_TOKEN"
         "INVALID_TOKEN"
         "TOKEN_EXPIRED"
         "INVALID_JSON"
         "SERVER_ERROR"   
         "QUERY_ERROR"
         "QUERY_USER_ERROR"    
         "INVALID_URL"   
         "QUERY_REPORT_ERROR"
         "QUERY_GROUP_DOES_NOT_EXISTS"
         "INVALID_PROJECT_OR_SESSION"
         "ERROR_WHILE_GETTING_FILE_SETTING"
         "INVALID_FILE_SET_PARAMS"
         "FILE_NOT_FOUND"
         "DELETING_FILE_ERROR"

      //   "INVALID_QUERY_NAME"
         "INVALID_TARGET_QUERY"
         "QUERY_NOT_FOUND"
         "ACCESS_DENIED" 
         "INVALID_SERVICE_TOKEN"
         "DOCUMENT_LOCK_STATE_UNAVAILABLE"
         API_BAD_PARENT
         MOD_CHANGE_FORBIDDEN

             */



        public static JObject GetOKMessage()
        {
            return new JObject(new JProperty("SymbCode", "OK"));
        }

        public static JObject GetErrorMsgInvalidJSON()
        {
            //return JObject.Parse(ClientErrorMsg.GetErrorMsgJson("Invalid guest request Json!", 102,"INVALID_JSON"));
            return new JObject(new JProperty("Message", "Invalid request Json!"),                              
                               new JProperty("SymbCode", "INVALID_JSON")//,
                              // new JProperty("Type", "SYSTEM")
                                 );
        }

        public static string GetErrorMsgJson(string msg,string symbcode,string type ="SYSTEM")
        {
            return new JObject(new JProperty("Message", msg),                       
                               new JProperty("SymbCode", symbcode)//, 
                              // new JProperty("Type", type)                               
                                 ).ToString();
        }

        public static JObject GetErrorMsgJsonJ(string msg, string symbcode, string type = "SYSTEM")
        {
            return new JObject(new JProperty("Message", msg),                      
                               new JProperty("SymbCode", symbcode)//,
                          //     new JProperty("Type", type)
                                 );
        }
    }
}

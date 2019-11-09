# AppServer Base Http Framework Alpha

Basic Application Server framework for .Net (Alpha version)

## API Server Usage

HttpServer usage

```
using AppServerBase.HttpServer;
using System;
using System.Threading;

namespace SNMPAgent
{
    class Program
    {
        static void Main(string[] args)
        {
           var httpserver = new HTTPServer();
		   httpserver.Start( "http://+:80","https://+:443" );
		   Thread.Sleep(10 * 1000);
        }
	}
}
```

To have API called you also need to create a module.

```
using AppServerBase.HttpServer;
using Newtonsoft.Json.Linq;

namespace SNMPAgent.Statistics
{
    [ServerModule("statistics")]
    class StatisticsModule : HttpServerModule
    {
        [ServerMethod("global")]
        public JObject GetGlobalStatistics(
            [JSONParam("token")] string token,
			[NotRequired] 
			[JSONParam("from_date")] DateTime fromDate)
        {

            //Your code

            return ClientMsg.GetOKMessage();
        }
    }
}
```

Module methods can be used with different types of params.
Parameters could also be marked with attribute "NotRequired" so it wont create an error if the parameter is missing

### GET/URL params

URLParam(0) Takes a value of url by index (position) from module + method url. 
This is used for urls like this https://drive.google.com/drive/my-drive/1/2
where "drive" will be a module and the "my-drive" will be a method, URLParam(0) will take "1" as value

GetParam("") Taskes a value of Http-Get param from url by name

### POST params

Those parameters will be searched if the content-type of the post is equals to application/json or text/plain or application/x-www-form-urlencoded 

POSTParam("") Taskes a value of Http-Post param by name
JSONParam("") Takes a value of JSON Object by name in Http Post
JSONObjectParam("") Takes a JSON object inside JSON Object in Http Post by name, returns JObject (Newtonsoft.JSON)
JSONArrayParam("")  Takes a JSON array inside JSON Object in Http Post by name, returns JArray (Newtonsoft.JSON)

Those parameters will be searched if the content-type multipart/form-data

MultiPartOSPParam("") Takes binary multipart-data param. if several params has equal name engine will return an array 
MultiPartTextParam("") Takes text or application/json multipart-data param.


using Serilog;
using System;

namespace AppServerBase.Utils
{    
    public static class ServerLog
    {
        //private static volatile List<LogItem> LogList = new List<LogItem>();
        private static int FileRetainedCount = 7;
        private static string LogDirrectory = "";
       
        public static void SetLogConfiguration(string logDirrectory, int fileRetainedCount)
        {
            LogDirrectory = logDirrectory;
            FileRetainedCount = fileRetainedCount;
        }

        public static void Log(string message, string alias = "")
        {
            var baseAlias = "total";
            var basePrefix = "t";

            if (!string.IsNullOrEmpty(alias))
            {
                baseAlias = $"{baseAlias}_{alias}";
                basePrefix = $"{basePrefix}_{alias}";
            }

            var fileName = basePrefix + "_log_{Date}.txt";
            var dir = LogDirrectory + "/log/" + baseAlias + "/";

            var log = new LoggerConfiguration()
                .WriteTo.RollingFile(
                    dir + fileName,
                    retainedFileCountLimit: FileRetainedCount,
                    shared: true,
                    outputTemplate: "{Timestamp:dd.MM.yyyy HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}") 
                .CreateLogger();
            log.Information(message);
        }

        public static void LogDebug(string message, string alias = "")
        {
            //var dir = LogDirrectory + "/log/total/";

            var baseAlias = "debug";
            var basePrefix = "d";

            if (!string.IsNullOrEmpty(alias))
            {
                baseAlias = $"{baseAlias}_{alias}";
                basePrefix = $"{basePrefix}_{alias}";
            }

            var fileName = basePrefix + "_log_{Date}.txt";
            var dir = LogDirrectory + "/log/" + baseAlias + "/";

            var log = new LoggerConfiguration()
                .WriteTo.RollingFile(
                    dir + fileName,
                    retainedFileCountLimit: FileRetainedCount,
                    shared: true,
                    outputTemplate: "{Timestamp:dd.MM.yyyy HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}")
                .CreateLogger();
            log.Debug(message);
            
        }

        public static void LogError(string message, string alias = "")
        {
            var baseAlias = "error";
            var basePrefix = "e";
            

            if (!string.IsNullOrEmpty(alias))
            {
                baseAlias = $"{baseAlias}_{alias}";
                basePrefix = $"{basePrefix}_{alias}";
            }

            var fileName = basePrefix + "_log_{Date}.txt";
            var dir = LogDirrectory + "/log/"+ baseAlias + "/";

            var log = new LoggerConfiguration()
                .WriteTo.RollingFile(
                    dir + fileName,
                    retainedFileCountLimit: FileRetainedCount,
                    shared: true,
                    outputTemplate: "{Timestamp:dd.MM.yyyy HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}") 
                .CreateLogger();
            log.Error(message);
        }


        
    }
}

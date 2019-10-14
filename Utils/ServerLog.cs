using Serilog;
using System;

namespace AppServerBase.Utils
{
    public class LogItem
    {
        public string message;
        public string method;
        public DateTime event_data;

        public LogItem(string message, string method)
        {
            this.message = message;
            this.method = method;
            
            event_data = DateTime.Now;
        }
    }

    public enum MessageType { SERVER_ERROR, ORACLE_ERROR, USER_ERROR, USER_EVENT,MYSQL_ERROR };

    public static class ServerLog
    {
        //private static volatile List<LogItem> LogList = new List<LogItem>();
        private static int FileRetainedCount = 7;
        private static string LogDirrectory = "/log/";
       
        public static void SetLogConfiguration(string logDirrectory, int fileRetainedCount)
        {
            LogDirrectory = logDirrectory;
            FileRetainedCount = fileRetainedCount;
        }

        public static void Log(string message)
        {
            var dir = LogDirrectory + "/log/total/";

            var log = new LoggerConfiguration()
                .WriteTo.RollingFile(
                    dir + "t_log_{Date}.txt",
                    retainedFileCountLimit: FileRetainedCount,
                    shared: true,
                    outputTemplate: "{Timestamp:dd.MM.yyyy HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}") 
                .CreateLogger();
            log.Information(message);

        }

        public static void LogDebug(string message)
        {
            var dir = LogDirrectory + "/log/total/";

            var log = new LoggerConfiguration()
                .WriteTo.RollingFile(
                    dir + "d_log_{Date}.txt",
                    retainedFileCountLimit: FileRetainedCount,
                    shared: true,
                    outputTemplate: "{Timestamp:dd.MM.yyyy HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}")
                .CreateLogger();
            log.Information(message);

        }


        public static void LogError(string message, string ErrorCode, MessageType errorType)
        {
            var msg = @"";
            switch(errorType)
            {
                case MessageType.SERVER_ERROR:
                    msg += "CRITICAL SERVER ERROR!";                 
                    break;
                case MessageType.ORACLE_ERROR:
                    msg += "ORACLE ERROR!";
                    break;
                case MessageType.USER_ERROR:
                    msg += "USER ERROR!";
                    break;
                case MessageType.MYSQL_ERROR:
                    msg += "MYSQL ERROR!";
                    break;
                default:
                    break;
            }

            msg += "\n\n" + message;

            if (ErrorCode != "")
                msg += "\nError type: " + ErrorCode;

            var dir = LogDirrectory + "/log/error/";

            var log = new LoggerConfiguration()
                .WriteTo.RollingFile(
                    dir + "e_log_{Date}.txt",
                    retainedFileCountLimit: FileRetainedCount,
                    shared: true,
                    outputTemplate: "{Timestamp:dd.MM.yyyy HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}") 
                .CreateLogger();
            log.Information(msg);
        }

        public static void LogAuthEvent(string message)
        {
            var dir = LogDirrectory + "/log/auth/";

            var log = new LoggerConfiguration()
                .WriteTo.RollingFile(
                    dir + "a_log_{Date}.txt",
                    retainedFileCountLimit: FileRetainedCount,
                    shared: true,
                    outputTemplate: "{Timestamp:dd.MM.yyyy HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}") 
                .CreateLogger();
            log.Information(message);
        }

        //public static void LogUserEvent(Session session, string message)
        //{
        //    var dir = ServerConfig.GetServerDir() + "/log/" + session.schemeName + "/" + (session.login==""?"guest":session.login) + "/";

        //    var log = new LoggerConfiguration()
        //        .WriteTo.RollingFile(
        //            dir + "u_log_{Date}.txt",
        //            retainedFileCountLimit: ServerConfig.FileRetainedCount,
        //            shared: true,
        //            outputTemplate: "{Timestamp:dd.MM.yyyy HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}") 
        //        .CreateLogger();
        //    log.Information(message);
        //}

        public static void LogUserEvent(string login, string projectName, string message)
        {
            var dir = LogDirrectory + "/log/" + projectName + "/" + login + "/";

            var log = new LoggerConfiguration()
                .WriteTo.RollingFile(
                    dir + "u_log_{Date}.txt",
                    retainedFileCountLimit: FileRetainedCount,
                    shared: true,
                    outputTemplate: "{Timestamp:dd.MM.yyyy HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}")
                .CreateLogger();
            log.Information(message);
        }


        public static void LogEventer(string message)
        {
            var dir = LogDirrectory + "/log/";

            var log = new LoggerConfiguration()
                .WriteTo.RollingFile(
                    dir + "eventer_log_{Date}.txt",
                    retainedFileCountLimit: FileRetainedCount,
                    shared: true,
                    outputTemplate: "{Timestamp:dd.MM.yyyy HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}")
                .CreateLogger();
            log.Information(message);

        }
    }
}

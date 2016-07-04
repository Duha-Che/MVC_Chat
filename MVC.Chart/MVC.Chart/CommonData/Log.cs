using System;
using log4net;
using log4net.Appender;
using log4net.Config;

namespace MVC.Chart
{
    internal static class Log
    {
        static Log()
        {
            XmlConfigurator.Configure();
            var hierarchy =
                (log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository();
            foreach (IAppender each in hierarchy.Root.Appenders)
            {
                FileAppender fa = (FileAppender)each;
                if (fa != null && fa.Name == "MVC.Chat")
                {
                    fa.File = Settings.Default.LoggingPath;
                    fa.ActivateOptions();
                }
            }
        }
        private static readonly ILog log = LogManager.GetLogger("MVC.Chat");

        public static void Message(string format, params object[] args)
        {
            log.InfoFormat(format, args);
        }

        public static void Error(string format, params object[] args)
        {
            log.ErrorFormat(format, args);
        }

        public static void Error(Exception e, string format, params object[] args)
        {
            var message = String.Format(format, args);
            log.Error(message, e);
        }

        public static void Debug(string format, params object[] args)
        {
            log.DebugFormat(format, args);
        }

        public static void Debug(Exception e, string format, params object[] args)
        {
            var message = String.Format(format, args);
            log.Debug(message, e);
        }

        public static void Warning(string format, params object[] args)
        {
            log.WarnFormat(format, args);
        }

        public static void Warning(Exception e, string format, params object[] args)
        {
            var message = String.Format(format, args);
            log.Warn(message, e);
        }
    }
}
using System;

namespace MVC.Chart
{
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase
    {
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));

        public static Settings Default
        {
            get
            {
                return defaultInstance;
            }
        }

        public String LoggingPath { get; private set; }

        internal static void ApplySettings( string loggingPath)
        {
            Settings.Default.LoggingPath = loggingPath;
        }
    }
}
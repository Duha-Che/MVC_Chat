using System;

namespace DAL.Properties
{
    partial class Settings
    {
 
        public String LoggingPath { get; private set; }

        internal static void ApplySettings(string loggingPath)
        {
            Settings.Default.LoggingPath = loggingPath;
        }
    }

    public static class SettingsPropagator
    {
        public static void ApplySettings( string loggingPath)
        {
            Settings.ApplySettings( loggingPath);
        }
    }
}

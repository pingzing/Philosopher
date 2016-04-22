// Helpers/Settings.cs
using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace Philosopher.Multiplat.Helpers
{
    /// <summary>
    /// This is the Settings static class that can be used in your Core solution or in any
    /// of your client applications. All settings are laid out the same exact way with getters
    /// and setters. 
    /// </summary>
    public static class Settings
    {
        private static ISettings AppSettings => CrossSettings.Current;

        #region Setting Constants        

        private const string HostnameKey = "hostname_key";
        private static readonly string HostnameDefault = "http://localhost";

        private const string PortKey = "port_key";
        private static readonly long PortDefault = 3000;

        #endregion

        public static string HostnameSetting
        {
            get { return AppSettings.GetValueOrDefault(HostnameKey, HostnameDefault); }
            set { AppSettings.AddOrUpdateValue(HostnameKey, value); }
        }

        public static long PortSetting
        {
            get { return AppSettings.GetValueOrDefault(PortKey, PortDefault); }
            set { AppSettings.AddOrUpdateValue(PortKey, value); }
        }

    }
}
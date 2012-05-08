namespace Ogdi.InteractiveSdk.Mvc
{
    using Microsoft.WindowsAzure.ServiceRuntime;
    using System.Collections.Specialized;
    using System.Configuration;

    public static class SettingsResolver
    {
        public static NameValueCollection Settings = ConfigurationManager.AppSettings;

        public static bool EnableAnalytics
        {
            get
            {
                return bool.Parse((RoleEnvironment.IsAvailable)
                       ? RoleEnvironment.GetConfigurationSettingValue("EnableAnalytics")
                       : Settings["EnableAnalytics"]);
            }
        }
    }
}
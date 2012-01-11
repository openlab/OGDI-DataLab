using System.Configuration;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Ogdi.Azure.Configuration
{
    public sealed class OgdiConfiguration
    {
        /// <summary>
        /// This method finds the value for the passed key from the 
        /// AppSettings in XML or Windows Azure role environment.
        /// </summary>
        /// <param name="key">The value name.</param>
        /// <returns>The value.</returns>
        public static string GetValue(string key)
        {
            return (RoleEnvironment.IsAvailable)
                ? RoleEnvironment.GetConfigurationSettingValue(key)
                : ConfigurationManager.AppSettings[key];
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Ogdi.Config;

namespace Ogdi.DataServices
{
    public static class AppSettings
    {
        static AppSettings()
        {
            RefreshAvailableEndpoints();
        }

        internal static Dictionary<string, AvailableEndpoint> RefreshAvailableEndpoints()
        {
            var ta =
                CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("OgdiConfigConnectionString"));

            var ogdiConfigContext = new OgdiConfigDataServiceContext(ta.TableEndpoint.AbsoluteUri, ta.Credentials);

            var availableEndpoints = ogdiConfigContext.AvailableEndpoints.ToDictionary(item => item.alias);

            HttpContext.Current.Cache[OgdiConfigDataServiceContext.EndpointsTableName] = availableEndpoints;

            return availableEndpoints;
        }

        public static string RootServiceNamespace
        {
            get
            {
                return RoleEnvironment.GetConfigurationSettingValue("RootServiceNamespace");
            }
        }

        public static string OgdiConfigTableStorageAccountName
        {
            get
            {
                return ParseFromConnectionString(ConnectionStringElement.AccountName);
            }
        }

        public static string OgdiConfigTableStorageAccountKey
        {
            get
            {
                return ParseFromConnectionString(ConnectionStringElement.AccountKey);
            }
        }

        private enum ConnectionStringElement { AccountName, AccountKey }
        private static string ParseFromConnectionString(ConnectionStringElement element)
        {
            var s = (element == ConnectionStringElement.AccountName) ? "AccountName=" : "AccountKey=";
            var cs = RoleEnvironment.GetConfigurationSettingValue("OgdiConfigConnectionString");
            cs.Replace(" ", string.Empty);
            var index = cs.IndexOf(s) + s.Length;
            var length = cs.IndexOf(';', index) - index;
            return (length > 0) ? cs.Substring(index, length) : cs.Substring(index);
        }

        public static string TableStorageBaseUrl
        {
            get
            {
                return RoleEnvironment.GetConfigurationSettingValue("TableStorageBaseUrl");
            }
        }

        public static string BlobStorageBaseUrl
        {
            get
            {
                return RoleEnvironment.GetConfigurationSettingValue("BlobStorageBaseUrl");
            }
        }

        public static Dictionary<string, AvailableEndpoint> EnabledStorageAccounts
        {
            get
            {
                var enabledStorageAccounts
                    = (Dictionary<string, AvailableEndpoint>)HttpContext.Current.Cache[OgdiConfigDataServiceContext.EndpointsTableName]
                    ?? RefreshAvailableEndpoints();

                return enabledStorageAccounts;
            }
        }

        public static AvailableEndpoint GetAvailableEndpointByAccountName(string accountName)
        {
            return EnabledStorageAccounts.Values.Where(r => r.storageaccountname == accountName).FirstOrDefault();

        }

        private const string _remainderRouteDataValue = "remainder";

        public static string RemainderRouteDataValue
        {
            get
            {
                return _remainderRouteDataValue;
            }
        }

        private const string _remainderRoutePatternSnippet = "{*" + _remainderRouteDataValue + "}";

        public static string RemainderRoutePatternSnippet
        {
            get
            {
                return _remainderRoutePatternSnippet;
            }
        }
    }
}
using Microsoft.ApplicationServer.Caching;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Ogdi.DataServices.Helper
{
    public class Cache
    {
        private bool IsEnabled = false;

        private DataCacheFactory _DataCacheFactory;
        private DataCache _DataCache;

        public Cache()
        {
            try
            {
                string useCache = RoleEnvironment.GetConfigurationSettingValue("UseCache");
                if (!string.IsNullOrEmpty(useCache) && useCache.Equals("1"))
                {
                    IsEnabled = true;
                    _DataCacheFactory = new DataCacheFactory();
                    _DataCache = _DataCacheFactory.GetDefaultCache();
                }
            }
            catch (RoleEnvironmentException)
            {}
        }

        public object Get(string key)
        {
            if (!IsEnabled)
            {
                return null;
            }

            return _DataCache.Get(key);
        }
        
        public void Put(string key, object val)
        {
            if (IsEnabled)
            {
                _DataCache.Put(key, val);
            }
        }
    }
}
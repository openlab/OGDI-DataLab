using Microsoft.ApplicationServer.Caching;
using Microsoft.WindowsAzure.ServiceRuntime;
using Ogdi.InteractiveSdk.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace Ogdi.InteractiveSdk.Mvc
{
    public class Cache
    {
        private static DataCacheFactory _DataCacheFactory = null;

        private static DataCache _DataCache = null;
        private static DataCache DataCache
        {
            get
            {
                if (_DataCacheFactory == null)
                {
                    _DataCacheFactory = new DataCacheFactory();
                }

                if (_DataCache == null)
                {
                    _DataCache = _DataCacheFactory.GetDefaultCache();
                }

                return _DataCache;
            }
        }

        private static bool IsEnabled()
        {
            try
            {
                string useCache = RoleEnvironment.GetConfigurationSettingValue("UseCache");
                if (!string.IsNullOrEmpty(useCache) && useCache.Equals("1"))
                {
                    return true;
                }
            }
            catch (RoleEnvironmentException)
            { }

            return false;
        }

        public static object Get(string key)
        {
            if (!IsEnabled())
            {
                return null;
            }

            return DataCache.Get(key);
        }

        public static void Put(string key, object val)
        {
            if (IsEnabled())
            {
                DataCache.Put(key, val);
            }
        }

		public static IEnumerable<EntitySet> EntitySets(string container)
		{
            IEnumerable<EntitySet> cachedData = Cache.Get("EntitySetCache_" + container) as List<EntitySet>;
            if (cachedData == null)
            {
                cachedData = GetEntitySets(container);
                Cache.Put("EntitySetCache_" + container, cachedData.ToList());
            }

            return cachedData;
		}

		private static EntitySet CreateEntitySet(XElement element, string containerAlias)
		{
			DateTime updateDate;
			DateTime.TryParse((element.Element("lastupdatedate") ?? new XElement("Dumb")).Value,out updateDate);
			DateTime releaseDate;
			if (!DateTime.TryParse((element.Element("releaseddate") ?? new XElement("Dumb")).Value, out releaseDate))
				releaseDate = updateDate;
			DateTime expiredDate;
			DateTime.TryParse((element.Element("expireddate") ?? new XElement("Dumb")).Value, out expiredDate);

			return new EntitySet(
				new Guid(element.Element("entityid").Value),
				element.Element("name") != null ? element.Element("name").Value : null,
				element.Element("entitykind") != null ? element.Element("entitykind").Value : null,
				element.Element("category") != null ? element.Element("category").Value : null,
				element.Element("description") != null ? element.Element("description").Value : null,
				element.Element("source") != null ? element.Element("source").Value : null,
				element.Element("metadataurl") != null ? element.Element("metadataurl").Value : null,
				element.Element("entityset") != null ? element.Element("entityset").Value : null,
				containerAlias
				)
                {
					LastUpdateDate = updateDate,
					ReleasedDate = releaseDate,
					ExpiredDate = expiredDate,
					UpdateFrequency =
						element.Element("updatefrequency") != null ? element.Element("updatefrequency").Value : null,
					Keywords = element.Element("keywords") != null ? element.Element("keywords").Value : null,
					Links = element.Element("links") != null ? element.Element("links").Value : null,
					PeriodCovered = element.Element("periodcovered") != null ? element.Element("periodcovered").Value : null,
					GeographicCoverage =
						element.Element("geographiccoverage") != null ? element.Element("geographiccoverage").Value : null,
					AdditionalInformation =
						element.Element("additionalinfo") != null ? element.Element("additionalinfo").Value : null,
					IsEmpty = element.Element("isempty") != null && element.Element("isempty").Value.Length == 4,
					CollectionMode = element.Element("collectionmode") !=null ?  element.Element("collectionmode").Value : null,
					CollectionInstruments = element.Element("collectioninstruments") != null ? element.Element("collectioninstruments").Value : null,
					DataDictionaryVariables = element.Element("datadictionary_variables") != null ? element.Element("datadictionary_variables").Value : null,
					TechnicalInfo = element.Element("technicalinfo") != null ? element.Element("technicalinfo").Value : null

				};
		}

		private static IEnumerable<EntitySet> GetEntitySets(string containerAlias)
		{
			var list = from element in Helper.ServiceObject.GetData(
					containerAlias,
                    Ogdi.InteractiveSdk.Mvc.Repository.Resources.EntitySetTableName, null,
                    Convert.ToInt32(Ogdi.InteractiveSdk.Mvc.Repository.Resources.EntitySetPageSize, CultureInfo.InvariantCulture),
					null, null
				).Elements("properties") 
                       select CreateEntitySet(element, containerAlias);

			return list.ToList();
		}
	}
}

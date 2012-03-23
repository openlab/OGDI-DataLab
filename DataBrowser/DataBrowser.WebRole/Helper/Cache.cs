using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Ogdi.InteractiveSdk.Mvc.Models;
using Ogdi.InteractiveSdk.Mvc.Repository;

namespace Ogdi.InteractiveSdk.Mvc
{
    /// <summary>
    /// Cache need to be implemented
    /// </summary>
    public class Cache
    {
        static public IEnumerable<EntitySet> EntitySets(String container)
        {
            if (HttpContext.Current == null)
                return GetEntitySets(container);

            IDictionary<string, IEnumerable<EntitySet>> entitySets;

            if (HttpContext.Current.Session["EntitySetCache"] == null)
            {
                HttpContext.Current.Session["EntitySetCache"] =
                entitySets = new Dictionary<string, IEnumerable<EntitySet>>();
            }
            else
            {
                entitySets = (IDictionary<string, IEnumerable<EntitySet>>)HttpContext.Current.Session["EntitySetCache"];
            }
            if (!entitySets.ContainsKey(container))
            {
                return entitySets[container] = GetEntitySets(container);
            }
            return entitySets[container];
        }

        private static EntitySet CreateEntitySet(XElement element, string containerAlias)
        {
            DateTime updateDate;
            DateTime.TryParse((element.Element("lastupdatedate") ?? new XElement("Dumb")).Value, out updateDate);
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
                    CollectionMode = element.Element("collectionmode") != null ? element.Element("collectionmode").Value : null,
                    CollectionInstruments = element.Element("collectioninstruments") != null ? element.Element("collectioninstruments").Value : null,
                    DataDictionaryVariables = element.Element("datadictionary_variables") != null ? element.Element("datadictionary_variables").Value : null,
                    TechnicalInfo = element.Element("technicalinfo") != null ? element.Element("technicalinfo").Value : null

                };
        }

        private static IEnumerable<EntitySet> GetEntitySets(string containerAlias)
        {
            var list = from element in Helper.ServiceObject.GetData(
                    containerAlias,
                    Resources.EntitySetTableName, null,
                    Convert.ToInt32(Resources.EntitySetPageSize, CultureInfo.InvariantCulture),
                    null, null
                ).Elements("properties")
                       select CreateEntitySet(element, containerAlias);

            return list.ToList();
        }
    }
}

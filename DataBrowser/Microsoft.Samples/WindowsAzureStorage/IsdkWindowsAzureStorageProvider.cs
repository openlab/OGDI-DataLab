using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Ogdi.InteractiveSdk.Mvc;

namespace Ogdi.InteractiveSdk.Mvc
{
    public class IsdkWindowsAzureStorageProvider : IsdkStorageProviderInterface
    {
        #region Properties

        public string ServiceUri {get; set;}
        public string PathDTD { get; set; }

        #endregion

        #region Constructors

        public IsdkWindowsAzureStorageProvider(string serviceUri,string pathDTD)
        {
            ServiceUri = serviceUri;
            PathDTD = pathDTD;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Helper method to create serviceUri through parameters passed.
        /// </summary>
        /// <param name="container">Alias of the container</param>
        /// <param name="tableName">EntitySet\Table name</param>
        /// <param name="filter">Filter query</param>
        /// <param name="pageSize">Number of rows to be fetched</param>
        /// <param name="nextPartitionKey">PartionKey to fetch next partion data</param>
        /// <param name="nextRowKey">RowKey to fetch row information</param>
        /// <returns>Returns constructed serviceUri</returns>
        private Uri LoadServiceUri(string container, string tableName,
                                            string filter, int pageSize,
                                            string nextPartitionKey, string nextRowKey)
        {
            var serviceUriBuilder = new StringBuilder();

            //serviceUriBuilder.Append(ConfigurationManager.AppSettings["serviceUri"]);
            //serviceUriBuilder.Append(RoleEnvironment.GetConfigurationSettingValue("serviceUri"));
            serviceUriBuilder.Append(ServiceUri);
            serviceUriBuilder.Append(container);
            serviceUriBuilder.Append("/");
            serviceUriBuilder.Append(tableName);
            if (pageSize > 0)
            {
                serviceUriBuilder.Append("?$top=");
                serviceUriBuilder.Append(pageSize);

            }
            if (!String.IsNullOrEmpty(filter))
            {
                if (pageSize > 0)
                    serviceUriBuilder.Append("&$filter=");
                else
                    serviceUriBuilder.Append("?$filter=");
                serviceUriBuilder.Append(filter);
            }

            if (!String.IsNullOrEmpty(nextPartitionKey) && !String.IsNullOrEmpty(nextRowKey))
            {
                serviceUriBuilder.Append("&NextPartitionKey=");
                serviceUriBuilder.Append(nextPartitionKey);
                serviceUriBuilder.Append("&NextRowKey=");
                serviceUriBuilder.Append(nextRowKey);
            }

            return new Uri(serviceUriBuilder.ToString());
        }

        /// <summary>
        /// Strip namespaces from a hierarchy of XML elements.
        /// </summary>
        /// <param name="root">XML element.</param>
        /// <returns>Same as <paramref>root</paramref> parameter.</returns>
        /// Note:-This Code has been referred from previous Asp.net project.
        private static XElement StripNamespaces(XElement root)
        {
            // found this code at http://social.msdn.microsoft.com/Forums/en-US/linqprojectgeneral/thread/bed57335-827a-4731-b6da-a7636ac29f21/
            foreach (XElement e in root.DescendantsAndSelf())
            {
                if (e.Name.Namespace != XNamespace.None)
                {
                    e.Name = XNamespace.None.GetName(e.Name.LocalName);
                }
                if (e.Attributes().Any(a => a.IsNamespaceDeclaration || a.Name.Namespace != XNamespace.None))
                {
                    e.ReplaceAttributes(e.Attributes().Select(a => a.IsNamespaceDeclaration ? null : a.Name.Namespace != XNamespace.None ? new XAttribute(XNamespace.None.GetName(a.Name.LocalName), a.Value) : a));
                }
            }

            return root;
        }

        /// <summary>
        /// Function to get all the Headers for a particular Entity Set
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private void AddMetadataToXElement(string containerAlias, XElement data)
        {
            var propertiesElements = data.Elements("properties");
            int nResults = propertiesElements.Count();

            if (nResults > 0)
            {
                var tableName = data.Attribute("tableName").Value;

                var filter = "entityset eq '" + tableName + "'";
                var enityMetaData = GetMetadata(containerAlias, AzureResources.metaDataTableName, filter);
                data.FirstNode.AddBeforeSelf(enityMetaData.FirstNode);
            }
        }

        #endregion

        #region Storage Overrided Methods

        /// <summary>
        /// Get an XML element containing data from the specified table + container combination, 
        /// filtering according to the filter criteria specified by the caller.
        /// </summary>
        /// <param name="container">Alias of the container, pass null for all records.</param>
        /// <param name="tableName">EntitySet\Table name, pass null for all records.</param>
        /// <param name="filter">Filter criteria, in Azure Table Services query syntax.</param>
        /// <param name="pageSize">Number of rows to be fetched</param>
        /// <param name="nextPartitionKey">PartionKey to fetch next partion data</param>
        /// <param name="nextRowKey">RowKey to fetch row information</param>
        /// <returns>An XML element containing the results of the query.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="WebException"></exception>
        /// <exception cref="Exception"></exception>
        public override XElement GetData(string container, string tableName,
                                        string filter, int pageSize,
                                        string nextPartitionKey, string nextRowKey)
        {
            XElement xmlData = null;

            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException(AzureResources.TableNameCannotBeNull);
            }

            if(pageSize < 0)
            {
                throw new ArgumentException(AzureResources.PagesizeNotZEROOrNegative);
            }

            Uri serviceUri = LoadServiceUri(container, tableName, filter, pageSize, nextPartitionKey, nextRowKey);

            // Store the partitionkey and rowkey as prevpartitionkey and prevrowkey before getting new set of data.
            string currentPartitionKeyStr = string.Empty;
            string currentRowKeyStr = string.Empty;
            if (!string.IsNullOrEmpty(nextPartitionKey))
            {
                currentPartitionKeyStr = nextPartitionKey;
            }

            if (!string.IsNullOrEmpty(nextRowKey))
            {
                currentRowKeyStr = nextRowKey;
            }

            string nextPartitionKeyStr = string.Empty;
            string nextRowKeyStr = string.Empty;

            var webRequest = HttpWebRequest.Create(serviceUri);
            var response = webRequest.GetResponse();
            var responseStream = response.GetResponseStream();

            if (response.Headers[AzureResources.continuation_nextPartionKey] != null)
                nextPartitionKeyStr = response.Headers[AzureResources.continuation_nextPartionKey];

            if (response.Headers[AzureResources.continuation_nextRowKey] != null)
                nextRowKeyStr = response.Headers[AzureResources.continuation_nextRowKey];

            var feed = XElement.Load(XmlReader.Create(responseStream));

            var propertiesElements = feed.Elements(XNamespace.Get(AzureResources.nsAtom) + "entry").Elements(XNamespace.Get(AzureResources.nsAtom) + "content").Elements(XNamespace.Get(AzureResources.nsMetadata) + "properties");
            // Remove PartitionKey, RowKey, and Timestamp because we don't want users to focus on these.
            // They are required by Azure Table storage, but will most likely go away
            // when we move to SDS.
            propertiesElements.Elements(XNamespace.Get(AzureResources.nsDataServices) + "PartitionKey").Remove();
            propertiesElements.Elements(XNamespace.Get(AzureResources.nsDataServices) + "RowKey").Remove();
            propertiesElements.Elements(XNamespace.Get(AzureResources.nsDataServices) + "Timestamp").Remove();

            // XmlDataSource doesn't support namespaces well
            // http://www.hanselman.com/blog/PermaLink,guid,8147b263-24fc-498d-83d1-546f4dde3fc3.aspx
            // Therefore, we will return XML that doesn't have any
            XElement root = new XElement("Root", propertiesElements);
            root.Add(new XAttribute("tableName", tableName));
            root.Add(new XAttribute("currentPartitionKey", currentPartitionKeyStr));
            root.Add(new XAttribute("currentRowKey", currentRowKeyStr));
            root.Add(new XAttribute("nextPartitionKey", nextPartitionKeyStr));
            root.Add(new XAttribute("nextRowKey", nextRowKeyStr));
            xmlData = StripNamespaces(root);
            return xmlData;
        }

        /// <summary>
        /// This method will return complete data for selected entitySet.
        /// Get an XML element containing data from the specified table + container combination,
        /// filtering according to the filter criteria specified by the caller.
        /// </summary>
        /// <param name="container">Alias of the container</param>
        /// <param name="tableName">EntitySet\Table name</param>
        /// <param name="filter">Filter criteria, in Azure Table Services query syntax.</param>
        /// <returns>An XML element containing the results of the query.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="WebException"></exception>
        /// <exception cref="Exception"></exception>
        public override XElement GetData(string container, string tableName, string filter)
        {
            XElement tempGetData = null;
            string tempNextPartitionKey = string.Empty;
            string tempNextRowKey = string.Empty;
                        
            if (string.IsNullOrEmpty(container))
            {
                throw new ArgumentNullException(AzureResources.ContainerCannotBeNull);
            }

            //handling internal paging.
            do
            {
                if (tempGetData == null)
                {
                    // 1000 is the max results Azure Table Storage allows per query
                    tempGetData = GetData(container, tableName, filter, 1000, null, null);
                    tempNextPartitionKey = tempGetData.Attribute("nextPartitionKey").Value;
                    tempNextRowKey = tempGetData.Attribute("nextRowKey").Value;
                }
                else
                {
                    // 1000 is the max results Azure Table Storage allows per query
                    XElement tp = GetData(container, tableName, filter, 1000, tempNextPartitionKey, tempNextRowKey);
                    tempGetData.Add(tp.Elements("properties"));

                    // Update the partitionkey values at the top.
                    tempGetData.SetAttributeValue("currentPartitionKey", tp.Attribute("currentPartitionKey").Value);
                    tempGetData.SetAttributeValue("currentRowKey", tp.Attribute("currentRowKey").Value);
                    tempGetData.SetAttributeValue("nextPartitionKey", tp.Attribute("nextPartitionKey").Value);
                    tempGetData.SetAttributeValue("nextRowKey", tp.Attribute("nextRowKey").Value);

                    tempNextPartitionKey = tp.Attribute("nextPartitionKey").Value;
                    tempNextRowKey = tp.Attribute("nextRowKey").Value;
                }
            }
            while (!string.IsNullOrEmpty(tempNextPartitionKey) && !string.IsNullOrEmpty(tempNextRowKey));

            return tempGetData;
        }

        /// <summary>
        /// DAISY Plugin -  This method will return complete data for selected entitySet.
        /// Get an XML element containing data from the specified table + container combination,
        /// filtering according
        /// to the filter criteria specified by the caller.
        /// </summary>
        /// <param name="container">Alias of the container</param>
        /// <param name="tableName">EntitySet\Table name</param>
        /// <param name="filter">Filter criteria, in Azure Table Services query syntax.</param>
        /// <returns>An XML element containing the results of the query.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="WebException"></exception>
        /// <exception cref="Exception"></exception>
        public override XDocument GetDataAsDaisy(string container, string tableName, string filter)
        {
            XElement tempGetData = null;
            string tempNextPartitionKey = string.Empty;
            string tempNextRowKey = string.Empty;
            XDocument daisyDataXml = null;

            if (string.IsNullOrEmpty(container))
            {
                throw new ArgumentNullException(AzureResources.ContainerCannotBeNull);
            }

            //handling internal paging.
            do
            {
                if (tempGetData == null)
                {
                    // 1000 is the max results Azure Table Storage allows per query
                    tempGetData = GetData(container, tableName, filter, 1000, null, null);
                    tempNextPartitionKey = tempGetData.Attribute("nextPartitionKey").Value;
                    tempNextRowKey = tempGetData.Attribute("nextRowKey").Value;
                }
                else
                {
                    // 1000 is the max results Azure Table Storage allows per query
                    XElement tp = GetData(container, tableName, filter, 1000, tempNextPartitionKey, tempNextRowKey);
                    tempGetData.Add(tp.Elements("properties"));

                    // Update the partitionkey values at the top.
                    tempGetData.SetAttributeValue("currentPartitionKey", tp.Attribute("currentPartitionKey").Value);
                    tempGetData.SetAttributeValue("currentRowKey", tp.Attribute("currentRowKey").Value);
                    tempGetData.SetAttributeValue("nextPartitionKey", tp.Attribute("nextPartitionKey").Value);
                    tempGetData.SetAttributeValue("nextRowKey", tp.Attribute("nextRowKey").Value);

                    tempNextPartitionKey = tp.Attribute("nextPartitionKey").Value;
                    tempNextRowKey = tp.Attribute("nextRowKey").Value;
                }
            }
            while (!string.IsNullOrEmpty(tempNextPartitionKey) && !string.IsNullOrEmpty(tempNextRowKey));

            //Function to get the header information of a particular entity set
            AddMetadataToXElement(container, tempGetData);

            String xmlDataFromAzure = tempGetData.ToString();

            TextReader inputXMLDataForTrans = new StringReader(xmlDataFromAzure);
            XmlReader xmlReader = XmlReader.Create(inputXMLDataForTrans);

            // Translate dat in daisy format.
            DTBookTranslation.DTBook objBook = new DTBookTranslation.DTBook();

            TextReader validatedDTBookXml;
            //Xml got from azure environment is given as input for DTBook translation method
            string mappedPath = (HttpContext.Current!=null) ? HttpContext.Current.Server.MapPath(PathDTD) 
                : Path.Combine(Environment.CurrentDirectory,PathDTD);
            validatedDTBookXml = objBook.TranslationOfAzureXml(xmlReader, mappedPath);
            daisyDataXml = XDocument.Load(validatedDTBookXml);
            return daisyDataXml;
        }

        /// <summary>
        /// Get colums of dataset
        /// </summary>
        /// <param name="container"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private List<string> GetColumns(string container, string tableName)
        {
            // Set the filter
            string tableNameFilter = "entityset eq '" + tableName + "'";

            XElement metaDataXML = GetMetadata(container,
                "EntityMetadata", tableNameFilter);

            // Remove the unnecessary columns
            var properties = metaDataXML.Elements("properties");
            properties.Elements("entityset").Remove();
            properties.Elements("entitykind").Remove();

            // Set the column list
            var propertyMetaData = metaDataXML.Elements("properties").First().Elements();
            List<string> columns = new List<string>();
            foreach (var property in propertyMetaData)
            {
                columns.Add(property.Name.ToString());
            }
            return columns;
        }

        /// <summary>
        /// This method will return complete data for selected entitySet as xml
        /// </summary>
        /// <param name="container">Alias of the container</param>
        /// <param name="tableName">EntitySet\Table name</param>
        /// <param name="filter">Filter criteria, in Azure Table Services query syntax.</param>
        /// <returns>Xml containing the results of the query.</returns>
        private XElement GetDataAsXElement(string container, string tableName, string filter)
        {
            string root = string.Format("<Root tableName=\"{0}\" currentPartitionKey=\"\" currentRowKey=\"\" nextPartitionKey=\"\" nextRowKey=\"\" />",tableName);

            XElement tempGetData = XElement.Parse(root);
            string tempNextPartitionKey = string.Empty;
            string tempNextRowKey = string.Empty;

            //handling internal paging.
            do
            {
                // TODO Refactor paging handling. This loop logic is confusing.

                // 1000 is the max results Azure Table Storage allows per query
                XElement tp = GetData(container, tableName, filter, 1000, tempNextPartitionKey, tempNextRowKey);
                tempGetData.Add(tp.Elements("properties"));

                // Update the partitionkey values at the top.
                tempGetData.SetAttributeValue("currentPartitionKey", tp.Attribute("currentPartitionKey").Value);
                tempGetData.SetAttributeValue("currentRowKey", tp.Attribute("currentRowKey").Value);
                tempGetData.SetAttributeValue("nextPartitionKey", tp.Attribute("nextPartitionKey").Value);
                tempGetData.SetAttributeValue("nextRowKey", tp.Attribute("nextRowKey").Value);

                tempNextPartitionKey = tp.Attribute("nextPartitionKey").Value;
                tempNextRowKey = tp.Attribute("nextRowKey").Value;

            }
            while (!string.IsNullOrEmpty(tempNextPartitionKey) && !string.IsNullOrEmpty(tempNextRowKey));            

            return tempGetData;
        }

        private string GetElement(XElement e, string key)
        {
            return e.Element(key) != null ? e.Element(key).Value : string.Empty;
        }

        /// <summary>
        /// This method will return complete data for selected entitySet as csv formatted string
        /// </summary>
        /// <param name="container">Alias of the container</param>
        /// <param name="tableName">EntitySet\Table name</param>
        /// <param name="filter">Filter criteria, in Azure Table Services query syntax.</param>
        /// <returns>An string in csv format containing the results of the query.</returns>
        public override string GetdDataAsCsv(string container, string tableName, string filter)
        {
            System.Diagnostics.Trace.WriteLine(string.Format("{0} - {1}", DateTime.Now.ToString(), "1"));            

            List<string> columns = GetColumns(container, tableName);
            XElement xml = GetDataAsXElement(container, tableName, filter);

            try
            {
                //DEBUG
                var currentUICulture = Thread.CurrentThread.CurrentUICulture;
                string listSeparator = currentUICulture.TextInfo.ListSeparator;
                StringBuilder sbAllEntities = new StringBuilder();

                foreach (string column in columns)
                {
                    sbAllEntities.Append(column);
                    sbAllEntities.Append(listSeparator);
                }
                sbAllEntities.Remove(sbAllEntities.Length - 1, 1);
                sbAllEntities.Append(Environment.NewLine);
                                
                foreach (var element in xml.Elements("properties"))
                {
                    foreach (string column in columns)
                    {
                        Decimal parseTest = 0;
                        string value = GetElement(element, column);
                        if (!string.IsNullOrEmpty(value))
                        {
                            if (Decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture ,out parseTest))
                            {
                                value = parseTest.ToString(CultureInfo.CreateSpecificCulture(currentUICulture.Name));
                            }
                            else
                            {
                                value = value.Replace(listSeparator, " ");
                                value = value.Replace('\n', ' ');
                            }                      
                            sbAllEntities.Append(value);
                        }
                        else
                        {
                            sbAllEntities.Append(string.Empty);
                        }
                        sbAllEntities.Append(listSeparator);
                    }
                    sbAllEntities.Remove(sbAllEntities.Length - 1, 1);
                    sbAllEntities.Append(Environment.NewLine);
                }
                System.Diagnostics.Trace.WriteLine(string.Format("{0} - {1}", DateTime.Now.ToString(), "2"));
                return sbAllEntities.ToString();
            }
            catch
            {
                return null;
            }

        }

        // GetMetaData internally calls GetData method. This method is introduced to keep ogdi code more readable.
        // Wherever we need to get table's metadata, instead of GetData, we now call GetMetaData.
        /// <summary>
        /// Gets details of header columns returning after quering container + tablename combination.
        /// </summary>
        /// <param name="container">Alias of the container</param>
        /// <param name="tableName">table name to fetch metadata from</param>
        /// <param name="filter">Filter query value in string format</param>
        /// <returns>Details of header columns returning after quering container + tablename combination.</returns>
        public override XElement GetMetadata(string container, string tableName, string filter)
        {
            // 1000 is the max results Azure Table Storage allows per query
            return GetData(container, tableName, filter, 1000, null, null);
        }

        #endregion
    }
}

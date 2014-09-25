using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Threading;
using System.Collections;
using System.Configuration;
using System.Web;
using Ogdi.Data.DataLoader;
using System.Text.RegularExpressions;
using System.Globalization;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure;
using System.Data.Services.Client;

namespace GenerateColumnMetadata
{
    class Program
    {
        public static TableHelper tableHelper;
        private static  CloudStorageAccount account = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["DataConnectionString"]);
        private static readonly TimeSpan s_retryTimeout = new TimeSpan(0, 3, 0);
        private static readonly TimeSpan s_retryWaitTime = new TimeSpan(0, 0, 15);
        private static readonly string filename = "GenerateColumnsMetadata_" + DateTime.Now.Date.ToString("MMddyy") + "_" + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString();
        public static TextWriter tw = new StreamWriter(filename + ".txt");

        static void Main(string[] args)
        {
            //Create TableColumnsMetadata if doesn't exist
            CreateTable("TableColumnsMetadata");

            QueryResult resulTableStorageXML = new QueryResult();
            tableHelper = new TableHelper(account.Credentials.AccountName, GetValueFromConnectionString("AccountKey"));
            DataPagerStorage dataPager = new DataPagerStorage(null, null, null);

            do
            {
                resulTableStorageXML = tableHelper.QueryEntities("EntityMetadata", string.Empty, dataPager);
                List<EntityMetadata> entityMetadata = EntityMetadata.LoadEntitiesFromTableStorageXML(resulTableStorageXML.Result);
                if (entityMetadata.Count.Equals(0))
                {
                    WriteInFile(filename, "No datasets in EntityMetadata");
                    return;
                }

                foreach (EntityMetadata entity in entityMetadata)
                {
                    //for each dataset in entitymetadata generate rdf
                    GenerateRdf(entity);
                }

                if (!string.IsNullOrEmpty(resulTableStorageXML.FilterNextPartitionKey))
                    dataPager = new DataPagerStorage(resulTableStorageXML.FilterNextPartitionKey, resulTableStorageXML.FilterNextRowKey, string.Empty);

            } while (!string.IsNullOrEmpty(resulTableStorageXML.FilterNextPartitionKey));
            tw.Close();
        }

        private static string GetValueFromConnectionString(string keyName)
        {
            keyName += "=";
            string[] values = ConfigurationManager.AppSettings["DataConnectionString"].Split(';');
            string value = values.Where(r => r.Contains(keyName)).FirstOrDefault();
            if (!string.IsNullOrEmpty(value))
            {
                value = value.Replace(keyName, "").Trim();
            }
            return value;
        }

        private static void GenerateRdf(EntityMetadata entity)
        {
            DataPagerStorage dataPager = new DataPagerStorage(null, null, null);
            QueryResult resulTableStorageXML = new QueryResult();
            Entity datasetUpdated = new Entity();
  
            try
            {
                do
                {
                    resulTableStorageXML = tableHelper.QueryEntities(entity.Entityset, string.Empty, dataPager);
                    List<Entity> datasetMetadata = TableMetadata.LoadGenericEntitiesFromTableStorageXML(resulTableStorageXML.Result);

                    if (datasetMetadata.Count.Equals(0))
                    {
                        WriteInFile(filename, "Dataset " + entity.Entityset + " is empty.");
                        return;
                    }

                    foreach (Entity datasetLine in datasetMetadata)
                    {
                        if (datasetLine["rdfsnippet"] == null || datasetLine["rdfsnippet"].ToString() == string.Empty)
                            datasetUpdated = GenerateRdfSnippet(datasetLine, entity);

                        if (datasetUpdated == null)
                            WriteInFile(filename, "Error generating RdfSnippet for dataset " + entity.Entityset + " in line with entityid " + datasetLine["entityid"].ToString());
                    }

                    if (!GenerateTableColumnMetadata(entity))
                    {
                        WriteInFile(filename, "Error generating TableColumnsMetadata for dataset " + entity.Entityset);
                        return;
                    }

                    if (!string.IsNullOrEmpty(resulTableStorageXML.FilterNextPartitionKey))
                        dataPager = new DataPagerStorage(resulTableStorageXML.FilterNextPartitionKey, resulTableStorageXML.FilterNextRowKey, string.Empty);

                } while (!string.IsNullOrEmpty(resulTableStorageXML.FilterNextPartitionKey));

                WriteInFile(filename, "Rdf data for dataset " + entity.Entityset + " generated successfully");
            }
            catch (Exception e)
            {
                WriteInFile(filename, "Error in dataset " + entity.Entityset + " " + e.Message);
            }
        }

        public static bool GenerateTableColumnMetadata(EntityMetadata entity)
        {
            DataPagerStorage dataPager = new DataPagerStorage(null, null, null);

            Filter filter = new Filter();
            filter.Conditions = new List<Condition>();
            Condition condition = new Condition();
            condition.Value = entity.Entityset.ToLower();
            condition.Attibute = "entityset";
            condition.OperatorFilter = "eq";

            filter.Conditions.Add(condition);
            string strFilter = filter.GetRESTFilter();

            //if exists in TableColumnsMetadata
            List<Entity> tableColumnsMetadata = TableMetadata.LoadGenericEntitiesFromTableStorageXML(tableHelper.QueryEntities("TableColumnsMetadata", strFilter, dataPager).Result);
            if (tableColumnsMetadata.Count.Equals(0))
            {
                TableColumnsMetadataItem columnsMeta;
                Entity columnMetaLine;
                string defaultDescription = ConfigurationManager.AppSettings["DefaultDescription"].ToString();
                string defaultDescriptionToAdd = string.Empty;

                try
                {
                    foreach (DictionaryEntry column in entity.Columns)
                    {
                        defaultDescriptionToAdd = string.Format(defaultDescription, column.Key.ToString(), entity.Entityset);
                        columnsMeta = new TableColumnsMetadataItem(column.Key.ToString(), string.Empty, defaultDescriptionToAdd, "ogdi=ogdiUrl");
                        columnsMeta.EntitySet = entity.Entityset.ToLower();
                        columnMetaLine = GetSchemaEntity(columnsMeta);
                        if (!StoreEntity("TableColumnsMetadata", columnMetaLine, entity.Entityset, column.Key.ToString()))
                        {
                            WriteInFile(filename, "Error generating columns metadata for dataset " + entity.Entityset + " in column " + column.Key.ToString());
                            return false;
                        }
                    }
                    return true;
                }
                catch (Exception e)
                {
                    WriteInFile(filename, "Error generating columns metadata for dataset " + entity.Entityset + " " + e.Message);
                    return false;
                }
            }
            else
            {
                //already has columns metadata
                return true;
            }
        }

        private static Entity GenerateRdfSnippet(Entity datasetLine, EntityMetadata entity)
        {
            XNamespace rdfNamespace = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";

            try
            {
                XElement rdfXml = new XElement(rdfNamespace + "RDF",
                new XAttribute(XNamespace.Xmlns + "rdf", rdfNamespace.ToString()));

                rdfXml.Add(new XAttribute(XNamespace.Xmlns + "ogdi", "ogdiUrl"));

                XNamespace customNS = "ogdiUrl";

                XElement rdfXmlDescriptionElement = new XElement(rdfNamespace + "Description");
                rdfXml.Add(rdfXmlDescriptionElement);

                foreach (Property item in datasetLine)
                {
                    if (item.Name != "EntityId" && item.Name != "PartitionKey" && item.Name != "RowKey" && item.Name != "Timestamp" && item.Name != "entityid" && item.Name != "rdfsnippet")
                    {
                        var header = item.Name;
                        var stringType = string.Empty;
                        if (entity.Columns[header] != null)
                            stringType = entity.Columns[header].ToString();

                        string stringValue = item.Value.ToString();

                        var datatype = GetRdfType(stringType);
                        var cleanHeader = CleanStringLower(header);

                        if (!string.IsNullOrEmpty(stringValue))
                        {
                            rdfXmlDescriptionElement.Add(new XElement(customNS + cleanHeader, stringValue.ToString(), new XAttribute(rdfNamespace + "datatype", datatype)));
                        }
                        else
                            rdfXmlDescriptionElement.Add(new XElement(customNS + cleanHeader));
                    }
                }

                try
                {
                    datasetLine[DataLoaderConstants.PropNameRdfSnippet.ToLower()] = rdfXml.ToString(SaveOptions.DisableFormatting);
                }
                catch (Exception)
                {
                    datasetLine.AddProperty(DataLoaderConstants.PropNameRdfSnippet.ToLower(), rdfXml.ToString(SaveOptions.DisableFormatting));
                }

                UpdateEntity(entity.Entityset, datasetLine, datasetLine["PartitionKey"].ToString(), datasetLine["RowKey"].ToString());

                return datasetLine;

            }
            catch (Exception)
            {
                return null;
            }
        }

        private static bool CreateTable(string tableName)
        {
            CloudTableClient tc = account.CreateCloudTableClient();

            DateTime retryUntil = DateTime.Now + s_retryTimeout;

            while (!tc.DoesTableExist(tableName))
            {
                try
                {
                    tc.CreateTable(tableName);
                    WriteInFile(filename, tableName + " was created");
                    return true;
                }
                catch (StorageClientException e)
                {
                    if (e.ErrorCode == StorageErrorCode.ResourceAlreadyExists &&
                        e.StatusCode == HttpStatusCode.Conflict &&
                        DateTime.Now < retryUntil)
                    {
                        Console.WriteLine("Retrying {0}...", tableName);
                        Thread.Sleep(s_retryWaitTime);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            WriteInFile(filename, tableName + " already created");
            return false;
        }

        private static bool UpdateEntity(string entitySetName, Entity entity, string parKey, string rowKey)
        {
            TableContext context = new TableContext(account.TableEndpoint.ToString(), account.Credentials)
            {
                RetryPolicy = RetryPolicies.RetryExponential(5, new TimeSpan(0, 0, 1))
            };

            TableEntity tableEntity = null;

            if (tableEntity == null)
                tableEntity = new TableEntity(entity, parKey, rowKey);

            context.AttachTo(entitySetName, tableEntity, "*");
            context.UpdateObject(tableEntity);

            try
            {
                context.SaveChanges();
                return true;
            }
            catch (StorageClientException e)
            {
                WriteInFile(filename, "ERROR " + entitySetName + " entity was not updated " + e.Message);
                return false;
            }
            catch (Exception e)
            {
                WriteInFile(filename, "ERROR " + entitySetName + " entity was not updated " + e.Message);
                return false;
            }
        }

        private static bool StoreEntity(string entitySetName, Entity entity, string parKey, string rowKey)
        {
            TableContext context = new TableContext(account.TableEndpoint.ToString(), account.Credentials)
            {
                RetryPolicy = RetryPolicies.RetryExponential(5, new TimeSpan(0, 0, 1))
            };

            TableEntity tableEntity = new TableEntity(entity, parKey, rowKey);

            context.AddObject(entitySetName, tableEntity);

            try
            {
                context.SaveChanges();
                return true;
            }
            catch (StorageClientException e)
            {
                WriteInFile(filename, "ERROR " + entitySetName + " entity was not created " + e.Message);
                return false;
            }
            catch (Exception e)
            {
                WriteInFile(filename, "ERROR " + entitySetName + " entity was not created " + e.Message);
                return false;
            }
        }

        private static string GetRdfType(string type)
        {
            type = type.ToLower();
            switch (type)
            {
                case ("string"):
                case ("system.string"):
                    return "xs:string";
                case ("int32"):
                case ("system.int32"):
                    return "xs:int";
                case ("int64"):
                case ("system.int64"):
                    return "xs:long";
                case ("double"):
                    return "xs:double";
                case ("bool"):
                    return "xs:boolean";
                case ("bool-0or1"):
                    return "xs:boolean";
                case ("datetime"):
                    return "xs:dateTime";
                case ("datetime-yyyymmdd"):
                    return "xs:dateTime";
                default:
                    throw new ArgumentException(DataLoaderConstants.MsgUnsupportedType, type);
            }
        }

        private static string CleanStringLower(string messy)
        {
            string final = CleanStringDiacritics(messy);
            final = CleanStringSpecialChars(final);
            return final.ToLower();
        }

        private static string CleanStringDiacritics(string messy)
        {
            String normalizedString = messy.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < normalizedString.Length; i++)
            {
                Char c = normalizedString[i];
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(c);
            }

            string final = stringBuilder.ToString();

            final = final.Replace("Ø", "O");
            final = final.Replace("ø", "o");
            final = final.Replace("ð", "o");
            final = final.Replace("Ð", "D");

            return final;
        }

        private static string CleanStringSpecialChars(string messy)
        {
            return Regex.Replace(messy, "[^0-9a-zA-Z]+", "");
        }

        private static void WriteInFile(string filename, string text)
        {
            tw.WriteLine(DateTime.Now.ToShortTimeString() + " - " + text);
        }

        private static Entity GetSchemaEntity(TableColumnsMetadataItem mapper)
        {
            var entity = new Entity();
            entity.AddProperty("EntitySet", mapper.EntitySet);
            entity.AddProperty("Column", mapper.Column);
            entity.AddProperty("ColumnSemantic", mapper.ColumnSemantic);
            entity.AddProperty("ColumnNamespace", mapper.ColumnNamespace);
            entity.AddProperty("ColumnDescription", mapper.ColumnDescription);

            return entity;
        }
    }

    #region aux classes
    public class TableContext : TableServiceContext
    {
        public TableContext(string baseAddress, StorageCredentials credentials)
            : base(baseAddress, credentials)
        {
            WritingEntity += OnWritingEntity;
            ReadingEntity += TableContext_ReadingEntity;
        }

        private void TableContext_ReadingEntity(object sender, ReadingWritingEntityEventArgs e)
        {
            XName xnProps = XName.Get("properties", e.Data.GetNamespaceOfPrefix("m").NamespaceName);
            XElement xeProps = e.Data.Descendants().Where(xe => xe.Name == xnProps).First();
            var props = new List<Property>();
            Guid ID = Guid.Empty;
            foreach (XElement prop in xeProps.Nodes())
            {
                if (prop.Name.LocalName == "PartitionKey")
                    ((TableEntity)e.Entity).PartitionKey = prop.Value;
                else if (prop.Name.LocalName == "RowKey")
                    ((TableEntity)e.Entity).RowKey = prop.Value;
                else if (prop.Name.LocalName.ToLower() == "EntityId".ToLower())
                    ID = new Guid(prop.Value);
                else if (prop.Name.LocalName == "Timestamp")
                {
                    ((TableEntity)e.Entity).Timestamp = DateTime.Parse(prop.Value);
                }
                else
                    props.Add(new Property(prop.Name.LocalName, prop.Value));
            }
            var entity = new Entity(ID);
            foreach (Property prop in props)
                entity.AddProperty(prop.Name, prop.Value);
            ((TableEntity)e.Entity).SetEntity(entity);
        }

        private void OnWritingEntity(object sender, ReadingWritingEntityEventArgs e)
        {
            XName xnProps = XName.Get("properties", e.Data.GetNamespaceOfPrefix("m").NamespaceName);
            XElement xeProps = e.Data.Descendants().Where(xe => xe.Name == xnProps).First();
            foreach (TableEntity.NameTypeValueTuple tuple in ((TableEntity)e.Entity).GetProperties())
            {
                if (tuple.Value is DateTime)
                    tuple.Value = ConvertToUtc((DateTime)tuple.Value);
                if (tuple.Type != null)
                {
                    xeProps.Add(new XElement(e.Data.GetNamespaceOfPrefix("d") + tuple.Name,
                                                new XAttribute(e.Data.GetNamespaceOfPrefix("m") + "type", tuple.Type),
                                                tuple.Value));
                }
                else
                {
                    xeProps.Add(new XElement(e.Data.GetNamespaceOfPrefix("d") + tuple.Name, tuple.Value));
                }
            }
        }

        private DateTime ConvertToUtc(DateTime date)
        {
            DateTime tstTime = TimeZoneInfo.ConvertTime(date, TimeZoneInfo.Local, TimeZoneInfo.Utc);

            return TimeZoneInfo.ConvertTimeToUtc(tstTime, TimeZoneInfo.Utc);
        }
    }

    public class TableMetadata
    {

        public TableMetadata()
        {
        }

        public static List<Entity> LoadGenericEntitiesFromTableStorageXML(string xml)
        {

            List<Entity> list = new List<Entity>();

            Entity entity = null;

            XElement xmlData = XElement.Parse(xml);

            XNamespace ns = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";
            var props = from prop in xmlData.Descendants(ns + "properties").Descendants()
                        select prop;

            XElement parent = null;

            foreach (XElement property in props)
            {
                if (parent == null)
                {
                    entity = new Entity();
                    parent = property.Parent;
                }

                if (parent != property.Parent)
                {
                    list.Add(entity);
                    entity = new Entity();
                    parent = property.Parent;

                }

                entity.AddProperty(property.Name.LocalName, property.Value);

            }
            if (entity != null)
                list.Add(entity);

            return list;
        }
    }

    public class RESTHelper
    {
        protected bool IsTableStorage { get; set; }

        private string endpoint;
        public string Endpoint
        {
            get
            {
                return endpoint;
            }
            internal set
            {
                endpoint = value;
            }
        }

        private string storageAccount;
        public string StorageAccount
        {
            get
            {
                return storageAccount;
            }
            internal set
            {
                storageAccount = value;
            }
        }

        private string storageKey;
        public string StorageKey
        {
            get
            {
                return storageKey;
            }
            internal set
            {
                storageKey = value;
            }
        }


        public RESTHelper(string endpoint, string storageAccount, string storageKey)
        {
            this.Endpoint = endpoint;
            this.StorageAccount = storageAccount;
            this.StorageKey = storageKey;
        }


        #region REST HTTP Request Helper Methods

        // Construct and issue a REST request and return the response.

        public HttpWebRequest CreateRESTRequest(string method, string resource, string requestBody = null, SortedList<string, string> headers = null,
            string ifMatch = "", string md5 = "")
        {
            byte[] byteArray = null;
            DateTime now = DateTime.UtcNow;
            string uri = Endpoint + resource;

            HttpWebRequest request = HttpWebRequest.Create(uri) as HttpWebRequest;
            request.Method = method;
            request.ContentLength = 0;
            request.Headers.Add("x-ms-date", now.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
            request.Headers.Add("x-ms-version", "2009-09-19");

            if (IsTableStorage)
            {
                request.ContentType = "application/atom+xml";

                request.Headers.Add("DataServiceVersion", "1.0;NetFx");
                request.Headers.Add("MaxDataServiceVersion", "1.0;NetFx");
            }

            if (headers != null)
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            if (!String.IsNullOrEmpty(requestBody))
            {
                request.Headers.Add("Accept-Charset", "UTF-8");

                byteArray = Encoding.UTF8.GetBytes(requestBody);
                request.ContentLength = byteArray.Length;
            }

            request.Headers.Add("Authorization", AuthorizationHeader(method, now, request, ifMatch, md5));

            if (!String.IsNullOrEmpty(requestBody))
            {
                request.GetRequestStream().Write(byteArray, 0, byteArray.Length);
            }

            return request;
        }


        // Generate an authorization header.

        public string AuthorizationHeader(string method, DateTime now, HttpWebRequest request, string ifMatch = "", string md5 = "")
        {
            string MessageSignature;

            if (IsTableStorage)
            {
                MessageSignature = String.Format("{0}\n\n{1}\n{2}\n{3}",
                    method,
                    "application/atom+xml",
                    now.ToString("R", System.Globalization.CultureInfo.InvariantCulture),
                    GetCanonicalizedResource(request.RequestUri, StorageAccount)
                    );
            }
            else
            {
                MessageSignature = String.Format("{0}\n\n\n{1}\n{5}\n\n\n\n{2}\n\n\n\n{3}{4}",
                    method,
                    (method == "GET" || method == "HEAD") ? String.Empty : request.ContentLength.ToString(),
                    ifMatch,
                    GetCanonicalizedHeaders(request),
                    GetCanonicalizedResource(request.RequestUri, StorageAccount),
                    md5
                    );
            }
            byte[] SignatureBytes = System.Text.Encoding.UTF8.GetBytes(MessageSignature);
            System.Security.Cryptography.HMACSHA256 SHA256 = new System.Security.Cryptography.HMACSHA256(Convert.FromBase64String(StorageKey));
            String AuthorizationHeader = "SharedKey " + StorageAccount + ":" + Convert.ToBase64String(SHA256.ComputeHash(SignatureBytes));
            return AuthorizationHeader;
        }

        // Get canonicalized headers.

        public string GetCanonicalizedHeaders(HttpWebRequest request)
        {
            ArrayList headerNameList = new ArrayList();
            StringBuilder sb = new StringBuilder();
            foreach (string headerName in request.Headers.Keys)
            {
                if (headerName.ToLowerInvariant().StartsWith("x-ms-", StringComparison.Ordinal))
                {
                    headerNameList.Add(headerName.ToLowerInvariant());
                }
            }
            headerNameList.Sort();
            foreach (string headerName in headerNameList)
            {
                StringBuilder builder = new StringBuilder(headerName);
                string separator = ":";
                foreach (string headerValue in GetHeaderValues(request.Headers, headerName))
                {
                    string trimmedValue = headerValue.Replace("\r\n", String.Empty);
                    builder.Append(separator);
                    builder.Append(trimmedValue);
                    separator = ",";
                }
                sb.Append(builder.ToString());
                sb.Append("\n");
            }
            return sb.ToString();
        }

        // Get header values.

        public ArrayList GetHeaderValues(NameValueCollection headers, string headerName)
        {
            ArrayList list = new ArrayList();
            string[] values = headers.GetValues(headerName);
            if (values != null)
            {
                foreach (string str in values)
                {
                    list.Add(str.TrimStart(null));
                }
            }
            return list;
        }

        // Get canonicalized resource.

        public string GetCanonicalizedResource(Uri address, string accountName)
        {
            StringBuilder str = new StringBuilder();
            StringBuilder builder = new StringBuilder("/");
            builder.Append(accountName);
            builder.Append(address.AbsolutePath);
            str.Append(builder.ToString());
            NameValueCollection values2 = new NameValueCollection();
            if (!IsTableStorage)
            {
                NameValueCollection values = HttpUtility.ParseQueryString(address.Query);
                foreach (string str2 in values.Keys)
                {
                    ArrayList list = new ArrayList(values.GetValues(str2));
                    list.Sort();
                    StringBuilder builder2 = new StringBuilder();
                    foreach (object obj2 in list)
                    {
                        if (builder2.Length > 0)
                        {
                            builder2.Append(",");
                        }
                        builder2.Append(obj2.ToString());
                    }
                    values2.Add((str2 == null) ? str2 : str2.ToLowerInvariant(), builder2.ToString());
                }
            }
            ArrayList list2 = new ArrayList(values2.AllKeys);
            list2.Sort();
            foreach (string str3 in list2)
            {
                StringBuilder builder3 = new StringBuilder(string.Empty);
                builder3.Append(str3);
                builder3.Append(":");
                builder3.Append(values2[str3]);
                str.Append("\n");
                str.Append(builder3.ToString());
            }
            return str.ToString();
        }

        #endregion

        #region Retry Delegate

        public delegate T RetryDelegate<T>();
        public delegate void RetryDelegate();

        const int retryCount = 3;
        const int retryIntervalMS = 200;

        // Retry delegate with default retry settings.

        public static T Retry<T>(RetryDelegate<T> del)
        {
            return Retry<T>(del, retryCount, retryIntervalMS);
        }

        // Retry delegate.

        public static T Retry<T>(RetryDelegate<T> del, int numberOfRetries, int msPause)
        {
            int counter = 0;
        RetryLabel:

            try
            {
                counter++;
                return del.Invoke();
            }
            catch (Exception ex)
            {
                if (counter > numberOfRetries)
                {
                    throw ex;
                }
                else
                {
                    if (msPause > 0)
                    {
                        Thread.Sleep(msPause);
                    }
                    goto RetryLabel;
                }
            }
        }


        // Retry delegate with default retry settings.

        public static bool Retry(RetryDelegate del)
        {
            return Retry(del, retryCount, retryIntervalMS);
        }


        public static bool Retry(RetryDelegate del, int numberOfRetries, int msPause)
        {
            int counter = 0;

        RetryLabel:
            try
            {
                counter++;
                del.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                if (counter > numberOfRetries)
                {
                    throw ex;
                }
                else
                {
                    if (msPause > 0)
                    {
                        Thread.Sleep(msPause);
                    }
                    goto RetryLabel;
                }
            }
        }

        #endregion
    }

    public class EntityMetadata
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string Entityid { get; set; }
        public string Entitykind { get; set; }
        public string Entityset { get; set; }
        public string Timestamp { get; set; }

        public OrderedDictionary Columns { get; set; }

        public EntityMetadata()
        {
            Columns = new OrderedDictionary();
        }

        public static List<EntityMetadata> LoadEntitiesFromTableStorageXML(string xml)
        {
            List<EntityMetadata> list = new List<EntityMetadata>();

            EntityMetadata eMetadata = null;

            System.Collections.Hashtable hashTableHeaders = new System.Collections.Hashtable();

            XElement xmlData = XElement.Parse(xml);
            XNamespace ns = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

            var props = from prop in xmlData.Descendants(ns + "properties").Descendants()
                        select prop;

            XElement parent = null;

            foreach (XElement property in props)
            {
                if (parent == null)
                {
                    parent = property.Parent;
                    eMetadata = new EntityMetadata();
                }

                if (parent != property.Parent)
                {
                    list.Add(eMetadata);
                    eMetadata = new EntityMetadata();
                    parent = property.Parent;

                }

                switch (property.Name.LocalName)
                {
                    case "entityid": { eMetadata.Entityid = property.Value; break; }
                    case "entitykind": { eMetadata.Entitykind = property.Value; break; }
                    case "entityset": { eMetadata.Entityset = property.Value; break; }
                    case "PartitionKey": { eMetadata.PartitionKey = property.Value; break; }
                    case "RowKey": { eMetadata.RowKey = property.Value; break; }
                    case "Timestamp": { eMetadata.Timestamp = property.Value; break; }
                    default:
                        {
                            eMetadata.Columns.Add(property.Name.LocalName, property.Value);
                            break;
                        }
                }

            }
            if (eMetadata != null)
                list.Add(eMetadata);

            return list;
        }
    }

    public class TableHelper : RESTHelper
    {
        // Constructor.

        public TableHelper(string storageAccount, string storageKey)
            : base("http://" + storageAccount + ".table.core.windows.net/", storageAccount, storageKey)
        {
            IsTableStorage = true;
        }

        public QueryResult QueryEntities(string tableName, string filter, DataPagerStorage dataPager)
        {
            QueryResult queryResult = new QueryResult();

            if (!string.IsNullOrWhiteSpace(filter))
                queryResult.Filter = "$filter=" + Uri.EscapeDataString(filter);

            if (!string.IsNullOrWhiteSpace(dataPager.NumberItems))
                queryResult.FilterTop = "&$top=" + Uri.EscapeDataString(dataPager.NumberItems);

            if (!string.IsNullOrWhiteSpace(dataPager.NextPartitionKey))
                queryResult.FilterNextPartitionKey = "&NextPartitionKey=" + Uri.EscapeDataString(dataPager.NextPartitionKey);

            if (!string.IsNullOrWhiteSpace(dataPager.NextRowKey))
                queryResult.FilterNextRowKey = "&NextRowKey=" + Uri.EscapeDataString(dataPager.NextRowKey);

            return Retry<QueryResult>(delegate()
            {
                HttpWebRequest request;
                HttpWebResponse response;

                string entityXml = null;
                try
                {
                    string resource = String.Format(tableName + "?" + queryResult.Filter + queryResult.FilterTop + queryResult.FilterNextPartitionKey + queryResult.FilterNextRowKey);
                    request = CreateRESTRequest("GET", resource, null, null);
                    request.Accept = "application/atom+xml,application/xml";

                    response = request.GetResponse() as HttpWebResponse;

                    if ((int)response.StatusCode == 200)
                    {
                        using (StreamReader reader = new StreamReader(response.GetResponseStream(),true))
                        {
                            string result = reader.ReadToEnd();

                            XNamespace ns = "http://www.w3.org/2005/Atom";
                            XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";

                            XElement entry = XElement.Parse(result);

                            entityXml = entry.ToString();
                        }

                        dataPager.AddPage(response.GetResponseHeader("x-ms-continuation-NextPartitionKey"), response.GetResponseHeader("x-ms-continuation-NextRowKey"));

                    }

                    response.Close();
                    queryResult.Result = entityXml;
                    queryResult.FilterNextPartitionKey = dataPager.NextPartitionKey;
                    queryResult.FilterNextRowKey = dataPager.NextRowKey;
                    return queryResult;
                }
                catch (WebException ex)
                {
                    if (ex.Status == WebExceptionStatus.ProtocolError &&
                        ex.Response != null &&
                        (int)(ex.Response as HttpWebResponse).StatusCode == 409)
                        return null;

                    throw;
                }
            });
        }
    }

    public class QueryResult
    {
        public string Result { get; set; }
        public string Url { get; set; }
        public string Filter { get; set; }
        public string FilterTop { get; set; }
        public string FilterNextPartitionKey { get; set; }
        public string FilterNextRowKey { get; set; }

    }

    public class DataPagerStorage
    {
        public System.Collections.Generic.Stack<Paging> stack = new System.Collections.Generic.Stack<Paging>();

        public string NextPartitionKey { get; set; }
        public string NextRowKey { get; set; }

        public string FirstPartitionKey { get; set; }
        public string FirstRowKey { get; set; }

        public bool hasPrevious = false;
        public bool hasNext = false;

        public string NumberItems { get; set; }

        public DataPagerStorage(string firstPartitionKey, string firstRowKey, string numberItems)
        {
            this.FirstPartitionKey = firstPartitionKey;
            this.FirstRowKey = firstRowKey;
            this.NextPartitionKey = firstPartitionKey;
            this.NextRowKey = firstRowKey;
            this.NumberItems = numberItems;

            NextPage();
        }

        public void NextPage()
        {
            if (stack.Count == 0)
                hasPrevious = false;
            else hasPrevious = true;

            stack.Push(new Paging(NextPartitionKey, NextRowKey));


        }
        public void PreviousPage()
        {
            Paging paging = stack.Pop();
            NextPartitionKey = paging.PartitionKey;
            NextRowKey = paging.RowKey;

            if (stack.Count == 0)
                hasPrevious = false;
            else hasPrevious = true;
        }
        public void FirstPage()
        {
            NextPartitionKey = FirstPartitionKey;
            NextRowKey = FirstRowKey;

            stack.Clear();
            this.NextPage();
        }
        public void AddPage(string nextPartitionKey, string nextRowKey)
        {
            NextPartitionKey = nextPartitionKey;
            NextRowKey = nextRowKey;

            if (string.IsNullOrWhiteSpace(NextRowKey))
                hasNext = false;
            else hasNext = true;
        }
    }

    public class Paging
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }

        public Paging(string partitionKey, string rowKey)
        {
            this.PartitionKey = partitionKey;
            this.RowKey = rowKey;
        }
    }

    public class Filter
    {

        public List<Condition> Conditions { get; set; }

        public string GetRESTFilter()
        {
            string filter = "";
            string valueType = "";

            try
            {
                if (Conditions != null && Conditions.Count > 0)
                {
                    foreach (Condition conditionItem in Conditions)
                    {
                        valueType = conditionItem.Attibute + " " + conditionItem.OperatorFilter + " '" + conditionItem.Value + "'";

                        filter = SetOpFilter(filter, valueType, conditionItem.OperatorConditions);
                    }
                }

                return filter;

            }
            catch (Exception)
            {

            }
            return string.Empty;
        }

        private static string SetOpFilter(string filter, string condition, string opFilter)
        {
            if (string.IsNullOrWhiteSpace(condition))
                return filter;

            if (string.IsNullOrWhiteSpace(filter))
                filter = condition;
            else
                filter += " " + opFilter + " " + condition;

            return filter;
        }

    }

    public class Condition
    {
        public string Attibute { get; set; }
        public string OperatorFilter { get; set; }
        public string Value { get; set; }
        public string OperatorConditions { get; set; }
        public string Type { get; set; }
    }   
    #endregion
}

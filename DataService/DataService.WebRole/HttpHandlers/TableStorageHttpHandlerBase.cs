using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System.Collections.Specialized;
using System.Configuration;
using Ogdi.DataServices.Properties;

namespace Ogdi.DataServices
{
    public class TableStorageHttpHandlerBase
    {
        // Setup namespaces
        protected static XNamespace _nsAtom = XNamespace.Get("http://www.w3.org/2005/Atom");
        protected static XNamespace _nsm = XNamespace.Get("http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
        protected static XNamespace _nsd = XNamespace.Get("http://schemas.microsoft.com/ado/2007/08/dataservices");

        // Setup namespace specific names
        protected static XName _entryXName = _nsAtom + "entry";
        protected static XName _contentXName = _nsAtom + "content";
        protected static XName _propertiesXName = _nsm + "properties";
        protected static XName _idXName = _nsAtom + "id";
        protected static XName _categoryXName = _nsAtom + "category";
        protected static XName _kmlSnippetXName = _nsd + "kmlsnippet";
        protected static XName _rdfSnippetXName = _nsd + "rdfsnippet";
        protected static XName _storageAccountNameXName = _nsd + "storageaccountname";
        protected static XName _storageAccountKeyXName = _nsd + "storageaccountkey";

        protected static readonly string _termNamespaceString = AppSettings.RootServiceNamespace + ".{0}.{1}";

        protected WebRequest CreateTableStorageSignedRequest(HttpContext context,
                                                             CloudStorageAccount account,
                                                             string requestUrl,
                                                             bool isAvailableEndpointRequest)
        {
            return CreateTableStorageSignedRequest(context, account, requestUrl, isAvailableEndpointRequest, false);
        }

        protected WebRequest CreateTableStorageSignedRequest(HttpContext context,
                                                             CloudStorageAccount account,
                                                             string requestUrl,
                                                             bool isAvailableEndpointsRequest,
                                                             bool ignoreQueryOptions)
        {
            if (account == null)
                throw new ArgumentNullException("Storage Account is not properly configured.");

            var azureTableRequestUrlBuilder = new StringBuilder(string.Format(requestUrl, account.Credentials.AccountName));

            if (isAvailableEndpointsRequest)
            {
                azureTableRequestUrlBuilder.Append("AvailableEndpoints");
            }

            //if (!ignoreQueryOptions)
            //{
            //    var queryString = context.Request.QueryString.ToString();

            //    if (!string.IsNullOrEmpty(queryString))
            //    {
            //        azureTableRequestUrlBuilder.Append("?");
            //        azureTableRequestUrlBuilder.Append(queryString);
            //    }
            //}

            if (!requestUrl.EndsWith("TableMetadata"))
                this.ConvertSkipToken(context, ref azureTableRequestUrlBuilder, ignoreQueryOptions);

            var request = HttpWebRequest.Create(azureTableRequestUrlBuilder.ToString());

            request.Headers["x-ms-version"] = "2011-08-18";
            request.Headers["DataServiceVersion"] = "2.0";
            request.Headers["MaxDataServiceVersion"] = "2.0";

            account.Credentials.SignRequestLite((HttpWebRequest)request);   
            return request;
        }

        protected void ConvertSkipToken(HttpContext context, ref StringBuilder azureTableRequestUrlBuilder, bool ignoreQueryOptions) 
        {
            var skiptoken = context.Request.QueryString["$skiptoken"];
            string nextPartitionKey = null;
            string nextRowKey = null;
            if (skiptoken != null)
            {
                skiptoken = skiptoken.Substring(1, skiptoken.Length - 2);
                int indexOfPK = skiptoken.IndexOf("&NextPartitionKey=");
                int indexOfRK = skiptoken.IndexOf("&NextRowKey=");

                if (indexOfPK < indexOfRK)
                {
                    nextPartitionKey = skiptoken.Substring(indexOfPK + "&NextPartitionKey=".Length, indexOfRK - (indexOfPK + "&NextPartitionKey=".Length));
                    nextRowKey = skiptoken.Substring(indexOfRK + "&NextRowKey=".Length);
                }
                else if (indexOfRK < indexOfPK)
                {
                    nextRowKey = skiptoken.Substring(indexOfRK + "&NextRowKey=".Length, indexOfPK - (indexOfRK + "&NextRowKey=".Length));
                    nextPartitionKey = skiptoken.Substring(indexOfPK + "&NextPartitionKey=".Length);
                }
            }

            NameValueCollection newQueryString = new NameValueCollection();
            if (!ignoreQueryOptions)
            {
                foreach (var key in context.Request.QueryString.AllKeys)
                {
                    if (key != "$skiptoken")
                    {
                        newQueryString.Add(key, context.Request.QueryString[key]);
                    }
                }
            }

            if (nextPartitionKey != null && nextRowKey != null)
            {
                newQueryString.Add("NextPartitionKey", nextPartitionKey);
                newQueryString.Add("NextRowKey", nextRowKey);
            }

            if (newQueryString.Count > 0)
            {
                if(!azureTableRequestUrlBuilder.ToString().Contains("?"))
                    azureTableRequestUrlBuilder.Append("?");

                foreach (var key in newQueryString.AllKeys)
                {
                    azureTableRequestUrlBuilder.Append("&");
                    azureTableRequestUrlBuilder.Append(key);
                    azureTableRequestUrlBuilder.Append("=");
                    azureTableRequestUrlBuilder.Append(newQueryString[key]);
                }
            }
        }

        protected WebRequest CreateBlobStorageSignedRequest(string blobId, string ogdiAlias, string entitySet)
        {
            if (string.IsNullOrWhiteSpace(ogdiAlias))
                return WebRequest.Create(string.Format(AppSettings.BlobStorageBaseUrl,
                    AppSettings.OgdiConfigTableStorageAccountName));

            var blobRequestUrlBuilder = new StringBuilder();
            blobRequestUrlBuilder.Append(string.Format(AppSettings.BlobStorageBaseUrl,
                                                       AppSettings.EnabledStorageAccounts[ogdiAlias].storageaccountname));
            blobRequestUrlBuilder.Append(entitySet.ToLower());
            blobRequestUrlBuilder.Append("/");
            blobRequestUrlBuilder.Append(blobId);

            if (!AppSettings.EnabledStorageAccounts.ContainsKey(ogdiAlias))
                throw new ConfigurationErrorsException(Resources.AliasNotConfiguredExceptionMessage);

            var cloudStorageAccount = AppSettings.ParseStorageAccount(AppSettings.EnabledStorageAccounts[ogdiAlias].storageaccountname,
                                                                      AppSettings.EnabledStorageAccounts[ogdiAlias].storageaccountkey);

            var request = HttpWebRequest.Create(blobRequestUrlBuilder.ToString());
            request.Headers.Add("x-ms-version", "2011-08-18");

            cloudStorageAccount.Credentials.SignRequest((HttpWebRequest)request);

            return request;
        }

        #region RDF TableColumnsMetadata
        public static List<string> GetTableColumnsMetadata(string entitySet, string ogdiAlias)
        {
            var accountName =
                    AppSettings.EnabledStorageAccounts[ogdiAlias].storageaccountname;
            var accountKey =
                    AppSettings.EnabledStorageAccounts[ogdiAlias].storageaccountkey;
            var connString = "DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}";
            var cloudStorageAccount = CloudStorageAccount.Parse(string.Format(connString, accountName, accountKey));
            var context = new TableColumnsMetadataDataServiceContext(cloudStorageAccount.TableEndpoint.ToString(), cloudStorageAccount.Credentials)
            {
                RetryPolicy =
                    RetryPolicies.RetryExponential(RetryPolicies.DefaultClientRetryCount,
                                                   RetryPolicies.DefaultClientBackoff)
            };

            List<string> namespaces = new List<string>();

            var query = from entity in context.TableColumnsMetadataTable
                        where entity.PartitionKey == entitySet
                        select entity;

            string namespaceAdded;

            foreach (TableColumnsMetadataEntity c in query)
            {
                namespaceAdded = string.Empty;
                namespaceAdded = namespaces.Find(item => item.Split('=')[0] == c.columnnamespace.Split('=')[0]);
                if (namespaceAdded == string.Empty || namespaceAdded == null)
                {
                    namespaces.Add(c.columnnamespace);
                }
            }
            return namespaces;
        }
        #endregion
    }

    #region RDF TableColumnsMetadata
    class TableColumnsMetadataDataServiceContext : TableServiceContext
    {
        internal TableColumnsMetadataDataServiceContext(string baseAddress,
               StorageCredentials credentials)
            : base(baseAddress, credentials)
        {
        }

        internal const string TableColumnsMetadataTableName = "TableColumnsMetadata";

        public IQueryable<TableColumnsMetadataEntity> TableColumnsMetadataTable
        {
            get
            {
                return this.CreateQuery<TableColumnsMetadataEntity>(TableColumnsMetadataTableName);
            }
        }
    }

    public class TableColumnsMetadataEntity : TableServiceEntity
    {

        public TableColumnsMetadataEntity()
            : base()
        {
            PartitionKey = Guid.NewGuid().ToString();
            RowKey = String.Empty;
        }

        public TableColumnsMetadataEntity(string partitionKey, string rowKey)
            : base(partitionKey, rowKey)
        {
        }

        public string entityset
        {
            set;
            get;
        }
        public string column
        {
            set;
            get;
        }
        public string columnnamespace
        {
            set;
            get;
        }
        public string columndescription
        {
            set;
            get;
        }
    }
    #endregion
}

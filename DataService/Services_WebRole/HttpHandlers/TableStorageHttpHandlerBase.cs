using System;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Web;
using System.Xml.Linq;
using Microsoft.WindowsAzure;
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
        protected static XName _storageAccountNameXName = _nsd + "storageaccountname";
        protected static XName _storageAccountKeyXName = _nsd + "storageaccountkey";

        protected static string _termNameString = AppSettings.RootServiceNamespace + ".{0}.{1}";

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

            Debug.WriteLine(requestUrl);

            var azureTableRequestUrlBuilder = new StringBuilder(string.Format(requestUrl, account.Credentials.AccountName));

            if (isAvailableEndpointsRequest)
            {
                azureTableRequestUrlBuilder.Append("AvailableEndpoints");
            }

            if (!ignoreQueryOptions)
            {
                var queryString = context.Request.QueryString.ToString();

                if (!string.IsNullOrEmpty(queryString))
                {
                    azureTableRequestUrlBuilder.Append("?");
                    azureTableRequestUrlBuilder.Append(queryString);
                }
            }

            var request = WebRequest.Create(azureTableRequestUrlBuilder.ToString());

            account.Credentials.SignRequestLite((HttpWebRequest)request);

            return request;
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

            var cloudStorageAccount =
                AppSettings.ParseStorageAccount(AppSettings.EnabledStorageAccounts[ogdiAlias].storageaccountname,
                                                AppSettings.EnabledStorageAccounts[ogdiAlias].storageaccountkey);

            var request = WebRequest.Create(blobRequestUrlBuilder.ToString());
            request.Headers.Add("x-ms-version", "2011-08-18");

            cloudStorageAccount.Credentials.SignRequest((HttpWebRequest)request);

            return request;
        }
    }
}

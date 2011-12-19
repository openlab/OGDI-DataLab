using System.Net;
using System.Text;
using System.Web;
using System.Xml.Linq;
using Microsoft.WindowsAzure;

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

        private const string _connString = "DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}";

        protected WebRequest CreateTableStorageSignedRequest(HttpContext context,
                                                     string accountName, string storageAccountKey,
                                                     string requestUrl,
                                                     bool isAvailableEndpointsRequest)
        {
            return CreateTableStorageSignedRequest(context, accountName, storageAccountKey, requestUrl,
                                            isAvailableEndpointsRequest, false);
        }

        protected WebRequest CreateTableStorageSignedRequest(HttpContext context,
                                                             string accountName, string storageAccountKey,
                                                             string requestUrl,
                                                             bool isAvailableEndpointsRequest,
                                                             bool ignoreQueryOptions)
        {
            var azureTableRequestUrlBuilder = new StringBuilder(string.Format(requestUrl, accountName));

            var cloudStorageAccount = CloudStorageAccount.Parse(string.Format(_connString, accountName, storageAccountKey));

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

            cloudStorageAccount.Credentials.SignRequestLite((HttpWebRequest)request);

            return request;
        }

        protected WebRequest CreateBlobStorageSignedRequest(string blobId, string ogdiAlias, string entitySet)
        {
            var accountName =
                    AppSettings.EnabledStorageAccounts[ogdiAlias].storageaccountname;
            var accountKey =
                    AppSettings.EnabledStorageAccounts[ogdiAlias].storageaccountkey;

            var blobRequestUrlBuilder = new StringBuilder();
            blobRequestUrlBuilder.Append(string.Format(AppSettings.BlobStorageBaseUrl, accountName));
            blobRequestUrlBuilder.Append(entitySet.ToLower());
            blobRequestUrlBuilder.Append("/");
            blobRequestUrlBuilder.Append(blobId);

            var cloudStorageAccount = CloudStorageAccount.Parse(string.Format(_connString, accountName, accountKey));

            var request = WebRequest.Create(blobRequestUrlBuilder.ToString());
            request.Headers.Add("x-ms-version", "2009-09-19");

            cloudStorageAccount.Credentials.SignRequest((HttpWebRequest)request);

            return request;
        }
    }
}

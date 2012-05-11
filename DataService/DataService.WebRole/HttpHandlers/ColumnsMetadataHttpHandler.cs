using System.Web;
using System.Net;
using System.Xml.Linq;
using System.Xml;
using Microsoft.WindowsAzure;
using System.Text;

namespace Ogdi.DataServices
{
    public class ColumnsMetadataHttpHandler : IHttpHandler
    {
        private const string START_SERVICEDOCUMENT_TEMPLATE =
     @"<?xml version='1.0' encoding='utf-8'?>
            <rss version='2.0' xmlns:atom='http://www.w3.org/2005/Atom'>
              <channel>
                <title>{0} Columns Metadata</title>
                <language>en-us</language>
            ";

        private const string ITEM_TEMPLATE =
            @"
                <item>
                    <title>{0}</title>
                    <description>
                      <![CDATA[
                        {1}
                      ]]>
                    </description>
                    <author></author>
                    <id>{2}</id>
                    <published>{3}</published>
                </item>
            ";

        private const string END_SERVICEDOCUMENT_TEMPLATE =
            @"  </channel>
            </rss>";

        private const string tableColumnsMetadata = "TableColumnsMetadata";

        protected static XNamespace _nsAtom = XNamespace.Get("http://www.w3.org/2005/Atom");
        protected static XNamespace _nsm = XNamespace.Get("http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
        protected static XNamespace _nsd = XNamespace.Get("http://schemas.microsoft.com/ado/2007/08/dataservices");

        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            if (!this.IsHttpGet(context))
            {
                this.RespondForbidden(context);
            }
            else
            {
                context.Response.Headers["DataServiceVersion"] = "1.0;";
                context.Response.CacheControl = "no-cache";
                context.Response.ContentType = "application/xml;charset=utf-8";

                var accountName = AppSettings.OgdiConfigTableStorageAccountName;
                var accountKey = AppSettings.OgdiConfigTableStorageAccountKey;

                var requestUrl = AppSettings.TableStorageBaseUrl + tableColumnsMetadata;
                string[] urlParams = context.Request.Url.AbsolutePath.Split('/');
                string entitySet = urlParams[urlParams.Length - 1];

                var queryString = "$filter=PartitionKey eq '" + entitySet + "'";
                WebRequest request = CreateTableStorageRequest(queryString, accountName, accountKey, requestUrl);
                try
                {
                    var response = request.GetResponse();
                    var responseStream = response.GetResponseStream();

                    var feed = XElement.Load(XmlReader.Create(responseStream));

                    context.Response.Write(string.Format(START_SERVICEDOCUMENT_TEMPLATE, entitySet));

                    var propertiesElements = feed.Elements(_nsAtom + "entry").Elements(_nsAtom + "content").Elements(_nsm + "properties");

                    foreach (var e in propertiesElements)
                    {
                        context.Response.Write(string.Format(ITEM_TEMPLATE,
                            e.Element(_nsd + "column") != null ? e.Element(_nsd + "column").Value : "",
                            e.Element(_nsd + "columndescription") != null && e.Element(_nsd + "columndescription").Value != string.Empty ? e.Element(_nsd + "columndescription").Value : e.Element(_nsd + "columnnamespace").Value,
                            e.Element(_nsd + "column") != null ? e.Element(_nsd + "column").Value : "",
                            e.Element(_nsd + "Timestamp") != null ? e.Element(_nsd + "Timestamp").Value : ""
                            ));
                    }

                    context.Response.Write(END_SERVICEDOCUMENT_TEMPLATE);
                }
                catch (WebException ex)
                {
                    var response = ex.Response as HttpWebResponse;
                    context.Response.StatusCode = (int)response.StatusCode;
                    context.Response.End();
                }
            }
        }

        private WebRequest CreateTableStorageRequest(string queryString,
                                                             string accountName, string storageAccountKey,
                                                             string requestUrl)
        {
            var azureTableRequestUrlBuilder = new StringBuilder(string.Format(requestUrl, accountName));

            var connString = "DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}";
            var cloudStorageAccount = CloudStorageAccount.Parse(string.Format(connString, accountName, storageAccountKey));

            if (!string.IsNullOrEmpty(queryString))
            {
                azureTableRequestUrlBuilder.Append("?");
                azureTableRequestUrlBuilder.Append(queryString);
            }

            var request = HttpWebRequest.Create(azureTableRequestUrlBuilder.ToString());

            cloudStorageAccount.Credentials.SignRequestLite((HttpWebRequest)request);

            return request;
        }
    }
}
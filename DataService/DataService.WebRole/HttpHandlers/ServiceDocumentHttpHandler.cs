using System;
using System.Web.Routing;
using System.Web;
using System.Net;
using System.Xml.Linq;
using System.Xml;

namespace Ogdi.DataServices 
{
    public class ServiceDocumentHttpHandler : TableStorageHttpHandlerBase, IHttpHandler
    {
        public string OgdiAlias { get; set; }        

        private const string START_SERVICEDOCUMENT_TEMPLATE =
@"<?xml version='1.0' encoding='utf-8' standalone='yes'?>
<service xml:base='{0}' xmlns:atom='http://www.w3.org/2005/Atom' xmlns:app='http://www.w3.org/2007/app' xmlns='http://www.w3.org/2007/app'>
  <workspace>
    <atom:title>Default</atom:title>
";

        private const string COLLECTION_TEMPLATE =
@"    <collection href='{0}'>
      <atom:title>{0}</atom:title>
    </collection>
";

        private const string END_SERVICEDOCUMENT_TEMPLATE =
@"  </workspace>
</service>";

        #region IHttpHandler Members

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

                var xmlBase = "http://" + context.Request.Url.Host + context.Request.Url.AbsolutePath;

                var requestUrl = AppSettings.TableStorageBaseUrl + "TableMetadata";
                WebRequest request =
                   CreateTableStorageSignedRequest(context,
                       AppSettings.ParseStorageAccount(
                                           AppSettings.EnabledStorageAccounts[OgdiAlias].storageaccountname,
                                           AppSettings.EnabledStorageAccounts[OgdiAlias].storageaccountkey),
                                           requestUrl, false);

                try
                {
                    var response = request.GetResponse();

                    if (response != null)
                    {
                        var responseStream = response.GetResponseStream();
                        if (responseStream != null)
                        {
                            var feed = XElement.Load(XmlReader.Create(responseStream));

                            context.Response.Write(string.Format(START_SERVICEDOCUMENT_TEMPLATE, xmlBase));

                            var propertiesElements = feed.Elements(_nsAtom + "entry").Elements(_nsAtom + "content").Elements(_nsm + "properties");

                            foreach (var e in propertiesElements)
                            {
                                var entitySet = e.Element(_nsd + "entityset");
                                context.Response.Write(string.Format(COLLECTION_TEMPLATE,
                                    (entitySet != null) ? entitySet.Value : string.Empty));
                            }
                        }
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

        #endregion
    }
}

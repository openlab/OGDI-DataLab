using System.Web;
using System.Xml.Linq;

namespace Ogdi.DataServices.v1
{
    public class AvailableEndpointsHttpHandler : AbstractHttpHandler
    {
        public override void ProcessRequest(HttpContext httpContext)
        {
            base.ProcessRequest(httpContext);

            _Account = AppSettings.Account;

            XElement feed = this.GetFeed("AvailableEndpoints");

            // Setup URL replacements
            _AzureTableUrlToReplace = string.Format(AppSettings.TableStorageBaseUrl, AppSettings.OgdiConfigTableStorageAccountName);
            _AzureTableUrlReplacement = string.Format("{0}://{1}/v1/", _HttpContext.Request.Url.Scheme, _HttpContext.Request.Url.Host);

            this.Render(feed);
        }

        protected override void Render(XElement feed)
        {
            _HttpContext.Response.ContentType = _atomXmlContentType;

            string idValue = feed.Element(_idXName).Value;
            feed.Element(_idXName).Value = ReplaceAzureUrlInString(idValue);

            string baseValue = feed.Attribute(XNamespace.Xml + "base").Value;
            feed.Attribute(XNamespace.Xml + "base").Value = ReplaceAzureUrlInString(baseValue);

            // The xml payload coming back has a <kmlsnippet> property.  We want to
            // hide that from the consumer of our service by removing it.
            // NOTE: We only use kmlsnippet when returning KML.

            // Iterate through all the entries to update 
            // Azure Table Storage url for //feed/entry/id
            // and remove kmlsnippet element from all instances of
            // //feed/entry/content/properties

            foreach (var entry in feed.Elements(_entryXName))
            {
                idValue = entry.Element(_idXName).Value;
                entry.Element(_idXName).Value = ReplaceAzureUrlInString(idValue);

                var properties = entry.Elements(_contentXName).Elements(_propertiesXName);

                properties.Elements(_storageAccountNameXName).Remove();
                properties.Elements(_storageAccountKeyXName).Remove();
            }

            _HttpContext.Response.Write(feed.ToString());
        }
    }
}
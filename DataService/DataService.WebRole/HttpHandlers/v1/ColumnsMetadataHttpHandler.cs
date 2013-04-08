using System.Collections.Specialized;
using System.Web;
using System.Xml.Linq;

namespace Ogdi.DataServices.v1
{
    public class ColumnsMetadataHttpHandler : AbstractHttpHandler
    {
        #region Render variables

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

        #endregion

        public override void ProcessRequest(HttpContext httpContext)
        {
            base.ProcessRequest(httpContext);
            
            _Account = AppSettings.Account;

            NameValueCollection additionalQueryStrings = new NameValueCollection();
            additionalQueryStrings.Add("$filter", string.Format("entityset eq '{0}'", EntitySet));

            XElement feed = this.GetFeed("TableColumnsMetadata", additionalQueryStrings);
            
            this.Render(feed);
        }

        protected override void Render(XElement feed)
        {
            _HttpContext.Response.ContentType = _xmlContentType;

            _HttpContext.Response.Write(string.Format(START_SERVICEDOCUMENT_TEMPLATE, EntitySet));

            foreach (var e in feed.Elements(_nsAtom + "entry").Elements(_nsAtom + "content").Elements(_nsm + "properties"))
            {
                _HttpContext.Response.Write(string.Format(ITEM_TEMPLATE,
                    e.Element(_nsd + "column") != null ? e.Element(_nsd + "column").Value : string.Empty,
                    e.Element(_nsd + "columndescription") != null && e.Element(_nsd + "columndescription").Value != string.Empty ? e.Element(_nsd + "columndescription").Value : e.Element(_nsd + "columnnamespace").Value,
                    e.Element(_nsd + "column") != null ? e.Element(_nsd + "column").Value : string.Empty,
                    e.Element(_nsd + "Timestamp") != null ? e.Element(_nsd + "Timestamp").Value : string.Empty));
            }

            _HttpContext.Response.Write(END_SERVICEDOCUMENT_TEMPLATE);
        }
    }
}
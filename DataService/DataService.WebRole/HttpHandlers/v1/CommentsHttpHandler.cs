using System.Net;
using System.Web;
using System.Xml.Linq;

namespace Ogdi.DataServices.v1
{
    public class CommentsHttpHandler : AbstractHttpHandler
    {
        #region Render varibles

        private const string START_SERVICEDOCUMENT_TEMPLATE =
    @"<?xml version='1.0' encoding='utf-8'?>
			<rss version='2.0' xmlns:atom='http://www.w3.org/2005/Atom'>
			  <channel>
				<title>Comments</title>
				<language>en-us</language>
			";

        private const string ITEM_TEMPLATE =
            @"
				<item>
					<title>{0}</title>
					<description>
					  <![CDATA[
						Status: {1};
						Type: {2};
						Description: {3};
					  ]]>
					</description>
					<author>{4}</author>
					<id>{5}</id>
					<published>{6}</published>
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

            XElement feed = this.GetFeed("Comments");

            this.Render(feed);
        }

        protected override void Render(XElement feed)
        {
            _HttpContext.Response.ContentType = _xmlContentType;

            string xmlBase = string.Format("http://{0}{1}", _HttpContext.Request.Url.Host, _HttpContext.Request.Url.AbsolutePath);
            _HttpContext.Response.Write(string.Format(START_SERVICEDOCUMENT_TEMPLATE, xmlBase));

            foreach (var e in feed.Elements(_nsAtom + "entry").Elements(_nsAtom + "content").Elements(_nsm + "properties"))
            {
                if (e != null)
                {
                    _HttpContext.Response.Write(string.Format(ITEM_TEMPLATE,
                        e.Element(_nsd + "Subject") != null ? e.Element(_nsd + "Subject").Value : string.Empty,
                        e.Element(_nsd + "Status") != null ? e.Element(_nsd + "Status").Value : string.Empty,
                        e.Element(_nsd + "Type") != null ? e.Element(_nsd + "Type").Value : string.Empty,
                        e.Element(_nsd + "Comment") != null ? e.Element(_nsd + "Comment").Value : string.Empty,
                        e.Element(_nsd + "Email") != null ? e.Element(_nsd + "Email").Value : string.Empty,
                        e.Element(_nsd + "RowKey") != null ? e.Element(_nsd + "RowKey").Value : string.Empty,
                        e.Element(_nsd + "PostedOn") != null ? e.Element(_nsd + "PostedOn").Value : string.Empty));
                }
            }

            _HttpContext.Response.Write(END_SERVICEDOCUMENT_TEMPLATE);
        }
    }
}
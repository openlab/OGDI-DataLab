using System.Net;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace Ogdi.DataServices
{
    public class CommentsHttpHandler : TableStorageHttpHandlerBase, IHttpHandler
    {
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

                var account = AppSettings.Account;
                var requestUrl = AppSettings.TableStorageBaseUrl + "Comments";

                WebRequest request = CreateTableStorageSignedRequest(context, account, requestUrl, false);

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

                            var propertiesElements =
                                feed.Elements(_nsAtom + "entry").Elements(_nsAtom + "content").Elements(_nsm + "properties");

                            foreach (var e in propertiesElements)
                            {
                                if (e == null) continue;
                                XElement subject = e.Element(_nsd + "Subject");
                                XElement status = e.Element(_nsd + "Status");
                                XElement type = e.Element(_nsd + "Type");
                                XElement comment = e.Element(_nsd + "Comment");
                                XElement email = e.Element(_nsd + "Email");
                                XElement rowKey = e.Element(_nsd + "RowKey");
                                XElement postedOn = e.Element(_nsd + "PostedOn");

                                context.Response.Write(string.Format(ITEM_TEMPLATE,
                                                                     subject != null ? subject.Value : string.Empty,
                                                                     status != null ? status.Value : string.Empty,
                                                                     type != null ? type.Value : string.Empty,
                                                                     comment != null ? comment.Value : string.Empty,
                                                                     email != null ? email.Value : string.Empty,
                                                                     rowKey != null ? rowKey.Value : string.Empty,
                                                                     postedOn != null ? postedOn.Value : string.Empty));
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

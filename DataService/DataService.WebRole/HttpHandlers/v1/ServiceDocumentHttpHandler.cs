using System.Web;
using System.Xml.Linq;

namespace Ogdi.DataServices.v1
{
    public class ServiceDocumentHttpHandler : AbstractHttpHandler
    {
        #region Render varibles

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

        #endregion

        public override void ProcessRequest(HttpContext httpContext)
        {
            base.ProcessRequest(httpContext);

            _Account = AppSettings.ParseStorageAccount(
                AppSettings.EnabledStorageAccounts[OgdiAlias].storageaccountname,
                AppSettings.EnabledStorageAccounts[OgdiAlias].storageaccountkey);

            XElement feed = this.GetFeed("TableMetadata", null, false);

            this.Render(feed);
        }

        protected override void Render(XElement feed)
        {
            _HttpContext.Response.ContentType = _xmlContentType;

            string xmlBase = string.Format("http://{0}:{1}{2}", _HttpContext.Request.Url.Host, _HttpContext.Request.Url.Port, _HttpContext.Request.Url.AbsolutePath);
            _HttpContext.Response.Write(string.Format(START_SERVICEDOCUMENT_TEMPLATE, xmlBase));

            var propertiesElements = feed.Elements(_nsAtom + "entry").Elements(_nsAtom + "content").Elements(_nsm + "properties");
            foreach (var e in propertiesElements)
            {
                XElement entitySet = e.Element(_nsd + "entityset");
                _HttpContext.Response.Write(string.Format(COLLECTION_TEMPLATE, (entitySet != null) ? entitySet.Value : string.Empty));
            }

            _HttpContext.Response.Write(END_SERVICEDOCUMENT_TEMPLATE);
        }
    }
}
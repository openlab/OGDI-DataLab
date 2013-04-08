using Ogdi.Config;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Ogdi.DataServices.v1
{
    public class NestedServiceDocumentHttpHandler : AbstractHttpHandler
    {
        #region Render variables

        private const string START_SERVICEDOCUMENT_TEMPLATE =
@"<?xml version='1.0' encoding='utf-8' standalone='yes'?>
<service xml:base='{0}' xmlns:atom='http://www.w3.org/2005/Atom' xmlns:app='http://www.w3.org/2007/app' xmlns='http://www.w3.org/2007/app'>
  <workspace>
    <atom:title>Default</atom:title>
";

        private const string COLLECTION_TEMPLATE =
@"    <collection href='{0}'>
      <atom:title>{1}</atom:title>
      <accept>application/atomsvc+xml</accept>
    </collection>
";

        private const string END_SERVICEDOCUMENT_TEMPLATE =
@"  </workspace>
</service>";

        #endregion

        public override void ProcessRequest(HttpContext httpContext)
        {
            base.ProcessRequest(httpContext);

            _Account = AppSettings.Account;

            this.Render(null);
        }

        protected override void Render(XElement feed)
        {
            _HttpContext.Response.ContentType = _xmlContentType;

            OgdiConfigDataServiceContext ogdiConfigContext = new OgdiConfigDataServiceContext(_Account.TableEndpoint.AbsoluteUri, _Account.Credentials);
            string xmlBase = string.Format("http://{0}{1}", _HttpContext.Request.Url.Host, _HttpContext.Request.Url.AbsolutePath);
            List<AvailableEndpoint> list = ogdiConfigContext.AvailableEndpoints.ToList();

            _HttpContext.Response.Write(string.Format(START_SERVICEDOCUMENT_TEMPLATE, xmlBase));

            foreach (AvailableEndpoint item in list)
            {
                _HttpContext.Response.Write(string.Format(COLLECTION_TEMPLATE, item.alias, item.description));
            }

            _HttpContext.Response.Write(END_SERVICEDOCUMENT_TEMPLATE);
        }
    }
}
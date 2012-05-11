using System;
using System.Web.Routing;
using System.Web;

namespace Ogdi.DataServices
{
    public class NestedServiceDocumentRouteHandler : IRouteHandler
    {
        #region IRouteHandler Members

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {

            return new NestedServiceDocumentHttpHandler();
        }

        #endregion
    }
}

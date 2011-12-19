using System.Web;
using System.Web.Routing;

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

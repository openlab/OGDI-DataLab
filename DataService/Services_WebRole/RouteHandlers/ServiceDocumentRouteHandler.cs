using System.Web;
using System.Web.Routing;

namespace Ogdi.DataServices
{
    public class ServiceDocumentRouteHandler : IRouteHandler
    {
        #region IRouteHandler Members

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            var serviceDocumentHttpHandler = new ServiceDocumentHttpHandler()
            {
                OgdiAlias = requestContext.RouteData.Values["OgdiAlias"] as string
            };
            
            return serviceDocumentHttpHandler;
        }

        #endregion
    }
}

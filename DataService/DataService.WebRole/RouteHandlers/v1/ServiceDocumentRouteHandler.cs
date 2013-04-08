using System.Web;
using System.Web.Routing;

namespace Ogdi.DataServices.v1
{
    public class ServiceDocumentRouteHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            IHttpHandler HttpHandler = new ServiceDocumentHttpHandler()
            {
                OgdiAlias = requestContext.RouteData.Values["OgdiAlias"] as string
            };

            return HttpHandler;
        }
    }
}
using System.Web.Routing;
using System.Web;

namespace Ogdi.DataServices.v1
{
    public class ColumnsMetadataRouteHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            IHttpHandler HttpHandler = new ColumnsMetadataHttpHandler()
            {
                OgdiAlias = requestContext.RouteData.Values["OgdiAlias"] as string,
                EntitySet = requestContext.RouteData.Values["EntitySet"] as string
            };

            return HttpHandler;
        }
    }
}
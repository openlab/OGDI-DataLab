using System.Web.Routing;
using System.Web;
using Ogdi.DataServices.Helper;

namespace Ogdi.DataServices.v1
{
    public class MetadataRouteHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            IHttpHandler HttpHandler = new MetadataHttpHandler()
            {
                OgdiAlias = requestContext.RouteData.Values["OgdiAlias"] as string,
                Cache = new Cache()
            };

            return HttpHandler;
        }
    }
}

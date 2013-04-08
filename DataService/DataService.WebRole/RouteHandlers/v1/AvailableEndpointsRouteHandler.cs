using System.Web;
using System.Web.Routing;

namespace Ogdi.DataServices.v1
{
    public class AvailableEndpointsRouteHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            IHttpHandler HttpHandler = new AvailableEndpointsHttpHandler();
            return HttpHandler;
        }
    }
}
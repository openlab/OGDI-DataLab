using System.Web;
using System.Web.Routing;

namespace Ogdi.DataServices.v1
{
    public class NestedServiceDocumentRouteHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            IHttpHandler HttpHandler = new NestedServiceDocumentHttpHandler();
            return HttpHandler;
        }
    }
}
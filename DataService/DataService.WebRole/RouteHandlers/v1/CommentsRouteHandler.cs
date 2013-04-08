using System.Web;
using System.Web.Routing;

namespace Ogdi.DataServices.v1
{
    public class CommentsRouteHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            IHttpHandler HttpHandler = new CommentsHttpHandler();
            return HttpHandler;
        }
    }
}
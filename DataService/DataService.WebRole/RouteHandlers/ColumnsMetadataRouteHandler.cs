using System;
using System.Web.Routing;
using System.Web;

namespace Ogdi.DataServices
{
    public class ColumnsMetadataRouteHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            var httpHandler = new ColumnsMetadataHttpHandler();
            return httpHandler;
        }
    }
}
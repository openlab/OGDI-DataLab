using System;
using System.Web.Routing;
using System.Web;

namespace Ogdi.DataServices
{
    public class CommentsRouteHandler : IRouteHandler
    {
        #region IRouteHandler Members

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            var httpHandler = new CommentsHttpHandler();
            return httpHandler;
        }

        #endregion
    }
}

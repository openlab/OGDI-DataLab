using System.Web;
using System.Web.Routing;

namespace Ogdi.DataServices
{
    public class MetaDataRouteHandler : IRouteHandler
    {
        #region IRouteHandler Members

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            var metaDataHttpHandler = new MetaDataHttpHandler()
            {
                OgdiAlias = requestContext.RouteData.Values["OgdiAlias"] as string
            };


            return metaDataHttpHandler;
        }

        #endregion
    }
}

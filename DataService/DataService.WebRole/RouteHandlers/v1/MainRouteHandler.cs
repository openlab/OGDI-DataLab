using System.Web;
using System.Web.Routing;

namespace Ogdi.DataServices.v1
{
    public class MainRouteHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            string ogdiAlias = requestContext.RouteData.Values["OgdiAlias"] as string;
            string entitySet = requestContext.RouteData.Values["EntitySet"] as string;
            string remainder = requestContext.RouteData.Values["Remainder"] as string;

            if (entitySet.EndsWith("()"))
            {
                entitySet = entitySet.Substring(0, entitySet.Length - 2);
            }

            if (!AppSettings.EnabledStorageAccounts.ContainsKey(ogdiAlias))
            {
                // If the requested OgdiAlias for the storage account is not already cached, then refresh the cache.
                // If it is still not in the cache, then this is a bad request.
                AppSettings.RefreshAvailableEndpoints();
                if (!AppSettings.EnabledStorageAccounts.ContainsKey(ogdiAlias))
                {
                    Ogdi.Config.AvailableEndpoint endPoint = AppSettings.GetAvailableEndpointByAccountName(ogdiAlias);
                    if (endPoint == null)
                    {
                        return new NotFoundHandler();
                    }
                    ogdiAlias = endPoint.alias;
                }
            }

            IHttpHandler HttpHandler = new MainHttpHandler()
            {
                OgdiAlias = ogdiAlias,
                EntitySet = entitySet,
                Remainder = remainder
            };

            return HttpHandler;
        }
    }
}
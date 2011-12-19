using System.Web;
using System.Web.Routing;

namespace Ogdi.DataServices
{
    public class V1RouteHandler : IRouteHandler
    {
        private string _remainder;
        private string _ogdiAlias;
        private string _entitySet;
        private string _azureTableRequestEntityUrl;
        private bool _isAvailableEndpointsRequest;

        #region IRouteHandler Members

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            _azureTableRequestEntityUrl = AppSettings.TableStorageBaseUrl;
            IHttpHandler handlerToReturn = null;

            _isAvailableEndpointsRequest = string.Compare(((Route)requestContext.RouteData.Route).Url, "v1/AvailableEndpoints", true) == 0;
            _remainder = requestContext.RouteData.Values["remainder"] as string;
            _ogdiAlias = requestContext.RouteData.Values["OgdiAlias"] as string;
            _entitySet = requestContext.RouteData.Values["EntitySet"] as string;

            if (!_isAvailableEndpointsRequest)
            {
                if (AppSettings.EnabledStorageAccounts.ContainsKey(_ogdiAlias))
                {
                    handlerToReturn = GetTableStorageProxyHandler();
                }
                else
                {
                    // If the requested OgdiAlias for the storage account is not already cached,
                    // then refresh the cache.  If it is still not in the cache, then this is 
                    // a bad request.
                    AppSettings.RefreshAvailableEndpoints();

                    if (AppSettings.EnabledStorageAccounts.ContainsKey(_ogdiAlias))
                    {
                        handlerToReturn = GetTableStorageProxyHandler();
                    }
                    else
                    {
                        //Possile OgdiAlias is acccountName
                        Ogdi.Config.AvailableEndpoint endPoint = AppSettings.GetAvailableEndpointByAccountName(_ogdiAlias);

                        if (endPoint != null)
                        {
                            _ogdiAlias = endPoint.alias;
                            handlerToReturn = GetTableStorageProxyHandler();
                        }
                        else
                        {
                            handlerToReturn = new NotFoundHandler();
                        }
                    }
                }
            }
            else
            {
                handlerToReturn = GetTableStorageProxyHandler();
            }

            return handlerToReturn;
        }

        #endregion

        private IHttpHandler GetTableStorageProxyHandler()
        {
            _azureTableRequestEntityUrl += _entitySet + "/";

            if (_remainder != null)
            {
                _azureTableRequestEntityUrl += _remainder;
            }

            var azureTableStorageProxyHttpHandler = new V1OgdiTableStorageProxyHttpHandler 
            { 
                OgdiAlias = _ogdiAlias, 
                EntitySet = _entitySet, 
                IsAvailableEndpointsRequest = _isAvailableEndpointsRequest, 
                AzureTableRequestEntityUrl = _azureTableRequestEntityUrl 
            };

            return azureTableStorageProxyHttpHandler;
        }
    }
}
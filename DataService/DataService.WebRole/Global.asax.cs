using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Linq;
using System.Web.Routing;

namespace Ogdi.DataServices
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            CloudStorageAccount.SetConfigurationSettingPublisher((configName, configSetter) =>
            {
                configSetter(RoleEnvironment.GetConfigurationSettingValue(configName));
                RoleEnvironment.Changed += (senderRole, argRole) =>
                {
                    if (argRole.Changes.OfType<RoleEnvironmentConfigurationSettingChange>()
                        .Any((change) => (change.ConfigurationSettingName == configName)))
                    {
                        if (!configSetter(RoleEnvironment.GetConfigurationSettingValue(configName)))
                        {
                            RoleEnvironment.RequestRecycle();
                        }
                    }
                };
            });

            RegisterRoutes(RouteTable.Routes);
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.Add("v1AvailableEndpoints", new Route("v1/AvailableEndpoints", new v1.AvailableEndpointsRouteHandler()));
            routes.Add("v1Comments", new Route("v1/Comments", new v1.CommentsRouteHandler()));
            routes.Add("v1MetaData", new Route("v1/{OgdiAlias}/$metadata", new v1.MetadataRouteHandler()));
            routes.Add("v1ColumnsMetadata", new Route("v1/ColumnsMetadata/{OgdiAlias}/{EntitySet}", new v1.ColumnsMetadataRouteHandler()));
            routes.Add("v1MainRoute", new Route("v1/{OgdiAlias}/{EntitySet}/{*remainder}", new v1.MainRouteHandler()));
            routes.Add("v1ServiceDocument", new Route("v1/{OgdiAlias}", new v1.ServiceDocumentRouteHandler()));
            routes.Add("v1NestedServiceDocuments", new Route("v1", new v1.NestedServiceDocumentRouteHandler()));
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}
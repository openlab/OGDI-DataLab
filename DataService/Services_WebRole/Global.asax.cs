using System;
using System.Linq;
using System.Web.Routing;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Ogdi.DataServices
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            RegisterRoutes(RouteTable.Routes);

            // This code sets up a handler to update CloudStorageAccount instances when their corresponding
            // configuration settings change in the service configuration file.
            CloudStorageAccount.SetConfigurationSettingPublisher((configName, configSetter) =>
            {
                configSetter(RoleEnvironment.GetConfigurationSettingValue(configName));
                RoleEnvironment.Changed += (s, arg) =>
                                               {
                                                   if (!arg.Changes.OfType<RoleEnvironmentConfigurationSettingChange>()
                                                            .Any(
                                                                (change) =>
                                                                (change.ConfigurationSettingName == configName)))
                                                       return;
                                                   if (
                                                       !configSetter(
                                                           RoleEnvironment.GetConfigurationSettingValue(configName)))
                                                   {
                                                       RoleEnvironment.RequestRecycle();
                                                   }
                                               };
            });
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            // We use ASP.NET Routing to determine whether or not to handle incoming requests
            
            var v1RouteHandler = new V1RouteHandler();

            routes.Add("V1AvailableEndpoints", new Route("v1/AvailableEndpoints", v1RouteHandler));
            routes.Add("CommentsRouteHandler", new Route("v1/Comments", new CommentsRouteHandler()));
            routes.Add("V1MetaData", new Route("v1/{OgdiAlias}/$metadata", new MetaDataRouteHandler()));
            routes.Add("V1PrimaryRoute", new Route("v1/{OgdiAlias}/{EntitySet}/{*remainder}", v1RouteHandler));
            routes.Add("V1ServiceDocument", new Route("v1/{OgdiAlias}", new ServiceDocumentRouteHandler()));
            routes.Add("V1NestedServiceDocuments", new Route("v1", new NestedServiceDocumentRouteHandler()));
            
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
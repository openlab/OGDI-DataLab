using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Ogdi.InteractiveSdk.Mvc
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Mvc")]
    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{controller}/{action}/{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "LoadLegalDisclaimerMapRoute",
                "DataCatalog/LoadLegalDisclaimerMapRoute/{containerAlias}",
                new { controller = "DataCatalog", action = "ReturnLegalDisclaimerForThisAlias", containerAlias = "" }
                );

            routes.MapRoute(
                "LoadDataCatalogMapRoute",
                "DataCatalog/LoadDataCatalogMapRoute/{containerAlias}/{entitySetName}",
                new { controller = "DataCatalog", action = "LoadDataCatalogByContainerAlias", containerAlias = "", entitySetName = "" }
                );

            routes.MapRoute(
                "LoadDataCatalogEntitySets",
                "DataCatalog/LoadDataCatalogEntitySets/{containerAlias}/{categoryName}",
                new { controller = "DataCatalog", action = "LoadEntitySetsByCategory", containerAlias = "", categoryName = "" }
                );

            routes.MapRoute(
                "LoadSampleCodeDataView",
                "DataBrowser/LoadSampleCodeDataView/",
                new { controller = "DataBrowser", action = "LoadDataViewSampleCode" }
                );

            routes.MapRoute(
                "LoadSampleCodeMapView",
                "DataBrowser/LoadSampleCodeMapView/",
                new { controller = "DataBrowser", action = "LoadMapViewSampleCode" }
                );

            routes.MapRoute(
                "LoadSampleCodeBarChartView",
                "DataBrowser/LoadSampleCodeBarChartView/",
                new { controller = "DataBrowser", action = "LoadBarChartSampleCode" }
                );


            routes.MapRoute(
                "LoadSampleCodePieChartView",
                "DataBrowser/LoadSampleCodePieChartView/",
                new { controller = "DataBrowser", action = "LoadPieChartSampleCode" }
                );


            routes.MapRoute(
                "DataBrowserPaging",
                "DataBrowser/DataBrowserPaging/",
                new { controller = "DataBrowser", action = "PagingClicked" }
                );

            routes.MapRoute(
                "DataBrowserRun",
                "DataBrowser/DataBrowserRun/",
                new { controller = "DataBrowser", action = "RunButtonClicked" }
                );

            routes.MapRoute(
                "DataBrowserRunBarChart",
                "DataBrowser/DataBrowserRunBarChart/",
                new { controller = "DataBrowser", action = "RunBarChartButtonClicked" }
                );


            routes.MapRoute(
                "DataBrowserRunPieChart",
                "DataBrowser/DataBrowserRunPieChart/",
                new { controller = "DataBrowser", action = "RunPieChartButtonClicked" }
                );

            routes.MapRoute(
                "DataBrowserError",
                "DataBrowser/DataBrowserError/",
                new { controller = "DataBrowser", action = "ShowClientsideError" }
                );

            routes.MapRoute(
                "ControllerAction",
                "{controller}/{action}",
                new { controller = "Home", action = "Index" }
                );

            routes.MapRoute(
                "DataBrowser",
                "DataBrowser/{container}/{entitySetName}",
                new { controller = "DataBrowser", action = "Index", container = "", entitySetName = "" }
                );

            routes.MapRoute(
                "VoteRoute",
                "Rates/{action}/{itemKey}",
                new { controller = "Rates", action = "Index", itemKey = "" }
                );

            routes.MapRoute(
                "Default",                                              // Route name
                "{controller}/{action}/{id}",                           // URL with parameters
                new { controller = "DataCatalog", action = "DataSetList", id = "" }  // Parameter defaults
            );
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        protected void Application_Start()
        {
            RegisterRoutes(RouteTable.Routes);

            CloudStorageAccount.SetConfigurationSettingPublisher(
                (configName, configSetter) => configSetter(RoleEnvironment.GetConfigurationSettingValue(configName)));
        }
    }
}

using System;
using System.Configuration;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using Ogdi.Config;

namespace ConfigTool
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            var ta = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["OgdiConfigConnectionString"]);
            var tc = ta.CreateCloudTableClient();
            if (!tc.DoesTableExist(OgdiConfigDataServiceContext.EndpointsTableName))
            {
                tc.CreateTable(OgdiConfigDataServiceContext.EndpointsTableName);
            }
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
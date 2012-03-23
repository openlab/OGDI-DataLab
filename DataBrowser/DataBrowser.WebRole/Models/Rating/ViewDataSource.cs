using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure;
using System.Configuration;
using Microsoft.WindowsAzure.StorageClient;

namespace Ogdi.InteractiveSdk.Mvc.Models.Rating
{
    public class ViewDataSource
    {
        private static CloudStorageAccount storageAccount;
        private ViewDataContext context;

        static ViewDataSource()
        {
            storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["OgdiConfigConnectionString"]) ;

            CloudTableClient.CreateTablesFromModel(
                typeof(ViewDataContext),
                ConfigurationManager.AppSettings["TableStorageEndpoint"],
                storageAccount.Credentials);
        }

        public ViewDataSource()
        {
            this.context = new ViewDataContext(ConfigurationManager.AppSettings["TableStorageEndpoint"], storageAccount.Credentials);
            this.context.RetryPolicy = RetryPolicies.Retry(3, TimeSpan.FromSeconds(1));
        }

        public void View(string datasetId)
        {
            ViewEntry item = this.context.ViewEntry.Where(t => t.DatasetId == datasetId).FirstOrDefault();
            //if (this.context.ViewEntry.Where(t => t.DatasetId == datasetId).ToList().Count > 0)
            if(item != null)
            {
                item.Views++;
                this.context.UpdateObject(item);
            }
            else
            {
                item = new ViewEntry() {DatasetId = datasetId, Views=1 };
                this.context.AddObject("ViewEntry", item);
            }
            
            this.context.SaveChanges();
        }

        public IList<ViewEntry> SelectAll()
        {
            var results = from g in this.context.ViewEntry
                          select g;
            return results.ToList();
        }
    }
}

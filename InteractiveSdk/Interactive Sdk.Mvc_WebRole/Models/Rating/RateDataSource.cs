using System;
using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using Ogdi.Azure.Configuration;

namespace Ogdi.InteractiveSdk.Mvc.Models.Rating
{
    public class RateDataSource
    {
        private static CloudStorageAccount storageAccount;
        private RateDataContext context;

        static RateDataSource()
        {
			storageAccount = CloudStorageAccount.Parse(OgdiConfiguration.GetValue("DataConnectionString"));

            CloudTableClient.CreateTablesFromModel(
                typeof(RateDataContext),
				storageAccount.TableEndpoint.AbsoluteUri,
                storageAccount.Credentials);
        }

        public RateDataSource()
        {
			this.context = new RateDataContext(storageAccount.TableEndpoint.AbsoluteUri, storageAccount.Credentials);
            this.context.RetryPolicy = RetryPolicies.Retry(3, TimeSpan.FromSeconds(1));
        }

        public void AddVote(RateEntry item)
        {
            this.context.AddObject("RateEntry", item);
            this.context.SaveChanges();
        }

        public IQueryable<RateEntry> SelectAll()
        {
            return this.context.RateEntry;
        }
              

        
    }
}

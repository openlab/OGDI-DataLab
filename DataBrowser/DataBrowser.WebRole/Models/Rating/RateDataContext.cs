using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Ogdi.InteractiveSdk.Mvc.Models.Rating
{
    public class RateDataContext : TableServiceContext
    {
        public RateDataContext(string baseAddress, StorageCredentials credentials)
            : base(baseAddress, credentials)
        {
        }

        public IQueryable<RateEntry> RateEntry
        {
            get
            {
                return this.CreateQuery<RateEntry>("RateEntry");
            }
        }
    }
}

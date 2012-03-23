using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Ogdi.InteractiveSdk.Mvc.Models.Request
{
    public class RequestDataContext : TableServiceContext
    {
        public RequestDataContext(string baseAddress, StorageCredentials credentials)
            : base(baseAddress, credentials)
        {

        }

        public IQueryable<RequestEntry> Requests
        {
            get
            {
                return this.CreateQuery<RequestEntry>("Requests");
            }
        }
    }
}

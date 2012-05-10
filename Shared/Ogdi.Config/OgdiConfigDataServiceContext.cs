using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Ogdi.Config
{
    public class OgdiConfigDataServiceContext : TableServiceContext
    {
        public static readonly string EndpointsTableName = "AvailableEndpoints";
        
        public OgdiConfigDataServiceContext(string baseAddress, StorageCredentials credentials)
            : base(baseAddress, credentials)
        {
        }

        public IQueryable<AvailableEndpoint> AvailableEndpoints
        {
            get
            {
                return CreateQuery<AvailableEndpoint>(EndpointsTableName);
            }
        }
    }
}

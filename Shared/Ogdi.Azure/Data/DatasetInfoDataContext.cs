using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Ogdi.Azure.Data
{
    public sealed class DatasetInfoDataContext : TableServiceContext
    {
        public static readonly string AnalyticInfoTableName = "AnalyticInfo";

        public DatasetInfoDataContext(string baseAddress, StorageCredentials credentials)
            : base( baseAddress, credentials)
        {
        }

        private IQueryable<AnalyticInfo> _analyticInfo;
        public IQueryable<AnalyticInfo> AnalyticInfo
        {
            get
            {
                if(_analyticInfo == null)
                    _analyticInfo = CreateQuery<AnalyticInfo>(AnalyticInfoTableName)
                        .Where(a => a.PartitionKey == "analytics")
                        .AsTableServiceQuery()
                        .ToList().AsQueryable();

                return _analyticInfo;
            }
        }
    }
}

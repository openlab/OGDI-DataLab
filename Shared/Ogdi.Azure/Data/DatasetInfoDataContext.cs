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

        public IQueryable<AnalyticInfo> AnalyticInfo
        {
            get
            {
				return CreateQuery<AnalyticInfo>(AnalyticInfoTableName);
            }
        }
    }
}

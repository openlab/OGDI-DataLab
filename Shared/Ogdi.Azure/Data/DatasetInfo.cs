using System;
using Microsoft.WindowsAzure.StorageClient;

namespace Ogdi.Azure.Data
{
    public class AnalyticInfo : TableServiceEntity
    {
        public int views_total { get; set; }
        public int views_today { get; set; }
        public double views_average { get; set; }
        public DateTime last_viewed { get; set; }
        public int PositiveVotes { get; set; }
        public int NegativeVotes { get; set; }

		[Obsolete("This ctor is only for TableServiceContext infrastructure. Use ctor with parameters in your code.")]
		public AnalyticInfo()
        {
        }

		public AnalyticInfo(String itemKey)
		{
            RowKey = itemKey;
            PartitionKey = "analytics";
		}
    }
}

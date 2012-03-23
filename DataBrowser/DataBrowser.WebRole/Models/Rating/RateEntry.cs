using System;
using Microsoft.WindowsAzure.StorageClient;

namespace Ogdi.InteractiveSdk.Mvc.Models.Rating
{
    public class RateEntry : TableServiceEntity
    {
        public DateTime RateDate { get; set; }
        public string User { get; set; }
        public int RateValue { get; set; }
        public String ItemKey { get; set; }

        public RateEntry()
        {
            this.RowKey = Guid.NewGuid().ToString();
        }

    }
}

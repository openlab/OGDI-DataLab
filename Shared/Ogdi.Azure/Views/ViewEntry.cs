using System;
using Microsoft.WindowsAzure.StorageClient;

namespace Ogdi.Azure.Views
{
    public class ViewEntry : TableServiceEntity
    {
        public DateTime Date { get; set; }
        public string User { get; set; }
        public String ItemKey { get; set; }
        public String RequestedUrl { get; set; }
        
        public ViewEntry()
        {
            this.RowKey = Guid.NewGuid().ToString();
            this.PartitionKey = "views";
        }
    }
}

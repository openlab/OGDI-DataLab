using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.StorageClient;

namespace Ogdi.InteractiveSdk.Mvc.Models.Rating
{
    public class ViewEntry: TableServiceEntity
    {
        public ViewEntry()
        {
            this.RowKey = Guid.NewGuid().ToString();
            this.PartitionKey = "Views";
        }
        public string DatasetId
        {
            get;
            set;
        }
        public int Views { get; set; }
    }
}

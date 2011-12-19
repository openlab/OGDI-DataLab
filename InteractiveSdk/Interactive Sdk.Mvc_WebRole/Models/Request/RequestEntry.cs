using System;
using System.Web.Mvc;
using Microsoft.WindowsAzure.StorageClient;

namespace Ogdi.InteractiveSdk.Mvc.Models.Request
{
    public class RequestEntry : TableServiceEntity
    {
        public RequestEntry()
        {
            this.RowKey = Guid.NewGuid().ToString();
            this.PartitionKey = "Rate";
        }

        public string Name { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public DateTime? PostedDate { get; set; }
        public string Links { get; set; }
        public string DatasetLink { get; set; }
        public int Comments {get; set; }

        
        public SelectList GetAvailableStatuses()
        {
            return new SelectList(new string[] { "New", "In Progress", "Completed" }, Status);
        }
    }
}

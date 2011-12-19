using System;
using Microsoft.WindowsAzure.StorageClient;

namespace Ogdi.InteractiveSdk.Mvc.Models.Comments
{
    public class CommentEntry : TableServiceEntity
    {
        public string DatasetId { get; set; }

        public string Subject { get; set; }
        public string Comment { get; set; }
        public string Username { get; set; }
        public DateTime PostedOn { get; set; }
        public string Email { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public bool Notify { get; set; }
        public string ParentType { get; set; }
        public CommentEntry() : base(string.Empty, Guid.NewGuid().ToString()) { }
    }
}

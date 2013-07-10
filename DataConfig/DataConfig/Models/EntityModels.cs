using Microsoft.WindowsAzure.Storage.Table;

namespace DataConfig.Models
{
    public class AvailableEndpoint : TableEntity
    {
        public AvailableEndpoint()
        { }

        public AvailableEndpoint(string partitionKey, string rowKey)
        {
            this.PartitionKey = partitionKey;
            this.RowKey = rowKey;
        }

        public string alias { get; set; }
        public string description { get; set; }
        public string disclaimer { get; set; }
        public string storageaccountname { get; set; }
        public string storageaccountkey { get; set; }
    }

    public class EntityMetadata : TableEntity
    {
        public EntityMetadata()
        { }

        public EntityMetadata(string partitionKey, string rowKey)
        {
            this.PartitionKey = partitionKey;
            this.RowKey = rowKey;
        }

        public string entityid { get; set; }
        public string entityset { get; set; }
        public string entitykind { get; set; }
    }
}
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Linq;

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

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(alias) && !string.IsNullOrEmpty(storageaccountname) && !string.IsNullOrEmpty(storageaccountkey);
        }

        public string alias { get; set; }
        public string description { get; set; }
        public string disclaimer { get; set; }
        public string storageaccountname { get; set; }
        public string storageaccountkey { get; set; }
    }

    public class TableMetadata : TableEntity
    {
        public TableMetadata()
        { }

        public TableMetadata(string partitionKey, string rowKey)
        {
            this.PartitionKey = partitionKey;
            this.RowKey = rowKey;
        }

        public IEnumerable<string> KeywordsList
        {
            get
            {
                if (!string.IsNullOrEmpty(this.keywords))
                {
                    return (from k in this.keywords.Split(',') select k.ToLower().Trim());
                }

                return null;
            }
        }

        public string entityset { get; set; }
        public string name { get; set; }
        public string source { get; set; }
        public string category { get; set; }
        public string keywords { get; set; }
        public string entitykind { get; set; }
    }

    public class TableColumnsMetadata : TableEntity
    {
        public TableColumnsMetadata()
        { }

        public TableColumnsMetadata(string partitionKey, string rowKey)
        {
            this.PartitionKey = partitionKey;
            this.RowKey = rowKey;
        }

        public string entityset { get; set; }
        public string column { get; set; }
        public string columnsemantic { get; set; }
        public string columnnamespace { get; set; }
        public string columndescription { get; set; }
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

        public string entityset { get; set; }
        public string entitykind { get; set; }
    }

    public class ProcessorParams : TableEntity
    {
        public ProcessorParams()
        { }

        public ProcessorParams(string partitionKey, string rowKey)
        {
            this.PartitionKey = partitionKey;
            this.RowKey = rowKey;
        }

        public string entityset { get; set; }
        public string entitykind { get; set; }
    }
}
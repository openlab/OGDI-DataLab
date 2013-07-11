using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace DataConfig.Helper
{
    public class Azure
    {
        public const string ConnectionString = "DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}";

        public class Table
        {
            public const string AvailableEndpoints = "AvailableEndpoints";
            public const string EntityMetadata = "EntityMetadata";
            public const string ProcessorParams = "ProcessorParams";
            public const string TableColumnsMetadata = "TableColumnsMetadata";
            public const string TableMetadata = "TableMetadata";
        }

        static public CloudTable GetCloudTable(string storageName, string storageKey, string tableName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(string.Format(ConnectionString, storageName, storageKey));
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            return tableClient.GetTableReference(tableName);
        }
    }
}
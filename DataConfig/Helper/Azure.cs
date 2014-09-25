using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace DataConfig.Helper
{
    public class Azure
    {
        private const string ConnectionString = "DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}";

        public class Table
        {
            public const string AvailableEndpoints = "AvailableEndpoints";
            public const string EntityMetadata = "EntityMetadata";
            public const string ProcessorParams = "ProcessorParams";
            public const string TableColumnsMetadata = "TableColumnsMetadata";
            public const string TableMetadata = "TableMetadata";
        }

        static public bool TestConnection(string storageName, string storageKey)
        {
            try
            {
                CloudTable availableEndpoints = Azure.GetCloudTable(storageName, storageKey, Azure.Table.AvailableEndpoints);
                availableEndpoints.Exists();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        static public CloudTable GetCloudTable(string storageName, string storageKey, string tableName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(string.Format(ConnectionString, storageName, storageKey));
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            return tableClient.GetTableReference(tableName);
        }
    }
}
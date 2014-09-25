namespace DataConfig.Models
{
    public class AddCatalog
    {
        public string ConfigStorageName { get; set; }
        public string ConfigStorageKey { get; set; }
        public string Alias { get; set; }
        public string Description { get; set; }
        public string Disclaimer { get; set; }
        public string DataStorageName { get; set; }
        public string DataStorageKey { get; set; }
    }

    public class DeleteCatalog
    {
        public string ConfigStorageName { get; set; }
        public string ConfigStorageKey { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
    }

    public class DeleteDataset
    {
        public string Catalog { get; set; }
        public string DataStorageName { get; set; }
        public string DataStorageKey { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
    }
}
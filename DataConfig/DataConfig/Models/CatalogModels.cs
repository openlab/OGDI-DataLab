namespace DataConfig.Models
{
    public class LoadCatalog
    {
        public string ConfigStorageName { get; set; }
        public string ConfigStorageKey { get; set; }
    }

    public class AddCatalog
    {
        public string ConfigStorageName { get; set; }
        public string ConfigStorageKey { get; set; }
        public string Alias { get; set; }
        public string Description { get; set; }
        public string Disclaimer { get; set; }
        public string StorageName { get; set; }
        public string StorageKey { get; set; }
    }

    public class DeleteCatalog
    {
        public string ConfigStorageName { get; set; }
        public string ConfigStorageKey { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
    }
}
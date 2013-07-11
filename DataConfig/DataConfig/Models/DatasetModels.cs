namespace DataConfig.Models
{
    public class LoadDataset
    {
        public string StorageName { get; set; }
        public string StorageKey { get; set; }
    }

    public class DeleteDataset
    {
        public string StorageName { get; set; }
        public string StorageKey { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
    }
}
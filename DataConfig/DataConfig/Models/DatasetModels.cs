namespace DataConfig.Models
{
    public class DeleteDataset
    {
        public string DataStorageName { get; set; }
        public string DataStorageKey { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
    }
}
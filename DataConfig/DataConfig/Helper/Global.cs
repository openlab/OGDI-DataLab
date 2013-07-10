namespace DataConfig.Helper
{
    public class Global
    {
        public class Table
        {
            public const string AvailableEndpoints = "AvailableEndpoints";
            public const string EntityMetadata = "EntityMetadata";
            public const string ProcessorParams = "ProcessorParams";
            public const string TableColumnsMetadata = "TableColumnsMetadata";
            public const string TableMetadata = "TableMetadata";
        }

        public const string ConnectionString = "DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}";
    }
}

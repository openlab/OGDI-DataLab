using System.Configuration;

namespace Ogdi.Data.DataLoader
{
    public abstract class DataLoaderParams
    {
        public EntityProducerParams ProducerParams { get; set; }

        public TableProcessorParams ProcessorParams { get; set; }

        public TableMetadataEntity TableMetadataEntity { get; set; }

        public static int ConcurrentSetsCount
        {
            get { return int.Parse(ConfigurationManager.AppSettings["ConcurrentSetsCount"] ?? "40"); }
        }

        public virtual void Save(string fileName)
        {
        }

        public virtual void Validate(string fileName)
        {
        }
    }
}
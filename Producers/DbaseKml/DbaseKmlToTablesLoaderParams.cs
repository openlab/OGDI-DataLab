using System;
using System.Xml.Serialization;

namespace Ogdi.Data.DataLoader.DbaseKml
{
    [XmlRoot(ElementName = "DbaseKmlDataLoaderParams")]
    public class DbaseKmlToTablesDataLoaderParams : DbaseKmlDataLoaderParams
    {
        public static DbaseKmlToTablesDataLoaderParams FromFile(string fileName)
        {
            throw new NotImplementedException();
        }

        private static DbaseKmlToTablesDataLoaderParams CreateEmptyData()
        {
            var data = new DbaseKmlToTablesDataLoaderParams
                           {
                               TableMetadataEntity = new TableMetadataEntity(),
                               ProducerParams = new DbaseKmlEntityProducerParams { KmlElementsToStrip = new string[0] },
                               ProcessorParams = new TableProcessorParams
                                                     {
                                                         PartitionKeyPropertyName = "New.Guid",
                                                         RowKeyPropertyName = "New.Guid",
                                                         TableMetadataPartitionKeyPropertyName = "Name",
                                                         TableMetadataRowKeyPropertyName = "New.Guid",
                                                         EntityMetadataPartitionKeyPropertyName = "EntitySet",
                                                         EntityMetadataRowKeyPropertyName = "EntityKind"
                                                     }
                           };

            return data;
        }
    }
}

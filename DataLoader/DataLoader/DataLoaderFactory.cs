using System;
using System.IO;
using System.Xml.Serialization;
using Ogdi.Data.DataLoader.Csv;
using Ogdi.Data.DataLoader.DbaseKml;
using Ogdi.Data.DataLoader.Kml;

namespace Ogdi.Data.DataLoader
{
    public enum SourceDataType
    {
        DbfAndKml,
        Csv,
        Kml
    }

    public enum DataLoadingTarget
    {
        Console,
        Tables
    }

    //What to do if data set is already in storage (only for TableDataLoader and TableEntityProcessor)
    public enum TableOverwriteMode
    {
        Create, //wipe and reload
        Add, //add to existing data set (must exist, must have identical metadata except for last updated date which should be newer and be updated), raise exception when inserting entity with the same rawID
        Update //update or add data to existing data set (conditions  should be the same as in add, update entity if exist, add if not)
    }

    public static class DataLoaderFactory
    {
        public static IDataLoader CreateDataLoader(SourceDataType type, DataLoadingTarget target, string fileSetName, TableOverwriteMode overwriteMode, bool sourceOrder)
        {
            string dir = Directory.GetCurrentDirectory();
            string file = string.Concat(fileSetName, DataLoaderConstants.FileExtConfig);

            using (var stream = File.Open(Path.Combine(dir, file), FileMode.Open, FileAccess.Read))
                switch (type)
                {
                    case SourceDataType.DbfAndKml:
                        return CreateLoader<DbaseKmlToTablesDataLoaderParams, DbaseKmlEntityProducer>(fileSetName, target, stream, sourceOrder, overwriteMode);

                    case SourceDataType.Csv:
                        return CreateLoader<CsvToTablesDataLoaderParams, CsvEntityProducer>(fileSetName, target, stream, sourceOrder, overwriteMode);

                    case SourceDataType.Kml:
                        return CreateLoader<KmlToTablesDataLoaderParams, KmlEntityProducer>(fileSetName, target, stream, sourceOrder, overwriteMode);
                }
            throw new NotSupportedException(type.ToString());
        }

        /// <summary>
        /// Creates either a Console or WPF Data Loader
        /// </summary>
        /// <typeparam name="DLParams">Specific Data Loader Create</typeparam>
        /// <typeparam name="Producer">Specific Entity to Produce</typeparam>
        /// <param name="fileSetName"></param>
        /// <param name="target"></param>
        /// <param name="stream"></param>
        /// <param name="sourceOrder"></param>
        /// <param name="overwriteMode"></param>
        /// <returns></returns>
        private static IDataLoader CreateLoader<DLParams, Producer>(string fileSetName, DataLoadingTarget target, FileStream stream, bool sourceOrder, TableOverwriteMode overwriteMode)
            where DLParams : DataLoaderParams
            where Producer : EntityProducer
        {
            switch (target)
            {
                case DataLoadingTarget.Console:
                    return CreateConsoleLoader<DLParams, Producer>(stream, fileSetName, sourceOrder);
                case DataLoadingTarget.Tables:
                    return CreateTablesLoader<DLParams, Producer>(stream, fileSetName, sourceOrder, overwriteMode);
            }
            throw new NotSupportedException(target.ToString());
        }

        private static IDataLoader CreateTablesLoader<DLParams, Producer>(FileStream stream, string fileSetName, bool sourceOrder, TableOverwriteMode overwriteMode)
            where DLParams : DataLoaderParams
            where Producer : EntityProducer
        {
            var serializer = new XmlSerializer(typeof(DLParams));
            var p = (DLParams)serializer.Deserialize(stream);
            p.TableMetadataEntity.EntityKind = p.TableMetadataEntity.EntitySet + "Item";

            if (p.ProducerParams.PlacemarkParams != null)
            {
                p.TableMetadataEntity.KML = true;
            }

            if (p.ProcessorParams != null)
            {
                p.ProcessorParams.EntityKind = p.TableMetadataEntity.EntityKind;
                p.ProcessorParams.EntitySet = p.TableMetadataEntity.EntitySet;
            }

            if (sourceOrder && p.ProcessorParams != null && !String.IsNullOrEmpty(p.ProcessorParams.PartitionKeyPropertyName)
                    && p.ProcessorParams.PartitionKeyPropertyName.ToLower() != DataLoaderConstants.ValueUniqueAutoGen)
                throw new ApplicationException("PartitionKey must be empty or equal to 'New.Guid' in .cfg when use sourceOrder");

            if (overwriteMode == TableOverwriteMode.Update && p.ProcessorParams != null
                && (String.IsNullOrEmpty(p.ProcessorParams.PartitionKeyPropertyName)
                    || p.ProcessorParams.PartitionKeyPropertyName.ToLower() == DataLoaderConstants.ValueUniqueAutoGen
                    || String.IsNullOrEmpty(p.ProcessorParams.PartitionKeyPropertyName)
                    || p.ProcessorParams.PartitionKeyPropertyName.ToLower() == DataLoaderConstants.ValueUniqueAutoGen))
                throw new ApplicationException("PartitionKey and RowKey must be provided in .cfg for /mode=update");

            EntityProducer producer = null;

            if (!p.TableMetadataEntity.IsEmpty)
                producer = (Producer)Activator.CreateInstance(typeof(Producer),
                    fileSetName,
                    p.TableMetadataEntity.EntitySet,
                    p.TableMetadataEntity.EntityKind,
                    p.ProducerParams,
                    sourceOrder);

            var processor = new TableEntityProcessor(p.ProcessorParams, overwriteMode);
            return new TableDataLoader(p, producer, processor, overwriteMode);
        }

        private static IDataLoader CreateConsoleLoader<DLParams, Producer>(FileStream stream, string fileSetName, bool sourceOrder)
            where DLParams : DataLoaderParams
            where Producer : EntityProducer
        {
            var serializer = new XmlSerializer(typeof(DLParams));
            var p = (DLParams)serializer.Deserialize(stream);
            p.TableMetadataEntity.EntityKind = p.TableMetadataEntity.EntitySet + "Item";
            EntityProducer producer = null;
            if (!p.TableMetadataEntity.IsEmpty)
                producer = (Producer)Activator.CreateInstance(typeof(Producer),
                    fileSetName,
                    p.TableMetadataEntity.EntitySet,
                    p.TableMetadataEntity.EntityKind,
                    p.ProducerParams,
                    sourceOrder);
            var processor = new CoutEntityProcessor();
            var loader = new DataLoader(p, producer, processor);
            return loader;
        }
    }
}

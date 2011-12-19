using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using LumenWorks.Framework.IO.Csv;

namespace Ogdi.Data.DataLoader.Csv
{
    [XmlRoot(ElementName = "CsvDataLoaderParams")]
    public class CsvToTablesDataLoaderParams : DataLoaderParams
    {
        public static CsvToTablesDataLoaderParams FromFile(UploadParam uploadParam)
        {
            string configFileName = string.Empty;
            string csvFileName = string.Empty;
            CsvToTablesDataLoaderParams parameters;

            if (!string.IsNullOrEmpty(uploadParam.Directory) && !string.IsNullOrEmpty(uploadParam.Name))
            {
                configFileName = Path.Combine(uploadParam.Directory, string.Concat(uploadParam.Name, DataLoaderConstants.FileExtConfig));
                csvFileName = Path.Combine(uploadParam.Directory, string.Concat(uploadParam.Name, DataLoaderConstants.FileExtCsv));
            }

            //Check if (*.cfg)  file exists
            if (!string.IsNullOrEmpty(configFileName) && File.Exists(configFileName))
            {
                CsvToTablesDataLoaderParams data;

                using (var stream = File.Open(configFileName, FileMode.Open, FileAccess.Read))
                {
                    data = SerializationHelper.DeserializeFromFile<CsvToTablesDataLoaderParams>(stream);
                }
                if (data.TableMetadataEntity.ReleasedDate.Year < 2008)
                    data.TableMetadataEntity.ReleasedDate = DateTime.Now;

                if (data.TableMetadataEntity.ExpiredDate.Year < 2008)
                    data.TableMetadataEntity.ExpiredDate = DateTime.Now.AddDays(1);

                parameters = data;
            }
            else
            {
                //if there's no (*.cfg), check if (*.csv) file exists
                parameters = File.Exists(csvFileName)
                    ? CreateEmptyData(csvFileName)
                    : new CsvToTablesDataLoaderParams
                          {
                              TableMetadataEntity = new TableMetadataEntity { IsEmpty = true, ReleasedDate = DateTime.Now, ExpiredDate = DateTime.Now.AddDays(1), LastUpdateDate = DateTime.Now },
                              ProcessorParams = new TableProcessorParams
                                                    {
                                                        PartitionKeyPropertyName = string.Empty,
                                                        RowKeyPropertyName = string.Empty,
                                                        TableMetadataPartitionKeyPropertyName = string.Empty,
                                                        TableMetadataRowKeyPropertyName = string.Empty,
                                                        EntityMetadataPartitionKeyPropertyName = string.Empty,
                                                        EntityMetadataRowKeyPropertyName = string.Empty,
                                                        SourceTimeZoneName = string.Empty
                                                    }
                          };
            }
            return parameters;
        }

        private static CsvToTablesDataLoaderParams CreateEmptyData(string fileName)
        {
            var name = Path.GetFileNameWithoutExtension(fileName);

            // Create all insctances
            var dataLoaderParams = new CsvToTablesDataLoaderParams();

            // Generate TableMetadataEntity properties
            dataLoaderParams.TableMetadataEntity = new TableMetadataEntity
            {
                EntitySet = name,
                Name = name,
                Source = name,
                Category = name,
                ReleasedDate = DateTime.Now,
                ExpiredDate = DateTime.Now.AddDays(1),
                LastUpdateDate = DateTime.Now
            };

            dataLoaderParams.ProducerParams = new EntityProducerParams
            {
                PlacemarkParams = new PlacemarkParams(),
                PropertyToTypeMap = new PropertyToTypeMapper()
            };

            // Generate ProcessorParams properties
            dataLoaderParams.ProcessorParams = new TableProcessorParams
            {
                PartitionKeyPropertyName = "New.Guid",
                RowKeyPropertyName = "New.Guid",
                TableMetadataPartitionKeyPropertyName = "Name",
                TableMetadataRowKeyPropertyName = "New.Guid",
                EntityMetadataPartitionKeyPropertyName = "EntitySet",
                EntityMetadataRowKeyPropertyName = "EntityKind"
            };

            using (var reader = new CsvReader(new StreamReader(fileName), true))
            {
                foreach (string header in reader.GetFieldHeaders())
                {
                    if (header.Trim().ToLower() == "name")
                    {
                        dataLoaderParams.ProducerParams.PlacemarkParams.NameProperties = new[] { "name" };
                        dataLoaderParams.ProducerParams.PlacemarkParams.NamePropertyFormatString = "{0}";
                    }

                    if (header.Trim().ToLower() == "id")
                    {
                        dataLoaderParams.ProcessorParams.RowKeyPropertyName = "id";
                    }

                    if (header.Trim().ToLower().StartsWith("lat"))
                    {
                        dataLoaderParams.ProducerParams.PlacemarkParams.LatitudeProperty = header.Trim();
                    }

                    if (header.Trim().ToLower().StartsWith("lon"))
                    {
                        dataLoaderParams.ProducerParams.PlacemarkParams.LongitudeProperty = header.Trim();
                    }

                    dataLoaderParams.ProducerParams.PropertyToTypeMap.Add(header.Trim(), "string");
                }
            }
            return dataLoaderParams;
        }

        public override void Validate(string fileName)
        {
            base.Validate(fileName);

            if (TableMetadataEntity.IsEmpty)
                return;

            //Create loader to verify params
            var sb = new StringBuilder();
            var missedFields = new List<string>();

            string directory = Path.GetDirectoryName(fileName);
            string name = Path.GetFileNameWithoutExtension(fileName);
            string csvFile = Path.Combine(directory, name + ".csv");

            using (var reader = new CsvReader(new StreamReader(csvFile), true))
            {
                string[] headers = reader.GetFieldHeaders();

                missedFields.AddRange(headers.Where(header => ProducerParams.PropertyToTypeMap.GetProperties().Where(r => r.Key == header).Count() == 0));
            }

            if (missedFields.Count != 0)
            {
                sb.AppendFormat("\r\n\r\nWarning - Missed csv fields:");
                foreach (string missedField in missedFields)
                {
                    sb.AppendFormat("\r\n   {0}", missedField);
                }
            }

            AddEmptyFieldsInfo(TableMetadataEntity, sb, "Metadata");
            AddEmptyFieldsInfo(ProcessorParams, sb, "Processor Params");

            if (sb.Length == 0)
                return;

            sb.Insert(0, "There is some warnings in the metadata!");
            sb.Append("\r\n\r\n Are you want to continue saving ?");

            throw new WarningException(sb.ToString());
        }

        public override void Save(string fileName)
        {
            base.Save(fileName);

            if (ProducerParams != null && ProducerParams.PlacemarkParams != null)
            {
                var names = ProducerParams.PropertyToTypeMap.Mappings
                    .Where(x => !string.IsNullOrEmpty(x.IndexInName))
                    .OrderBy(x => x.IndexInName)
                    .Select(x => x.Name).ToArray();

                ProducerParams.PlacemarkParams.NameProperties = names;
            }
            SerializationHelper.SerializeToFile(fileName, this);
        }

        private static void AddEmptyFieldsInfo(object obj, StringBuilder sb, string tabName)
        {
            var emptyRequiredFields = ValidateFieldAttribute.GetFieldList(FieldType.Required, obj);
            var emptyOptinalFields = ValidateFieldAttribute.GetFieldList(FieldType.Optinal, obj);

            if (emptyRequiredFields.Count > 0)
            {
                sb.AppendFormat("\r\n\r\nWarning - Empty required fields ({0}):", tabName);
                foreach (string field in emptyRequiredFields)
                {
                    sb.AppendFormat("\r\n   {0}", field);
                }
            }

            if (emptyOptinalFields.Count > 0)
            {
                sb.AppendFormat("\r\n\r\nWarning - Empty optional fields ({0}):", tabName);
                foreach (string field in emptyOptinalFields)
                {
                    sb.AppendFormat("\r\n   {0}", field);
                }
            }
        }
    }
}

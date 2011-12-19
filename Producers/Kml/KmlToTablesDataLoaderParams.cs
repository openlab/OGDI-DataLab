using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Ogdi.Data.DataLoader.Kml
{
    [XmlRoot(ElementName = "KmlDataLoaderParams")]
    public class KmlToTablesDataLoaderParams : DataLoaderParams
    {
        public static KmlToTablesDataLoaderParams FromFile(UploadParam uploadParam)
        {
            string configFileName = string.Empty;
            string kmlFileName = string.Empty;
            KmlToTablesDataLoaderParams parameters;

            if (!string.IsNullOrEmpty(uploadParam.Directory) && !string.IsNullOrEmpty(uploadParam.Name))
            {
                configFileName = Path.Combine(uploadParam.Directory, string.Concat(uploadParam.Name, DataLoaderConstants.FileExtConfig));
                kmlFileName = Path.Combine(uploadParam.Directory, string.Concat(uploadParam.Name, DataLoaderConstants.FileExtKml));
            }

            //Check if (*.cfg) file exists
            if (!string.IsNullOrEmpty(configFileName) && File.Exists(configFileName))
            {
                KmlToTablesDataLoaderParams data;

                using (var stream = File.Open(configFileName, FileMode.Open, FileAccess.Read))
                {
                    data = SerializationHelper.DeserializeFromFile<KmlToTablesDataLoaderParams>(stream);
                }
                if (data.TableMetadataEntity.ReleasedDate.Year < 2008)
                    data.TableMetadataEntity.ReleasedDate = DateTime.Now;

                if (data.TableMetadataEntity.ExpiredDate.Year < 2008)
                    data.TableMetadataEntity.ExpiredDate = DateTime.Now.AddDays(1);

                parameters = data;
            }
            else
            {
                parameters = File.Exists(kmlFileName)
                                 ? CreateEmptyData(kmlFileName)
                                 : new KmlToTablesDataLoaderParams
                                       {
                                           TableMetadataEntity =
                                               new TableMetadataEntity
                                                   {
                                                       IsEmpty = true,
                                                       ReleasedDate = DateTime.Now,
                                                       ExpiredDate = DateTime.Now.AddDays(1),
                                                       LastUpdateDate = DateTime.Now
                                                   },
                                           ProcessorParams =
                                                new TableProcessorParams
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

        private static KmlToTablesDataLoaderParams CreateEmptyData(string fileName)
        {
            string name = Path.GetFileNameWithoutExtension(fileName);

            var dataLoaderParams = new KmlToTablesDataLoaderParams();

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

            dataLoaderParams.ProcessorParams = new TableProcessorParams
                                                   {
                                                       PartitionKeyPropertyName = DataLoaderConstants.ValueUniqueAutoGenInitCaps,
                                                       RowKeyPropertyName = DataLoaderConstants.ValueUniqueAutoGenInitCaps,
                                                       TableMetadataPartitionKeyPropertyName = "Name",
                                                       TableMetadataRowKeyPropertyName = DataLoaderConstants.ValueUniqueAutoGenInitCaps,
                                                       EntityMetadataPartitionKeyPropertyName = DataLoaderConstants.PropNameEntitySet,
                                                       EntityMetadataRowKeyPropertyName = DataLoaderConstants.PropNameEntityKind
                                                   };


            using (var kmlFile = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                var kmlDocument = XDocument.Load(kmlFile);

                if (kmlDocument != null)
                {
                    var kmlNamespace = kmlDocument.Root.Attributes("xmlns").First().Value;
                    PropertyToTypeMapper mapper = dataLoaderParams.ProducerParams.PropertyToTypeMap;

                    foreach (var item in kmlDocument.Descendants(DataLoaderConstants.ElemNamePlacemark))
                    {
                        mapper.Add(DataLoaderConstants.ElemNameName, item.Element(DataLoaderConstants.ElemNameName).Value);
                        mapper.Add(DataLoaderConstants.ElemNameDescription, item.Element(DataLoaderConstants.ElemNameDescription).Value);

                        mapper.Add("Boundary", item.Element());



                    }

                }
            }

            return dataLoaderParams;
        }
    }
}
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
                                                    },
                                                    TableColumnsMetadata = new TableColumnsMetadata
                                                    {
                                                        TableColumnsMetadataPartitionKeyPropertyName = string.Empty,
                                                        TableColumnsMetadataRowKeyPropertyName = string.Empty
                                                    }
                                       };
            }
            return parameters;
        }

        private static KmlToTablesDataLoaderParams CreateEmptyData(string fileName)
        {
            var name = Path.GetFileNameWithoutExtension(fileName);

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
                                                        TableColumnsMetadataPartitionKeyPropertyName = DataLoaderConstants.PropNameEntitySet,
                                                        TableColumnsMetadataRowKeyPropertyName = "Column",
                                                        EntityMetadataPartitionKeyPropertyName = DataLoaderConstants.PropNameEntitySet,
                                                        EntityMetadataRowKeyPropertyName = DataLoaderConstants.PropNameEntityKind
                                                    };


            dataLoaderParams.TableColumnsMetadata = new TableColumnsMetadata
            {
                PropertyToTypeColumnsMetadata = new PropertyToTypeColumnsMetadataMapper()
            };

            using (var kmlFile = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                const string mapToString = "string";
                const string mapToDouble = "double";
                const string nameAttribute = "name";

                var kmlDocument = XDocument.Load(kmlFile);

                if (kmlDocument != null)
                {
                    XNamespace kmlNamespace = kmlDocument.Root.Attributes("xmlns").First().Value;
                    PropertyToTypeMapper propertyTypeMapper = dataLoaderParams.ProducerParams.PropertyToTypeMap;

                    var firstPlacemarkInKmlFile =
                        kmlDocument.Descendants(kmlNamespace + DataLoaderConstants.ElemNamePlacemark).First();

                    if (!firstPlacemarkInKmlFile.IsEmpty)
                    {
                        propertyTypeMapper.Add(DataLoaderConstants.ElemNameName, mapToString);
                        propertyTypeMapper.Add(DataLoaderConstants.ElemNameDescription, mapToString);

                        #region RDF

                        string defaultDescription = DefaultDescription();
                        string rdfNamespace = "ogdi=\"ogdiUrl\"";

                        dataLoaderParams.TableColumnsMetadata.PropertyToTypeColumnsMetadata.Add(DataLoaderConstants.ElemNameName.Trim(), string.Empty, FormattedDescription(defaultDescription, DataLoaderConstants.ElemNameName), rdfNamespace);
                        dataLoaderParams.TableColumnsMetadata.PropertyToTypeColumnsMetadata.Add(DataLoaderConstants.ElemNameDescription.Trim(), string.Empty, FormattedDescription(defaultDescription, DataLoaderConstants.ElemNameDescription), rdfNamespace);
                        
                        #endregion

                        if (firstPlacemarkInKmlFile.Element(kmlNamespace + DataLoaderConstants.ElemNamePoint) != null)
                        {
                            propertyTypeMapper.Add(DataLoaderConstants.PropNameLongitude, mapToDouble);
                            propertyTypeMapper.Add(DataLoaderConstants.PropNameLatitude, mapToDouble);
                            dataLoaderParams.ProducerParams.PlacemarkParams.LongitudeProperty = DataLoaderConstants.PropNameLongitude;
                            dataLoaderParams.ProducerParams.PlacemarkParams.LatitudeProperty = DataLoaderConstants.PropNameLatitude;

                            #region RDF

                            dataLoaderParams.TableColumnsMetadata.PropertyToTypeColumnsMetadata.Add(DataLoaderConstants.PropNameLongitude.Trim(), string.Empty, FormattedDescription(defaultDescription, DataLoaderConstants.PropNameLongitude), rdfNamespace);
                            dataLoaderParams.TableColumnsMetadata.PropertyToTypeColumnsMetadata.Add(DataLoaderConstants.PropNameLatitude.Trim(), string.Empty, FormattedDescription(defaultDescription, DataLoaderConstants.PropNameLatitude), rdfNamespace);

                            #endregion
                        }
                        else if (firstPlacemarkInKmlFile.Element(kmlNamespace + DataLoaderConstants.ElemNamePolygon) != null)
                        {
                            propertyTypeMapper.Add(DataLoaderConstants.PropNameKmlCoords, mapToString);

                            #region RDF

                            dataLoaderParams.TableColumnsMetadata.PropertyToTypeColumnsMetadata.Add(DataLoaderConstants.PropNameKmlCoords.Trim(), string.Empty, FormattedDescription(defaultDescription, DataLoaderConstants.PropNameKmlCoords), rdfNamespace);

                            #endregion
                        }

                        foreach (var simpleData in firstPlacemarkInKmlFile.Descendants(kmlNamespace + DataLoaderConstants.ElemNameSimpleData))
                        {
                            if (simpleData.Attribute(nameAttribute) == null) continue;
                            string val = string.Concat("sd0", simpleData.Attribute(nameAttribute).Value);
                            propertyTypeMapper.Add(val, mapToString);

                            #region RDF

                            dataLoaderParams.TableColumnsMetadata.PropertyToTypeColumnsMetadata.Add(val.Trim(), string.Empty, FormattedDescription(defaultDescription, val), rdfNamespace);

                            #endregion
                        }
                    }
                }
            }

            return dataLoaderParams;
        }

        private static string DefaultDescription()
        {
            XDocument xmlDoc = XDocument.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @".\RdfNamespaces.xml"));

            foreach (XElement desc in xmlDoc.Descendants("metadata"))
            {
                return desc.Element("description").Value.ToString();
            }

            return string.Empty;
        }

        private static string FormattedDescription(string defaultDescription, string parameter)
        {
            return string.Format(defaultDescription, parameter, DataLoaderConstants.ElemNamePlacemark);
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
    }
}
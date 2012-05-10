using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Ogdi.Data.DataLoader.Kml
{
    public class KmlEntityProducer : EntityProducer
    {
        private XElement _kml;
        private readonly Entity _schemaEntity;
        private readonly EntityProducerParams _params;
        private readonly bool _sourceOrder;

        public KmlEntityProducer(string fileSetName, string entitySet, string entityKind, EntityProducerParams parameters, bool sourceOrder)
        {
            _kml = GetKml(fileSetName);
            _params = parameters;
            _schemaEntity = GetSchemaEntity(entitySet, entityKind, parameters.PropertyToTypeMap);
            _sourceOrder = sourceOrder;
        }

        private static Entity GetSchemaEntity(string entitySet, string entityKind, PropertyToTypeMapper mapper)
        {
            var entity = new Entity();
            entity.AddProperty(DataLoaderConstants.PropNameEntitySet, entitySet);
            entity.AddProperty(DataLoaderConstants.PropNameEntityKind, entityKind);

            foreach (var p in mapper.GetProperties())
            {
                entity.AddProperty(p.Key, GetPropertyType(p.Value));
            }

            return entity;
        }

        private static XElement GetKml(string fileSetName)
        {
            string dir = Directory.GetCurrentDirectory();
            var file = string.Concat(fileSetName, DataLoaderConstants.FileExtKml);
            var reader = XmlReader.Create(Path.Combine(dir, file));
            return XElement.Load(reader);
        }

        public override Entity SchemaEntity
        {
            get { return _schemaEntity; }
        }

        public override IEnumerable<Entity> GetEntitiesEnumerator(OnContinueExceptionCallback exceptionNotifier, DataLoaderParams Params)
        {
            int count = 0;
            string initialTimePrefix = GetSecondsFrom2000Prefix();
            var properties = _params.PropertyToTypeMap.GetProperties().ToDictionary(c => c.Key.ToLower(), c => c.Value);
            bool isExceptionOccurred = false;
            var kmlNamespace = XNamespace.Get(DataLoaderConstants.NsKmlNew);
            string[] separators = new string[] { ",", " ", Environment.NewLine };

            List<XElement> placemarks =
                _kml.Descendants(kmlNamespace + DataLoaderConstants.ElemNamePlacemark).ToList();

            #region RDF
            string entitySet = SchemaEntity["EntitySet"].ToString();

            // rdf get the columns namespaces
            List<TableColumnsMetadataEntity> columnMetadata = TableDataLoader.GetRdfMetadataColumnNamespace(entitySet);
            List<string> namespacesRdf = new List<string>();
            List<string> validNamespaces = new List<string>();

            string namespaceToAdd = string.Empty;
            if (columnMetadata != null)
            {
                foreach (TableColumnsMetadataEntity columnMeta in columnMetadata)
                {
                    if (!string.IsNullOrEmpty(columnMeta.columnnamespace))
                    {
                        namespaceToAdd = columnMeta.columnnamespace.Split('=')[0] + '=';

                        if (namespaceToAdd.Split('=')[0] != string.Empty)
                        {
                            if (!validNamespaces.Contains(namespaceToAdd))
                            {
                                namespacesRdf.Add(columnMeta.columnnamespace);
                                validNamespaces.Add(namespaceToAdd);
                            }
                        }
                    }
                }
            }
            #endregion

            for (var i = 0; i < placemarks.Count; i++)
            {
                var entity = new Entity();
                isExceptionOccurred = false;

                #region RDF

                XNamespace rdfNamespace = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";

                XElement rdfXml = new XElement(rdfNamespace + "RDF",
                new XAttribute(XNamespace.Xmlns + "rdf", rdfNamespace.ToString()));

                // add new namespaces to the rdf snippet if they exist
                if (namespacesRdf != null)
                {
                    foreach (string ns in namespacesRdf)
                    {
                        if (!string.IsNullOrEmpty(ns))
                        {
                            rdfXml.Add(new XAttribute(XNamespace.Xmlns + ns.ToString().Split('=')[0], ns.ToString().Split('=')[1]));
                        }
                    }
                }

                XElement rdfXmlDescriptionElement = new XElement(rdfNamespace + "Description");
                rdfXml.Add(rdfXmlDescriptionElement);
                #endregion

                try
                {
                    XElement placemark = placemarks[i];

                    if (placemark == null)
                        continue;

                    foreach (var item in properties)
                    {
                        #region RDF

                        var header = item.Key;
                        var stringValue = item.Value;
                        var rdfValue = String.Empty;

                        if (header == Params.ProcessorParams.PartitionKeyPropertyName)
                        {
                            rdfXmlDescriptionElement.Add(new XAttribute(rdfNamespace + "about", stringValue));
                        }

                        var datatype = GetRdfType(stringValue);
                        var cleanHeader = CleanStringLower(header);

                        var columnNs = columnMetadata.First(column => column.column == header);
                        XNamespace customNS = columnNs.columnnamespace.ToString().Split('=')[1];

                        #endregion

                        // Name & Description of Placemark
                        if (item.Key == DataLoaderConstants.ElemNameName || item.Key == DataLoaderConstants.ElemNameDescription)
                        {
                            entity.AddProperty(item.Key,
                                               placemark.Element(kmlNamespace + item.Key).Value);

                            #region RDF
                            rdfValue = placemark.Element(kmlNamespace + header).Value;
                            #endregion
                        }

                        if (item.Key == DataLoaderConstants.PropNameKmlCoords
                            || (item.Key == DataLoaderConstants.PropNameLatitude && properties.ContainsKey(DataLoaderConstants.PropNameLongitude)))
                        {
                            if (placemark.Element(kmlNamespace + DataLoaderConstants.ElemNamePoint) != null)
                            {
                                var point = placemark.Element(kmlNamespace + DataLoaderConstants.ElemNamePoint);
                                var positionTuple =
                                    point.Element(kmlNamespace + DataLoaderConstants.ElemNameCoordinates).Value.Split(',');

                                entity.AddProperty(DataLoaderConstants.PropNameLatitude, ExtractLatitude(positionTuple));
                                entity.AddProperty("longitude", ExtractLongitude(positionTuple));
                                entity.AddProperty("altitude", ExtractAltitude(positionTuple));
                                //entity.AddProperty(DataLoaderConstants.PropNameKmlSnippet,
                                //                   string.Concat("<Placemark>", point.ToString(SaveOptions.DisableFormatting), "</Placemark>"));

                                #region RDF

                                if (stringValue != string.Empty)
                                {
                                    rdfXmlDescriptionElement.Add(new XElement(customNS + "latitude", ExtractLatitude(positionTuple), new XAttribute(rdfNamespace + "datatype", datatype)));
                                    rdfXmlDescriptionElement.Add(new XElement(customNS + "longitude", ExtractLongitude(positionTuple), new XAttribute(rdfNamespace + "datatype", datatype)));
                                    if (!string.IsNullOrEmpty(ExtractAltitude(positionTuple)))
                                        rdfXmlDescriptionElement.Add(new XElement(customNS + "altitude", ExtractAltitude(positionTuple), new XAttribute(rdfNamespace + "datatype", datatype)));
                                }
                                else
                                {
                                    rdfXmlDescriptionElement.Add(new XElement(customNS + "latitude"));
                                    rdfXmlDescriptionElement.Add(new XElement(customNS + "longitude"));
                                    if (!string.IsNullOrEmpty(ExtractAltitude(positionTuple)))
                                        rdfXmlDescriptionElement.Add(new XElement(customNS + "altitude"));
                                }

                                #endregion
                            }
                            else if (placemark.Element(kmlNamespace + DataLoaderConstants.ElemNamePolygon) != null)
                            {
                                var polygon =
                                    placemark.Element(kmlNamespace + DataLoaderConstants.ElemNamePolygon);

                                var outerBoundary =
                                    polygon.Element(kmlNamespace + DataLoaderConstants.ElemNameOuterBoundaryIs);

                                var linearRing =
                                    outerBoundary.Element(kmlNamespace + DataLoaderConstants.ElemNameLinearRing);

                                var polygonTupleList =
                                    linearRing.Element(kmlNamespace + DataLoaderConstants.ElemNameCoordinates).Value;


                                entity.AddProperty(DataLoaderConstants.PropNameKmlCoords,
                                                   polygonTupleList);

                                string[] positionTuple = polygonTupleList.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                                entity.AddProperty(DataLoaderConstants.PropNameLatitude, ExtractLatitude(positionTuple));
                                entity.AddProperty(DataLoaderConstants.PropNameLongitude, ExtractLongitude(positionTuple));
                                entity.AddProperty("altitude", ExtractAltitude(positionTuple));
                               // entity.AddProperty(DataLoaderConstants.PropNameKmlSnippet,
                               //                   string.Concat("<Placemark>", polygon.ToString(SaveOptions.DisableFormatting), "</Placemark>"));

                                #region RDF

                                if (stringValue != string.Empty)
                                {
                                    rdfXmlDescriptionElement.Add(new XElement(customNS + DataLoaderConstants.PropNameLatitude, ExtractLatitude(positionTuple), new XAttribute(rdfNamespace + "datatype", datatype)));
                                    rdfXmlDescriptionElement.Add(new XElement(customNS + DataLoaderConstants.PropNameLongitude, ExtractLongitude(positionTuple), new XAttribute(rdfNamespace + "datatype", datatype)));
                                    if (!string.IsNullOrEmpty(ExtractAltitude(positionTuple)))
                                        rdfXmlDescriptionElement.Add(new XElement(customNS + "altitude", ExtractAltitude(positionTuple), new XAttribute(rdfNamespace + "datatype", datatype)));
                                }
                                else
                                {
                                    rdfXmlDescriptionElement.Add(new XElement(customNS + DataLoaderConstants.PropNameLatitude));
                                    rdfXmlDescriptionElement.Add(new XElement(customNS + DataLoaderConstants.PropNameLongitude));
                                    if (!string.IsNullOrEmpty(ExtractAltitude(positionTuple)))
                                        rdfXmlDescriptionElement.Add(new XElement(customNS + "altitude"));
                                }
                               
                                #endregion
                            }
                        }

                        if (item.Key.StartsWith("sd0"))
                        {
                            var schemaData =
                                placemark.Descendants(kmlNamespace + DataLoaderConstants.ElemNameSimpleData);

                            var property = schemaData.FirstOrDefault(e => e.HasAttributes && e.Attributes("name").First().Value == item.Key.Remove(0, 3));
                            string value = (property != null) ? property.Value : string.Empty;

                            entity.AddProperty(item.Key, GetPropertyValue(item.Value, value));

                            #region RDF
                            rdfValue = GetPropertyValue(item.Value, value).ToString();
                            #endregion
                        }

                        
                        #region RDF

                        if (item.Key == DataLoaderConstants.ElemNameName || item.Key == DataLoaderConstants.ElemNameDescription || item.Key.StartsWith("sd0"))
                        {
                            if (stringValue != string.Empty)
                            {
                                rdfXmlDescriptionElement.Add(new XElement(customNS + cleanHeader, rdfValue.ToString(), new XAttribute(rdfNamespace + "datatype", datatype)));
                            }
                            else
                                rdfXmlDescriptionElement.Add(new XElement(customNS + cleanHeader));
                        }

                        #endregion
                    }

                    #region RDF
                    entity.AddProperty(DataLoaderConstants.PropNameRdfSnippet, rdfXml.ToString(SaveOptions.DisableFormatting));
                    #endregion

                    var ps = placemark.ToString(SaveOptions.DisableFormatting).Replace(
                                                            DataLoaderConstants.NsKmlOld,
                                                            DataLoaderConstants.NsKmlNew);

                    entity.AddProperty(DataLoaderConstants.PropNameKmlSnippet, ps);

                    if (_sourceOrder)
                    {
                        entity.SetNumber(count, initialTimePrefix);
                    }
                }
                catch (Exception ex)
                {
                    exceptionNotifier(new EntityProcessingException(ex.Message, ex));
                    isExceptionOccurred = true;
                }

                if (isExceptionOccurred) continue;

                yield return entity;
                count++;
            }
        }

        private static string ExtractAltitude(string[] positionTuple)
        {
            if (positionTuple.Length > 2)
                return positionTuple[2];
            return string.Empty;
        }

        private static string ExtractLatitude(string[] positionTuple)
        {
            if (positionTuple.Length > 0)
                return positionTuple[0];
            return string.Empty;
        }

        private static string ExtractLongitude(string[] positionTuple)
        {
            if (positionTuple.Length > 1)
                return positionTuple[1];
            return string.Empty;
        }

        public override void ValidateParams()
        {

        }
    }
}

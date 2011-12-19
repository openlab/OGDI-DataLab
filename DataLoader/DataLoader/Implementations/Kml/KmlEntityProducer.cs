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

        public override IEnumerable<Entity> GetEntitiesEnumerator(OnContinueExceptionCallback exceptionNotifier)
        {
            int count = 0;
            string initialTimePrefix = GetSecondsFrom2000Prefix();
            var properties = _params.PropertyToTypeMap.GetProperties().ToDictionary(c => c.Key.ToLower(), c => c.Value);
            bool isExceptionOccurred = false;
            var kmlNamespace = XNamespace.Get(DataLoaderConstants.NsKmlNew);
            string[] separators = new string[] { ",", " ", Environment.NewLine };

            List<XElement> placemarks =
                _kml.Descendants(kmlNamespace + DataLoaderConstants.ElemNamePlacemark).ToList();

            for (var i = 0; i < placemarks.Count; i++)
            {
                var entity = new Entity();
                isExceptionOccurred = false;

                try
                {
                    XElement placemark = placemarks[i];

                    if (placemark == null)
                        continue;

                    foreach (var item in properties)
                    {
                        // Name & Description of Placemark
                        if (item.Key == DataLoaderConstants.ElemNameName)
                        {
                            entity.AddProperty(DataLoaderConstants.ElemNameName,
                                               placemark.Element(kmlNamespace + DataLoaderConstants.ElemNameName).Value);
                        }

                        if (item.Key == DataLoaderConstants.ElemNameDescription)
                        {
                            entity.AddProperty(DataLoaderConstants.ElemNameDescription,
                                               placemark.Element(kmlNamespace + DataLoaderConstants.ElemNameDescription).Value);
                        }

                        if (item.Key == DataLoaderConstants.PropNameKmlCoords)
                        {
                            if (placemark.Element(kmlNamespace + DataLoaderConstants.ElemNamePoint) != null)
                            {
                                var point = placemark.Element(kmlNamespace + DataLoaderConstants.ElemNamePoint);
                                var positionTuple =
                                    point.Element(kmlNamespace + DataLoaderConstants.ElemNameCoordinates).Value.Split(',');

                                entity.AddProperty("latitude", ExtractLatitude(positionTuple));
                                entity.AddProperty("longitude", ExtractLongitude(positionTuple));
                                entity.AddProperty("altitude", ExtractAltitude(positionTuple));
                                entity.AddProperty(DataLoaderConstants.PropNameKmlSnippet,
                                                   string.Concat("<Placemark>", point.ToString(SaveOptions.DisableFormatting), "</Placemark>"));
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

                                entity.AddProperty("latitude", ExtractLatitude(positionTuple));
                                entity.AddProperty("longitude", ExtractLongitude(positionTuple));
                                entity.AddProperty("altitude", ExtractAltitude(positionTuple));
                                entity.AddProperty(DataLoaderConstants.PropNameKmlSnippet,
                                                  string.Concat("<Placemark>", polygon.ToString(SaveOptions.DisableFormatting), "</Placemark>"));
                            }
                        }

                        if (item.Key.StartsWith("sd0"))
                        {
                            var schemaData =
                                placemark.Descendants(kmlNamespace + DataLoaderConstants.ElemNameSimpleData);

                            var property = schemaData.FirstOrDefault(e => e.HasAttributes && e.Attributes("name").First().Value == item.Key.Remove(0, 3));
                            string value = (property != null) ? property.Value : string.Empty;

                            entity.AddProperty(item.Key, GetPropertyValue(item.Value, value));
                        }
                    }
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

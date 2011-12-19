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
            bool isExceptionOccurred = false;

            try
            {
                IList<XElement> placemarks =
                _kml.Descendants().Where(element => element.Name.LocalName == DataLoaderConstants.ElemNamePlacemark).ToList();

                for (var i = 0; i < placemarks.Count; i++)
                {
                    
                }
            }
            catch (Exception ex)
            {
                exceptionNotifier(new EntityProcessingException(ex.Message, ex));
                isExceptionOccurred = true;
            }

            if (isExceptionOccurred)
            {


            }

            return null;
        }

        public override void ValidateParams()
        {

        }
    }
}

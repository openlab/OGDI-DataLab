using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Ogdi.Data.DataLoader.DbaseKml
{
    // TODO: consider implementing IDisposable
    internal class DbaseKmlEntityProducer : EntityProducer
    {
        DbDataReader _dataReader;
        XElement _kml;
        DbaseKmlEntityProducerParams _params;
        Entity _schemaEntity;
        bool _sourceOrder;

        public DbaseKmlEntityProducer(string fileSetName, string entitySet, string entityKind, DbaseKmlEntityProducerParams parameters, bool sourceOrder)
        {
            EntityCount = GetDataReader(fileSetName, out _dataReader);
            _kml = GetKml(fileSetName);
            _params = parameters;
            _schemaEntity = GetSchemaEntity(_dataReader, entitySet, entityKind);
            _sourceOrder = sourceOrder;
        }

        public override void ValidateParams()
        {
            _schemaEntity.ValidateProperty(_params.MatchPropertyName);
        }

        public override Entity SchemaEntity
        {
            get { return _schemaEntity; }
        }

        public override IEnumerable<Entity> GetEntitiesEnumerator(OnContinueExceptionCallback exceptionNotifier)
        {
            int count = 0;
            string initialTimePrefix = GetSecondsFrom2000Prefix();

            do
            {
                bool isExceptionOccured = false;
                var entity = new Entity();

                try
                {
                    for (int i = 0; i < _dataReader.FieldCount; ++i)
                    {
                        entity.AddProperty(_dataReader.GetName(i), _dataReader[i]);
                    }

                    var placemark = _kml.Descendants().Where(e => e.Name.LocalName == DataLoaderConstants.ElemNamePlacemark).
                                                    Where(e => e.Element(e.Name.Namespace + _params.MatchElementName) != null).
                                                    Where(e => e.Element(e.Name.Namespace + _params.MatchElementName)
                                                        .Value.Contains(_dataReader[_params.MatchPropertyName].ToString()))
                                                        .FirstOrDefault();
                    if (placemark != null)
                    {
                        placemark = new XElement(placemark);

                        foreach (string name in _params.KmlElementsToStrip)
                        {
                            if (string.IsNullOrEmpty(name)) continue;
                            placemark.Element(string.Concat(placemark.Name.Namespace, name)).Remove();
                        }

                        placemark.Element(string.Concat(placemark.Name.Namespace, DataLoaderConstants.ElemNameDescription)).SetValue(entity.Id);

                        var p = placemark.ToString(SaveOptions.DisableFormatting)
                                                .Replace(DataLoaderConstants.NsKmlOld, DataLoaderConstants.NsKmlNew);

                        entity.AddProperty(DataLoaderConstants.PropNameKmlSnippet, p);
                    }
                    if (_sourceOrder)
                    {
                        entity.SetNumber(count, initialTimePrefix);
                    }
                }
                catch (Exception ex)
                {
                    exceptionNotifier(new EntityProcessingException(entity.ToString(), ex));
                    isExceptionOccured = true;
                }


                if (!isExceptionOccured)
                {
                    yield return entity;
                    count++;
                }
            }
            while (_dataReader.Read());
        }

        private static int GetDataReader(string fileSetName, out DbDataReader reader)
        {
            string file = fileSetName + DataLoaderConstants.FileExtDbase;
            var builder = new OleDbConnectionStringBuilder();
            builder.DataSource = Directory.GetCurrentDirectory();
            builder.Provider = "Microsoft.Jet.OLEDB.4.0";
            builder["Extended Properties"] = "dBASE III";

            var connection = new OleDbConnection(builder.ConnectionString);
            connection.Open();

            var query = string.Format(DataLoaderConstants.QuerySelectAll, file);
            reader = new OleDbCommand(query, connection).ExecuteReader();

            query = string.Format(DataLoaderConstants.QueryRowCount, file);
            return (int)new OleDbCommand(query, connection).ExecuteScalar();
        }

        private static XElement GetKml(string fileSetName)
        {
            string dir = Directory.GetCurrentDirectory();
            string file = fileSetName + DataLoaderConstants.FileExtKml;
            var reader = XmlReader.Create(Path.Combine(dir, file));
            return XElement.Load(reader);
        }

        private static Entity GetSchemaEntity(DbDataReader reader, string entitySet, string entityKind)
        {
            reader.Read();

            var entity = new Entity();
            entity.AddProperty(DataLoaderConstants.PropNameEntitySet, entitySet);
            entity.AddProperty(DataLoaderConstants.PropNameEntityKind, entityKind);
            for (int i = 0; i < reader.FieldCount; ++i)
            {
                entity.AddProperty(reader.GetName(i), reader.GetFieldType(i).ToString());
            }

            return entity;
        }
    }
}

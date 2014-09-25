using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using LumenWorks.Framework.IO.Csv;
using System.Globalization;

//http://www.codeproject.com/KB/database/CsvReader.aspx

namespace Ogdi.Data.DataLoader.Csv
{
    // TODO: consider implementing IDisposable
    class CsvEntityProducer : EntityProducer
    {
        readonly CsvReader _csvReader; //http://www.codeproject.com/KB/database/CsvReader.aspx
        readonly EntityProducerParams _params;
        readonly Entity _schemaEntity;
        readonly bool _sourceOrder;
        char separator;
        char numberSeparator;
        Encoding enc;

        public CsvEntityProducer(string fileSetName, string entitySet, string entityKind, EntityProducerParams parameters, bool sourceOrder)
        {
            //Init CSV Separators and encoding file 
            InitSeparators(fileSetName);
            GetEncoding(fileSetName);

            // Calculate entities count
            // TODO: Need to rewrite in better way.
            using (var countReader = new CsvReader(new StreamReader(fileSetName + DataLoaderConstants.FileExtCsv,enc), true, separator))
            {   
                while (countReader.ReadNextRecord())
                {
                    EntityCount++;
                }
            }

            var reader = new StreamReader(string.Concat(fileSetName, DataLoaderConstants.FileExtCsv),enc);
            _csvReader = new CsvReader(reader, true, separator);

            _params = parameters;
            _schemaEntity = GetSchemaEntity(entitySet, entityKind, parameters.PropertyToTypeMap);
            _sourceOrder = sourceOrder;
        }

        //Initialize CSV Separators 
        private void InitSeparators(string fileSetName)
        {
            using (StreamReader read = new StreamReader(fileSetName + DataLoaderConstants.FileExtCsv))
            {
                if (read.ReadLine().Contains(';'))
                {
                    separator = ';';
                    numberSeparator = ',';
                }
                else
                {
                    separator = ',';
                    numberSeparator = '.';
                }
            }
        }

        /// <summary>
        /// Get Encoding Format of file 
        /// </summary>
        /// <param name="path">Chemin du fichier</param>
        /// <returns>File Encoding</returns>
        private void GetEncoding(string path)
        {
            string encode;
            using (FileStream fs = File.OpenRead(path + DataLoaderConstants.FileExtCsv))
            {
                Ude.CharsetDetector cdet = new Ude.CharsetDetector();
                cdet.Feed(fs);
                cdet.DataEnd();
                if (cdet.Charset != null)
                {
                    encode = cdet.Charset;
                }
                else
                {
                    encode = "failed";
                }
            }
            if (encode == "failed")
                enc = Encoding.Default;
            else
            {
                switch (encode.ToLower())
                {
                    case "utf-8": enc = Encoding.UTF8; break;
                    case "utf-16le": enc = Encoding.Unicode; break;
                    case "utf-16be": enc = Encoding.BigEndianUnicode; break;
                    case "windows-1252": goto default;
                    default: enc = Encoding.Default; break;
                }
            }
        }

        //check that source data contains columns listed in metadata
        public override void ValidateParams()
        {
            if (_params.PropertyToTypeMap != null)
            {
                string[] headers = _csvReader.GetFieldHeaders();

                foreach (KeyValuePair<string, string> pair in _params.PropertyToTypeMap.GetProperties())
                {
                    bool isContain = headers.Contains(pair.Key);

                    if (!isContain)
                        throw new ParamsValidationException(pair.Key);
                }
        }

            if (_params.PlacemarkParams != null)
            {
                _schemaEntity.ValidateProperty(_params.PlacemarkParams.LatitudeProperty);
                _schemaEntity.ValidateProperty(_params.PlacemarkParams.LongitudeProperty);

                if (_params.PlacemarkParams.NameProperties != null)
                {
                    foreach (string t in _params.PlacemarkParams.NameProperties)
                    {
                        _schemaEntity.ValidateProperty(t);
                    }
                }
            }
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
            string[] headers = _csvReader.GetFieldHeaders();
            bool isInlineKmlSnippetProvided = false;
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

            while (_csvReader.ReadNextRecord())
            {
                var entity = new Entity();
                bool isExceptionOccured = false;

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
                    for (int i = 0; i < _csvReader.FieldCount; ++i)
                    {
                        if (_csvReader[i] == null)
                            continue;

                        if (_csvReader[i].GetType() == typeof(DBNull))
                            continue;

                        var header = headers[i];

                        if (!isInlineKmlSnippetProvided)
                            isInlineKmlSnippetProvided = header.Equals(DataLoaderConstants.PropNameKmlSnippet,
                                                                       StringComparison.InvariantCultureIgnoreCase);
                        
                        try
                        {
                            if (!properties.ContainsKey(header.ToLower()))
                                throw new ApplicationException("The row data is mismached with column definitions in data file.");

                            var stringType = properties[header.ToLower()];
                            var stringValue = _csvReader[i];

                            if (string.IsNullOrEmpty(stringValue))
                                continue;

                            var v = GetPropertyValue(stringType, stringValue);
                            entity.AddProperty(header, v);

                            #region RDF

                            if (header == Params.ProcessorParams.PartitionKeyPropertyName)
                            {
                                rdfXmlDescriptionElement.Add(new XAttribute(rdfNamespace + "about", stringValue));
                            }

                            var datatype = GetRdfType(stringType);
                            var cleanHeader = CleanStringLower(header);

                            var columnNs = columnMetadata.First(column => column.column == header);
                            XNamespace customNS = columnNs.columnnamespace.ToString().Split('=')[1];

                            if (stringValue != string.Empty)
                            {
                                rdfXmlDescriptionElement.Add(new XElement(customNS + cleanHeader, v.ToString(), new XAttribute(rdfNamespace + "datatype", datatype)));
                            }
                            else
                                rdfXmlDescriptionElement.Add(new XElement(customNS + cleanHeader));

                            #endregion
                        }
                        catch (FormatException ex)
                        {
                            var sb = new StringBuilder();
                            sb.Append("Could not add property:");
                            sb.AppendFormat("\r\nName\t = '{0}'", header);
                            sb.AppendFormat("\r\nType\t = '{0}'", _params.PropertyToTypeMap.GetPropertyType(header));
                            sb.AppendFormat("\r\nValue\t = '{0}'", _csvReader[i]);

                            exceptionNotifier(new EntityProcessingException(sb.ToString(), ex));
                        }
                    }

                    #region RDF
                    entity.AddProperty(DataLoaderConstants.PropNameRdfSnippet, rdfXml.ToString(SaveOptions.DisableFormatting));
                    #endregion

                    if (!isInlineKmlSnippetProvided && _params.PlacemarkParams != null)
                    {
                        var longitude = _csvReader[_params.PlacemarkParams.LongitudeProperty];
                        var latitude = _csvReader[_params.PlacemarkParams.LatitudeProperty];

                        if (longitude.GetType() != typeof(DBNull) && latitude.GetType() != typeof(DBNull))
                        {
                            decimal lon;
                            decimal lat;

                            FormatLonLat(out lon, out lat, longitude, latitude);

                            if (!(lon == 0 && lat == 0))
                            {
                                var placemark = new XElement(XNamespace.Get(DataLoaderConstants.NsKmlNew) + DataLoaderConstants.ElemNamePlacemark);

                                var names = new string[_params.PlacemarkParams.NameProperties.Length];

                                for (int i = 0; i < _params.PlacemarkParams.NameProperties.Length; i++)
                                    names[i] = _csvReader[_params.PlacemarkParams.NameProperties[i]];

                                placemark.Add(new XElement(
                                    XNamespace.Get(DataLoaderConstants.NsKmlNew) + DataLoaderConstants.ElemNameName,
                                    new XCData(string.Format(_params.PlacemarkParams.NamePropertyFormatString ?? "{0}", names))));

                                var coords = new XElement(XNamespace.Get(DataLoaderConstants.NsKmlNew) + DataLoaderConstants.ElemNameCoordinates, lon.ToString(CultureInfo.InvariantCulture) + "," + lat.ToString(CultureInfo.InvariantCulture));
                                placemark.Add(new XElement(
                                    XNamespace.Get(DataLoaderConstants.NsKmlNew) + DataLoaderConstants.ElemNamePoint,
                                    coords));

                                placemark.Add(new XElement(XNamespace.Get(DataLoaderConstants.NsKmlNew) + DataLoaderConstants.ElemNameDescription, entity.Id));

                                var ps = placemark.ToString(SaveOptions.DisableFormatting).Replace(
                                                            DataLoaderConstants.NsKmlOld,
                                                            DataLoaderConstants.NsKmlNew);

                                entity.AddProperty(DataLoaderConstants.PropNameKmlSnippet, ps);
                            }
                        }
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


                if (isExceptionOccured) continue;

                yield return entity;
                count++;
            }
        }

        //Format longitude and latitude to InvariantCulture
        private void FormatLonLat(out decimal lon, out decimal lat, string longitude, string latitude)
        {
            var formatInfo = CultureInfo.InvariantCulture.NumberFormat;

            if (numberSeparator == ',')
            {
                lon = decimal.Parse(longitude.Replace(',', '.'), formatInfo);
                lat = decimal.Parse(latitude.Replace(',', '.'), formatInfo);
            }
            else
            {
                lon = decimal.Parse(longitude, formatInfo);
                lat = decimal.Parse(latitude, formatInfo);
            }
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
    }
}

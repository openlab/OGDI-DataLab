using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using LumenWorks.Framework.IO.Csv;
using System.Globalization; //http://www.codeproject.com/KB/database/CsvReader.aspx

namespace Ogdi.Data.DataLoader
{
    // TODO: consider implementing IDisposable
    class CsvEntityProducer : EntityProducer
    {
    	readonly CsvReader _csvReader; //http://www.codeproject.com/KB/database/CsvReader.aspx

    	readonly CsvEntityProducerParams _params;
    	readonly Entity _schemaEntity;
    	readonly bool _sourceOrder;

        public CsvEntityProducer(string fileSetName, string entitySet, string entityKind, CsvEntityProducerParams parameters, bool sourceOrder)
        {            
            //_dataReader = GetDataReader(fileSetName);

            // Calculate entities count
            // TODO: Need to rewrite in better way.
            using(var countReader = new CsvReader(new StreamReader(fileSetName + DataLoaderConstants.FileExtCsv), true))
            {
                while (countReader.ReadNextRecord())
                {
                    EntityCount++;
                }
            }

            var reader = new StreamReader(fileSetName + DataLoaderConstants.FileExtCsv);
            _csvReader = new CsvReader(reader, true);

            _params = parameters;
            _schemaEntity = GetSchemaEntity(entitySet, entityKind, parameters.PropertyToTypeMap);
            _sourceOrder = sourceOrder;            
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
                    for (int i = 0; i < _params.PlacemarkParams.NameProperties.Length; i++)
                    {
                        _schemaEntity.ValidateProperty(_params.PlacemarkParams.NameProperties[i]);
                    }
                }
            }
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
            string[] headers = _csvReader.GetFieldHeaders();

            while (_csvReader.ReadNextRecord())
            {
                var entity = new Entity();
                bool isExceptionOccured = false;

                try
                {
                    for (int i = 0; i < _csvReader.FieldCount; ++i)
                    {
                    	if (_csvReader[i].GetType() == typeof (DBNull))
							continue;
                    	var n = headers[i];
                    	try
                    	{
                    		if (!properties.ContainsKey(n.ToLower()))
                    			throw new ApplicationException("The row data is mismached with column definitions in data file.");

                    		var stringType = properties[n.ToLower()];
                    		var stringValue = _csvReader[i];

                    		if(string.IsNullOrEmpty(stringValue))
                    			continue;

                    		var v = GetPropertyValue(stringType, stringValue);
                    		entity.AddProperty(n, v);
                    	}
                    	catch (FormatException ex)
                    	{
                    		var sb = new StringBuilder();
                    		sb.Append("Could not add property:");
                    		sb.AppendFormat("\r\nName\t = '{0}'", n);
                    		sb.AppendFormat("\r\nType\t = '{0}'", _params.PropertyToTypeMap[n]);
                    		sb.AppendFormat("\r\nValue\t = '{0}'", _csvReader[i]);

                    		exceptionNotifier(new EntityProcessingException(sb.ToString(), ex));
                    	}
                    }

                    if (_params.PlacemarkParams != null)
                    {
                        var longitude = _csvReader[_params.PlacemarkParams.LongitudeProperty];
                        var latitude = _csvReader[_params.PlacemarkParams.LatitudeProperty];

                        if (longitude.GetType() != typeof(DBNull) && latitude.GetType() != typeof(DBNull))
                        {
                            var lon = decimal.Parse(longitude.Replace('.', ','));
                            var lat = decimal.Parse(latitude.Replace('.', ','));

                            if (!(lon == 0 && lat == 0))
                            {
                                var placemark = new XElement(XNamespace.Get(DataLoaderConstants.NsKmlNew) + DataLoaderConstants.ElemNamePlacemark);

                                var names = new string[_params.PlacemarkParams.NameProperties.Length];

                                for (int i = 0; i < _params.PlacemarkParams.NameProperties.Length; i++)
                                    names[i] = _csvReader[_params.PlacemarkParams.NameProperties[i]];

                                placemark.Add(new XElement(
                                    XNamespace.Get(DataLoaderConstants.NsKmlNew) + DataLoaderConstants.ElemNameName,
                                    new XCData(string.Format(_params.PlacemarkParams.NamePropertyFormatString, names))));

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


                if (!isExceptionOccured)
                {
                    yield return entity;
                    count++;
                }
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

        private static object GetPropertyValue(string type, string value)
        {
        	type = type.ToLower();
        	switch (type)
        	{
        		case ("string"):
        			return value;
        		case ("int32"):
        			return Int32.Parse(value);
        		case ("int64"):
        			return Int64.Parse(value);
        		case ("double"):
        			return double.Parse(value.Replace('.', ','));
        		case ("bool"):
        			return bool.Parse(value);
        		case ("bool-0or1"):
        			return int.Parse(value) == 1 ? true : false;
        		case ("datetime"):
        			return DateTime.Parse(value);
        		case ("datetime-yyyymmdd"):
        			{
        				var s = value.Trim();
        				var y = int.Parse(s.Substring(0, 4));
        				var m = int.Parse(s.Substring(4, 2));
        				var d = int.Parse(s.Substring(6, 2));
        				return new DateTime(y, m, d);
        			}
        		default:
        			throw new ArgumentException(DataLoaderConstants.MsgUnsupportedType, type);
        	}
        }

    	private static string GetPropertyType(string type)
    	{
    		type = type.ToLower();
    		switch (type)
    		{
    			case ("string"):
    				return typeof(string).ToString();
    			case ("int32"):
    				return typeof(Int32).ToString();
    			case ("int64"):
    				return typeof(Int64).ToString();
    			case ("double"):
    				return typeof(double).ToString();
    			case ("bool"):
    				return typeof(bool).ToString();
    			case ("bool-0or1"):
    				return typeof(Int32).ToString();
    			case ("datetime"):
    				return typeof(DateTime).ToString();
    			case ("datetime-yyyymmdd"):
    				return typeof(DateTime).ToString();
    			default:
    				throw new ArgumentException(DataLoaderConstants.MsgUnsupportedType, type);
    		}
    	}
    }
}

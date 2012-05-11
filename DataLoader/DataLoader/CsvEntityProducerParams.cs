using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Ogdi.Data.DataLoader
{
    public class CsvEntityProducerParams
    {
        // Optional parameters used in KML snippet generation
        public PlacemarkParams PlacemarkParams { get; set; }

        // Mapping of csv column names to their datatypes
        public PropertyToTypeMapper PropertyToTypeMap { get; set; }
    }

    public class PlacemarkParams
    {
        // Name of the property containing latitude value
        public string LatitudeProperty { get; set; }

        // Name of the property containing longitude value
        public string LongitudeProperty { get; set; }

        // Placemark name element value format string
        public string NamePropertyFormatString { get; set; }
    
        // Name of the properties to include in Placemark name element value
        public string[] NameProperties { get; set; }
    }

    // Helper class encapsulating property name to type mapping
    public class PropertyToTypeMapper : IXmlSerializable
    {
    	private Dictionary<string, string> _propToType;

    	private Dictionary<string, string> PropToType
    	{
			get { return _propToType ?? (_propToType = new Dictionary<string, string>()); }
    	}

    	public void Add(string key, string value)
        {
            PropToType.Add(key, value);
        }

        public string this[string propName]
        {
			get { return PropToType[propName]; }
        }

        public IEnumerable<KeyValuePair<string, string>> GetProperties()
        {
            return PropToType;
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            var s = (XElement)XNode.ReadFrom(reader);

            foreach (var e in s.Elements())
                PropToType.Add(e.Name.LocalName, e.Value);
        }

        public void WriteXml(XmlWriter writer)
        {
            foreach (var key in PropToType.Keys)
                writer.WriteElementString(key, PropToType[key]);
        }
    }
}

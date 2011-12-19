using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Ogdi.Data.DataLoader
{
    public class EntityProducerParams
    {
        // Optional parameters used in KML snippet generation
        public PlacemarkParams PlacemarkParams { get; set; }

        // Mapping of column names to their datatypes
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
        private readonly ObservableCollection<PropertyToType> _mappings = new ObservableCollection<PropertyToType>();

        public ObservableCollection<PropertyToType> Mappings
        {
            get { return _mappings; }
        }

        public void Add(string key, string value)
        {
            _mappings.Add(new PropertyToType(key, value, string.Empty));
        }

        public string GetPropertyType(string name)
        {
            return _mappings.Where(x => x.Name == name).First().Type;
        }

        public PropertyToType GetByName(string name)
        {
            return _mappings.FirstOrDefault(x => x.Name == name);
        }

        public IEnumerable<KeyValuePair<string, string>> GetProperties()
        {
            return _mappings.Select(x => new KeyValuePair<string, string>(x.Name, x.Type));
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            var s = (XElement)XNode.ReadFrom(reader);

            foreach (var e in s.Elements())
            {
                Add(e.Name.LocalName, e.Value);
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            foreach (var mapping in _mappings)
                writer.WriteElementString(mapping.Name, mapping.Type);
        }
    }
}

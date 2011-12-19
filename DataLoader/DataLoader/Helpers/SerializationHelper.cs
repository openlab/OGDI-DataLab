using System.IO;
using System.Xml.Serialization;

namespace Ogdi.Data.DataLoader
{
    public static class SerializationHelper
    {
        public static T DeserializeFromFile<T>(Stream stream)
        {
            var serializer = new XmlSerializer(typeof (T));

            return (T) serializer.Deserialize(stream);
        }

        public static T DeserializeFromFile<T>(string fileName)
        {
            T data;
                
            using (var stream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            {
                data = SerializationHelper.DeserializeFromFile<T>(stream);
            }
            return data;
        }

        public static void SerializeToFile(string fileName, object data)
        {
            using (FileStream stream = File.Open(fileName, FileMode.Create, FileAccess.Write))
            {
                var serializer = new XmlSerializer(data.GetType());
                serializer.Serialize(stream, data);
            }
        }
    }
}
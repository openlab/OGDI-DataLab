using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using LumenWorks.Framework.IO.Csv;
using System.Xml.Linq;
using System.Security.AccessControl;

namespace Ogdi.Data.DataLoader.Csv
{
    [XmlRoot(ElementName = "CsvDataLoaderParams")]
    public class CsvToTablesDataLoaderParams : DataLoaderParams
    {
        public static CsvToTablesDataLoaderParams FromFile(UploadParam uploadParam)
        {
            string configFileName = string.Empty;
            string csvFileName = string.Empty;
            CsvToTablesDataLoaderParams parameters;

            if (!string.IsNullOrEmpty(uploadParam.Directory) && !string.IsNullOrEmpty(uploadParam.Name))
            {
                configFileName = Path.Combine(uploadParam.Directory, string.Concat(uploadParam.Name, DataLoaderConstants.FileExtConfig));
                csvFileName = Path.Combine(uploadParam.Directory, string.Concat(uploadParam.Name, DataLoaderConstants.FileExtCsv));
            }

            //Check if (*.cfg)  file exists
            if (!string.IsNullOrEmpty(configFileName) && File.Exists(configFileName))
            {
                CsvToTablesDataLoaderParams data;

                using (var stream = File.Open(configFileName, FileMode.Open, FileAccess.Read))
                {
                    data = SerializationHelper.DeserializeFromFile<CsvToTablesDataLoaderParams>(stream);
                }
                if (data.TableMetadataEntity.ReleasedDate.Year < 2008)
                    data.TableMetadataEntity.ReleasedDate = DateTime.Now;

                if (data.TableMetadataEntity.ExpiredDate.Year < 2008)
                    data.TableMetadataEntity.ExpiredDate = DateTime.Now.AddDays(1);

                parameters = data;
            }
            else
            {
                //if there's no (*.cfg), check if (*.csv) file exists
                parameters = File.Exists(csvFileName)
                    ? CreateEmptyData(csvFileName)
                    : new CsvToTablesDataLoaderParams
                    {
                        TableMetadataEntity = new TableMetadataEntity { IsEmpty = true, ReleasedDate = DateTime.Now, ExpiredDate = DateTime.Now.AddDays(1), LastUpdateDate = DateTime.Now },
                        ProcessorParams = new TableProcessorParams
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
        
    /// <summary>
    /// Get Encoding Format of file 
    /// </summary>
    /// <param name="path">Chemin du fichier</param>
    /// <returns>File Encoding</returns>
        private static Encoding GetEncoding(string path)
        {
            string encode;
            using (FileStream fs = File.OpenRead(path))
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
                fs.Close();
            }
            if (encode == "failed")
                return Encoding.Default;
            else
            {
                switch (encode.ToLower())
                {
                    case "utf-8": return Encoding.UTF8;
                    case "utf-16le": return Encoding.Unicode;
                    case "utf-16be": return Encoding.BigEndianUnicode;
                    case "windows-1252": goto default;
                    default: return Encoding.Default;
                }
            }
        }

        public static char GetSeparator(string fileName)
        {
            using (StreamReader read = new StreamReader(fileName))
                return read.ReadLine().Contains(';') ? ';' : ',';
        }

        private static CsvToTablesDataLoaderParams CreateEmptyData(string fileName)
        {
            char separator = GetSeparator(fileName);
            var name = Path.GetFileNameWithoutExtension(fileName);

            // Create all instances
            var dataLoaderParams = new CsvToTablesDataLoaderParams();

            // Generate TableMetadataEntity properties
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
                PropertyToTypeMap = new PropertyToTypeMapper(),
            };

            // Generate ProcessorParams properties
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

            using (var reader = new CsvReader(new StreamReader(fileName,GetEncoding(fileName)), true, GetSeparator(fileName)))
            {
                string defaultDescription = DefaultDescription();
                string defaultDescriptionToAdd = string.Empty;
                foreach (string header in reader.GetFieldHeaders())
                {
                    if (header.Trim().ToLower() == "name")
                    {
                        dataLoaderParams.ProducerParams.PlacemarkParams.NameProperties = new[] { "name" };
                        dataLoaderParams.ProducerParams.PlacemarkParams.NamePropertyFormatString = "{0}";
                    }

                    if (header.Trim().ToLower() == "id")
                    {
                        dataLoaderParams.ProcessorParams.RowKeyPropertyName = "id";
                    }

                    if (header.Trim().ToLower().StartsWith("lat"))
                    {
                        dataLoaderParams.ProducerParams.PlacemarkParams.LatitudeProperty = header.Trim();
                    }

                    if (header.Trim().ToLower().StartsWith("lon"))
                    {
                        dataLoaderParams.ProducerParams.PlacemarkParams.LongitudeProperty = header.Trim();
                    }

                    dataLoaderParams.ProducerParams.PropertyToTypeMap.Add(header.Trim(), "string");
                    defaultDescriptionToAdd = string.Format(defaultDescription, header, name);
                    dataLoaderParams.TableColumnsMetadata.PropertyToTypeColumnsMetadata.Add(header.Trim(), string.Empty, defaultDescriptionToAdd, "ogdi=\"ogdiUrl\"");
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

        public override void Validate(string fileName)
        {
            base.Validate(fileName);

            if (TableMetadataEntity.IsEmpty)
                return;

            //Create loader to verify params
            var sb = new StringBuilder();
            var missedFields = new List<string>();

            string directory = Path.GetDirectoryName(fileName);
            string name = Path.GetFileNameWithoutExtension(fileName);
            string csvFile = Path.Combine(directory, name + ".csv");
            string[] headers;

            if (ProducerParams.PlacemarkParams != null && !File.Exists(fileName))
                BindToMap(csvFile);
            

            using (var reader = new CsvReader(new StreamReader(csvFile,GetEncoding(csvFile)), true, GetSeparator(csvFile)))
            {
                headers = reader.GetFieldHeaders();

                missedFields.AddRange(headers.Where(header => ProducerParams.PropertyToTypeMap.GetProperties().Where(r => r.Key == header).Count() == 0));
            }

            if (missedFields.Count != 0)
            {
                sb.AppendFormat("\r\n\r\n{0}",Ressources.Error.MissField);
                foreach (string missedField in missedFields)
                {
                    sb.AppendFormat("\r\n   {0}", missedField);
                }
            }

            AddEmptyFieldsInfo(TableMetadataEntity, sb, "Metadata");
            AddEmptyFieldsInfo(ProcessorParams, sb, "Processor Params");

            if (headers.Contains(""))
                sb.AppendFormat("\r\n\r\n{0}",Ressources.Error.MissField);

            CheckHeaders(headers, sb);

            if (sb.Length == 0)
                return;

            sb.Insert(0, Ressources.Error.Warning);
            sb.AppendFormat("\r\n\r\n {0}",Ressources.Error.Continue);



            throw new WarningException(sb.ToString());
        }

        private void BindToMap(string path)
        {
            Encoding encode = GetEncoding(path);
            var lon = ProducerParams.PlacemarkParams.LongitudeProperty;
            var lat = ProducerParams.PlacemarkParams.LatitudeProperty;
            List<string> csv;
            ReadMapInfo(out csv, encode, path, lon, lat);
            if (csv.Count != 0)
                WriteMapInfo(csv, encode, path, lon, lat);
        }

        private void ReadMapInfo(out List<string> csv, Encoding encode, string path, string lon, string lat)
        {
            csv = new List<string>();
            csv.Add(string.Empty);
            char separator = CsvToTablesDataLoaderParams.GetSeparator(path);


            if (lon != "longitude" || lat != "latitude")
            {
                using (StreamReader reader = new StreamReader(path, encode))
                {
                    string[] tmp = reader.ReadLine().Split(separator);

                    tmp.SetValue("longitude", tmp.ToList().IndexOf(lon));
                    tmp.SetValue("latitude", tmp.ToList().IndexOf(lat));
                    for (int i = 0; i < tmp.Length; i++)
                    {
                        csv[0] += tmp[i];
                        if (i < tmp.Length - 1)
                            csv[0] += separator;
                    }
                    do
                    {
                        csv.Add(reader.ReadLine());
                    } while (!reader.EndOfStream);
                }
            }
        }

        private void WriteMapInfo(List<string> csv, Encoding encode, string path,string lon, string lat)
        {
            using (StreamWriter writer = new StreamWriter(path, false, encode))
            {
                foreach(string s in csv)
                    writer.WriteLine(s);
                ProducerParams.PropertyToTypeMap.Mappings.SingleOrDefault(x => x.Name == lon).Name = "longitude";
                ProducerParams.PropertyToTypeMap.Mappings.SingleOrDefault(x => x.Name == lat).Name = "latitude";

                TableColumnsMetadata.PropertyToTypeColumnsMetadata.Mappings.SingleOrDefault(x => x.Column == lon).Column = "longitude";
                TableColumnsMetadata.PropertyToTypeColumnsMetadata.Mappings.SingleOrDefault(x => x.Column == lat).Column = "latitude";

                ProducerParams.PlacemarkParams.LongitudeProperty = "longitude";
                ProducerParams.PlacemarkParams.LatitudeProperty = "latitude";
            }
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

        private static void CheckHeaders(string[] headers, StringBuilder sb)
        {
            string[] collection = { "partitionkey", "rowkey", "timestamp", "entityid" };
            List <string> h = new List<string>();
            foreach (var s in headers)
            {
                h.Add(s.ToLower());
                if (s.Contains(' '))
                    sb.AppendFormat("\r\n\r\n{0}",Ressources.Error.Headers);
            }
            foreach (var s in collection)
                if (h.Contains(s))
                    sb.AppendFormat("\r\n\r\n{0} : {1}", Ressources.Error.Unavailable , s);
        }


        private static void AddEmptyFieldsInfo(object obj, StringBuilder sb, string tabName)
        {
            var emptyRequiredFields = ValidateFieldAttribute.GetFieldList(FieldType.Required, obj);
            var emptyOptinalFields = ValidateFieldAttribute.GetFieldList(FieldType.Optinal, obj);

            if (emptyRequiredFields.Count > 0)
            {
                sb.AppendFormat("\r\n\r\n{0}({1}):",Ressources.Error.Required, tabName);
                foreach (string field in emptyRequiredFields)
                {
                    sb.AppendFormat("\r\n   {0}", field);
                }
            }

            if (emptyOptinalFields.Count > 0)
            {
                sb.AppendFormat("\r\n\r\n{0} ({1}):",Ressources.Error.Optional, tabName);
                foreach (string field in emptyOptinalFields)
                {
                    sb.AppendFormat("\r\n   {0}", field);
                }
            }
        }
    }
}

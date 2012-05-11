using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Linq;
using Microsoft.WindowsAzure.StorageClient;
using System.ComponentModel;
using System.IO;

namespace Ogdi.Data.DataLoader
{
    public class TableColumnsMetadata
    {
        public TableColumnsMetadata() { }

        public string TableColumnsMetadataPartitionKeyPropertyName { get; set; }
        public string TableColumnsMetadataRowKeyPropertyName { get; set; }

        public static Entity GetSchemaEntity(TableColumnsMetadataItem mapper)
        {
            var entity = new Entity();
            entity.AddProperty(DataLoaderConstants.TableColumnsMetadataEntitySet, mapper.EntitySet);
            entity.AddProperty(DataLoaderConstants.TableColumnsMetadataColumn, mapper.Column);
            entity.AddProperty(DataLoaderConstants.TableColumnsMetadataColumnSemantic, mapper.ColumnSemantic);
            entity.AddProperty(DataLoaderConstants.TableColumnsMetadataColumnNamespace, mapper.ColumnNamespace);
            entity.AddProperty(DataLoaderConstants.TableColumnsMetadataColumnDescription, mapper.ColumnDescription);

            return entity;
        }

        public PropertyToTypeColumnsMetadataMapper PropertyToTypeColumnsMetadata { get; set; }
    }

    public class TableColumnsMetadataEntity
    {
        public string entityset { get; set; }

        public string column { get; set; }

        public string columnnamespace { get; set; }

        public string columndescription { get; set; }

        public string columnsemantic { get; set; }
    }

    public class TableColumnsMetadataItem
    {
        private string _columnSemantic;
        private string _columnNamespace;
        public bool _enableNamespace = true;
        public bool _enableDescription = true;

        public string EntitySet { get; set; }

        public string Column { get; set; }

        public string ColumnNamespace
        {
            get
            {
                return _columnNamespace;
            }
            set
            {
                _columnNamespace = value;
            }
        }

        public string ColumnDescription { get; set; }

        public bool enableNamespace { get; set; }
        public bool enableDescription { get; set; }

        public string ColumnSemantic
        {
            get
            {
                return _columnSemantic;
            }
            set
            {
                _columnSemantic = value;
            }
        }

        public TableColumnsMetadataItem(TableColumnsMetadataEntity data)
            : this()
        {
            new TableColumnsMetadataItem(data.column, data.columnsemantic, data.columndescription, data.columnnamespace);
        }

        public TableColumnsMetadataItem(string column, string columnsemantic, string columndescription, string columnnamespace)
        {
            Column = column;
            ColumnSemantic = columnsemantic;
            ColumnNamespace = columnnamespace;
            ColumnDescription = columndescription;
        }

        public TableColumnsMetadataItem()
        {
        }
    }

    // Helper class encapsulating property name to type mapping
    public class PropertyToTypeColumnsMetadataMapper : IXmlSerializable
    {
        private readonly ObservableCollection<TableColumnsMetadataItem> _mappings = new ObservableCollection<TableColumnsMetadataItem>();

        public ObservableCollection<TableColumnsMetadataItem> Mappings
        {
            get { return _mappings; }
        }

        public void Add(string column, string columnSemantic, string columnDescription, string columnNamespace)
        {
            _mappings.Add(new TableColumnsMetadataItem(column, columnSemantic, columnDescription, columnNamespace));
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            var s = (XElement)XNode.ReadFrom(reader);
            string column = string.Empty;
            string columnSemantic = string.Empty;
            string columnDescription = string.Empty;
            string columnNamespace = string.Empty;

            foreach (XElement item in s.Nodes())
            {
                foreach (XElement columnMetadata in item.Descendants())
                {
                    switch (columnMetadata.Name.LocalName)
                    {
                        case DataLoaderConstants.TableColumnsMetadataColumn:
                            column = columnMetadata.Value;
                            break;
                        case DataLoaderConstants.TableColumnsMetadataColumnSemantic:
                            columnSemantic = columnMetadata.Value;
                            break;
                        case DataLoaderConstants.TableColumnsMetadataColumnNamespace:
                            columnNamespace = columnMetadata.Value;
                            break;
                        case DataLoaderConstants.TableColumnsMetadataColumnDescription:
                            columnDescription = columnMetadata.Value;
                            break;
                    }
                }
                Add(column, columnSemantic, columnDescription, columnNamespace);
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            string namespaceUrl = string.Empty;

            foreach (var mapping in _mappings)
            {
                writer.WriteStartElement(mapping.GetType().Name);

                writer.WriteElementString(DataLoaderConstants.TableColumnsMetadataColumn, mapping.Column);
                writer.WriteElementString(DataLoaderConstants.TableColumnsMetadataColumnSemantic, mapping.ColumnSemantic);

                if (mapping.ColumnSemantic == string.Empty)
                {

                    if (!ValidateNamespace(mapping.ColumnNamespace))
                        mapping.ColumnNamespace = "ogdi=ogdiUrl";

                    if (mapping.ColumnNamespace.Contains('"'))
                        namespaceUrl = mapping.ColumnNamespace.Replace("\"", "");
                    else
                        namespaceUrl = mapping.ColumnNamespace;

                    writer.WriteElementString(DataLoaderConstants.TableColumnsMetadataColumnNamespace, namespaceUrl);
                    writer.WriteElementString(DataLoaderConstants.TableColumnsMetadataColumnDescription, mapping.ColumnDescription);
                }
                else
                {
                    namespaceUrl = ReadNamespace(mapping.ColumnSemantic);
                    writer.WriteElementString(DataLoaderConstants.TableColumnsMetadataColumnNamespace, namespaceUrl);
                    writer.WriteElementString(DataLoaderConstants.TableColumnsMetadataColumnDescription, "");
                }
                writer.WriteEndElement();
            }
        }

        private static string ReadNamespace(string propertyName)
        {
            string namespaceUrl = string.Empty;

            XDocument xmlDoc = XDocument.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @".\RdfNamespaces.xml"));

            foreach (XElement nspace in xmlDoc.Descendants("namespace"))
            {
                if (nspace.Element("property").Value.ToString() == propertyName)
                {
                    return namespaceUrl = nspace.Element("prefix").Value.ToString() + '=' + nspace.Element("url").Value.ToString();
                }
            }

            return string.Empty;
        }

        private bool ValidateNamespace(string namespaceToValidate)
        {
            string namespaceUrl = string.Empty;

            if (!string.IsNullOrEmpty(namespaceToValidate))
            {
                if (namespaceToValidate.Contains('='))
                {
                    if (namespaceToValidate.Split('=')[0] != string.Empty && namespaceToValidate.Split('=')[1] != string.Empty)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }

            }
            else
            {
                return false;
            }
        }
    }
}

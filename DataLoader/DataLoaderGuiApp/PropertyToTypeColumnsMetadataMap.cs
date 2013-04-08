using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Linq;
using Ogdi.Data.DataLoader;

namespace Ogdi.Data.DataLoaderGuiApp
{
    public class PropertyToTypeColumnsMetadataMap
    {
        public ObservableCollection<string> ColumnSemantic { get; set; }
        public string EntitySet { get; set; }
        public string Column { get; set; }
        public string ColumnNamespace { get; set; }
        public string ColumnDescription { get; set; }

        public ObservableCollection<TableColumnsMetadataItem> PropertyToTypeColumnsMetadataItems { get; set; }
        public List<RdfNamespaces> ListRdfNamespaces { get; set; }

        public PropertyToTypeColumnsMetadataMap()
        {
            RdfNamespaces namespaces = new RdfNamespaces();
            ListRdfNamespaces = new List<RdfNamespaces>();
            ColumnSemantic = new ObservableCollection<string>();
            ColumnSemantic.Add("");

            XDocument xmlDoc = XDocument.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @".\RdfNamespaces.xml"));

            foreach (XElement nspace in xmlDoc.Descendants("namespace"))
            {
                namespaces.Property = nspace.Element("property").Value.ToString();
                namespaces.Prefix = nspace.Element("prefix").Value.ToString();
                namespaces.Url = nspace.Element("url").Value.ToString();
                ListRdfNamespaces.Add(namespaces);
                ColumnSemantic.Add(namespaces.Property);
            }
        }
    }

    public class RdfNamespaces
    {
        public RdfNamespaces() { }

        public string Property { get; set; }
        public string Prefix { get; set; }
        public string Url { get; set; }
    }
}

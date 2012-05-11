using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Ogdi.Data.DataLoader
{
    [XmlRoot(ElementName = "DbaseKmlDataLoaderParams")]
    public class DbaseKmlToTablesDataLoaderParams : DbaseKmlDataLoaderParams
    {
        public TableProcessorParams ProcessorParams { get; set; }
    }
}

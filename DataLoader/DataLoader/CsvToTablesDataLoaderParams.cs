using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Ogdi.Data.DataLoader
{
    [XmlRoot(ElementName = "CsvDataLoaderParams")]
    public class CsvToTablesDataLoaderParams : CsvDataLoaderParams
    {
        public TableProcessorParams ProcessorParams { get; set; }
    }
}

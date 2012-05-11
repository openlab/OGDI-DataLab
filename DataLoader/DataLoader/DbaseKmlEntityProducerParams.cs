using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Ogdi.Data.DataLoader
{
    public class DbaseKmlEntityProducerParams
    {
        // Unnecessary placemark element names that will not be part of KML sinppets
        public string[] KmlElementsToStrip { get; set; }

        // Values of this property are used to find corresponding placemark elements in KML file
        public string MatchPropertyName { get; set; }

        // Values of this placemark element are used to find corresponding placemark elements in KML file
        public string MatchElementName { get; set; }
    }
}

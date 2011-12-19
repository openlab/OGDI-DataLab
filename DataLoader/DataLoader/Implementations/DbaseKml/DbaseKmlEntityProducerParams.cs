namespace Ogdi.Data.DataLoader.DbaseKml
{
    public class DbaseKmlEntityProducerParams
    {
        // Unnecessary placemark element names that will not be part of KML snippets
        public string[] KmlElementsToStrip { get; set; }

        // Values of this property are used to find corresponding placemark elements in KML file
        public string MatchPropertyName { get; set; }

        // Values of this placemark element are used to find corresponding placemark elements in KML file
        public string MatchElementName { get; set; }
    }
}

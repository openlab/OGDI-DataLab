using System.Configuration;

namespace Ogdi.Data.DataLoader
{
    public static class DataLoaderConstants
    {
        public static readonly string EntitySetTableMetadata =
            ConfigurationManager.AppSettings["table metadata entity set"];

        public static readonly string EntitySetEntityMetadata =
            ConfigurationManager.AppSettings["entity metadata entity set"];

        public static readonly string EntitySetProcessorParams =
            ConfigurationManager.AppSettings["processor params entity set"];

        public static readonly string FileExtCsv = ".csv";
        public static readonly string FileExtDbase = ".dbf";
        public static readonly string FileExtKml = ".kml";
        public static readonly string FileExtKmz = ".kmz";
        public static readonly string FileExtRdf = ".rdf";
        public static readonly string FileExtConfig = ".cfg";

        public static readonly string TimestampColumnName = "Timestamp";
        public static readonly string RowKeyColumnName = "RowKey";
        public static readonly string PartitionKeyColumnName = "PartitionKey";

        public static readonly string PropNameEntityId = "EntityId";
        public static readonly string PropNameKmlSnippet = "KmlSnippet";
        public static readonly string PropNameEntitySet = "EntitySet";
        public static readonly string PropNameEntityKind = "EntityKind";
        public static readonly string PropNameKmlCoords = "kmlcoords";
        public static readonly string PropNameLatitude = "latitude";
        public static readonly string PropNameLongitude = "longitude";
        public static readonly string PropNameLastUpdateDate = "lastupdatedate";

        public static readonly string ElemNamePlacemark = "Placemark";
        public static readonly string ElemNameDescription = "description";
        public static readonly string ElemNameName = "name";
        public static readonly string ElemNamePoint = "Point";
        public static readonly string ElemNamePolygon = "Polygon";
        public static readonly string ElemNameOuterBoundaryIs = "outerBoundaryIs";
        public static readonly string ElemNameLinearRing = "LinearRing";
        public static readonly string ElemNameCoordinates = "coordinates";
        public static readonly string ElemNameSimpleData = "SimpleData";
        public static readonly string NsKmlOld = "http://earth.google.com/kml/2.2";
        public static readonly string NsKmlNew = "http://www.opengis.net/kml/2.2";

        public static readonly string ContentTypeKmlSnippet = "text/plain; charset=UTF-8";

        public static readonly string KmlSnippetReference =
            "<KmlSnippetReference><Container>{0}</Container><Blob>{1}</Blob></KmlSnippetReference>";

        #region RDF constants
        public const string RdfSnippetReference =
         "<RdfSnippetReference><Container>{0}</Container><Blob>{1}</Blob></RdfSnippetReference>";
        public const string PropNameRdfSnippet = "RdfSnippet";
        public const string ContentTypeRdfSnippet = "text/plain; charset=UTF-8";
        public const string TableColumnsMetadataTableName = "TableColumnsMetadata";
        public const string TableColumnsMetadataEntitySet = "EntitySet";
        public const string TableColumnsMetadataColumn = "Column";
        public const string TableColumnsMetadataColumnSemantic = "ColumnSemantic";
        public const string TableColumnsMetadataColumnNamespace = "ColumnNamespace";
        public const string TableColumnsMetadataColumnDescription = "ColumnDescription";
        #endregion

        public static readonly string QueryRowCount = "select count(*) from {0}";
        public static readonly string QuerySelectAll = "select * from {0}";

        public static readonly string MsgDuplicatePropNameException = "Duplicate property name: {0} {1}";
        public static readonly string MsgUnsupportedClrType = "CLR type {0} is not supported";
        public static readonly string MsgUnsupportedType = "Type {0} is not supported";
        public static readonly string MsgPropertyNotFound = "Property {0} not found";
        public static readonly string MsgDuplicateEntityException = "Duplicate entity.\n{0}";
        public static readonly string MsgEntityProcessingException = "Failed to process entity.\n\t{0}";

        public static readonly string ValueUniqueAutoGen = "new.guid";
        public static readonly string ValueUniqueAutoGenInitCaps = "New.Guid";

        // Since 12:00:00 am Jan 1, 2000
        public static readonly long InitialDateTime2000 = 630822816000000000;
    }
}
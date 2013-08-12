using System;
using System.Reflection;

namespace Ogdi.Data.DataLoader
{
    public class TableMetadataEntity
    {
        //constructors
        public TableMetadataEntity() { }
        public TableMetadataEntity(TableEntity data) : this()
        {
            foreach (var prop in data.GetProperties())
            {
                if (prop.Name.ToLower() == "name")
                    Name = prop.Value.ToString();
                if (prop.Name.ToLower() == "category")
                    Category = prop.Value.ToString();
                if (prop.Name.ToLower() == "description")
                    Description = prop.Value.ToString();
                if (prop.Name.ToLower() == "source")
                    Source = prop.Value.ToString();
                if (prop.Name.ToLower() == "metadataurl")
                    MetadataUrl = prop.Value.ToString();
                if (prop.Name.ToLower() == "entityset")
                    EntitySet = prop.Value.ToString();
                if (prop.Name.ToLower() == "updatefrequency")
                    UpdateFrequency = prop.Value.ToString();
                if (prop.Name.ToLower() == "entitykind")
                    EntityKind = prop.Value.ToString();
                if (prop.Name.ToLower() == "keywords")
                    Keywords = prop.Value.ToString();
                if (prop.Name.ToLower() == "links")
                    Links = prop.Value.ToString();
                if (prop.Name.ToLower() == "periodcovered")
                    PeriodCovered = prop.Value.ToString();
                if (prop.Name.ToLower() == "geographiccoverage")
                    GeographicCoverage = prop.Value.ToString();
                if (prop.Name.ToLower() == "collectionmode")
                    CollectionMode = prop.Value.ToString();

                if (prop.Name.ToLower() == "lastupdatedate")
                {
                    LastUpdateDate = DateTime.Parse(prop.Value.ToString());
                }
                if (prop.Name.ToLower() == "releaseddate")
                {
                    ReleasedDate = DateTime.Parse(prop.Value.ToString());
                }
                if (prop.Name.ToLower() == "expireddate")
                {
                    ExpiredDate = DateTime.Parse(prop.Value.ToString());
                }

                if (prop.Name.ToLower() == "technicalinfo")
                    TechnicalInfo = prop.Value.ToString();
                if (prop.Name.ToLower() == "collectioninstruments")
                    CollectionInstruments = prop.Value.ToString();
                if (prop.Name.ToLower() == "datadictionary_variables")
                    DataDictionary_Variables = prop.Value.ToString();
                if (prop.Name.ToLower() == "additionalinfo")
                    AdditionalInfo = prop.Value.ToString();

                if (prop.Name.ToLower() == "isempty")
                    IsEmpty = prop.Value.ToString().Length == 4;
            }
        }

        // Required fields
        [ValidateField("Entity Set")]
        public string EntitySet { get; set; }

        [ValidateField("Dataset Name")]
        public string Name { get; set; }

        [ValidateField("Data Source")]
        public string Source { get; set; }

        [ValidateField("Category")]
        public string Category { get; set; }

        [ValidateField("Technical Information", FieldType.Optinal)]
        public string TechnicalInfo { get; set; }

        [ValidateField("Collection Instruments", FieldType.Optinal)]
        public string CollectionInstruments { get; set; }

        [ValidateField("DataDictionary_Variables", FieldType.Optinal)]
        public string DataDictionary_Variables  { get; set; }

        [ValidateField("AdditionalInfo", FieldType.Optinal)]
        public string AdditionalInfo { get; set; }

        [ValidateField("Description", FieldType.Optinal)]
        public string Description { get; set; }

        [ValidateField("Keywords", FieldType.Optinal)]
        public string Keywords { get; set; }

        [ValidateField("Links", FieldType.Optinal)]
        public string Links { get; set; }

        [ValidateField("PeriodCovered", FieldType.Optinal)]
        public string PeriodCovered { get; set; }

        [ValidateField("GeographicCoverage", FieldType.Optinal)]
        public string GeographicCoverage { get; set; }

        [ValidateField("CollectionMode", FieldType.Optinal)]
        public string CollectionMode { get; set; }

        [ValidateField("UpdateFrequency", FieldType.Optinal)]        
        public string UpdateFrequency { get; set; }

        [ValidateField("LastUpdateDate", FieldType.Optinal)]        
        public DateTime LastUpdateDate { get; set; }

        [ValidateField("ReleasedDate", FieldType.Optinal)]        
        public DateTime ReleasedDate { get; set; }

        [ValidateField("ExpiredDate", FieldType.Optinal)]        
        public DateTime ExpiredDate { get; set; }

        [ValidateField("MetadataUrl", FieldType.Optinal)]        
        public string MetadataUrl { get; set; }

        [ValidateField("EntityKind", FieldType.Optinal)]                        
        public string EntityKind { get; set; }

        public bool IsEmpty { get; set; }

        public bool KML { get; set; }

        public static implicit operator Entity(TableMetadataEntity me)
        {
            var e = new Entity();
            foreach (var pi in me.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                e.AddProperty(pi.Name, pi.GetValue(me, null));
            }
            return e;
        }
    }
}

using System;
using System.Reflection;

namespace Ogdi.Data.DataLoader
{
    public class TableProcessorParams
    {
        public TableProcessorParams() { }

        public TableProcessorParams(TableEntity data)
        {
            foreach (var prop in data.GetProperties())
            {
                if (prop.Name.ToLower() == "sourcetimezonename")
                    SourceTimeZoneName = prop.Value.ToString();
                if (prop.Name.ToLower() == "partitionkeypropertyname")
                    PartitionKeyPropertyName = prop.Value.ToString();
                if (prop.Name.ToLower() == "rowkeypropertyname")
                    RowKeyPropertyName = prop.Value.ToString();
                if (prop.Name.ToLower() == "tablemetadatapartitionkeypropertyname")
                    TableMetadataPartitionKeyPropertyName = prop.Value.ToString();
                if (prop.Name.ToLower() == "tablemetadatarowkeypropertyname")
                    TableMetadataRowKeyPropertyName = prop.Value.ToString();
                if (prop.Name.ToLower() == "entitymetadatapartitionkeypropertyname")
                    EntityMetadataPartitionKeyPropertyName = prop.Value.ToString();
                if (prop.Name.ToLower() == "entitymetadatarowkeypropertyname")
                    EntityMetadataRowKeyPropertyName = prop.Value.ToString();
                if (prop.Name.ToLower() == "entityset")
                    EntitySet = prop.Value.ToString();
                if (prop.Name.ToLower() == "entitykind")
                    EntityKind = prop.Value.ToString();

            }
        }

        TimeZoneInfo _sourceTimeZone = TimeZoneInfo.Local;

        public string SourceTimeZoneName
        {
            get
            {
                return _sourceTimeZone != null ? _sourceTimeZone.DisplayName : "";
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    _sourceTimeZone = null;
                    return;
                }
                var timeZones = TimeZoneInfo.GetSystemTimeZones();
                foreach (var zone in timeZones)
                {
                    if (zone.DisplayName != value) continue;
                    _sourceTimeZone = zone;
                    return;
                }
                //if timezone not found, throw exception.
                throw new TimeZoneNotFoundException(value);
            }
        }

        public TimeZoneInfo GetSourceTimeZone()
        {
            return _sourceTimeZone;
        }

        [ValidateField("Partition Key Property Name")]
        public string PartitionKeyPropertyName { get; set; }

        [ValidateField("Row Key Property Name")]
        public string RowKeyPropertyName { get; set; }

        [ValidateField("Table Metadata Partition Key Property Name")]
        public string TableMetadataPartitionKeyPropertyName { get; set; }

        [ValidateField("Table Metadata Row Key Property Name")]
        public string TableMetadataRowKeyPropertyName { get; set; }

        [ValidateField("Entity Metadata Partition Key Property Name")]
        public string EntityMetadataPartitionKeyPropertyName { get; set; }

        [ValidateField("Entity Metadata Row Key Property Name")]
        public string EntityMetadataRowKeyPropertyName { get; set; }

        [ValidateField("Entity Set Property Name")]
        public string EntitySet { get; set; }

        [ValidateField("Entity Kind Property Name")]
        public string EntityKind { get; set; }

        public static implicit operator Entity(TableProcessorParams me)
        {
            Entity e = new Entity();
            foreach (var pi in me.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                e.AddProperty(pi.Name, pi.GetValue(me, null));
            }
            return e;
        }
    }
}

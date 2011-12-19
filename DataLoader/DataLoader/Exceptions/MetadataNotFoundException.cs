using System;

namespace Ogdi.Data.DataLoader
{
    [Serializable]
    public class MetadataNotFoundException : Exception
    {
        public MetadataNotFoundException(String entitySetName, MetadataKind metadataKind)
            : base(ConstructMessage(entitySetName, metadataKind))
        {
        }

        private static string ConstructMessage(String entitySetName, MetadataKind metadataKind)
        {
            string mess = metadataKind + " metadata for " + entitySetName + " was not found in the storage.";
            return mess;
        }
    }

    public enum MetadataKind
    {
        Table,
        Entity,
        ProcessorParams
    }
}
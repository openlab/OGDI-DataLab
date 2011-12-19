using System;

namespace Ogdi.Data.DataLoader
{
    [Serializable]
    public class MetadataChangedException : Exception
    {
        public MetadataChangedException(String entitySetName, string message)
            : base(ConstructMessage(entitySetName, message))
        {
        }

        private static string ConstructMessage(String entitySetName, string message)
        {
            string mess = "Metadata for " + entitySetName + " was changed." + Environment.NewLine;
            mess += message;
            return mess;
        }
    }
}
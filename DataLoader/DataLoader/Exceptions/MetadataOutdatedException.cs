using System;

namespace Ogdi.Data.DataLoader
{
    [Serializable]
    public class MetadataOutdatedException : Exception
    {
        public MetadataOutdatedException(DateTime oldDate, DateTime newDate)
            : base(ConstructMessage(oldDate, newDate))
        {
        }

        private static string ConstructMessage(DateTime oldDate, DateTime newDate)
        {
            string oldDateS = oldDate == DateTime.MinValue ? "null" : oldDate.ToString();
            string newDateS = newDate == DateTime.MinValue ? "null" : newDate.ToString();
            string mess = "Existing Metadata (" + oldDateS + ") is newer then processed (" + newDateS + ")";
            return mess;
        }
    }
}
using System;

namespace Ogdi.Data.DataLoader
{
    [Serializable]
    public class EntityAlreadyExistsException : Exception
    {
        public EntityAlreadyExistsException(string entitySetName, string rowKeyColumn, string rowKeyValue,
                                            string parKeyColumn, string parKeyValue)
            : base(ConstructMessage(entitySetName, rowKeyColumn, rowKeyValue, parKeyColumn, parKeyValue))
        {
        }

        private static string ConstructMessage(string entitySetName, string rowKeyColumn, string rowKeyValue,
                                               string parKeyColumn, string parKeyValue)
        {
            string condition1 = String.Format("{0} = '{1}'", rowKeyColumn, rowKeyValue);
            string condition2 = String.Format("{0} = '{1}'", parKeyColumn, parKeyValue);
            string mess = entitySetName + " with " + condition1 + " and " + condition2;
            mess += " already exists in the storage.";
            return mess;
        }
    }
}
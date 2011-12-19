using System;
using System.Runtime.Serialization;

namespace Ogdi.Data.DataLoader
{
    [Serializable]
    public class DuplicateEntityException : Exception
    {
        public DuplicateEntityException()
        {
        }

        public DuplicateEntityException(string message) : this(message, null)
        {
        }

        public DuplicateEntityException(string message, Exception innerException) :
            base(string.Format(DataLoaderConstants.MsgDuplicateEntityException, message), innerException)
        {
        }

        protected DuplicateEntityException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
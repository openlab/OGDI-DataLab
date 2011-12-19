using System;
using System.Runtime.Serialization;

namespace Ogdi.Data.DataLoader
{
    [Serializable]
    public class EntityProcessingException : Exception
    {
        public EntityProcessingException()
        {
        }

        public EntityProcessingException(string message) : this(message, null)
        {
        }

        public EntityProcessingException(string message, Exception innerException)
            : base(string.Format(DataLoaderConstants.MsgEntityProcessingException, message), innerException)
        {
        }

        protected EntityProcessingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
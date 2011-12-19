using System;

namespace Ogdi.Data.DataLoader
{
    [Serializable]
    public class ParamsValidationException : Exception
    {
        public ParamsValidationException(string propertyName)
            : base(string.Format("Can not find property {0} in data file", propertyName))
        {            
        }
    }
}

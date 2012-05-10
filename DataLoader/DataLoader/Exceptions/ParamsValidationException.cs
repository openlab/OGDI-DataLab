using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

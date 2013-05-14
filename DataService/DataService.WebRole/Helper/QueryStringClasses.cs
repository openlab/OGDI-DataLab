using System.Collections.Generic;

namespace Ogdi.DataServices.Helper
{
    public class TopQuery
    {
        public bool All = false;

        private int _Value;
        public int Value
        {
            get
            {
                return _Value;
            }
            set
            {
                _Value = value;
                this.Left = value;
            }
        }

        public int Left;

        public TopQuery(int defaultValue)
        {
            this.Value = defaultValue;
            this.Left = defaultValue;
        }
    }

    public class SkipQuery
    {
        private int _Value = 0;
        public int Value
        {
            get
            {
                return _Value;
            }
            set
            {
                _Value = value;
                this.Left = value;
            }
        }

        public int Left = 0;
    }

    public class OrderbyQuery
    {
        public string Value;
    }

    public class FilterQuery
    {
        public string Value;
    }

    public class Pagination
    {
        public string NextPartitionKey;
        public string NextRowKey;
        public string Url;
    }
}
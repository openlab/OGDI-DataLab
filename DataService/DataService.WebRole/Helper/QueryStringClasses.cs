using System.Collections.Generic;

namespace Ogdi.DataServices.Helper
{
    public class TopQuery
    {
        public bool All = false;
        public int Value;

        public TopQuery(int value)
        {
            this.Value = value;
        }
    }

    public class SkipQuery
    {
        public int Value = 0;
    }

    public class OrderbyQuery
    {
        public string Value;
    }

    public class FilterQuery
    {
        public List<string> Values;

        public FilterQuery()
        {
            this.Values = new List<string>();
        }
    }

    public class Pagination
    {
        public string NextPartitionKey;
        public string NextRowKey;
        public string Url;
    }
}
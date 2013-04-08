using System.Collections.Specialized;
using System.Text;

namespace Ogdi.DataServices.Helper
{
    public class QueryString : NameValueCollection
    {
        public override string ToString()
        {
            StringBuilder queryString = new StringBuilder();

            bool first = true;
            foreach (string key in base.AllKeys)
            {
                if (!first)
                {
                    queryString.Append("&");
                }

                queryString.AppendFormat("{0}={1}", key, base[key]);

                first = false;
            }

            return queryString.ToString();
        }
    }
}
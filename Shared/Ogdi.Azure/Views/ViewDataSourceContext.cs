using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Ogdi.Azure.Views
{
    public class ViewDataSourceContext : TableServiceContext
    {
        public static readonly string ViewTableName = "ViewEntries";

        public ViewDataSourceContext(string baseAddress, StorageCredentials credentials)
            : base( baseAddress, credentials)
        {
        }

        public IQueryable<ViewEntry> ViewEntries
        {
            get
            {
                return CreateQuery<ViewEntry>(ViewTableName);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure;

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

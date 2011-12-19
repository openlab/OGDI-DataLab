using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure;

namespace Ogdi.InteractiveSdk.Mvc.Models.Rating
{
    public class ViewDataContext : TableServiceContext
    {
        public ViewDataContext(string baseAddress, StorageCredentials credentials)
            : base(baseAddress, credentials)
        {
        }

        public IQueryable<ViewEntry> ViewEntry
        {
            get
            {
                return this.CreateQuery<ViewEntry>("ViewEntry");
            }
        }
    }
}

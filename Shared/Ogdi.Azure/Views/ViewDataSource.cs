using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using Ogdi.Azure.Configuration;

namespace Ogdi.Azure.Views
{
	public class ViewDataSource
	{
		private static readonly CloudStorageAccount StorageAccount;
		private static readonly object Mutex = new object();

		private readonly ViewDataSourceContext _context;

		static ViewDataSource()
		{
			StorageAccount = CloudStorageAccount.Parse(OgdiConfiguration.GetValue("DataConnectionString"));

			CloudTableClient.CreateTablesFromModel(
				typeof(ViewDataSourceContext),
				StorageAccount.TableEndpoint.AbsoluteUri,
				StorageAccount.Credentials);
		}

		public ViewDataSource()
		{
		    _context = new ViewDataSourceContext(StorageAccount.TableEndpoint.AbsoluteUri, StorageAccount.Credentials)
		                   {RetryPolicy = RetryPolicies.Retry(3, TimeSpan.FromSeconds(1))};
		}

		public void AddView(ViewEntry item)
		{
			_context.AddObject(ViewDataSourceContext.ViewTableName, item);
			_context.SaveChanges();
		}

		public IEnumerable<ViewEntry> SelectAll()
		{
			return _context.ViewEntries.AsEnumerable();
		}
	}
}

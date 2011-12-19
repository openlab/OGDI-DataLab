using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using Ogdi.Azure.Configuration;

namespace Ogdi.InteractiveSdk.Mvc.Models.Request
{
    public class RequestDataSource
    {

        private static CloudStorageAccount account;
        private RequestDataContext context;

        static RequestDataSource()
		{
			account = CloudStorageAccount.Parse(OgdiConfiguration.GetValue("OgdiConfigConnectionString"));
			CloudTableClient.CreateTablesFromModel(typeof(RequestDataContext), account.TableEndpoint.AbsoluteUri, account.Credentials);
		}

        public RequestDataSource()
		{
			this.context = new RequestDataContext(account.TableEndpoint.AbsoluteUri, account.Credentials);
			this.context.RetryPolicy = RetryPolicies.Retry(3, TimeSpan.FromSeconds(1));
		}

        public void AddRequest(RequestEntry item)
        {
            this.context.AddObject("Requests", item);
            this.context.SaveChanges();
        }

        public void UpdateRequest(RequestEntry item)
        {
            this.context.UpdateObject(item);
            this.context.SaveChanges();
        }

        public void DeleteRequest(string requestId)
        {
            var item = GetById(requestId);
            this.context.DeleteObject(item);
            this.context.SaveChanges();
        }

        public IEnumerable<RequestEntry> Select()
        {
            return this.context.Requests.AsEnumerable();
        }

        public RequestEntry GetById(string requestId)
        {
            var result = (from g in this.context.Requests
                         where g.RowKey == requestId
                         select g).FirstOrDefault();

			if(result == null)
				return null;

            return result;
        }

        public IQueryable<RequestEntry> SelectAll()
        {
            return from r in this.context.Requests
                   where r.Status != "Hidden"
                   select r;
        }

        public IQueryable<RequestEntry> SelectAllWithHidden()
        {
            return from r in this.context.Requests                   
                   select r;
        }


    }
}

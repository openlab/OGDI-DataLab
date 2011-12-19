using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using Ogdi.Azure.Configuration;

namespace Ogdi.InteractiveSdk.Mvc.Models.Comments
{
	public class CommentsDataSource
	{
		private static CloudStorageAccount account;
		private CommentsDataContext context;

		static CommentsDataSource()
		{
			account = CloudStorageAccount.Parse(OgdiConfiguration.GetValue("OgdiConfigConnectionString"));
			CloudTableClient.CreateTablesFromModel(typeof(CommentsDataContext), account.TableEndpoint.AbsoluteUri, account.Credentials);
			
		}

		public CommentsDataSource()
		{
			this.context = new CommentsDataContext(account.TableEndpoint.AbsoluteUri, account.Credentials);
			this.context.RetryPolicy = RetryPolicies.Retry(3, TimeSpan.FromSeconds(1));
		}

        public void AddComment(CommentEntry item)
        {
            this.context.AddObject("Comments", item);
            this.context.SaveChanges();
        }

        public void DeleteComment(string id)
        {
            var result = (from g in this.context.Comments
                          where g.RowKey == id
                          select g).FirstOrDefault();

            if (result != null)
            {
                this.context.DeleteObject(result);
                this.context.SaveChanges();
            }
        }

        public void DeleteByParent(string parentId, string container)
        {
            var result = (from g in this.context.Comments
                          where g.DatasetId == parentId && g.PartitionKey == container
                          select g);

            foreach (CommentEntry ce in result)
            {
                this.context.DeleteObject(ce);
            }
            this.context.SaveChanges();
        }

        public void UpdateStatusByParent(string parentId, string container,string status)
        {
            var result = (from g in this.context.Comments
                          where g.DatasetId == parentId && g.PartitionKey == container
                          select g);

            foreach (CommentEntry ce in result)
            {
                ce.Status = status;
                this.context.UpdateObject(ce);
            }
            this.context.SaveChanges();
        }

        public IEnumerable<CommentEntry> SelectAll()
        {
            var results = from g in this.context.Comments
                          where g.Status != "Hidden"
                          select g;
            
            return results;
        }

        public IEnumerable<CommentEntry> SelectAllWithHidden()
        {
            var results = from g in this.context.Comments                          
                          select g;

            return results;
        }

        public CommentEntry GetById(string id)
        {
            var result = (from g in this.context.Comments
                          where g.RowKey == id
                          select g).FirstOrDefault();

            return result;
        }

        //public IEnumerable<CommentEntry> GetByParentAndUser(string parentId, string container, string parentType, string user)
        //{
        //    var result = (from g in this.context.Comments
        //                  where g.DatasetId == parentId && g.PartitionKey == container && g.ParentType == parentType && g.Email == user
        //                  select g).AsEnumerable();

        //    return result;
        //}

        public void Update(CommentEntry entry)
        {
            this.context.UpdateObject(entry);
            this.context.SaveChanges();
        }

        //public IEnumerable<string> GetSubscribers(string objectId, string container, string parentType, string exclude)
        //{
        //    var results = from com in this.context.Comments
        //                  where com.Notify == true
        //                  && com.Email != ""
        //                  && com.DatasetId == objectId
        //                  && com.PartitionKey == container
        //                  && com.ParentType == parentType
        //                  && com.RowKey != exclude
        //                  select com;

        //    return results.AsEnumerable().Select(c => c.Email).Distinct();
        //}
        
	}
}

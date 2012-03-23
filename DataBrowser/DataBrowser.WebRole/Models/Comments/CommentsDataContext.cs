using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Ogdi.InteractiveSdk.Mvc.Models.Comments
{
	public class CommentsDataContext : TableServiceContext
	{
		public CommentsDataContext(string baseAddress, StorageCredentials credentials)
			: base(baseAddress, credentials)
		{
		}

		public IQueryable<CommentEntry> Comments
		{
			get
			{
                return this.CreateQuery<CommentEntry>("Comments");
			}
		}
	}
}

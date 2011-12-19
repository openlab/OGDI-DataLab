using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ogdi.InteractiveSdk.Mvc.Models.Comments
{
	public class CommentsViewModel
	{
		public List<CommentInfo> Comments { get; set; }
		public List<String> CommentTypes { get; set; }
		public CommentsViewModel()
		{
			this.Comments = new List<CommentInfo>();
			this.CommentTypes = new List<String>();
			this.PopulateCommentTypes();
		}

		private void PopulateCommentTypes()
		{
			this.CommentTypes.Add("General Comment");
			this.CommentTypes.Add("Data Request");
			this.CommentTypes.Add("Data Error");
		}
	}
}

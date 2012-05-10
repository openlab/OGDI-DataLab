using System;
using System.Globalization;
using System.Text;
using System.Web.Mvc;
using System.Xml.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using Ogdi.Azure.Configuration;
using Ogdi.InteractiveSdk.Mvc.Models.Comments;
using Ogdi.InteractiveSdk.Mvc.Repository;

namespace Ogdi.InteractiveSdk.Mvc.Controllers
{
	public class CommentsController : Controller
	{
		public ActionResult Index(string datasetId)
		{
			var result = new CommentInfo(datasetId);
			result.Comments.Add(new Comment
			{
				Subject = "Hello world",
				Body = "Proin vulputate tincidunt ullamcorper. In nibh orci, aliquet vitae rhoncus eu, luctus eu metus. Donec justo nunc, eleifend sit amet ullamcorper nec, tempor nec risus. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed auctor sapien nisl. In hac habitasse platea dictumst. Nullam ac vehicula erat. Mauris at sem nec turpis posuere interdum ut blandit metus. In hac habitasse platea dictumst. Nullam ullamcorper aliquet laoreet. Nam ipsum sem, consequat faucibus luctus et, sollicitudin vitae nulla. Aenean ante magna, fringilla sit amet porttitor in, lacinia vel nibh. Aenean bibendum accumsan sem, non sollicitudin magna commodo in.",
				Type = "This is a request",
				Status = "Request is pending"
			});

			result.Comments.Add(new Comment
			{
				Subject = "Hey man wassup?",
				Body = "Sed vitae iaculis quam. Vestibulum molestie sem vitae velit mattis dapibus ultricies augue ornare. Etiam sagittis varius sem. Morbi sagittis mollis ipsum, ac eleifend lectus adipiscing sit amet. Aenean venenatis ultricies felis ut mattis. Sed ultricies justo sed ante blandit non interdum enim porttitor. Vestibulum ac posuere ipsum. Maecenas viverra, eros non imperdiet vestibulum, mauris est dictum ipsum, sed dapibus tortor nisi quis neque. Integer et libero id elit luctus tempor sed ac metus. Donec lectus risus, lobortis id placerat quis, varius id lorem.",
				Type = "This is a request",
				Status = "Request completed on ..."
			});
			return View("Test", result);
		}

		[AcceptVerbs(HttpVerbs.Post)]
		public ActionResult Add(string name, string subject, string comment, string email, string type, bool notify, string datasetId, string datasetName, string parentType, string container, string captchaChallenge, string captchaResponse)
		{
			var validCaptcha = Recaptcha.Validate(captchaChallenge, captchaResponse, Request.UserHostAddress);
			if (!validCaptcha || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(comment) || string.IsNullOrEmpty(datasetId))
				return EmptyHtml();

			var result = new Comment
				{
					Subject = subject,
					Body = comment,
					Posted = DateTime.Now,
					Email = email,
					Type = type,
					Status = "New",
					Notify = notify && !string.IsNullOrEmpty(email),
					ParentName = datasetId,
					ParentType = parentType,
					Author = name,
					ParentContainer = container,
				};

			CommentRepository.AddComment(result);

			string linkToParent = Request.UrlReferrer.AbsoluteUri;

			var ni = new NotifyInfo
			{
				CommentEntry = result,
				Link = linkToParent,
				DatasetName = datasetName,
			};
			Action<NotifyInfo> notification = SendNotification;
			notification.BeginInvoke(ni, null, null);

			return View("Comment", result);
		}

		private ActionResult EmptyHtml()
		{
			return Content("", "text/html");
		}

		[AcceptVerbs(HttpVerbs.Post)]
		[Authorize(Roles = "Administrator")]
		public ActionResult Reply(string subject, string comment, string datasetId, string parentType, string container, string origRowKey)
		{
			var result = new Comment
			{
				Subject = subject,
				Body = comment,
				Posted = DateTime.Now,
				Type = "General Comment (no reply required)",
				Status = "N/A",
				Notify = false,
				ParentName = datasetId,
				ParentType = parentType,
				Author = User.Identity.Name,
				ParentContainer = container
			};

			CommentRepository.AddComment(result);

			Comment original = CommentRepository.GetComment(origRowKey);
			original.Status = "Replied";
			CommentRepository.Update(original);

			return Json("Replied", JsonRequestBehavior.AllowGet);
		}

		[AcceptVerbs(HttpVerbs.Post)]
		[Authorize(Roles = "Administrator")]
		public ActionResult UpdateStatus(string rowKey, string status)
		{
			Comment original = CommentRepository.GetComment(rowKey);
			original.Status = status;
			CommentRepository.Update(original);

			return Json(new StatusInfo { Status = status, Show = true }, JsonRequestBehavior.AllowGet);
		}

		class StatusInfo
		{
			public string Status { get; set; }
			public bool Show { get; set; }
		}

		private void SendNotification(NotifyInfo ni)
		{
			var ce = ni.CommentEntry;

			var storageAccount = CloudStorageAccount.Parse(OgdiConfiguration.GetValue("DataConnectionString"));
			var queueStorage = storageAccount.CreateCloudQueueClient();
			var queue = queueStorage.GetQueueReference("workercommands");

			foreach (string subscriber in CommentRepository.GetSubscribers(ce.ParentName, ce.ParentContainer, ce.ParentType, ce.RowKey))
			{
				string unsubscribeLink = string.Format("{0}://{1}:{2}/comments/unsubscribe/?id={3}&type={4}&user={5}",
									Request.UrlReferrer.Scheme,
									Request.UrlReferrer.Host,
									Request.UrlReferrer.Port,
									ce.RowKey,
									ce.ParentType,
									subscriber);
				string datasetName = ni.DatasetName;
				string userName = !string.IsNullOrEmpty(ni.CommentEntry.Author) ? ni.CommentEntry.Author : "Anonymous";
				string link = ni.Link;
				string commentLink = ni.Link;
				string siteName = "OGDI site";

				var body = new StringBuilder();
				body.AppendFormat("{0} posted new comment to <a href=\"{1}\">{2}</a>", userName, link, datasetName);
				body.AppendLine("<br/><br/>");
				body.AppendLine("<div style='font-family:Verdana;text-transform:uppercase;font-size:14pt;'><b>");
				body.AppendLine(ni.CommentEntry.Subject);
				body.AppendLine("</div></b>");
				body.AppendLine("<br/>");
				body.AppendLine(ni.CommentEntry.Body);
				body.AppendLine("<br/><br/><br/>");
				body.AppendLine("<div style='font-family:Verdana;font-size:7pt;'>");
				body.AppendFormat("<a href=\"{0}\">Stop following</a>", unsubscribeLink);
				body.AppendLine("<br/><hr/>");
				body.AppendFormat("You received this notification because you subscribed to comment updates on {0} on the {1}", datasetName, siteName);
				body.AppendLine("<div>");

				var messageXml = new XDocument(
					new XElement("command",
						new XAttribute("commandname", "SendMail"),
						new XElement("EMailMessage",
							new XElement("To", subscriber),
							new XElement("Subject", "OGDI: " + datasetName),
							new XElement("Body", body.ToString()),
							new XElement("IsBodyHtml", "true")
					)));

				var message = new CloudQueueMessage(messageXml.ToString());
				queue.AddMessage(message);
			}
		}

		public ActionResult Unsubscribe(string id, string type, string user)
		{
			Comment comment = CommentRepository.GetComment(id);
			if (comment == null)
				throw new ApplicationException(String.Format("Comment with id: {0} has not been found.", comment));

			return View(comment);
		}


		[AcceptVerbs(HttpVerbs.Post)]
		public ActionResult Unsubscribe(string id, string type, string user, string container, bool accept)
		{
			foreach (Comment ce in CommentRepository.GetByParentAndUser(id, container, type, user))
			{
				if (ce.Notify)
				{
					ce.Notify = false;
					CommentRepository.Update(ce);
				}
			}
			return Json("You successfully unsubscribed");
		}

		//[AcceptVerbs(HttpVerbs.Post)]
		//[Authorize(Roles = "Administrator")]
		//public bool DeleteComment(string id)
		//{
		//    Comment comment = CommentRepository.GetComment(id);
		//    comment.Status = "Deleted";
		//    CommentRepository.Update(comment);

		//    return true;
		//}

		[Authorize(Roles = "Administrator"), OutputCache(CacheProfile = "Comments_AgencyComments")]
		public ActionResult AgencyComments(string ShowStatus, string FromHidden, string ToHidden)
		{
			DateTime? fromDate = null;
			if (!string.IsNullOrEmpty(FromHidden))
			{
				DateTime dt;
				if (DateTime.TryParse(FromHidden, CultureInfo.CreateSpecificCulture("en-US"), DateTimeStyles.AdjustToUniversal, out dt))
					fromDate = dt;
			}

			DateTime? toDate = null;
			if (!string.IsNullOrEmpty(ToHidden))
			{
				DateTime dt;
				if (DateTime.TryParse(ToHidden, CultureInfo.CreateSpecificCulture("en-US"), DateTimeStyles.AdjustToUniversal, out dt))
					toDate = dt;
			}

			CommentFilter filter = new CommentFilter()
			{
				From = fromDate,
				To = toDate,
				Status = ShowStatus
			};

			var model = new AgencyCommentViewModel { Filter = filter };

			return View("AgencyComments", model);
		}

		struct NotifyInfo
		{
			public Comment CommentEntry;
			public string Link;
			public string DatasetName;
		}
	}
}

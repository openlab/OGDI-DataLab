using System;
using System.Web.Mvc;
using Ogdi.InteractiveSdk.Mvc.Models.Rating;
using Ogdi.InteractiveSdk.Mvc.Repository;

namespace Ogdi.InteractiveSdk.Mvc.Controllers
{
	public class RatesController : Controller
	{
		//
		// POST
		[AcceptVerbs(HttpVerbs.Post)]
		public ActionResult ThumbDown(String itemKey, string captchaChallenge, string captchaResponse)
		{
			if (!string.IsNullOrEmpty(captchaChallenge) && !string.IsNullOrEmpty(captchaResponse))
			{
				var validCaptcha = Recaptcha.Validate(captchaChallenge, captchaResponse, Request.UserHostAddress);
				if (!validCaptcha)
					return this.EmptyHtml();
			}
			else if (string.IsNullOrEmpty(captchaResponse) && !string.IsNullOrEmpty(captchaChallenge))
			{
				return this.EmptyHtml();
			}

			AddDatasetVote(-1, itemKey);
			return this.GetRefreshedRatesHtml(itemKey);
		}

		private ActionResult EmptyHtml()
		{
			return this.Content("", "text/html");
		}

		[AcceptVerbs(HttpVerbs.Post)]
		public ActionResult ThumbUp(String itemKey, string captchaChallenge, string captchaResponse)
		{
			if (!string.IsNullOrEmpty(captchaChallenge) && !string.IsNullOrEmpty(captchaResponse))
			{
				var validCaptcha = Recaptcha.Validate(captchaChallenge, captchaResponse, Request.UserHostAddress);
				if (!validCaptcha)
				{
					return this.EmptyHtml();
				}
			}
			else if (string.IsNullOrEmpty(captchaResponse) && !string.IsNullOrEmpty(captchaChallenge))
			{
				return this.EmptyHtml();
			}
			AddDatasetVote(1, itemKey);
			return this.GetRefreshedRatesHtml(itemKey);
		}

		[AcceptVerbs(HttpVerbs.Post)]
		public bool IsAuthenticated()
		{
			return Request.IsAuthenticated;
		}

		[AcceptVerbs(HttpVerbs.Post)]
		public bool RecaptchaValidate(string challenge, string response)
		{
			return Recaptcha.Validate(challenge, response, Request.UserHostAddress);
		}


		private ActionResult GetRefreshedRatesHtml(String itemKey)
		{
			VoteResults vr = RatingRepository.GetVoteResults(itemKey);
			return this.PartialView("RatesPlusMinus", vr);
		}

		private void AddDatasetVote(int value, String itemKey)
		{
			var item = new Rate
			{
				RateValue = value,
				ItemKey = itemKey,
				RateDate = DateTime.Now,
				User = CurrentUser,
			};

			RatingRepository.AddVote(item);
		}

		private string CurrentUser
		{
			get
			{
				return !Request.IsAuthenticated
					? Request.UserHostAddress
					: Request.LogonUserIdentity.Name;
			}
		}
	}
}

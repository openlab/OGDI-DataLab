using System.Web.Mvc;

namespace Ogdi.InteractiveSdk.Mvc.Controllers
{
	public class AgencyController : Controller
	{
		[Authorize(Roles = "Administrator")]
		public ActionResult Index()
		{
			return RedirectToAction("AgencyComments", "Comments");
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;

namespace Ogdi.InteractiveSdk.Mvc.Controllers
{
    public class HistoryIframeHelperController : Controller
    {
        //
        // GET: /HistoryIframeHelper/

        public ActionResult Index()
        {
            return PartialView();
        }

    }
}

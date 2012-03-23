using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Ogdi.Azure;
using Ogdi.InteractiveSdk.Mvc.Models;
using Ogdi.InteractiveSdk.Mvc.Models.Comments;
using Ogdi.InteractiveSdk.Mvc.Models.Request;
using Ogdi.InteractiveSdk.Mvc.Repository;

namespace Ogdi.InteractiveSdk.Mvc.Controllers
{
    public class RequestController : Controller
    {
        public ActionResult Index()
        {
            var model = new RequestListModel();
            return View(model);
        }

        public ActionResult List(int pageSize, int pageNumber, string orderField, string orderType)
        {
            var data = new CommonListData();

            var direction = SortDirection.Desc;
            if (orderType != null && orderType == "Asc")
                direction = SortDirection.Asc;

            var field = Field.Name;
            if (orderField != null)
            {
                switch (orderField)
                {
                    case "Name":
                        field = Field.Name;
                        break;
                    case "Description":
                        field = Field.Description;
                        break;
                    case "Status":
                        field = Field.Status;
                        break;
                    case "Date":
                        field = Field.Date;
                        break;
                    case "Rating":
                        field = Field.Rating;
                        break;
                    case "Views":
                        field = Field.Views;
                        break;
                    default:
                        field = Field.Name;
                        break;
                }
            }
            data.OrderBy = new OrderByInfo { Direction = direction, Field = field };
            if (pageSize != 0)
                data.PageSize = pageSize;
            if (pageNumber != 0)
                data.PageNumber = pageNumber;

            Session["RequestListData"] = data;

            var model = new RequestListModel(data);

            return View(model);
        }

        public ActionResult ListDataJSON()
        {
            var data = (CommonListData)Session["RequestListData"] ?? new CommonListData();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Details(string id)
        {
            Request request = RequestRepository.GetRequest(id);
            if (request == null)
                throw new ApplicationException("Selected request does not exist any more.");

            AnalyticsRepository.RegisterView(Helper.GenerateRequestKey(id),
                HttpContext.Request.RawUrl,
                HttpContext.Request.UserHostName);

            return View(request);
        }

        public ActionResult New()
        {
            var rq = new Request();
            return View(rq);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult New(Request rq)
        {

            var validCaptcha = Recaptcha.Validate(Request.Form["recaptcha_challenge_field"],
                Request.Form["recaptcha_response_field"], Request.UserHostAddress);

            if (!validCaptcha)
            {
                ModelState.AddModelError("eidCommentsRecaptcha", "Words mismatch");
            }
            if (rq.Name.Trim().Length == 0)
            {
                ModelState.AddModelError("Name", "Name is required");
            }
            if (!Regex.IsMatch(rq.Email, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$"))
            {
                ModelState.AddModelError("Email", "Email format is invalid");
            }
            if (rq.Subject.Trim().Length == 0)
            {
                ModelState.AddModelError("Subject", "Subject is required");
            }
            if (rq.Description.Trim().Length == 0)
            {
                ModelState.AddModelError("Description", "Description is required");
            }

            if (!ModelState.IsValid)
            {
                return View(rq);
            }

            rq.PostedDate = DateTime.UtcNow;
            rq.Status = "Submitted";
            RequestRepository.AddRequest(rq);
            return RedirectToAction("Index");
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [Authorize(Roles = "Administrator")]
        public void UpdateStatus(string id, string status)
        {
            Request re = RequestRepository.GetRequest(id);
            if (re == null)
                throw new ApplicationException("Selected request does not exist any more.");

            re.Status = status;
            RequestRepository.UpdateRequest(re);
        }


        [Authorize(Roles = "Administrator")]
        public ActionResult RequestsManage()
        {
            var filter = new RequestFilter
            {
                Status = "All",
                From = null,
                To = null
            };

            var model = new AgencyRequestsViewModel();
            model.Filter = filter;
            return View(model);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [Authorize(Roles = "Administrator")]
        public ActionResult RequestsManage(string ShowStatus, string FromHidden, string ToHidden)
        {
            DateTime? from = null;
            if (!string.IsNullOrEmpty(FromHidden))
            {
                DateTime dt;
                if (DateTime.TryParse(FromHidden, CultureInfo.CreateSpecificCulture("en-US"), DateTimeStyles.AdjustToUniversal, out dt))
                    from = dt;
            }

            DateTime? to = null;
            if (!string.IsNullOrEmpty(ToHidden))
            {
                DateTime dt;
                if (DateTime.TryParse(ToHidden, CultureInfo.CreateSpecificCulture("en-US"), DateTimeStyles.AdjustToUniversal, out dt))
                    to = dt;
            }

            var filter = new RequestFilter
                {
                    Status = ShowStatus,
                    From = from,
                    To = to
                };

            var model = new AgencyRequestsViewModel();
            model.Filter = filter;
            return View(model);
        }

        class StatusInfo
        {
            public string Status { get; set; }
            public bool Show { get; set; }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [Authorize(Roles = "Administrator")]
        public ActionResult DeleteRequest(string rowKey)
        {
            Request re = RequestRepository.GetRequest(rowKey);
            if (re == null)
                throw new ApplicationException("Requested request does not exist.");

            var commentsDataSource = new CommentsDataSource();
            commentsDataSource.UpdateStatusByParent(rowKey, "Request", "Hidden");

            re.Status = "Hidden";
            RequestRepository.UpdateRequest(re);

            //string result = string.Format("<h2 style='color:red'>Request: \"{0}\" is deleted</h2>", re.Subject);
            //return Json(result, JsonRequestBehavior.AllowGet);
            return Json(new StatusInfo { Status = re.Status, Show = true }, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [Authorize(Roles = "Administrator")]
        public ActionResult CompleteRequest(string rowKey, string link, string datasetLink, string releasedDate)
        {
            Request re = RequestRepository.GetRequest(rowKey);
            if (re == null)
                throw new ApplicationException("Requested request does not exist.");

            re.Links = link;
            re.DatasetLink = datasetLink;
            re.Status = "Completed";
            if (!string.IsNullOrEmpty(releasedDate))
            {
                DateTime dt;
                if (DateTime.TryParse(releasedDate, CultureInfo.CreateSpecificCulture("en-US"), DateTimeStyles.AdjustToUniversal, out dt))
                    re.ReleaseDate = dt;
            }

            RequestRepository.UpdateRequest(re);

            string result = string.Format("<h2 style='color:green'>Request: \"{0}\" is completed</h2>", re.Subject);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

    }
}

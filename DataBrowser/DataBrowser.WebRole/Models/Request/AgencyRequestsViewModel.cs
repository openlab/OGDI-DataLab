using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Ogdi.InteractiveSdk.Mvc.Repository;

namespace Ogdi.InteractiveSdk.Mvc.Models.Request
{

    public class RequestFilter
    {
        public string Status { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public string FromAsString
        {
            get
            {
                return (From.HasValue) ? From.Value.ToString("MM/dd/yy", CultureInfo.CreateSpecificCulture("en-US")) : string.Empty;
            }
        }

        public string ToAsString
        {
            get
            {
                return (To.HasValue) ? To.Value.ToString("MM/dd/yy", CultureInfo.CreateSpecificCulture("en-US")) : string.Empty;
            }
        }
    }

    public class AgencyRequestsViewModel
    {
        public SelectList Statuses
        {
            get
            {
                return new SelectList(new string[] { "All", "Submitted" }, "All");
            }
        }

        public RequestFilter Filter { get; set; }


        private IEnumerable<Request> _list = null;

        public IEnumerable<Request> List
        {
            get
            {
                if (_list == null)
                {
                    _list = from request in RequestRepository.GetRequests("All",Filter.From, Filter.To)
                                where ShowComment(Filter.Status, request)
                                select request;
                }

                return _list;

            }
        }


        private static bool ShowComment(string statusFilter, Request comm)
        {
            switch (statusFilter)
            {
                case "Submitted":
                    return comm.Status == "Submitted";
            }
            return true;

        }
    }
}

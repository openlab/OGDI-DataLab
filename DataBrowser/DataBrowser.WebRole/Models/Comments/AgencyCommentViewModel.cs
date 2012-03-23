using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Ogdi.InteractiveSdk.Mvc.Repository;

namespace Ogdi.InteractiveSdk.Mvc.Models.Comments
{
    public class CommentFilter
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

    public class AgencyCommentViewModel
    {        
        public SelectList Statuses
        {
            get
            {
                return new SelectList(new string[] { "All", "New", "Action Required", "Reply Required"}, "All");
            }
        }
        
        public CommentFilter Filter { get; set; }


        private IEnumerable<Comment> _comments = null;

        public IEnumerable<Comment> Comments 
        {
            get
            {
                if (_comments == null)
                {
                    _comments = from comm in CommentRepository.GetComments(Filter.From, Filter.To)
                                 where ShowComment(Filter.Status, comm)
                                 select comm;
                }

                return _comments;

            }
        }


        private static bool ShowComment(string statusFilter, Comment comm)
        {
            switch (statusFilter)
            {
                case "New":
                    return comm.Status == "New" && comm.Type != "General Comment (no reply required)";
                case "Action Required":
                    return comm.Status != "NoActionRequired" && (comm.Type == "Data Request" || comm.Type == "Data Error");
                case "Reply Required":
                    return (comm.Status == "New" || comm.Status == "Addressed") && comm.Type != "General Comment (no reply required)";
                case "All":
                case "":
                case null:
                    return comm.Type != "General Comment (no reply required)";
                default:
                    return true;
            }
        }
    }
}

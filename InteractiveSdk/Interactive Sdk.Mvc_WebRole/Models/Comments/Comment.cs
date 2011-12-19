using System;

namespace Ogdi.InteractiveSdk.Mvc.Models.Comments
{
    public class Comment
    {
        public string RowKey { get; set; }
        public string ParentName { get; set; }
        public string ParentType { get; set; }
        public string ParentContainer { get; set; }
        public string ParentDisplay { get; set; }
        public string Author { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTime Posted { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public bool Notify { get; set; }
        public string Email { get; set; }
        public string ParentLink
        {
            get
            {
                if (ParentType == "Dataset")
                {
                    return string.Format("~/DataBrowser/{0}/{1}#param=NOFILTER--DataView--Results", ParentContainer, ParentName);
                }
                if (ParentType == "Request")
                {
                    return string.Format("~/Request/Details/{0}", ParentName);
                }
                return string.Empty;
            }
        }

        public string PostedOnAsText
        {
            get
            {
                return this.Posted.ToString(Globals.FullDateTimeFormat);
            }
        }

        public Comment()
        {
            RowKey = Guid.NewGuid().ToString();
        }
        
    }
}

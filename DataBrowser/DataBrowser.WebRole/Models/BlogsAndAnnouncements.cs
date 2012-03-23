
namespace Ogdi.InteractiveSdk.Mvc.Models
{
    public class BlogAndAnnouncement
    {
        private string title = string.Empty;
        private string link = string.Empty;
        private string description = string.Empty;
        private string publishDate = string.Empty;

        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        public string Link
        {
            get { return link; }
            set { link = value; }
        }

        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        public string PublishDate
        {
            get { return publishDate; }
            set { publishDate = value; }
        }
    }
}

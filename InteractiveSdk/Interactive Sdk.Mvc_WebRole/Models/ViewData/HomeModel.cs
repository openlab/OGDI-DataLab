using System.Collections.Generic;

namespace Ogdi.InteractiveSdk.Mvc.Models
{
    public class HomeModel
    {

        private List<Ogdi.InteractiveSdk.Mvc.Models.BlogAndAnnouncement> newsData;

        public List<Ogdi.InteractiveSdk.Mvc.Models.BlogAndAnnouncement> NewsData
        {
            get { return newsData; }
            set { newsData = value; }
        }

    }
}

using System;

namespace Ogdi.InteractiveSdk.Mvc.Models.ViewData
{
    public class DatasetInfo
    {
        public int ViewsTotal { get; set; }
        public int ViewsToday { get; set; }
        public double ViewsAverage { get; set; }
        public DateTime LastViewed { get; set; }
        public int PositiveVotes { get; set; }
        public int NegativeVotes { get; set; }
    }
}

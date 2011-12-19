using System;
using System.Collections.Generic;
using System.Linq;
using Ogdi.InteractiveSdk.Mvc.Models.Rating;
using Ogdi.InteractiveSdk.Mvc.Models.Request;

namespace Ogdi.InteractiveSdk.Mvc.Reports
{
    public class RequestsDataSource
    {
        private readonly IEnumerable<RequestEntry> _requestsList;
        private readonly IEnumerable<RateEntry> _rates;

        public RequestsDataSource()
        {
            var rqds = new RequestDataSource();
            _requestsList = rqds.Select();

            var rds = new RateDataSource();
            _rates = rds.SelectAll();
            _rates = (from r in _rates select r);
        }

        public List<RequestInfo> GetRateList(DateTime datefrom, DateTime todate)
        {
            var rates =
                _rates.Where(r => r.RateDate > datefrom && r.RateDate < todate)
                    .GroupBy(r => r.ItemKey,
                                  (a, b) =>
                                  new
                                      {
                                          ItemKey = a,
                                          PositiveVotes = b.Count(t => t.RateValue > 0),
                                          NegativeVotes = b.Count(t => t.RateValue < 0)
                                      });

            var result = (from rq in _requestsList
                          join r in rates on rq.RowKey equals r.ItemKey
                          select new RequestInfo
                             {
                                 RequestId = rq.RowKey,
                                 Subject = rq.Subject,
                                 Description = rq.Description,
                                 Status = rq.Status,
                                 PostedDate = rq.PostedDate.HasValue ? rq.PostedDate.Value : DateTime.MinValue,
                                 PositiveVotes = r.PositiveVotes,
                                 NegativeVotes = r.NegativeVotes,
                                 Views = 0
                             });

            return result.OrderByDescending(r => r.PositiveVotes).OrderByDescending(r => r.PositiveVotes + r.NegativeVotes).ToList();
        }
    }

    public class RequestInfo
    {
    	public string RequestId { get; set; }
    	public string Subject { get; set; }
    	public string Description { get; set; }
    	public string Status { get; set; }
    	public DateTime PostedDate { get; set; }
    	public int PositiveVotes { get; set; }
    	public int NegativeVotes { get; set; }
    	public int Views { get; set; }
    }
}


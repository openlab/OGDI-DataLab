using System;
using System.Collections.Generic;
using System.Linq;
using Ogdi.InteractiveSdk.Mvc.Repository;

namespace Ogdi.InteractiveSdk.Mvc.Models.Request
{

    public class RequestListModel
    {
        public RequestFilter Filter {get; set;}

        public RequestListModel()
        {
            OrderBy = new OrderByInfo { Direction = SortDirection.Desc, Field = Field.Name};
            PageSize = 15;
            PageNumber = 1;
        }

        public RequestListModel(CommonListData data)
        {
            OrderBy = data.OrderBy;
            PageSize = data.PageSize;
            PageNumber = data.PageNumber;
        }

        public RequestListModel(OrderByInfo orderBy, int pageSize, int pageNumber)
        {
            OrderBy = orderBy;
            PageSize = pageSize;
            PageNumber = pageNumber;
        }

        public RequestListModel(RequestFilter filter)
        {
            Filter = filter;
        }

        public OrderByInfo OrderBy { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        private int _totalPages;
        public int TotalPages
        {
            get
            {
                if (!_isInitialized)
                    Init();

                return _totalPages;
            }
        }

        private IEnumerable<Request> _list;
        private bool _isInitialized;

        private void Init()
        {
            var result =            
                RequestRepository.GetRequests(String.Empty, null, null);
            
            IOrderedEnumerable<Request> sortedResult;

            if (OrderBy.Direction == SortDirection.Asc)
            {
                switch (OrderBy.Field)
                {
                    case Field.Name:
                        sortedResult = result.OrderBy(t => t.Subject);
                        break;
                    case Field.Description:
                        sortedResult = result.OrderBy(t => t.Description);
                        break;
                    case Field.Status:
                        sortedResult = result.OrderBy(t => t.Status);
                        break;
                    case Field.Date:
                        sortedResult = result.OrderBy(t => t.PostedDate);
                        break;
                    case Field.Rating:
                        sortedResult = result.OrderBy(t => t.PositiveVotes).OrderBy(t => t.PositiveVotes - t.NegativeVotes);
                        break;
                    case Field.Views:
                        sortedResult = result.OrderBy(t => t.Views);
                        break;
                    default:
                        sortedResult = result.OrderBy(t => t.Subject);
                        break;
                }
            }
            else
            {
                switch (OrderBy.Field)
                {
                    case Field.Name:
                        sortedResult = result.OrderByDescending(t => t.Subject);
                        break;
                    case Field.Description:
                        sortedResult = result.OrderByDescending(t => t.Description);
                        break;
                    case Field.Status:
                        sortedResult = result.OrderByDescending(t => t.Status);
                        break;
                    case Field.Date:
                        sortedResult = result.OrderByDescending(t => t.PostedDate);
                        break;
                    case Field.Rating:
                        sortedResult = result.OrderByDescending(t => t.PositiveVotes).OrderByDescending(t => t.PositiveVotes - t.NegativeVotes);
                        break;
                    case Field.Views:
                        sortedResult = result.OrderByDescending(t => t.Views);
                        break;
                    default:
                        sortedResult = result.OrderByDescending(t => t.Subject);
                        break;
                }
            }

            _totalPages = ((sortedResult.Count() - 1) / PageSize) + 1;

            _list = sortedResult
                .Select((t, i) => new { t, i })
                    .Where(p => (p.i < PageNumber * PageSize))
                    .Where(p => (p.i >= (PageNumber - 1) * PageSize))
                .Select(p => p.t);

            _isInitialized = true;            
        }

        public IEnumerable<Request> List
        {
            get
            {
                if (!_isInitialized)
                    Init();

                return _list;
            }
        }
    }
}

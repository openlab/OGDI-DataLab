using System;

namespace Ogdi.InteractiveSdk.Mvc.Models
{
    [Serializable]
    public class CommonListData
    {
        public CommonListData()
        {
            OrderBy = new OrderByInfo() { Field = Field.Name, Direction = SortDirection.Asc };
            PageSize = 15;
            PageNumber = 1;
        }

        public CommonListData(OrderByInfo orderBy, int pageSize, int pageNumber)
        {
            OrderBy = orderBy;
            PageSize = pageSize;
            PageNumber = pageNumber;
        }

        public OrderByInfo OrderBy { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
    }

    [Serializable]
    public struct OrderByInfo
    {
        public SortDirection Direction { get; set; }
        public Field Field { get; set; }
    }

    [Serializable]
    public enum SortDirection
    {
        Asc,
        Desc
    }
    [Serializable]
    public enum Field
    {
        Name,
        Description,
        Category,
        Status,
        Date,
        Rating,
        Views
    }
}
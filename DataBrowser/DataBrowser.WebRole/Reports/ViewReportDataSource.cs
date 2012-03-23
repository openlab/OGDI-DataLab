using System;
using System.Collections.Generic;
using System.Linq;
using Ogdi.Azure.Views;
using Ogdi.InteractiveSdk.Mvc.Models;
using Ogdi.InteractiveSdk.Mvc.Models.Request;
using Ogdi.InteractiveSdk.Mvc.Repository;

namespace Ogdi.InteractiveSdk.Mvc.Reports
{
    public class ViewReportDataSource
    {
        private readonly IDictionary<string, EntitySet> _entities;
        private readonly IDictionary<string, RequestEntry> _requestsList;

        public ViewReportDataSource()
        {
            var entities = new List<EntitySet>(EntitySetRepository.GetEntitySets().AsEnumerable());

            var rqds = new RequestDataSource();
            var requestsList = rqds.Select();


            _entities = entities.ToDictionary(
                e => Helper.GenerateDatasetItemKey(e.ContainerAlias, e.EntitySetName),
                e => e);

            _requestsList = requestsList.ToDictionary(
                e => Helper.GenerateRequestKey(e.RowKey),
                e => e);
        }

        public IEnumerable<View> GetViews(DateTime datefrom, DateTime todate)
        {
            ViewDataSource viewDS = new ViewDataSource();

            var views =  (from v in viewDS.SelectAll()
                   where v.Date >= datefrom && v.Date <= todate
                   select v).AsEnumerable();

            var l =  from v in views
                   select CreateView(v);

            return l;
        }

        private View CreateView(ViewEntry v)
        {
            String type = v.ItemKey.Contains("||") ? "DataSet" : "Request";
            View result = new View()
            {
                Date = v.Date,
                ItemKey = v.ItemKey,
                RequestedUrl = v.RequestedUrl,
                EntityType = type,
            };

            if (type == "DataSet")
            {
                EntitySet e = null;
                if (_entities.TryGetValue(v.ItemKey, out e))
                {
                    result.Name = e.EntitySetName;
                    result.Description = e.Description;
                    result.DatasetCategoryValue = e.CategoryValue;
                    result.DatasetContainerAlias = e.ContainerAlias;
                }
            }
            else
            {
                RequestEntry r = null;
                if (_requestsList.TryGetValue(v.ItemKey, out r))
                    result.Name = r.Subject;
            }

            return result;
        }
    }


    public class View
    {
        public DateTime Date { get; set; }
        public string User { get; set; }
        public String ItemKey { get; set; }
        public String RequestedUrl { get; set; }
        public String Name { get; set; }
        public String Description { get; set; }
        public String EntityType { get; set; }
        public string DatasetCategoryValue { get; set; }
        public string DatasetContainerAlias { get; set; }
    }
}

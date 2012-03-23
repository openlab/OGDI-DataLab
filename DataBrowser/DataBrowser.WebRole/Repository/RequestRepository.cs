using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using Ogdi.Azure.Data;
using Ogdi.InteractiveSdk.Mvc.Models.Request;

namespace Ogdi.InteractiveSdk.Mvc.Repository
{
    public class RequestRepository
    {
        static public Request GetRequest(string requestId)
        {
            RequestDataSource reqDS = new RequestDataSource();
            DatasetInfoDataSource viewDs = new DatasetInfoDataSource();

            RequestEntry req = reqDS.GetById(requestId);
            AnalyticInfo dsInfo = viewDs.GetAnalyticSummary(Helper.GenerateRequestKey(requestId));

            return new Request()
            {
                DatasetLink = req.DatasetLink,
                Description = req.Description,
                Links = req.Links,
                NegativeVotes = dsInfo.NegativeVotes,
                PositiveVotes = dsInfo.PositiveVotes,
                PostedDate = req.PostedDate,
                ReleaseDate = req.ReleaseDate,
                Status = req.Status,
                Subject = req.Subject,
                Views = dsInfo.views_total,
                RequestID = req.RowKey
            };
        }

        static public IEnumerable<Request> GetRequests(string status,DateTime? postedFrom, DateTime? postedTo)
        {
            RequestDataSource reqDS = new RequestDataSource();
            DatasetInfoDataSource viewDs = new DatasetInfoDataSource();
            
            // We should use values SqlDateTime.MinValue otherwies exception during query execution
            //January 1, 1753.
            if (!postedFrom.HasValue)
                postedFrom = SqlDateTime.MinValue.Value;

            //December 31, 9999.
            if (!postedTo.HasValue)
                postedTo = DateTime.UtcNow;

            IQueryable<RequestEntry> requests = (status == "All") ? reqDS.SelectAllWithHidden() : reqDS.SelectAll();                             

            var datasetViews = viewDs.SelectAll();

            var requestList = (from req in requests
                               where                               
                               req.PostedDate >= postedFrom &&
                               req.PostedDate <= postedTo
                               select req).AsEnumerable();

            var result = (from request in requestList
                          join r1 in datasetViews on request.RowKey equals r1.RowKey into lstWithViews
                          from es2 in lstWithViews.DefaultIfEmpty()
                          select new Request
                          {
                              Name = request.Name,
                              Email = request.Email,
                              RequestID = request.RowKey,
                              Subject = request.Subject,
                              Description = request.Description,
                              Status = request.Status,
                              ReleaseDate = request.ReleaseDate,
                              PostedDate = request.PostedDate,
                              Links = request.Links,
                              DatasetLink = request.DatasetLink,
                              PositiveVotes = (es2 != null ? es2.PositiveVotes : 0),
                              NegativeVotes = (es2 != null ? es2.NegativeVotes : 0),
                              Views = es2 != null ? es2.views_total : 0,
                          });

            return result;
        }

        static public void AddRequest(Request item)
        {
            RequestDataSource reqDS = new RequestDataSource();
            RequestEntry request = new RequestEntry();
            Convert.CopyFields(item, request);
            reqDS.AddRequest(request);
        }

        static public void UpdateRequest(Request item)
        {
            RequestDataSource reqDS = new RequestDataSource();
            RequestEntry request = reqDS.GetById(item.RequestID);
            Convert.CopyFields(item, request);
            reqDS.UpdateRequest(request);
        }

        static public void DeleteRequest(string requestID)
        {
            RequestDataSource reqDS = new RequestDataSource();
            reqDS.DeleteRequest(requestID);
        }

        class Convert
        {
            public static Request FromTableEntry(RequestEntry req)
            {
                return new Request()
                {
                    Comments = req.Comments,
                    DatasetLink = req.DatasetLink,
                    Description = req.Description,
                    Links = req.Links,
                    PostedDate = req.PostedDate,
                    ReleaseDate = req.ReleaseDate,
                    Status = req.Status,
                    Subject = req.Subject,
                    RequestID = req.RowKey
                };
            }

            public static RequestEntry CopyFields(Request source, RequestEntry target)
            {
                target.Comments = source.Comments;
                target.DatasetLink = source.DatasetLink;
                target.Description = source.Description;
                target.Links = source.Links;
                target.PostedDate = source.PostedDate;
                target.ReleaseDate = source.ReleaseDate;
                target.Status = source.Status;
                target.Subject = source.Subject;
                target.Email = source.Email;
                target.Name = source.Name;

                if (!String.IsNullOrEmpty(source.RequestID))
                    target.RowKey = source.RequestID;

                return target;

            }
        }
        
    }
}

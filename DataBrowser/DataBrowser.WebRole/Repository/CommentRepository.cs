using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using Ogdi.InteractiveSdk.Mvc.Models;
using Ogdi.InteractiveSdk.Mvc.Models.Comments;
using Ogdi.InteractiveSdk.Mvc.Models.Request;

namespace Ogdi.InteractiveSdk.Mvc.Repository
{
    public class CommentRepository
    {
        static public void AddComment(Comment comment)
        {
            CommentsDataSource comDS = new CommentsDataSource();
            CommentEntry comm = new CommentEntry();
            Converter.CopyFields(comment, comm);
            comDS.AddComment(comm);
        }

        static public void DeleteComment(string id)
        {
            CommentsDataSource comDS = new CommentsDataSource();
            comDS.DeleteComment(id);
        }

        static public void DeleteByParent(string parentId, string container)
        {
            CommentsDataSource comDS = new CommentsDataSource();
            comDS.DeleteByParent(parentId, container);
        }

        static public void Update(Comment comment)
        {
            CommentsDataSource comDS = new CommentsDataSource();
            CommentEntry comm = comDS.GetById(comment.RowKey);
            Converter.CopyFields(comment, comm);
            comDS.Update(comm);
        }
                      

        static public Comment GetComment(string id)
        {
            CommentsDataSource comDS = new CommentsDataSource();
            
            CommentEntry comm = comDS.GetById(id);

            return CreateCommentEnumerator(new List<CommentEntry>() { comm }).FirstOrDefault();
        }

        static public IEnumerable<String> GetSubscribers(string objectId, string container, string parentType, string exclude)
        {
            CommentsDataSource comDS = new CommentsDataSource();
            var results = from com in comDS.SelectAll()
                          where com.Notify == true
                          && com.Email != ""
                          && com.DatasetId == objectId
                          && com.PartitionKey == container
                          && com.ParentType == parentType
                          && com.RowKey != exclude
                          select com;

            return results.AsEnumerable().Select(c => c.Email).Distinct();
        }

        static public IEnumerable<Comment> GetByParentAndUser(string parentId, string container, string parentType, string user)
        {
            CommentsDataSource comDS = new CommentsDataSource();

            var comments = from g in comDS.SelectAll()
                          where g.DatasetId == parentId && g.PartitionKey == container && g.ParentType == parentType && g.Email == user
                          select g;

            return CreateCommentEnumerator(comments);

        }

        static public IEnumerable<Comment> GetComments(DateTime? fromDate, DateTime? toDate)
        {
            // We should use values SqlDateTime.MinValue otherwies exception during query execution
            //January 1, 1753.
            if (!fromDate.HasValue)
                fromDate = SqlDateTime.MinValue.Value;

            //December 31, 9999.
            if (!toDate.HasValue)
                toDate = DateTime.Now;

            var commentDS = new CommentsDataSource();

            var comments = (from comm in commentDS.SelectAllWithHidden()
                            where comm.PostedOn >= fromDate && comm.PostedOn <= toDate
                            select comm).AsEnumerable();

            return CreateCommentEnumerator(comments);
        }

        static public IEnumerable<Comment> GetDatasetComments(string datasetId, string parentType, string container)
        {
            var commentDS = new CommentsDataSource();
            
            var comments = from g in commentDS.SelectAll()
                          where g.DatasetId == datasetId && g.ParentType == parentType && container == g.PartitionKey
                          select g;

            return CreateCommentEnumerator(comments);
        }

        private static IEnumerable<Comment> CreateCommentEnumerator(IEnumerable<CommentEntry> comments)
        {
            var reqDS = new RequestDataSource();
            var requests = reqDS.Select().ToDictionary(t => t.RowKey, t => t.Subject);

            var entities = new Dictionary<String, String>();
                
            foreach(EntitySet e in EntitySetRepository.GetEntitySets())
                entities[e.ContainerAlias + e.EntitySetName] = e.Name;

            return (from comm in comments
                    select new Comment()
                    {
                        RowKey = comm.RowKey,
                        Author = comm.Username,
                        Body = comm.Comment,
                        ParentName = comm.DatasetId,
                        ParentType = comm.ParentType,
                        ParentContainer = comm.PartitionKey,
                        ParentDisplay = GetParentDisplayName(comm, requests, entities),
                        Posted = comm.PostedOn,
                        Status = comm.Status,
                        Subject = comm.Subject,
                        Type = comm.Type,
                        Email = comm.Email
                    }).OrderBy(t => t.Posted);
        }

        private static string GetParentDisplayName(CommentEntry comm, Dictionary<string, string> requests, Dictionary<string, string> entities)
        {
            String result;

            if(comm.ParentType == "Request")
            {
                requests.TryGetValue(comm.DatasetId, out result);
            }
            else
            {
                entities.TryGetValue(comm.PartitionKey + comm.DatasetId, out result);
            }

            return result;
        }


        class Converter
        {
            static public CommentEntry CopyFields(Comment source, CommentEntry target)
            {
                if(!String.IsNullOrEmpty(source.RowKey))
                    target.RowKey = source.RowKey;

                target.Username = source.Author;
                target.Comment = source.Body;
                target.DatasetId = source.ParentName;
                target.ParentType = source.ParentType;
                target.PartitionKey = source.ParentContainer;
                target.PostedOn = source.Posted;
                target.Status = source.Status;
                target.Subject = source.Subject;
                target.Type = source.Type;
                target.Notify = source.Notify;
                target.Email = source.Email;

                return target;
            }

            static public Comment FromTableEntry(CommentEntry comm)
            {
                return new Comment()
                    {
                        RowKey = comm.RowKey,
                        Author = comm.Username,
                        Body = comm.Comment,
                        ParentName = comm.DatasetId,
                        ParentType = comm.ParentType,
                        ParentContainer = comm.PartitionKey,
                        Posted = comm.PostedOn,
                        Status = comm.Status,
                        Subject = comm.Subject,
                        Notify = comm.Notify,
                        Type = comm.Type,
                        Email = comm.Email
                    };
            }
        }
    }
}

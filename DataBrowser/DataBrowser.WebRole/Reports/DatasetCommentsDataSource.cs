using System;
using System.Collections.Generic;
using System.Linq;
using Ogdi.InteractiveSdk.Mvc.Models;
using Ogdi.InteractiveSdk.Mvc.Models.Comments;
using Ogdi.InteractiveSdk.Mvc.Repository;

namespace Ogdi.InteractiveSdk.Mvc.Reports
{
    public class DatasetCommentsDataSource
    {
        private readonly IEnumerable<Container> _containerAliases =
            ContainerRepository.GetAllContainers();
        private IEnumerable<DatasetComment> _list;

        public DatasetCommentsDataSource()
        {
            IEnumerable<EntitySet> entitySets = new List<EntitySet>();

            foreach (Container container in ContainerAliases)
            {
                var sets = EntitySetRepository.GetEntitySets(container.Alias, null) as List<EntitySet>;
                if (sets != null)
                {
                    entitySets = entitySets.Union(sets);
                }
            }
            
            var cds = new CommentsDataSource();
            IEnumerable<CommentEntry> comments = cds.SelectAll().Where(t => t.ParentType == ParentType.Dataset.ToString());

            _list = (from es in entitySets
                     join c in comments on es.EntitySetName equals c.DatasetId 
                     select new DatasetComment(es.EntityId, es.Name, es.Description, es.CategoryValue, es.ContainerAlias, es.LastUpdateDate, es.EntitySetName, c.Subject, c.Username, c.Email, c.PostedOn));

            _list = _list.OrderBy(c => c.CommentDate)
                .OrderBy(c => c.DatasetName)
                .OrderBy(c => c.DatasetCategoryValue)
                .OrderBy(c => c.DatasetContainerAlias);

       }

        public IEnumerable<Container> ContainerAliases
        {
            get
            {
                return _containerAliases;
            }
        }

        public List<DatasetComment> GetList()
        {
            return _list.ToList();
        }

        public List<DatasetComment> GetList(DateTime datefrom, DateTime todate)
        {
            return _list.Where(c => c.CommentDate > datefrom && c.CommentDate < todate).ToList();
        }

    }

    public class DatasetComment
    {
        public DatasetComment(Guid datasetId, string datasetName, string description, string datasetCategoryValue, string datasetContainerAlias, DateTime datasetLastUpdateDate, string datasetMetadataUrl, string commentSubject, string commentAuthorUsername, string commentAuthorEmail, DateTime commentDate)
        {
            this.datasetId = datasetId;
            this.datasetName = datasetName;
            this.description = description;
            this.datasetCategoryValue = datasetCategoryValue;
            this.datasetContainerAlias = datasetContainerAlias;
            this.datasetLastUpdateDate = datasetLastUpdateDate;
            this.datasetMetadataUrl = datasetMetadataUrl;
            this.commentSubject = commentSubject;
            this.commentAuthorUsername = commentAuthorUsername;
            this.commentAuthorEmail = commentAuthorEmail;
            this.commentDate = commentDate;
        }

        private Guid datasetId;
        private string datasetName;
        private string description;
        private string datasetCategoryValue;
        private string datasetContainerAlias;
        private DateTime datasetLastUpdateDate;
        private string datasetMetadataUrl;
        private string commentSubject;
        private string commentAuthorUsername;
        private string commentAuthorEmail;
        private DateTime commentDate;

        public DatasetInfo Dataset { get; set; }
        
        public Guid DatasetId
        {
            get { return datasetId; }
            set { datasetId = value; }
        }

        public string DatasetName
        {
            get { return datasetName; }
            set { datasetName = value; }
        }

        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        public string DatasetCategoryValue
        {
            get { return datasetCategoryValue; }
            set { datasetCategoryValue = value; }
        }

        public string DatasetContainerAlias
        {
            get { return datasetContainerAlias; }
            set { datasetContainerAlias = value; }
        }

        public DateTime DatasetLastUpdateDate
        {
            get { return datasetLastUpdateDate; }
            set { datasetLastUpdateDate = value; }
        }

        public string DatasetMetadataUrl
        {
            get { return datasetMetadataUrl; }
            set { datasetMetadataUrl = value; }
        }

        public string CommentSubject
        {
            get { return commentSubject; }
            set { commentSubject = value; }
        }

        public string CommentAuthorUsername
        {
            get { return commentAuthorUsername; }
            set { commentAuthorUsername = value; }
        }

        public string CommentAuthorEmail
        {
            get { return commentAuthorEmail; }
            set { commentAuthorEmail = value; }
        }

        public DateTime CommentDate
        {
            get { return commentDate; }
            set { commentDate = value; }
        }
    }

}

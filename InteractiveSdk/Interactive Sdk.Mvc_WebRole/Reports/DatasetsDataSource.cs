using System;
using System.Collections.Generic;
using System.Linq;
using Ogdi.InteractiveSdk.Mvc.Models;
using Ogdi.InteractiveSdk.Mvc.Models.Rating;
using Ogdi.InteractiveSdk.Mvc.Repository;

namespace Ogdi.InteractiveSdk.Mvc.Reports
{
    public class DatasetsDataSource
    {
        private readonly IEnumerable<Container> _containerAliases = ContainerRepository.GetAllContainers();

        private readonly IEnumerable<EntitySet> _entities;
        private readonly IEnumerable<RateEntry> _rates;

        public DatasetsDataSource()
        {
            _entities = new List<EntitySet>(EntitySetRepository.GetEntitySets().AsEnumerable());

            RateDataSource rds = new RateDataSource();
            _rates = from r in rds.SelectAll() select r;
        }

        public List<DatasetInfo> GetRateList(DateTime datefrom, DateTime todate)
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
                                      }).ToList();
            var result = (from en in _entities
                          join r in rates on Helper.GenerateDatasetItemKey(en.ContainerAlias, en.EntitySetName) equals r.ItemKey
                          select new DatasetInfo
                             {
                                 DatasetId = en.EntityId, 
                                 DatasetName = en.EntitySetName,
                                 Description = en.Description,
                                 DatasetCategoryValue = en.CategoryValue,
                                 DatasetContainerAlias = en.ContainerAlias,
                                 DatasetLastUpdateDate = en.LastUpdateDate,
                                 DatasetMetadataUrl = en.MetadataUrl,
                                 PositiveVotes = r.PositiveVotes, 
                                 NegativeVotes = r.NegativeVotes, 
                                 Views = 0
                             }).ToList();

            return result.OrderBy(r => r.PositiveVotes).OrderBy(r => r.PositiveVotes + r.NegativeVotes).ToList();
        }
    }

    public class DatasetInfo
    {
    	public Guid DatasetId { get; set; }
    	public string DatasetName { get; set; }
    	public string Description { get; set; }
    	public string DatasetCategoryValue { get; set; }
    	public string DatasetContainerAlias { get; set; }
    	public DateTime DatasetLastUpdateDate { get; set; }
    	public string DatasetMetadataUrl { get; set; }
    	public int PositiveVotes { get; set; }
    	public int NegativeVotes { get; set; }
    	public int Views { get; set; }
    }

}
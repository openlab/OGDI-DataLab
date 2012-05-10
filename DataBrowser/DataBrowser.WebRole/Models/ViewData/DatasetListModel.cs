using System;
using System.Collections.Generic;
using System.Linq;
using Ogdi.Azure.Data;
using Ogdi.InteractiveSdk.Mvc.Repository;

namespace Ogdi.InteractiveSdk.Mvc.Models
{
	public sealed class DatasetListModel
	{
		private readonly OrderByInfo _orderBy;
		private readonly int _pageSize;
		private readonly int _pageNumber;
		private readonly IEnumerable<string> _containers;
		private readonly IEnumerable<Func<EntitySet, bool>> _filters;
		private IEnumerable<string> _categories;
		private readonly IDictionary<string, IEnumerable<EntitySet>> _entitySets = new Dictionary<string, IEnumerable<EntitySet>>();
		private IEnumerable<Container> _allContainers;
		private IEnumerable<AnalyticInfo> _datasetInfos;

		private static readonly IDictionary<Field, Func<EntitySetWrapper, object>> SortingFuncs = new Dictionary<Field, Func<EntitySetWrapper, object>>
		{
			{Field.Name, t => t.EntitySet.Name},
			{Field.Description, t => t.EntitySet.Description},
			{Field.Category, t => t.EntitySet.CategoryValue},
			{Field.Status, t => t.EntitySet.IsEmpty},
			{Field.Date, t => t.EntitySet.LastUpdateDate},
			{Field.Rating, t => t.Rating},
			{Field.Views, t => t.Views},
		};


		private int? _pageCount;

		public DatasetListModel(int pageSize, int pageNumber, OrderByInfo orderBy, IEnumerable<string> containers, IEnumerable<Func<EntitySet, bool>> filters)
		{
			_orderBy = orderBy;
			_pageSize = pageSize;
			_pageNumber = pageNumber;
			_containers = containers;
			_filters = filters;
		}

		public OrderByInfo OrderBy
		{
			get { return _orderBy; }
		}

		public int PageSize
		{
			get { return _pageSize; }
		}

		public int PageNumber
		{
			get { return _pageNumber; }
		}

		public int PageCount
		{
			get
			{
				if(!_pageCount.HasValue)
				{
					_pageCount = (GetDataSets(0, 0, OrderBy, _containers, _filters).Count() - 1) / PageSize + 1;
				}
				return _pageCount.Value;
			}
		}

		public IEnumerable<EntitySetWrapper> GetTopList(Field field)
		{
			return GetDataSets(0, 5, new OrderByInfo { Field = field, Direction = SortDirection.Desc }, null, null);
		}

		public IEnumerable<EntitySetWrapper> MainList
		{
			get { return GetDataSets((PageNumber - 1) * PageSize, PageSize, OrderBy, _containers, _filters); }
		}

		public IEnumerable<string> Categories
		{
			get
			{
				if (_categories == null)
				{
					_categories = (from category in GetEntitySets(null)
								   orderby category.CategoryValue
								   select category.CategoryValue).Distinct();
				}
				return _categories;
			}
		}

		public IEnumerable<Container> AllContainers
		{
			get
			{
				if (_allContainers == null)
				{
					_allContainers = ContainerRepository.GetAllContainers();
				}
				return _allContainers;
			}
		}
		

		private IEnumerable<AnalyticInfo> DatasetInfos
		{
			get
			{
				if (_datasetInfos == null)
				{
					var datasetInfoDataSource = new DatasetInfoDataSource();
					_datasetInfos = datasetInfoDataSource.SelectAll();
				}
				return _datasetInfos;
			}
		}

		private IEnumerable<EntitySet> GetEntitySets(IEnumerable<string> containers)
		{
			containers = containers ?? from container in AllContainers select container.Alias;
			foreach (var container in containers)
			{
				IEnumerable<EntitySet> sets;
				_entitySets.TryGetValue(container, out sets);
				if (sets == null)
				{
					sets = EntitySetRepository.GetEntitySets(container, null);
					_entitySets.Add(container, sets);
				}
				foreach (var set in sets)
					yield return set;
			}
		}

		private IEnumerable<EntitySetWrapper> _allDataSets;

		private IEnumerable<EntitySetWrapper> GetDataSets(int first, int count, OrderByInfo orderBy, IEnumerable<string> containers, IEnumerable<Func<EntitySet, bool>> filters)
		{
			var filteredSets = GetEntitySets(containers);
			IEnumerable<EntitySetWrapper> result;
			if (filters != null)
			{
				foreach (var filter in filters)
					filteredSets = filteredSets.Where(filter);
				result = GetDataSetWrappers(filteredSets);
			}
			else
			{
				// Cache non filtered list for top lists.
				result = _allDataSets ?? (_allDataSets = GetDataSetWrappers(filteredSets));
			}

			var field = SortingFuncs[orderBy.Field];

			var sortedResult = orderBy.Direction == SortDirection.Asc
				?           result.OrderBy(field).ThenBy(SortingFuncs[Field.Date]).ThenBy(SortingFuncs[Field.Rating]).ThenBy(SortingFuncs[Field.Views]).ThenByDescending(SortingFuncs[Field.Name]).ThenByDescending(SortingFuncs[Field.Category])
				: result.OrderByDescending(field).ThenBy(SortingFuncs[Field.Date]).ThenBy(SortingFuncs[Field.Rating]).ThenBy(SortingFuncs[Field.Views]).ThenByDescending(SortingFuncs[Field.Name]).ThenByDescending(SortingFuncs[Field.Category]);

			return count > 0
				? sortedResult.Skip(first - 1).Take(count)
				: sortedResult;
		}

		private IEnumerable<EntitySetWrapper> GetDataSetWrappers(IEnumerable<EntitySet> filteredSets)
		{
			return from es in filteredSets
				   join dsi in DatasetInfos on Helper.GenerateDatasetItemKey(es.ContainerAlias, es.EntitySetName) equals dsi.RowKey into setInfos
				   let dsi2 = setInfos.FirstOrDefault()
				   select new EntitySetWrapper
							{
								EntitySet = es,
								PositiveVotes = dsi2 != null ? dsi2.PositiveVotes : 0,
								NegativeVotes = dsi2 != null ? dsi2.NegativeVotes : 0,
								Views = dsi2 != null ? dsi2.views_total : 0
							};
		}
	}
}
using DataConfig.Helper;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataConfig.Models
{
    public class Data
    {
        public Data()
        {
            this.Errors = new List<string>();
        }
        
        public Data(string storageName, string storageKey)
        {
            this.ConfigStorageName = storageName;
            this.ConfigStorageKey = storageKey;
            this.Errors = new List<string>();
        }

        public List<string> Errors { get; set; }

        public string ConfigStorageName { get; set; }
        public string ConfigStorageKey { get; set; }

        public string SortOrder { get; set; }
        public string SortParam { get; set; }
        public string CatalogFilter { get; set; }
        public string CategoryFilter { get; set; }
        public string KeywordFilter { get; set; }

        private Dictionary<AvailableEndpoint, List<TableMetadata>> _Catalogs = null;
        public Dictionary<AvailableEndpoint, List<TableMetadata>> Catalogs
        {
            get
            {
                if (_Catalogs == null)
                {
                    _Catalogs = new Dictionary<AvailableEndpoint, List<TableMetadata>>();

                    try
                    {
                        CloudTable availableEndpoints = Azure.GetCloudTable(this.ConfigStorageName, this.ConfigStorageKey, Azure.Table.AvailableEndpoints);

                        availableEndpoints.CreateIfNotExists();

                        IEnumerable<AvailableEndpoint> catalogs = availableEndpoints.ExecuteQuery(new TableQuery<AvailableEndpoint>());
                        if (catalogs != null)
                        {
                            catalogs = catalogs.OrderBy(c => c.alias);
                            foreach (var catalog in catalogs)
                            {
                                try
                                {
                                    CloudTable tableMetadata = Azure.GetCloudTable(catalog.storageaccountname, catalog.storageaccountkey, Azure.Table.TableMetadata);

                                    IEnumerable<TableMetadata> tmpDatasets = tableMetadata.ExecuteQuery(new TableQuery<TableMetadata>());
                                    tmpDatasets = tmpDatasets.OrderBy(d => d.entityset);

                                    List<TableMetadata> datasets = tmpDatasets.ToList();
                                    datasets.ForEach(delegate(TableMetadata t) { t.Catalog = catalog.alias; });

                                    _Catalogs.Add(catalog, datasets);
                                }
                                catch (Exception)
                                {
                                    _Catalogs.Add(catalog, new List<TableMetadata>());
                                }
                            }
                        }
                    }
                    catch (Exception)
                    { }
                }

                return _Catalogs;
            }
        }

        private List<TableMetadata> _Datasets = null;
        public List<TableMetadata> Datasets
        {
            get
            {
                if (_Datasets == null)
                {
                    IEnumerable<TableMetadata> datasets = this.Catalogs.SelectMany(d => d.Value);

                    // Filter on catalog if any
                    if (!string.IsNullOrEmpty(this.CatalogFilter))
                    {
                        datasets = datasets.Where(d => d.Catalog.ToLower() == this.CatalogFilter.ToLower());
                    }

                    // Filter on category if any
                    if (!string.IsNullOrEmpty(this.CategoryFilter))
                    {
                        datasets = datasets.Where(d => d.category.ToLower() == this.CategoryFilter.ToLower());
                    }

                    // Filter on keyword if any
                    if (!string.IsNullOrEmpty(this.KeywordFilter))
                    {
                        datasets = datasets.Where(d => d.KeywordsList != null && d.KeywordsList.Contains(this.KeywordFilter));
                    }

                    // Order datasets
                    if (this.SortParam == "source")
                    {
                        _Datasets = (this.SortOrder == "asc" ? datasets.OrderBy(d => d.source).ToList() : datasets.OrderByDescending(d => d.source).ToList());
                    }
                    else if (this.SortParam == "category")
                    {
                        _Datasets = (this.SortOrder == "asc" ? datasets.OrderBy(d => d.category).ToList() : datasets.OrderByDescending(d => d.category).ToList());
                    }
                    else
                    {
                        _Datasets = (this.SortOrder == "asc" ? datasets.OrderBy(d => d.entityset).ToList() : datasets.OrderByDescending(d => d.entityset).ToList());
                    }
                }

                return _Datasets;
            }
        }

        private Dictionary<string, int> _AllCatalogs = null;
        public Dictionary<string, int> AllCatalogs
        {
            get
            {
                if (_AllCatalogs == null)
                {
                    _AllCatalogs = (from d in this.Datasets
                                    orderby d.Catalog
                                    group d by d.Catalog into cat
                                    select cat).ToDictionary(d => d.Key, d => d.Count());
                }

                return _AllCatalogs;
            }
        }

        private Dictionary<string, int> _AllCategories = null;
        public Dictionary<string, int> AllCategories
        {
            get
            {
                if (_AllCategories == null)
                {
                    _AllCategories = (from d in this.Datasets
                                      orderby d.category
                                      group d by d.category into cat
                                      select cat).ToDictionary(d => d.Key, d => d.Count());
                }

                return _AllCategories;
            }
        }

        private Dictionary<string, int> _AllKeywords = null;
        public Dictionary<string, int> AllKeywords
        {
            get
            {
                if (_AllKeywords == null)
                {
                    _AllKeywords = (from d in this.Datasets
                                    where d.KeywordsList != null
                                    from e in d.KeywordsList
                                    group d by e into key
                                    orderby key.Key
                                    select key).ToDictionary(d => d.Key, d => d.Count());
                }

                return _AllKeywords;
            }
        }
    }
}
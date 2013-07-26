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

        public string Order { get; set; }
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
                                CloudTable tableMetadata = Azure.GetCloudTable(catalog.storageaccountname, catalog.storageaccountkey, Azure.Table.TableMetadata);

                                IEnumerable<TableMetadata> datasets = tableMetadata.ExecuteQuery(new TableQuery<TableMetadata>());
                                if (datasets != null)
                                {
                                    datasets = datasets.OrderBy(d => d.entityset);
                                }

                                _Catalogs.Add(catalog, datasets.ToList());
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
                    IEnumerable<TableMetadata> datasets;

                    // Filter on catalog if any
                    if (!string.IsNullOrEmpty(this.CatalogFilter))
                    {
                        datasets = this.Catalogs.Where(d => d.Key.alias == this.CatalogFilter).Single().Value as IEnumerable<TableMetadata>;
                    }
                    else
                    {
                        datasets = this.Catalogs.SelectMany(d => d.Value);
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
                    _Datasets = datasets.OrderBy(d => d.entityset).ToList();
                }

                return _Datasets;
            }
        }

        private Dictionary<string, int> _Categories = null;
        public Dictionary<string, int> Categories
        {
            get
            {
                if (_Categories == null)
                {
                    _Categories = (from d in this.Datasets
                                   orderby d.category
                                   group d by d.category into cat
                                   select cat).ToDictionary(d => d.Key, d => d.Count());
                }

                return _Categories;
            }
        }

        private Dictionary<string, int> _Keywords = null;
        public Dictionary<string, int> Keywords
        {
            get
            {
                if (_Keywords == null)
                {
                    _Keywords = (from d in this.Datasets
                                 where d.KeywordsList != null
                                 from e in d.KeywordsList
                                 group d by e into key
                                 orderby key.Key
                                 select key).ToDictionary(d => d.Key, d => d.Count());
                }

                return _Keywords;
            }
        }
    }
}
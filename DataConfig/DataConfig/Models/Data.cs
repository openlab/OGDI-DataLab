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

        public int? Page { get; set; }
        public string Order { get; set; }
        public string CatalogFilter { get; set; }
        public string CategoryFilter { get; set; }
        public string KeywordFilter { get; set; }

        public IEnumerable<AvailableEndpoint> Catalogs
        {
            get
            {
                try
                {
                    CloudTable availableEndpoints = Azure.GetCloudTable(this.ConfigStorageName, this.ConfigStorageKey, Azure.Table.AvailableEndpoints);

                    availableEndpoints.CreateIfNotExists();

                    IEnumerable<AvailableEndpoint> catalogs = availableEndpoints.ExecuteQuery(new TableQuery<AvailableEndpoint>());
                    if (catalogs != null)
                    {
                        catalogs = catalogs.OrderBy(c => c.alias);
                    }

                    return catalogs;
                }
                catch (Exception)
                { }

                return null;
            }
        }

        public IEnumerable<TableMetadata> Datasets
        {
            get
            {
                if (this.Catalogs != null)
                {
                    IEnumerable<TableMetadata> datasets = null;

                    foreach (var catalog in this.Catalogs)
                    {
                        try
                        {
                            CloudTable tableMetadata = Azure.GetCloudTable(catalog.storageaccountname, catalog.storageaccountkey, Azure.Table.TableMetadata);

                            IEnumerable<TableMetadata> tmpDatasets = tableMetadata.ExecuteQuery(new TableQuery<TableMetadata>());

                            datasets = (datasets == null ? tmpDatasets : datasets.Concat(tmpDatasets));
                        }
                        catch (Exception)
                        { }
                    }

                    if (datasets != null)
                    {
                        datasets = datasets.OrderBy(d => d.entityset);
                    }

                    return datasets;
                }

                return null;
            }
        }
    }
}
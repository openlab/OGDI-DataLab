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

        private IEnumerable<AvailableEndpoint> _Catalogs;
        public IEnumerable<AvailableEndpoint> Catalogs
        {
            get
            {
                if (_Catalogs != null)
                {
                    return _Catalogs;
                }

                try
                {
                    CloudTable availableEndpoints = Azure.GetCloudTable(this.ConfigStorageName, this.ConfigStorageKey, Azure.Table.AvailableEndpoints);

                    availableEndpoints.CreateIfNotExists();

                    IEnumerable<AvailableEndpoint> catalogs = availableEndpoints.ExecuteQuery(new TableQuery<AvailableEndpoint>());
                    if (catalogs != null)
                    {
                        catalogs = catalogs.OrderBy(c => c.alias);
                    }

                    _Catalogs = catalogs;
                }
                catch (Exception)
                { }

                return _Catalogs;
            }
        }

        private IEnumerable<TableMetadata> _Datasets;
        public IEnumerable<TableMetadata> Datasets
        {
            get
            {
                if (_Datasets != null)
                {
                    return _Datasets;
                }

                try
                {
                    //TODO: Implement here
                }
                catch (Exception)
                { }

                return _Datasets;
            }
        }
    }
}
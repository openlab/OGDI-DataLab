using DataConfig.Helper;
using DataConfig.Models;
using DataConfig.Resources;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Web.Mvc;

namespace DataConfig.Controllers
{
    public class CatalogController : Controller
    {
        //
        // POST: /Catalog/Load

        [HttpPost]
        public ActionResult Load(LoadCatalog model)
        {
            try
            {
                CloudTable availableEndpoints = this.GetCloudTable(model.ConfigStorageName, model.ConfigStorageKey, Global.Table.AvailableEndpoints);
                if (!availableEndpoints.Exists())
                {
                    return Json(new { Error = string.Format(Messages.TableDoesNotExist, Global.Table.AvailableEndpoints) });
                }

                return Json(new { Result = availableEndpoints.ExecuteQuery(new TableQuery<AvailableEndpoint>()) });
            }
            catch (Exception)
            {
                return Json(new { Error = Messages.CannotConnect });
            }
        }

        //
        // POST: /Catalog/Delete

        [HttpPost]
        public ActionResult Delete(DeleteCatalog model)
        {
            try
            {
                CloudTable availableEndpoints = this.GetCloudTable(model.ConfigStorageName, model.ConfigStorageKey, Global.Table.AvailableEndpoints);
                if (!availableEndpoints.Exists())
                {
                    return Json(new { Error = string.Format(Messages.TableDoesNotExist, Global.Table.AvailableEndpoints) });
                }

                TableOperation retrieveOperation = TableOperation.Retrieve<AvailableEndpoint>(model.PartitionKey, (model.RowKey ?? string.Empty));
                TableResult retrievedResult = availableEndpoints.Execute(retrieveOperation);

                if (retrievedResult.Result == null)
                {
                    return Json(new { Error = Messages.CatalogNotFound });
                }

                AvailableEndpoint availableEndpoint = retrievedResult.Result as AvailableEndpoint;
                string dataStorageName = availableEndpoint.storageaccountname;
                string dataStorageKey = availableEndpoint.storageaccountkey;

                // Deleting row in AvailableEndpoints
                availableEndpoints.Execute(TableOperation.Delete(availableEndpoint));

                // Deleting all data tables
                CloudTable entityMetadata = this.GetCloudTable(dataStorageName, dataStorageKey, Global.Table.EntityMetadata);
                if (entityMetadata.Exists())
                {
                    TableQuery<EntityMetadata> query = new TableQuery<EntityMetadata>();
                    foreach (EntityMetadata entity in entityMetadata.ExecuteQuery(query))
                    {
                        this.GetCloudTable(dataStorageName, dataStorageKey, entity.entityset).DeleteIfExists();
                    }
                }

                // Deleting all metadata tables
                entityMetadata.DeleteIfExists();
                this.GetCloudTable(dataStorageName, dataStorageKey, Global.Table.ProcessorParams).DeleteIfExists();
                this.GetCloudTable(dataStorageName, dataStorageKey, Global.Table.TableColumnsMetadata).DeleteIfExists();
                this.GetCloudTable(dataStorageName, dataStorageKey, Global.Table.TableMetadata).DeleteIfExists();

                return Json(new { Result = string.Empty });
            }
            catch (Exception ex)
            {
                return Json(new { Error = ex.Message });
            }
        }

        //
        // POST: /Catalog/Add

        [HttpPost]
        public ActionResult Add(AddCatalog model)
        {
            try
            {
                CloudTable availableEndpoints = this.GetCloudTable(model.ConfigStorageName, model.ConfigStorageKey, Global.Table.AvailableEndpoints);

                availableEndpoints.CreateIfNotExists();

                AvailableEndpoint entity = new AvailableEndpoint(model.Alias, string.Empty);
                entity.alias = model.Alias;
                entity.description = model.Description;
                entity.disclaimer = model.Disclaimer;
                entity.storageaccountname = model.StorageName;
                entity.storageaccountkey = model.StorageKey;

                availableEndpoints.Execute(TableOperation.Insert(entity));

                return Json(new { Result = entity });
            }
            catch (Exception ex)
            {
                return Json(new { Error = ex.Message });
            }
        }

        private CloudTable GetCloudTable(string storageName, string storageKey, string tableName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(string.Format(Global.ConnectionString, storageName, storageKey));
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            return tableClient.GetTableReference(tableName);
        }
    }
}

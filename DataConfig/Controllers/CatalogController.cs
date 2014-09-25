using DataConfig.Helper;
using DataConfig.Models;
using DataConfig.Resources;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Web.Mvc;

namespace DataConfig.Controllers
{
    public class CatalogController : Controller
    {
        //
        // POST: /Catalog/Add

        [HttpPost]
        public ActionResult Add(AddCatalog model)
        {
            try
            {
                CloudTable availableEndpoints = Azure.GetCloudTable(model.ConfigStorageName, model.ConfigStorageKey, Azure.Table.AvailableEndpoints);

                availableEndpoints.CreateIfNotExists();

                AvailableEndpoint entity = new AvailableEndpoint(model.Alias, model.Alias);
                entity.alias = model.Alias;
                entity.description = model.Description;
                entity.disclaimer = model.Disclaimer;
                entity.storageaccountname = model.DataStorageName;
                entity.storageaccountkey = model.DataStorageKey;

                if (!entity.IsValid())
                {
                    return Json(new { Error = Messages.IncompleteForm });
                }

                availableEndpoints.Execute(TableOperation.Insert(entity));

                return Json(new { Result = string.Empty });
            }
            catch (Exception ex)
            {
                return Json(new { Error = ex.Message });
            }
        }

        //
        // POST: /Catalog/Delete

        [HttpPost]
        public ActionResult Delete(DeleteCatalog model)
        {
            try
            {
                CloudTable availableEndpoints = Azure.GetCloudTable(model.ConfigStorageName, model.ConfigStorageKey, Azure.Table.AvailableEndpoints);
                if (!availableEndpoints.Exists())
                {
                    return Json(new { Error = string.Format(Messages.TableDoesNotExist, Azure.Table.AvailableEndpoints) });
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
                CloudTable tableMetadata = Azure.GetCloudTable(dataStorageName, dataStorageKey, Azure.Table.TableMetadata);
                if (tableMetadata.Exists())
                {
                    foreach (TableMetadata entity in tableMetadata.ExecuteQuery(new TableQuery<TableMetadata>()))
                    {
                        Azure.GetCloudTable(dataStorageName, dataStorageKey, entity.entityset).DeleteIfExists();
                    }

                    tableMetadata.DeleteIfExists();
                }

                // Deleting all metadata tables
                Azure.GetCloudTable(dataStorageName, dataStorageKey, Azure.Table.EntityMetadata).DeleteIfExists();
                Azure.GetCloudTable(dataStorageName, dataStorageKey, Azure.Table.ProcessorParams).DeleteIfExists();
                Azure.GetCloudTable(dataStorageName, dataStorageKey, Azure.Table.TableColumnsMetadata).DeleteIfExists();

                return Json(new { Result = string.Empty });
            }
            catch (Exception ex)
            {
                return Json(new { Error = ex.Message });
            }
        }

    }
}

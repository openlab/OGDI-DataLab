using DataConfig.Helper;
using DataConfig.Models;
using DataConfig.Resources;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;

namespace DataConfig.Controllers
{
    public class DatasetController : Controller
    {
        //
        // POST: /Dataset/Delete

        [HttpPost]
        public ActionResult Delete(DeleteDataset model)
        {
            try
            {
                CloudTable endPoints = Azure.GetCloudTable(model.DataStorageName, model.DataStorageKey, Azure.Table.AvailableEndpoints);
                AvailableEndpoint catalog = endPoints.ExecuteQuery(new TableQuery<AvailableEndpoint>()).SingleOrDefault(x => x.alias == model.Catalog);

                CloudTable tableMetadata = Azure.GetCloudTable(catalog.storageaccountname, catalog.storageaccountkey, Azure.Table.TableMetadata);
                if (!tableMetadata.Exists())
                {
                    return Json(new { Error = string.Format(Messages.TableDoesNotExist, Azure.Table.TableMetadata) });
                }

                TableOperation retrieveOperation = TableOperation.Retrieve<TableMetadata>(model.PartitionKey, (model.RowKey ?? string.Empty));
                TableResult retrievedResult = tableMetadata.Execute(retrieveOperation);
                if (retrievedResult.Result == null)
                {
                    return Json(new { Error = Messages.DatasetNotFound });
                }

                TableMetadata entity = retrievedResult.Result as TableMetadata;
                string entitySet = entity.entityset;

                // Deleting row in TableMetadata
                tableMetadata.Execute(TableOperation.Delete(entity));

                // Deleting rows in TableColumnsMetadata
                CloudTable tableColumnsMetadata = Azure.GetCloudTable(catalog.storageaccountname, catalog.storageaccountkey, Azure.Table.TableColumnsMetadata);
                if (tableColumnsMetadata.Exists())
                {
                    TableQuery<TableColumnsMetadata> rangeQuery = new TableQuery<TableColumnsMetadata>().Where(TableQuery.GenerateFilterCondition("entityset", QueryComparisons.Equal, entitySet));
                    foreach (TableColumnsMetadata tmpEntity in tableColumnsMetadata.ExecuteQuery(rangeQuery))
                    {
                        tableColumnsMetadata.Execute(TableOperation.Delete(tmpEntity));
                    }
                }

                // Deleting rows in EntityMetadata
                CloudTable entityMetadata = Azure.GetCloudTable(catalog.storageaccountname, catalog.storageaccountkey, Azure.Table.EntityMetadata);
                if (entityMetadata.Exists())
                {
                    TableQuery<EntityMetadata> rangeQuery = new TableQuery<EntityMetadata>().Where(TableQuery.GenerateFilterCondition("entityset", QueryComparisons.Equal, entitySet));
                    foreach (EntityMetadata tmpEntity in entityMetadata.ExecuteQuery(rangeQuery))
                    {
                        entityMetadata.Execute(TableOperation.Delete(tmpEntity));
                    }
                }

                // Deleting rows in ProcessorParams
                CloudTable processorParams = Azure.GetCloudTable(catalog.storageaccountname, catalog.storageaccountkey, Azure.Table.ProcessorParams);
                if (processorParams.Exists())
                {
                    TableQuery<ProcessorParams> rangeQuery = new TableQuery<ProcessorParams>().Where(TableQuery.GenerateFilterCondition("entityset", QueryComparisons.Equal, entitySet));
                    foreach (ProcessorParams tmpEntity in processorParams.ExecuteQuery(rangeQuery))
                    {
                        processorParams.Execute(TableOperation.Delete(tmpEntity));
                    }
                }

                // Deleting the data table
                Azure.GetCloudTable(catalog.storageaccountname, catalog.storageaccountkey, entitySet).DeleteIfExists();

                return Json(new { Result = string.Empty });
            }
            catch (Exception ex)
            {
                return Json(new { Error = ex.Message });
            }
        }

    }
}

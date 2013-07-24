using DataConfig.Helper;
using DataConfig.Models;
using DataConfig.Resources;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Web.Mvc;

namespace DataConfig.Controllers
{
    public class DatasetController : Controller
    {
        /*
        public ActionResult Load(LoadDataset model)
        {
            try
            {
                CloudTable tableMetadata = Azure.GetCloudTable(model.StorageName, model.StorageKey, Azure.Table.TableMetadata);
                if (!tableMetadata.Exists())
                {
                    return Json(new { Result = string.Empty });
                }

                IEnumerable<TableMetadata> datasets = tableMetadata.ExecuteQuery(new TableQuery<TableMetadata>());
                if (datasets != null)
                {
                    datasets = datasets.OrderBy(d => d.entityset);
                }

                return Json(new { Result = datasets });
            }
            catch (Exception)
            {
                return Json(new { Error = Messages.CannotConnectLight });
            }
        }
         * */

        //
        // POST: /Dataset/Delete

        [HttpPost]
        public ActionResult Delete(DeleteDataset model)
        {
            try
            {
                CloudTable tableMetadata = Azure.GetCloudTable(model.DataStorageName, model.DataStorageKey, Azure.Table.TableMetadata);
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
                CloudTable tableColumnsMetadata = Azure.GetCloudTable(model.DataStorageName, model.DataStorageKey, Azure.Table.TableColumnsMetadata);
                if (tableColumnsMetadata.Exists())
                {
                    TableQuery<TableColumnsMetadata> rangeQuery = new TableQuery<TableColumnsMetadata>().Where(TableQuery.GenerateFilterCondition("entityset", QueryComparisons.Equal, entitySet));
                    foreach (TableColumnsMetadata tmpEntity in tableColumnsMetadata.ExecuteQuery(rangeQuery))
                    {
                        tableColumnsMetadata.Execute(TableOperation.Delete(tmpEntity));
                    }
                }

                // Deleting rows in EntityMetadata
                CloudTable entityMetadata = Azure.GetCloudTable(model.DataStorageName, model.DataStorageKey, Azure.Table.EntityMetadata);
                if (entityMetadata.Exists())
                {
                    TableQuery<EntityMetadata> rangeQuery = new TableQuery<EntityMetadata>().Where(TableQuery.GenerateFilterCondition("entityset", QueryComparisons.Equal, entitySet));
                    foreach (EntityMetadata tmpEntity in entityMetadata.ExecuteQuery(rangeQuery))
                    {
                        entityMetadata.Execute(TableOperation.Delete(tmpEntity));
                    }
                }

                // Deleting rows in ProcessorParams
                CloudTable processorParams = Azure.GetCloudTable(model.DataStorageName, model.DataStorageKey, Azure.Table.ProcessorParams);
                if (processorParams.Exists())
                {
                    TableQuery<ProcessorParams> rangeQuery = new TableQuery<ProcessorParams>().Where(TableQuery.GenerateFilterCondition("entityset", QueryComparisons.Equal, entitySet));
                    foreach (ProcessorParams tmpEntity in processorParams.ExecuteQuery(rangeQuery))
                    {
                        processorParams.Execute(TableOperation.Delete(tmpEntity));
                    }
                }

                // Deleting the data table
                Azure.GetCloudTable(model.DataStorageName, model.DataStorageKey, entitySet).DeleteIfExists();

                return Json(new { PartitionKey = model.PartitionKey, RowKey = model.RowKey ?? string.Empty });
            }
            catch (Exception ex)
            {
                return Json(new { Error = ex.Message });
            }
        }

    }
}

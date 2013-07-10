using DataConfig.Models;
using DataConfig.Resources.Views.Home;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DataConfig.Controllers
{
    public class CatalogController : Controller
    {
        private const string ConnectionString = "DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}";
        private const string TableName = "AvailableEndpoints";

        //
        // POST: /Catalog/Load

        [HttpPost]
        public ActionResult Load(string storageName, string storageKey)
        {
            if (string.IsNullOrEmpty(storageName))
            {
                return Json(new { Error = Index.EmptyStorageName });
            }

            if (string.IsNullOrEmpty(storageKey))
            {
                return Json(new { Error = Index.EmptyStorageKey });
            }

            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(string.Format(ConnectionString, storageName, storageKey));
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                CloudTable table = tableClient.GetTableReference(TableName);

                if (!table.Exists())
                {
                    return Json(new { Error = Index.TableDoesNotExist });
                }

                TableQuery<AvailableEndpoint> query = new TableQuery<AvailableEndpoint>();

                return Json(new { Result = table.ExecuteQuery(query) });
            }
            catch (Exception ex)
            {
                return Json(new { Error = ex.Message });
            }
        }
    }
}

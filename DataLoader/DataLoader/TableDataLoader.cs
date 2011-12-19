using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using Ogdi.Data.DataLoader.Csv;

namespace Ogdi.Data.DataLoader
{
    public class TableDataLoader : DataLoader
    {
        private static readonly CloudStorageAccount s_account =
            CloudStorageAccount.Parse(ConfigurationManager.AppSettings["DataConnectionString"]);

        private static readonly TimeSpan s_retryTimeout = new TimeSpan(0, 3, 0);
        private static readonly TimeSpan s_retryWaitTime = new TimeSpan(0, 0, 15);

        private static readonly string s_metadataEntityQueryTemplate =
            "{0}?$filter=entityset eq '{1}' and entitykind eq '{2}'";

        private readonly TableOverwriteMode overwriteMode;

        public TableDataLoader(DataLoaderParams parameters, EntityProducer producer, EntityProcessor processor,
                               TableOverwriteMode overwriteMode)
            : base(parameters, producer, processor)
        {
            this.overwriteMode = overwriteMode;
        }

        protected override void OnLoadStart()
        {
            if (overwriteMode == TableOverwriteMode.Create)
            {
                // drop entity set table, if one exists
                DeleteTable(Params.TableMetadataEntity.EntitySet);

                // drop related blob container, if it exists
                DeleteContainer(Params.TableMetadataEntity.EntitySet.ToLowerInvariant());

                // create entity set table
                CreateTable(Params.TableMetadataEntity.EntitySet);
            }
            else if (!DoesTableExists(Params.TableMetadataEntity.EntitySet))
                // check that table exists if Add or Update mode.
                throw new ApplicationException("Table '" + Params.TableMetadataEntity.EntitySet +
                                               "' does not exists. Use /mode=create to wipe and reload all data and metadata.");

            // create table metadata table if it doesn't exist, otherwise delete entity related to the entity set being loaded or check that metadata not changed
            if (overwriteMode == TableOverwriteMode.Create)
            {
                if (!CreateTable(DataLoaderConstants.EntitySetTableMetadata))
                    DeleteMetadata(DataLoaderConstants.EntitySetTableMetadata, Params.TableMetadataEntity.EntitySet,
                                   Params.TableMetadataEntity.EntityKind, Params);
            }
            else
            {
                //We don't need to check table metadata when updating data. 
                //                CheckMetadataChanges(DataLoaderConstants.EntitySetTableMetadata, Params.TableMetadataEntity.EntitySet, Params.TableMetadataEntity.EntityKind, Params.TableMetadataEntity, Params, MetadataKind.Table);
                //check and update LastUpdateDate for metadata
                DateTime newDate = Params.TableMetadataEntity.LastUpdateDate;
                CheckAndUpdateMetadataLastUpdateDate(DataLoaderConstants.EntitySetTableMetadata,
                                                     Params.TableMetadataEntity.EntitySet,
                                                     Params.TableMetadataEntity.EntityKind, newDate, Params);
            }


            // create entity metadata table if it doesn't exist, otherwise delete entity related to the entity set being loaded

            if (overwriteMode == TableOverwriteMode.Create)
            {
                if (!CreateTable(DataLoaderConstants.EntitySetEntityMetadata))
                    DeleteMetadata(DataLoaderConstants.EntitySetEntityMetadata, Params.TableMetadataEntity.EntitySet,
                                   Params.TableMetadataEntity.EntityKind, Params);
            }
            else
            {
                //check that metadata not changed
                CheckMetadataChanges(DataLoaderConstants.EntitySetEntityMetadata, Params.TableMetadataEntity.EntitySet,
                                     Params.TableMetadataEntity.EntityKind, Producer.SchemaEntity, Params,
                                     MetadataKind.Entity);
            }


            // create processor params table if it doesn't exist, otherwise delete entity related to the entity set being loaded
            if (overwriteMode == TableOverwriteMode.Create)
            {
                if (!CreateTable(DataLoaderConstants.EntitySetProcessorParams))
                    DeleteMetadata(DataLoaderConstants.EntitySetProcessorParams, Params.TableMetadataEntity.EntitySet,
                                   Params.TableMetadataEntity.EntityKind, Params);
            }
            else
            {
                //check that metadata not changed
                var pars = Params as CsvToTablesDataLoaderParams;
                if (pars != null)
                    CheckMetadataChanges(DataLoaderConstants.EntitySetProcessorParams,
                                         Params.TableMetadataEntity.EntitySet, Params.TableMetadataEntity.EntityKind,
                                         pars.ProcessorParams, pars, MetadataKind.ProcessorParams);
            }
        }

        private static void DeleteTable(string tableName)
        {
            CloudTableClient tc = s_account.CreateCloudTableClient();

            tc.DeleteTableIfExist(tableName);
        }

        private static void DeleteContainer(string containerName)
        {
            CloudBlobClient bc = s_account.CreateCloudBlobClient();

            CloudBlobContainer c = bc.GetContainerReference(containerName);

            try
            {
                c.Delete();
            }
            catch (StorageClientException e)
            {
                if (e.ErrorCode != StorageErrorCode.ContainerNotFound)
                    throw;
            }
        }

        private static bool DoesTableExists(string tableName)
        {
            CloudTableClient tc = s_account.CreateCloudTableClient();
            return tc.DoesTableExist(tableName);
        }

        private static bool CreateTable(string tableName)
        {
            CloudTableClient tc = s_account.CreateCloudTableClient();

            DateTime retryUntil = DateTime.Now + s_retryTimeout;

            while (!tc.DoesTableExist(tableName))
            {
                try
                {
                    tc.CreateTable(tableName);
                    return true;
                }
                catch (StorageClientException e)
                {
                    if (e.ErrorCode == StorageErrorCode.ResourceAlreadyExists &&
                        e.StatusCode == HttpStatusCode.Conflict &&
                        DateTime.Now < retryUntil)
                    {
                        Console.WriteLine("Retrying {0}...", tableName);
                        Thread.Sleep(s_retryWaitTime);
                    }
                    else
                    {
                        Debug.WriteLine(e.ToString());
                        throw;
                    }
                }
            }

            return false;
        }

        private static void DeleteMetadata(string metadataSet, string entitySet, string entityKind,
                                           DataLoaderParams parameters)
        {
            var context = new TableContext(s_account.TableEndpoint.ToString(), s_account.Credentials, parameters)
                              {
                                  RetryPolicy =
                                      RetryPolicies.RetryExponential(RetryPolicies.DefaultClientRetryCount,
                                                                     RetryPolicies.DefaultClientBackoff)
                              };

            string query = string.Format(s_metadataEntityQueryTemplate, metadataSet, entitySet, entityKind);
            List<TableEntity> results = context.Execute<TableEntity>(new Uri(query, UriKind.Relative)).ToList();
            if (results.Count == 1)
            {
                context.DeleteObject(results.First());
                context.SaveChanges();
            }
            else if (results.Count > 1)
            {
                throw new DuplicateEntityException(query);
            }
        }

        private static void CheckMetadataChanges(string metadataSet, string entitySet, string entityKind, Entity entity,
                                                 DataLoaderParams Params, MetadataKind metadataKind)
        {
            var context = new TableContext(s_account.TableEndpoint.ToString(), s_account.Credentials, Params)
                              {
                                  RetryPolicy =
                                      RetryPolicies.RetryExponential(RetryPolicies.DefaultClientRetryCount,
                                                                     RetryPolicies.DefaultClientBackoff)
                              };

            string query = string.Format(s_metadataEntityQueryTemplate, metadataSet, entitySet, entityKind);
            List<TableEntity> results = context.Execute<TableEntity>(new Uri(query, UriKind.Relative)).ToList();
            if (results.Count == 1)
            {
                var exceprionColumns = new[]
                                           {
                                               DataLoaderConstants.PropNameEntityId,
                                               DataLoaderConstants.PropNameLastUpdateDate
                                           };
                string differences = results[0].FindDifferences(entity, exceprionColumns);
                if (differences != null)
                    throw new MetadataChangedException(entitySet, differences);
            }
            else if (results.Count > 1)
            {
                throw new DuplicateEntityException(query);
            }
            else if (results.Count == 0)
            {
                throw new MetadataNotFoundException(entitySet, metadataKind);
            }
        }

        private static void CheckAndUpdateMetadataLastUpdateDate(string metadataSet, string entitySet, string entityKind,
                                                                 DateTime lastUpdateDate, DataLoaderParams Params)
        {
            var context = new TableContext(s_account.TableEndpoint.ToString(), s_account.Credentials, Params)
                              {
                                  RetryPolicy =
                                      RetryPolicies.RetryExponential(RetryPolicies.DefaultClientRetryCount,
                                                                     RetryPolicies.DefaultClientBackoff)
                              };

            string query = string.Format(s_metadataEntityQueryTemplate, metadataSet, entitySet, entityKind);
            List<TableEntity> results = context.Execute<TableEntity>(new Uri(query, UriKind.Relative)).ToList();
            if (results.Count == 1)
            {
                DateTime oldLastUpdateDate = new TableMetadataEntity(results[0]).LastUpdateDate;
                if (oldLastUpdateDate > lastUpdateDate)
                    throw new MetadataOutdatedException(oldLastUpdateDate, lastUpdateDate);

                results[0].UpdateProperty(DataLoaderConstants.PropNameLastUpdateDate, lastUpdateDate);
                context.UpdateObject(results[0]);
                context.SaveChanges();
            }
            else if (results.Count > 1)
            {
                throw new DuplicateEntityException(query);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Services.Client;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Ogdi.Data.DataLoader
{
    internal class TableEntityProcessor : EntityProcessor
    {
        //private static CloudBlobClient _blobClient;
        private const string S_ENTITY_QUERY_TEMPLATE = "{0}?$filter={1} eq {2} and  {3} eq {4}";
        private readonly TableProcessorParams _parameters;
        private readonly TableOverwriteMode _overwriteMode;

        public TableEntityProcessor(TableProcessorParams parameters, TableOverwriteMode overwriteMode)
        {
            _parameters = parameters;
            _overwriteMode = overwriteMode;
        }

        //check that rowkey and partition key columns are listed in metadata
        public override void ValidateParams(Entity schemaEntity)
        {
            schemaEntity.ValidateProperty(_parameters.PartitionKeyPropertyName);
            schemaEntity.ValidateProperty(_parameters.RowKeyPropertyName);
        }

        public override void ProcessEntity(string entitySetName, Entity entity)
        {
            StoreEntity(entitySetName, _parameters.PartitionKeyPropertyName, _parameters.RowKeyPropertyName, entity);
        }

        public override void ProcessTableMetadataEntity(string entitySetName, Entity entity)
        {
            if (_overwriteMode != TableOverwriteMode.Create)
                return;

            StoreEntity(
                entitySetName,
                _parameters.TableMetadataPartitionKeyPropertyName,
                _parameters.TableMetadataRowKeyPropertyName,
                entity);

            //store params
            StoreEntity(
                DataLoaderConstants.EntitySetProcessorParams,
                DataLoaderConstants.ValueUniqueAutoGen,
                DataLoaderConstants.ValueUniqueAutoGen,
                _parameters);
        }

        public override void ProcessEntityMetadataEntity(string entitySetName, Entity entity)
        {
            if (_overwriteMode != TableOverwriteMode.Create)
                return;

            StoreEntity(entitySetName, _parameters.EntityMetadataPartitionKeyPropertyName,
                        _parameters.EntityMetadataRowKeyPropertyName, entity);
        }

        private static string ConvertToProperPartitionKey(string parKey)
        {
            var pk = new StringBuilder();
            for (int i = 0; i < parKey.Length; ++i)
            {
                if (char.IsLetterOrDigit(parKey[i]) || parKey[i] == ':' || parKey[i] == '-')
                {
                    pk.Append(parKey[i]);
                }
            }

            return pk.ToString();
        }

        private static string ConvertValueToString(object val)
        {
            if (val.GetType() == typeof(DateTime))
            {
                var dt = (DateTime)val;
                return string.Format("{0:D4}-{1:D2}-{2:D2}{3:D2}:{4:D2}:{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour,
                                     dt.Minute, dt.Second);
            }

            return val.ToString();
        }

        private void StoreEntity(string entitySetName, string parKeyPropName, string rowKeyPropName, Entity entity)
        {
            var account =
                CloudStorageAccount.Parse(ConfigurationManager.AppSettings["DataConnectionString"]);
            var context = new TableContext(account.TableEndpoint.ToString(), account.Credentials, _parameters)
                              {
                                  RetryPolicy = RetryPolicies.RetryExponential(5, new TimeSpan(0, 0, 1))
                              };

            var kmlSnippet = (string)entity[DataLoaderConstants.PropNameKmlSnippet];
            if (kmlSnippet != null && kmlSnippet.Length > 32 * 1024)
            {
                string blobName = Guid.NewGuid().ToString();
                string containerName = entitySetName.ToLower();
                StoreKmlSnippetAsBlob(containerName, blobName, kmlSnippet);
                entity[DataLoaderConstants.PropNameKmlSnippet] = string.Format(DataLoaderConstants.KmlSnippetReference,
                                                                               containerName, blobName);
            }

            var kmlCoords = (string)entity[DataLoaderConstants.PropNameKmlCoords];
            if (kmlCoords != null && kmlCoords.Length > 32 * 1024)
            {
                string blobName = Guid.NewGuid().ToString();
                string containerName = entitySetName.ToLower();
                StoreKmlSnippetAsBlob(containerName, blobName, kmlCoords);
                entity[DataLoaderConstants.PropNameKmlCoords] = string.Format(DataLoaderConstants.KmlSnippetReference,
                                                                              containerName, blobName);
            }

            TableEntity tableEntity = null;
            bool isUpdate = false;
            if (_parameters != null)
            {
                string parKey;
                object pk = entity[parKeyPropName];
                if (string.IsNullOrEmpty(entity.Number))
                {
                    parKey = (pk != null) ? ConvertToProperPartitionKey(ConvertValueToString(pk)) : entity.Id.ToString();
                }
                else
                {
                    parKey = entity.Number;
                }

                string rowKey = null;
                object rk = entity[rowKeyPropName];
                if (rowKeyPropName.ToLower() == DataLoaderConstants.ValueUniqueAutoGen)
                {
                    rowKey = Guid.NewGuid().ToString();
                }
                else
                {
                    rowKey = (rk != null) ? ConvertValueToString(entity[rowKeyPropName]) : Guid.NewGuid().ToString();
                }


                //try to load entity from storage
                if (_overwriteMode == TableOverwriteMode.Add || _overwriteMode == TableOverwriteMode.Update)
                {
                    tableEntity = LoadEntity(context, entitySetName, rowKeyPropName, rowKey, rk, parKeyPropName, parKey, pk);
                    if (tableEntity != null && _overwriteMode == TableOverwriteMode.Add)
                        throw new EntityAlreadyExistsException(entitySetName, rowKeyPropName, rowKey, parKeyPropName,
                                                               parKey);
                    if (tableEntity != null)
                    {
                        tableEntity.UpdateEntity(entity);
                        isUpdate = true;
                    }
                }
                //if not found, create new
                if (tableEntity == null)
                    tableEntity = new TableEntity(entity, parKey, rowKey);
            }
            else
            {
                tableEntity = new TableEntity(entity);
            }

            if (!isUpdate)
                context.AddObject(entitySetName, tableEntity);
            else
                context.UpdateObject(tableEntity);

            try
            {
                context.SaveChanges();
            }
            catch (StorageClientException e)
            {
                if (e.ErrorCode == StorageErrorCode.ResourceAlreadyExists && e.StatusCode == HttpStatusCode.Conflict)
                {
                    throw new DuplicateEntityException(tableEntity.ToString(), e);
                }
            }
            catch (DataServiceRequestException e)
            {
                if (e.InnerException != null &&
                    ((DataServiceClientException)e.InnerException).StatusCode == (int)HttpStatusCode.Conflict)
                {
                    throw new DuplicateEntityException(tableEntity.ToString(), e);
                }
            }
        }

        private TableEntity LoadEntity(TableContext context, string entitySetName,
                                       string rowKeyColumn, string rowKeyValue, object origianlRowKeyValue,
                                       string parKeyColumn, string parKeyValue, object origianlParKeyValue)
        {
            // For string values add ''
            string rk = GetValueForQuery(rowKeyValue, origianlRowKeyValue);
            string pk = GetValueForQuery(parKeyValue, origianlParKeyValue);

            if (rowKeyColumn == "New.Guid") rowKeyColumn = "RowKey";
            if (parKeyColumn == "New.Guid") parKeyColumn = "PartitionKey";

            // columns should be in lower case
            string query = string.Format(S_ENTITY_QUERY_TEMPLATE, entitySetName, rowKeyColumn.ToLower(), rk, parKeyColumn.ToLower(), pk);

            List<TableEntity> results = context.Execute<TableEntity>(new Uri(query, UriKind.Relative)).ToList();
            if (results.Count == 0)
            {
                return null;
            }

            if (results.Count > 1)
            {
                throw new DuplicateEntityException(query);
            }

            return results[0];
        }

        private string GetValueForQuery(string stringValue, object originalValue)
        {
            // New value (Guid)
            if (originalValue == null)
            {
                return string.Format("'{0}'", stringValue.Replace("'","''"));
            }
            // String
            if (originalValue is string)
            {
                return string.Format("'{0}'", originalValue.ToString().Replace("'", "''"));
            }
            // Numeric
            return originalValue.ToString();
        }

        private void StoreKmlSnippetAsBlob(string containerName, string blobName, string kmlSnippet)
        {
            CloudStorageAccount acc =
                CloudStorageAccount.Parse(ConfigurationManager.AppSettings["DataConnectionString"]);

            CloudBlobClient bc = acc.CreateCloudBlobClient();

            CloudBlobContainer c = bc.GetContainerReference(containerName);

            if (c.CreateIfNotExist())
            {
                c.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Container });
            }

            CloudBlob b = c.GetBlobReference(blobName);

            b.Properties.ContentType = DataLoaderConstants.ContentTypeKmlSnippet;

            b.UploadByteArray(Encoding.UTF8.GetBytes(kmlSnippet));
        }
    }
}
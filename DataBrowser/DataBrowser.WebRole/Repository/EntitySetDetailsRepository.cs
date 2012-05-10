
/* 
 *   Copyright (c) Microsoft Corporation.  All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of Microsoft Corporation nor the names of its contributors 
 *       may be used to endorse or promote products derived from this software
 *       without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE REGENTS AND CONTRIBUTORS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE REGENTS AND CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using Ogdi.InteractiveSdk.Mvc.Models;
using Ogdi.InteractiveSdk.Mvc;
using Ogdi.InteractiveSdk.Mvc.Repository;

namespace Ogdi.InteractiveSdk.Mvc.Repository
{
    internal class EntitySetDetailsRepository
    {
        #region Constructors

        // Fxcop : added private constructor to prevent the compiler from generating
        // a default constructor.
        /// <summary>
        /// Default Constructor
        /// </summary>
        private EntitySetDetailsRepository()
        {
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Fetches the EntitySetDetails and returns EntitySetDetails object
        /// XML format will be different depending on tableName parameter.
        /// </summary>
        /// <param name="container">Container: Alias</param>
        /// <param name="tableName">EntitySetName</param>
        /// <param name="filter">Filter Parameters</param>
        /// <param name="pageSize">PageSize - For Paging Purpose</param>
        /// <param name="nextPartitionKey">Next Partition Key - 
        /// For Paging Purpose</param>
        /// <param name="nextRowKey">Next Row Key - For Paging Purpose</param>
        /// <param name="isFullData">true if required full data else false</param>
        /// <returns>returns an object of EntitySetDetails</returns>
        internal static EntitySetDetails GetBrowserData(string container,
            string tableName, string filter, int pageSize, string nextPartitionKey, 
            string nextRowKey, bool isFullData)
        {
            // Declare object of class EntitySetDetails
            EntitySetDetails entitySetDetails = null;

            // Validatie the parameters
            if ((!String.IsNullOrEmpty(container)) && 
                !(String.IsNullOrEmpty(tableName))
               && pageSize > 0)
            {

                // Create an instance of class Storage
                IsdkStorageProviderInterface storage = Helper.ServiceObject;

                // Define entitySetDetails
                entitySetDetails = new EntitySetDetails();

                // Set the properties of entitySetDetails object
                entitySetDetails.ContainerAlias = container;
                entitySetDetails.EntitySetName = tableName;

                // Set the filter
                string tableNameFilter = "entityset eq '" + tableName + "'";

                // Fetches the data from Azure Table Storage
                XElement metaDataXML = storage.GetMetadata(container,
                    Resources.MetaDataTableName, tableNameFilter);
                
                // Remove the unnecessary columns
                var properties = metaDataXML.Elements("properties");
                properties.Elements("entityset").Remove();
                properties.Elements("entitykind").Remove();

                // Set the column list
                var propertyMetaData = metaDataXML.Elements("properties").First().Elements();

                // Add the column names in the detailsTable of the object entitySetDetails
                foreach (var property in propertyMetaData)
                {
                    if (property.Name == "entityid")
                    {
                        entitySetDetails.DetailsTable.Columns.Add(
                            property.Name.ToString(), Type.GetType("System.Guid"));
                    }
                    else
                    {                        
                        entitySetDetails.DetailsTable.Columns.Add(
                            property.Name.ToString(), Type.GetType(property.Value));
                    }
                }

                // Get the browser data
                XElement browserDataXML = null;
                if (isFullData == false)
                { 
                     browserDataXML = storage.GetData(container, tableName,
                         filter, pageSize, nextPartitionKey, nextRowKey);

                     // set the properties of entitySetDetails object depending on the
                    // fetched results
                     entitySetDetails.NextPartitionKey = 
                         browserDataXML.Attribute("nextPartitionKey").Value;

                     entitySetDetails.NextRowKey = 
                         browserDataXML.Attribute("nextRowKey").Value;

                     entitySetDetails.CurrentPartitionKey = 
                         browserDataXML.Attribute("currentPartitionKey").Value;

                     entitySetDetails.CurrentRowKey =
                         browserDataXML.Attribute("currentRowKey").Value;
                }
                else
                {
                    browserDataXML = storage.GetData(container, tableName, filter);
                }

                // validate the XElement
                if (browserDataXML != null)
                {
                    // for each XML node, fetch the internal details
                    foreach (var element in browserDataXML.Elements("properties"))
                    {
                        try
                        {
                            // Get the row list for each elements
                            DataRow row = entitySetDetails.DetailsTable.NewRow();

                            // Add each cell in the row
                            foreach (var cell in element.Elements())
                            {
                                try
                                {
                                    row[cell.Name.ToString()] = cell.Value.ToString();
                                }
                                catch (Exception) { } //To handle the wrong cells
                            }

                            // Add the newly created row in the table
                            entitySetDetails.DetailsTable.Rows.Add(row);
                        }
                        catch (Exception)
                        {
                            // To handle the wrong rows
                        }
                    }
                }
            }

            // Return entitySetDetails
            return entitySetDetails;
        }


        /// <summary>
        /// This method gives the meta data for the given container & entitySet
        /// </summary>
        /// <param name="container">Container: Alias</param>
        /// <param name="tableName">EntitySetName</param>
        /// <returns>returns an object of EntitySetDetails</returns>
        internal static EntitySetDetails GetMetaData(string container, 
            string tableName)
        {
            // Declare object of class EntitySetDetails
            EntitySetDetails entitySetDetails = null;

            // Validatie the parameters
            if ((!String.IsNullOrEmpty(container)) && 
                !(String.IsNullOrEmpty(tableName)))
            {

                // Create an instance of class Storage
                IsdkStorageProviderInterface storage = Helper.ServiceObject;

                // Define entitySetDetails
                entitySetDetails = new EntitySetDetails();

                // Set the properties of entitySetDetails object
                entitySetDetails.ContainerAlias = container;
                entitySetDetails.EntitySetName = tableName;

                // Set the filter
                string tableNameFilter = "entityset eq '" + tableName + "'";

                // Fetches the data from Azure Table Storage
                XElement metaDataXML = storage.GetMetadata(container, 
                    Resources.MetaDataTableName, tableNameFilter);

                // Remove the unnecessary columns
                var properties = metaDataXML.Elements("properties");
                properties.Elements("entityset").Remove();
                properties.Elements("entitykind").Remove();

                // Set the column list
                var propertyMetaData = metaDataXML.Elements("properties").First().Elements();

                // Add the column names in the detailsTable of the object entitySetDetails
                foreach (var property in propertyMetaData)
                {
                    if (property.Name == "entityid")
                    {
                        entitySetDetails.DetailsTable.Columns.Add(property.Name.ToString(),
                            Type.GetType("System.Guid"));
                    }
                    else
                    {
                        entitySetDetails.DetailsTable.Columns.Add(property.Name.ToString(), 
                            Type.GetType(property.Value));
                    }
                }
            }

            return entitySetDetails;
        }
        #endregion
    }
}

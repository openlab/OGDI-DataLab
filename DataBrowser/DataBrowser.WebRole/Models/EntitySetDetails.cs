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
using System.Globalization;

namespace Ogdi.InteractiveSdk.Mvc.Models
{
    [Serializable]
    public class EntitySetDetails
    {
        #region Private Members

        // Private member for EntitySetDetails.ContainerAlias
        private string containerAlias = string.Empty;

        // Private member for EntitySetDetails.EntitySetName
        private string entitySetName = string.Empty;

        // Private member for EntitySetDetails.DetailsTable
        private DataTable detailsTable;

        // Private member for EntitySetDetails.NextPartitionKey
        private string nextPartitionKey = string.Empty;

        // Private member for EntitySetDetails.NextRowKey
        private string nextRowKey = string.Empty;

        // Private member for EntitySetDetails.CurrentPartitionKey
        private string currentPartitionKey = string.Empty;

        // Private member for EntitySetDetails.CurrentRowKey
        private string currentRowKey = string.Empty;

        #endregion

        #region Properties
        /// <summary>
        /// This property represents ContainerAlias of EntitySetDetails
        /// </summary>
        public string ContainerAlias
        {
            get { return containerAlias; }
            set { containerAlias = value; }
        }

        /// <summary>
        /// This property represents EntitySetName of EntitySetDetails
        /// </summary>
        public string EntitySetName
        {
            get { return entitySetName; }
            set { entitySetName = value; }
        }

        /// <summary>
        /// This property represents DetailsTable of EntitySetDetails
        /// This table will have list of all rows present in a 
        /// requested table.
        /// For instance, if we choose banklocations entityset,
        /// the DetailsTable will have records of all bank
        /// details under that container.
        /// </summary>
        public DataTable DetailsTable
        {
            get { return detailsTable; }
            set { detailsTable = value; }
        }

        /// <summary>
        /// This property represents nextPartitionKey used for paging
        /// </summary>
        public string NextPartitionKey
        {
            get { return nextPartitionKey; }
            set { nextPartitionKey = value; }
        }

        /// <summary>
        /// This property represents nextRowKey used for paging
        /// </summary>
        public string NextRowKey
        {
            get { return nextRowKey; }
            set { nextRowKey = value; }
        }

        /// <summary>
        /// This property represents currentPartitionKey used for paging
        /// </summary>
        public string CurrentPartitionKey
        {
            get { return currentPartitionKey; }
            set { currentPartitionKey = value; }
        }

        /// <summary>
        /// This property represents currentRowKey used for paging
        /// </summary>
        public string CurrentRowKey
        {
            get { return currentRowKey; }
            set { currentRowKey = value; }
        }     

        #endregion

        #region Constructors

        /// <summary>
        /// Default Constructor
        /// </summary>
        public EntitySetDetails()
        {
            this.ContainerAlias = string.Empty;
            this.EntitySetName = string.Empty;
            this.DetailsTable = new DataTable();
            //Fxcop:
            this.DetailsTable.Locale = CultureInfo.InvariantCulture;
            this.NextPartitionKey = string.Empty;
            this.NextRowKey = string.Empty;
            this.CurrentPartitionKey = string.Empty;
            this.CurrentRowKey = string.Empty;
        }

        /// <summary>
        /// Paramaterized Constructor
        /// </summary>
        /// <param name="containerAlias">containerAlias of 
        /// EntitySetDetails</param>
        /// <param name="entitySetName">entitySetName of
        /// EntitySetDetails</param>
        /// <param name="detailsTable">detailsTable of
        /// EntitySetDetails</param>
        /// <param name="nextPartitionKey">next partition
        /// key for paging</param>
        /// <param name="nextRowKey">next row key for
        /// paging</param>
        /// <param name="currentPartitionKey">current partition
        /// key for paging</param>
        /// <param name="currentRowKey">current row key for
        /// paging</param>
        public EntitySetDetails(string containerAlias, 
            string entitySetName, DataTable detailsTable, string nextPartitionKey,
            string nextRowKey, string currentPartitionKey, string currentRowKey)
        {
            this.ContainerAlias = containerAlias;
            this.EntitySetName = entitySetName;
            this.DetailsTable = detailsTable;
            //Fxcop:
            this.DetailsTable.Locale = CultureInfo.InvariantCulture;
            this.NextPartitionKey = nextPartitionKey;
            this.NextRowKey = nextRowKey;
            this.CurrentPartitionKey = currentPartitionKey;
            this.CurrentRowKey = currentRowKey;
        }
        #endregion

    }
}

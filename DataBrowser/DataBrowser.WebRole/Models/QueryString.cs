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

namespace Ogdi.InteractiveSdk.Mvc.Models
{
    /// <summary>
    /// Enumeration for ViewType on DataBrowser page
    /// </summary>
    public enum BrowserViewType
    {
        DataView,
        MapView,
        BarChart,
        PieChart,
        None
    }

    /// <summary>
    /// Enumeration for TagType for views on DataBrowser page
    /// </summary>
    public enum BrowserTagType
    {
        SampleCode,
        Result,
        None
    }

    /// <summary>
    /// This class in created to handle bookmarking.
    /// </summary>
    [Serializable]
    public class QueryString
    {
        #region Privates

        // Private member to store alias of the container
        private string containerAlias = string.Empty;

        // Private member to store name of the EntitySet
        private string entitySetName = string.Empty;

        // Private member to store filter expression in the QueryBox
        private string filter = string.Empty;
        
        // Private member to store type of the tag of type BrowserTagType
        private BrowserTagType tagType = BrowserTagType.None;

        // Private member to store type of the view of type BrowserViewType
        private BrowserViewType viewType = BrowserViewType.None;

        // Private member to store the filepath of the selected language
        private string languagePath = string.Empty;

        // Private member to store mode of the map i.e. 2D or 3D
        private int mapMode;

        // Private member to store latitude of the center of the map
        private double latitude;

        // Private member to store longitude of the center of the map
        private double longitude;

        // Private member to store zoom level of the map
        private int zoomLevel;

        // Private member to store style of the map like Aeriel / Road / etc.
        private string style = string.Empty;

        // Private member to store SceneId of the Bird's Eye View
        private long sceneId;

        // Private member to store BirdseyeSceneOrientation of 
        // the Bird's Eye View
        private string birdseyeSceneOrientation;

        // Private member to store  column name for x-axis
        private string xCol = string.Empty;

        // Private member to store date range for x-axis
        private string xRange = string.Empty;

        // Private member to store option for y-axis
        private string yOption = string.Empty;

        // Private member to store column name on y-axis
        private string yCol = string.Empty;

        // Private member to store coulmn option for Y-axis
        private string yColOption = string.Empty;
        
        #endregion

        #region Properties

        /// <summary>
        /// This property represents alias of the container
        /// </summary>
        public string ContainerAlias
        {
            get { return containerAlias; }
            set { containerAlias = value; }
        }

        /// <summary>
        /// This property represents name of the EntitySet
        /// </summary>
        public string EntitySetName
        {
            get { return entitySetName; }
            set { entitySetName = value; }
        }

        /// <summary>
        /// This property represents filter expression in the QueryBox
        /// </summary>
        public string Filter
        {
            get { return filter; }
            set { filter = value; }
        }

        /// <summary>
        /// This property represents type of the tag of type BrowserTagType
        /// </summary>
        public BrowserTagType TagType
        {
            get { return tagType; }
            set { tagType = value; }
        }

        /// <summary>
        /// This property represents type of the view of type BrowserViewType
        /// </summary>
        public BrowserViewType ViewType
        {
            get { return viewType; }
            set { viewType = value; }
        }

        /// <summary>
        /// This property represents the filepath of the selected language
        /// </summary>
        public string LanguagePath
        {
            get { return languagePath; }
            set { languagePath = value; }
        }
        
        /// <summary>
        /// This property represents mode of the map i.e. 2D or 3D
        /// </summary>
        public int MapMode
        {
            get { return mapMode; }
            set { mapMode = value; }
        }

        /// <summary>
        /// This property represents latitude of the center of the map
        /// </summary>
        public double Latitude
        {
            get { return latitude; }
            set { latitude = value; }
        }

        /// <summary>
        /// This property represents longitude of the center of the map
        /// </summary>
        public double Longitude
        {
            get { return longitude; }
            set { longitude = value; }
        }

        /// <summary>
        /// This property represents zoom level of the map
        /// </summary>
        public int ZoomLevel
        {
            get { return zoomLevel; }
            set { zoomLevel = value; }
        }

        /// <summary>
        /// This property represents style of the map like Aeriel / Road / etc.
        /// </summary>
        public string Style
        {
            get { return style; }
            set { style = value; }
        }

        /// <summary>
        /// This property represents SceneId of the Bird's Eye View
        /// </summary>
        public long SceneId
        {
            get { return sceneId; }
            set { sceneId = value; }
        }

        /// <summary>
        /// This property represents BirdseyeSceneOrientation
        /// of the Bird's Eye View
        /// </summary>
        public string BirdseyeSceneOrientation
        {
            get { return birdseyeSceneOrientation; }
            set { birdseyeSceneOrientation = value; }
        }

        /// <summary>
        /// This property represents coulmn option for Y-axis
        /// </summary>
        public string YColOption
        {
            get { return yColOption; }
            set { yColOption = value; }
        }

        /// <summary>
        /// This property represents column name on y-axis
        /// </summary>
        public string YCol
        {
            get { return yCol; }
            set { yCol = value; }
        }

        /// <summary>
        /// This property represents option for y-axis
        /// </summary>
        public string YOption
        {
            get { return yOption; }
            set { yOption = value; }
        }

        /// <summary>
        /// This property represents date range for x-axis
        /// </summary>
        public string XRange
        {
            get { return xRange; }
            set { xRange = value; }
        }

        /// <summary>
        /// This property represents column name for x-axis
        /// </summary>
        public string XCol
        {
            get { return xCol; }
            set { xCol = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default Constructor
        /// </summary>
        public QueryString()
        {
            this.ContainerAlias = string.Empty;            
            this.EntitySetName = string.Empty;
            this.Filter = string.Empty;
            this.LanguagePath = string.Empty;
            this.Latitude = 0.0;
            this.Longitude = 0.0;
            this.MapMode = 0;            
            this.Style = string.Empty;
            this.TagType = BrowserTagType.None;
            this.ViewType = BrowserViewType.None;
            this.ZoomLevel = 0;
            // Commented because of Fx-cop warning: Microsoft.Performance : 
            // 'QueryString.QueryString()' initializes field 
            // 'QueryString.sceneId' of type 'long' to 0. Remove this 
            // initialization because it will be done 
            // automatically by the runtime.            
            // this.sceneId = 0;
            this.birdseyeSceneOrientation = string.Empty;
        }

        /// <summary>
        /// Paramaterized Constructor
        /// </summary>        
        /// <param name="containerAlias">alias of the container</param>
        /// <param name="currentPartitionKey">currentPartitionKey used
        /// for paging</param>
        /// <param name="currentRowKey">currentRowKey  used for
        /// paging</param>
        /// <param name="entitySetName">name of the 
        /// entitySet</param>
        /// <param name="filter">filter expression</param>        
        /// <param name="languagePath">language path</param>
        /// <param name="latitude">latitude of the center of 
        /// the map</param>
        /// <param name="longitude">longitude of the center 
        /// of the map</param>
        /// <param name="mapMode">mode of the map</param>
        /// <param name="nextPartitionKey">nextPartitionKey 
        /// used for paging</param>
        /// <param name="nextRowKey">nextRowKey used for
        /// paging</param>        
        /// <param name="style">style of the map</param>
        /// <param name="tagType">type of the tag</param>
        /// <param name="viewType">type of the view</param>
        /// <param name="zoomLevel">zoom level of the
        /// map</param>
        /// <param name="sceneId">SceneId of the Bird's 
        /// Eye View</param>
        /// <param name="birdseyeSceneOrientation">
        /// BirdseyeSceneOrientation of the Bird's Eye
        /// View</param>
        public QueryString(string containerAlias, 
            string entitySetName, string filter,
            string languagePath, double latitude, double longitude,
            int mapMode, 
            string style, BrowserTagType tagType, BrowserViewType viewType,
            int zoomLevel, long sceneId, string birdseyeSceneOrientation)
        {
            this.ContainerAlias = containerAlias;           
            this.EntitySetName = entitySetName;
            this.Filter = filter;
            this.LanguagePath = languagePath;
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.MapMode = mapMode;
            this.Style = style;
            this.TagType = tagType;
            this.ViewType = viewType;
            this.ZoomLevel = zoomLevel;
            this.SceneId = sceneId;
            this.BirdseyeSceneOrientation = birdseyeSceneOrientation;
        }
        #endregion
    }
}


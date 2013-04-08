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
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;
using System.Xml.Linq;
using Ogdi.Azure.Configuration;
using Ogdi.Azure.Data;
using Ogdi.InteractiveSdk.Mvc.App_GlobalResources;
using Ogdi.InteractiveSdk.Mvc.Models;
using Ogdi.InteractiveSdk.Mvc.Repository;

namespace Ogdi.InteractiveSdk.Mvc.Controllers
{
	#region ContinuationToken

	/// <summary>
	/// Class used to store paging related keys
	/// </summary>
	[Serializable]
	public class ContinuationToken
	{
		public string NextPartitionKey { get; set; }
		public string NextRowKey { get; set; }
	}

	#endregion

	#region Enumerations

	/// <summary>
	/// Enumeration for different button clicks possible on the page
	/// </summary>
	enum ButtonClicked
	{
		Run,
		Previous,
		Next
	}

	/// <summary>
	/// Enumeration for different links of paging
	/// </summary>
	enum PagingConstants
	{
		Next,
		Previous
	}

	/// <summary>
	/// Enumeration for Date Ranges for Graphs
	/// </summary>
	enum DateRangeConstants
	{
		Daily,
		Monthly,
		Quarterly,
		Yearly,
		None
	}

	/// <summary>
	/// Enumeration for options selected for vertical-axis
	/// </summary>
	enum VerticalOptions
	{
		Option1,
		Option2
	}

	/// <summary>
	/// Enumeration for option selected for column on vertical-axis
	/// </summary>
	enum VerticalColumnOptions
	{
		Aggregate,
		Average
	}

	/// <summary>
	/// Enumeration for File Formats in which the Sample Code will 
	/// be represented
	/// </summary>
	enum FileFormat
	{
		CSharpFormat,
		HtmlFormat
	}

	#endregion

	[HandleError]
	public class DataBrowserController : Controller
	{
		private readonly DataBrowserModel viewDataModel = new DataBrowserModel();

		#region Common Public Methods

		/// <summary>
		/// This action provides required ViewData for loading DataBrowser page
		/// </summary>
		/// <param name="container">Container Name in string format</param>
		/// <param name="entitySetName">EntitySet Name in string format</param>
		/// <returns>Returns view with all necessary data</returns>
		public ActionResult Index(string container, string entitySetName)
		{
			// Hide error labels
			HideError();

			// Clear the cache
			if (HttpContext.Session["ChartData"] != null)
				HttpContext.Session.Remove("ChartData");
			if (HttpContext.Session["ChartFilter"] != null)
				HttpContext.Session.Remove("ChartFilter");

			// Create an instance of class QueryString
			QueryString queryString = new QueryString();

			// Set ContainerAlias & EntitySetName of queryString
			queryString.ContainerAlias = container;
			queryString.EntitySetName = entitySetName;

			try
			{
				// Reset ViewData
				ResetViewData();
				ResetMapViewData();

				// Set the ViewData for controls according to queryString
				LoadControls(container, entitySetName);

				this.ViewData.Model = viewDataModel;

				return View();
			}
			catch (Exception ex)
			{
				ShowError(ex.Message);

				ViewData.Model = viewDataModel;

				// returns the PatialView to load Error
				return PartialView("ErrorView", ViewData.Model);

			}
		}

		/// <summary>
		/// This action is called when an error occurs in client script
		/// </summary>
		/// <param name="error">Error from client script in string format</param>
		/// <returns>PartialView-ErrorView with neccessay data required for its rendering</returns>
		public ActionResult ShowClientsideError(string error)
		{
			// Set error parameters
			ShowError(error);

			ViewData.Model = viewDataModel;

			// returns the PatialView to load Error
			return PartialView("ErrorView", ViewData.Model);
		}

		#endregion

		#region DataView Public Methods

		/// <summary>
		/// This action provides necessary ViewData for loading Sample code for DataView according to 
		/// language file name passed
		/// </summary>
		/// <param name="selectedFileName">File name for which the sample code
		/// is to be loaded</param>
		/// <returns>PartialView-SampleCodeDataView with neccessay data required for its rendering</returns>
		[AcceptVerbs(HttpVerbs.Post)]
		public ActionResult LoadDataViewSampleCode(string selectedFileName, string container, string entitySet)
		{
			// Hide error labels
			HideError();
			try
			{
				// Set the viewDataModel.SampleCodeDataView
				viewDataModel.SampleCodeDataView = GetSampleCode(selectedFileName, FileFormat.CSharpFormat, container, entitySet);

				ViewData.Model = viewDataModel;

				// returns the PatialView to load SampleCode
				return PartialView("SampleCodeDataView", ViewData.Model);
			}
			catch (Exception ex)
			{
				ShowError(ex.Message);

				ViewData.Model = viewDataModel;

				// returns the PatialView to load Error
				return PartialView("ErrorView", ViewData.Model);
			}

		}

		/// <summary>
		/// This function gets the Daisy data saves it to a file which can be saved by client. 
		/// </summary>
		/// <param name="container">container alias</param>
		/// <param name="entitySet">entity set name</param>
		/// <param name="filter">filter criteria</param>
		public void Download(string container, string entitySet, string filter)
		{
			if (DownloadFullData(container, entitySet, filter, ".xml"))
			{
				return;
			}

			// Get the Daisy data
			XDocument daisyData = Helper.ServiceObject.GetDataAsDaisy(container,
				entitySet, string.IsNullOrEmpty(filter) ? null : filter.Trim().Equals(UIConstants.DBPC_NoFilterText) ? null : filter.Trim());

			//Save Dialog Box for the users to save the translated DTBook Xml file 
			// in their local directories
			Response.Clear();
			Response.ContentType = UIConstants.DBPC_DownloadContentType;
			string docName = daisyData.Elements("dtbook").Elements("book").Elements("frontmatter").
				Elements("doctitle").FirstOrDefault().Value;

			Response.AppendHeader("Content-Disposition", "attachment; filename=" +
				docName + ".xml");
			System.IO.StreamWriter streamWriter =
				new System.IO.StreamWriter(Response.OutputStream, System.Text.Encoding.Unicode);
			daisyData.Save(streamWriter);
			Response.End();
		}

		/// <summary>
		/// This function gets the CSV formated data saves it to a file which can be saved by client. 
		/// </summary>
		/// <param name="container">container alias</param>
		/// <param name="entitySet">entity set name</param>
		/// <param name="filter">filter criteria</param>
		public void DownloadCsv(string container, string entitySet, string filter)
		{
			if (DownloadFullData(container, entitySet, filter, ".csv"))
			{
				return;
			}

			string dataInCsvFormat = Helper.ServiceObject.GetdDataAsCsv(container,
				entitySet, string.IsNullOrEmpty(filter) ? null : filter.Trim().Equals(UIConstants.DBPC_NoFilterText) ? null : filter.Trim());

			Response.Clear();
			Response.ContentType = "text/csv";

			Response.OutputStream.Write(new UTF8Encoding().GetBytes(dataInCsvFormat), 0, new UTF8Encoding().GetByteCount(dataInCsvFormat));

			Response.AppendHeader("Content-Disposition", "attachment; filename=" + entitySet + ".csv");
			Response.End();


		}
		
		/// <summary>
		/// Download full data for dataset alredy stored in blob
		/// </summary>
		/// <param name="container">container alias</param>
		/// <param name="entitySet">entity set name</param>
		/// <param name="filter">filter criteria</param>
		/// <param name="ext">file extension</param>
		/// <returns></returns>
		private bool DownloadFullData(string container, string entitySet, string filter, string ext)
		{
			if (filter.Trim().Equals(UIConstants.DBPC_NoFilterText))
			{
				Response.Redirect(BlobRepositary.GetBlobUrl(container, entitySet, ext));
				return true;
			}
			return false;
		}

		/// <summary>
		/// This action provides necessary ViewData for loading records(Next or Previous) for DataView according to
		/// link type passed to it
		/// </summary>
		/// <param name="container">container name in string format</param>
		/// <param name="entitySet">EntitySet name in straing format</param>
		/// <param name="filter"> Filter name in string format</param>
		/// <param name="link">link string: "Next" for Next, "Previous" for 
		/// Previous, "Start" for Start</param>
		/// <returns>PartialView-DataView with neccessay data required for its rendering</returns>
		[AcceptVerbs(HttpVerbs.Post)]
		public ActionResult PagingClicked(string container, string entitySet, string filter, string link)
		{

			filter = string.IsNullOrEmpty(filter) ? null : filter.Trim().Equals(UIConstants.DBPC_NoFilterText) ? null : filter.Trim();

			// Hide error labels
			HideError();

			try
			{
				PagingConstants currentLink =
					(PagingConstants)Enum.Parse(typeof(PagingConstants), link);

				// Check for which link is clicked
				switch (currentLink)
				{
					// Next link
					case PagingConstants.Next:
						NextLinkClicked(container, entitySet, filter);
						break;

					// Previous link
					case PagingConstants.Previous:
						PreviousLinkClicked(container, entitySet, filter);
						break;
				}

				SetURLs(container, entitySet, filter);
				viewDataModel.Container = container;

				viewDataModel.EntitySetName = entitySet;

				ViewData.Model = viewDataModel;

				// returns the PatialView to load DataView
				return PartialView("DataView", ViewData.Model);
			}
			catch (WebException ex)
			{
				ShowServiceError(ex);

				ViewData.Model = viewDataModel;

				// returns the PatialView to load Error
				return PartialView("ErrorView", ViewData.Model);
			}
			catch (Exception)
			{
				ShowUnexpectedError();

				ViewData.Model = viewDataModel;

				// returns the PatialView to load Error
				return PartialView("ErrorView", ViewData.Model);
			}
		}

		/// <summary>
		/// This action provides necessary ViewData for loading records for DataView according to
		/// filterQuery passed to it.
		/// </summary>
		/// <param name="container">Container Name in string format</param>
		/// <param name="entitySet">Param Name in string format</param>
		/// <param name="filter">Query filter value in string format</param>
		/// <returns>PartialView-DataView with neccessay data required for its rendering</returns>
		[AcceptVerbs(HttpVerbs.Post)]
		public ActionResult RunButtonClicked(string container, string entitySet, string filter)
		{

			filter = string.IsNullOrEmpty(filter) ? null : filter.Trim().Equals(UIConstants.DBPC_NoFilterText) ? null : filter.Trim();

			// Hide error labels
			HideError();

			SetURLs(container, entitySet, filter);

			// Create an instance of class EntitySetDetails
			EntitySetDetails result = new EntitySetDetails();

			// Get the EntitySetDetails
			try
			{
				result = EntitySetDetailsRepository.GetBrowserData(container, entitySet, filter, Convert.ToInt32(UIConstants.DBPC_EntityDetailsTableSize, CultureInfo.InvariantCulture), null, null, false);

				// Page Load is like clicking the Run button automatically 
				// so we just set the value to ButtonClicked.Run
				SetPagingAndBind(result, ButtonClicked.Run, null);

				viewDataModel.Container = container;

				viewDataModel.EntitySetName = entitySet;

				viewDataModel.FilterText = filter;

				ViewData.Model = viewDataModel;

				// returns the PatialView to load DataView
				return PartialView("DataView", ViewData.Model);
			}
			catch (WebException ex)
			{
				ShowServiceError(ex);

				ViewData.Model = viewDataModel;

				// returns the PatialView to load Error
				return PartialView("ErrorView", ViewData.Model);

			}
			catch (Exception)
			{
				ShowUnexpectedError();

				ViewData.Model = viewDataModel;

				// returns the PatialView to load Error
				return PartialView("ErrorView", ViewData.Model);
			}
		}

		#endregion

		#region MapView Public Methods

		/// <summary>
		/// This action provides necessary ViewData for loading Sample code for MapView according to 
		/// language file name passed
		/// </summary>
		/// <param name="selectedFileName">File name for which the sample code
		/// is to be loaded</param>
		/// <returns>PartialView-SampleCodeMapView with neccessay data required for its rendering</returns>
		[AcceptVerbs(HttpVerbs.Post)]
		public ActionResult LoadMapViewSampleCode(string selectedFileName, string container, string entitySet)
		{
			// Hide error labels
			HideError();

			try
			{
				// Set the viewDataModel.SampleCodeMapView
				viewDataModel.SampleCodeMapView = GetSampleCode(selectedFileName, FileFormat.HtmlFormat, container, entitySet);

				ViewData.Model = viewDataModel;

				// returns the PatialView to load SampleCode
				return PartialView("SampleCodeMapView", ViewData.Model);
			}
			catch (Exception ex)
			{
				ShowError(ex.Message);

				ViewData.Model = viewDataModel;

				// returns the PatialView to load Error
				return PartialView("ErrorView", ViewData.Model);
			}

		}

		#endregion

		#region BarChart View Public Methods

		/// <summary>
		/// This action provides necessary ViewData for loading Sample code for BarChartView according to 
		/// language file name passed
		/// </summary>
		/// <param name="selectedFileName">File name for which the sample code
		/// is to be loaded</param>
		/// <returns>PartialView-SampleCodeBarChartView with neccessay data required for its rendering</returns>
		[AcceptVerbs(HttpVerbs.Post)]
		public ActionResult LoadBarChartSampleCode(string selectedFileName, string container, string entitySet)
		{
			// Hide error labels
			HideError();

			try
			{
				// Set the viewDataModel.SampleCodeBarChartView
				viewDataModel.SampleCodeBarChartView = GetSampleCode(selectedFileName, FileFormat.CSharpFormat, container, entitySet);

				ViewData.Model = viewDataModel;

				// returns the PatialView to load SampleCode
				return PartialView("SampleCodeBarChartView", ViewData.Model);
			}
			catch (Exception ex)
			{
				ShowError(ex.Message);

				ViewData.Model = viewDataModel;

				// returns the PatialView to load Error
				return PartialView("ErrorView", ViewData.Model);
			}

		}

		/// <summary>
		/// This action provides necessary ViewData for drawing Bar chart for BarChartView according to 
		/// parameters passed to it
		/// </summary>
		/// <param name="horizontalColumnName">Column at horizontal axis</param>
		/// <param name="dateRange">Date range on x-axis</param>
		/// <param name="verticalOptions">Option on y-axis: Option1 or Option2</param>
		/// <param name="verticalColumnName">Column at vertical axis</param>
		/// <param name="verticalColumnOptions">Option on y-axis for column
		/// selection: Aggregate or Average</param>
		/// <param name="container">Container Name in string format</param>
		/// <param name="entitySet">EntitySet Name in string format</param>
		/// <param name="filter">Filter query valu in string format</param>
		/// <returns>PartialView-BarChart with neccessay data required for its rendering</returns>
		[AcceptVerbs(HttpVerbs.Post)]
		public ActionResult RunBarChartButtonClicked(string horizontalColumnName,
			string dateRange, string verticalOptions, string verticalColumnName,
			string verticalColumnOptions, string container, string entitySet, string filter)
		{
			filter = string.IsNullOrEmpty(filter) ? null : filter.Trim().Equals(UIConstants.DBPC_NoFilterText) ? null : filter.Trim();
			bool result = GetChartData(horizontalColumnName, dateRange,
				verticalOptions, verticalColumnName, verticalColumnOptions, container, entitySet, filter);
			if (result == true)
			{

				ViewData.Model = viewDataModel;

				// Return Partial View of BarChart
				return View("BarChart", ViewData.Model);
			}
			else
			{

				ViewData.Model = viewDataModel;

				// returns the PatialView to load Error
				return PartialView("ErrorView", ViewData.Model);
			}
		}

		#endregion

		#region PieChart View Public Methods

		/// <summary>
		/// This action provides necessary ViewData for loading Sample code for PieChartView according to 
		/// language file name passed
		/// </summary>
		/// <param name="selectedFileName">File name for which the sample code
		/// is to be loaded</param>
		/// <returns>PartialView-SampleCodePieChartView with neccessay data required for its rendering</returns>
		[AcceptVerbs(HttpVerbs.Post)]
		public ActionResult LoadPieChartSampleCode(string selectedFileName, string container, string entitySet)
		{
			// Hide error labels
			HideError();

			try
			{
				// Set the viewDataModel.SampleCodePieChartView
				viewDataModel.SampleCodePieChartView = GetSampleCode(selectedFileName, FileFormat.CSharpFormat, container, entitySet);

				ViewData.Model = viewDataModel;

				// returns the PatialView to load SampleCode
				return PartialView("SampleCodePieChartView", ViewData.Model);
			}
			catch (Exception ex)
			{
				ShowError(ex.Message);

				ViewData.Model = viewDataModel;

				// returns the PatialView to load Error
				return PartialView("ErrorView", ViewData.Model);
			}
		}

		/// <summary>
		/// This action provides necessary ViewData for drawing Bar chart for BarChartView according to 
		/// parameters passed to it
		/// </summary>
		/// <param name="horizontalColumnName">Column at horizontal axis</param>
		/// <param name="dateRange">Date range on x-axis</param>
		/// <param name="verticalOptions">Option on y-axis: Option1 or Option2</param>
		/// <param name="verticalColumnName">Column at vertical axis</param>
		/// <param name="verticalColumnOptions">Option on y-axis for column
		/// selection: Aggregate or Average</param>
		/// <param name="container">Container Name in string format</param>
		/// <param name="entitySet">EntitySet Name in string format</param>
		/// <param name="filter">Filter query valu in string format</param>
		/// <returns>PartialView-PieChart with neccessay data required for its rendering</returns>
		[AcceptVerbs(HttpVerbs.Post)]
		public ActionResult RunPieChartButtonClicked(string horizontalColumnName,
			string dateRange, string verticalOptions, string verticalColumnName,
			string verticalColumnOptions, string container, string entitySet, string filter)
		{
			filter = string.IsNullOrEmpty(filter) ? null : filter.Trim().Equals(UIConstants.DBPC_NoFilterText) ? null : filter.Trim();
			bool result = GetChartData(horizontalColumnName, dateRange,
				verticalOptions, verticalColumnName, verticalColumnOptions, container,
				entitySet, filter);
			ViewData.Model = viewDataModel;
			return result
				? PartialView("PieChart", ViewData.Model)
				: PartialView("ErrorView", ViewData.Model);
		}

		#endregion

		#region Common Private Methods

		/// <summary>
		/// This method resets the ViewData to the default behavior
		/// </summary>
		private void ResetViewData()
		{
			// View to be visible. 0: DataView, 1: MapView
			viewDataModel.VisibleView = 0;

			// Tag to be visible. 0: SampleCode, 1: Results
			viewDataModel.VisibleTag = 1;

			// Zoom level of the map
			viewDataModel.ZoomLevel = -1;

			// Set filter text to default text
			viewDataModel.FilterText = DataBrowserResources.DataSetData.AdditionalFilterParameters;
			//viewDataModel.FilterText = UIConstants.DBPC_DefaultFilterText;
		}

		/// <summary>
		/// This method sets the value of viewDataModel.DBErrorLine1
		/// and viewDataModel.DBErrorLine2
		/// </summary>
		/// <param name="message">value to display in  viewDataModel.DBErrorLine1</param>
		/// <param name="detail">value to display in  viewDataModel.DBErrorLine2</param>
		protected void ShowError(string message, string detail)
		{
			// Set viewDataModel.DBErrorLine1
			viewDataModel.DBErrorLine1 = message;

			// Set viewDataModel.DBErrorLine2
			viewDataModel.DBErrorLine2 = detail;
		}

		/// <summary>
		/// This method sets the ServiceError
		/// </summary>
		/// <param name="ex">detailed error message</param>
		private void ShowServiceError(WebException ex)
		{
			// Set Error message
			ShowError(UIConstants.DBPC_ErrorServiceFail);
		}

		/// <summary>
		/// This method sets the UnexpectedError
		/// </summary>
		private void ShowUnexpectedError()
		{
			// Set Error message
			ShowError(UIConstants.DBPC_ErrorUnexpected);
		}

		/// <summary>
		/// This method resets the value of 
		/// viewDataModel.DBErrorLine1 and viewDataModel.DBErrorLine2
		/// </summary>
		private void HideError()
		{
			// Reset viewDataModel.DBErrorLine1
			viewDataModel.DBErrorLine1 = string.Empty;

			// Reset viewDataModel.DBErrorLine2
			viewDataModel.DBErrorLine2 = string.Empty;
		}

		/// <summary>
		/// This method shows the Exception message
		/// </summary>
		/// <param name="detail">detailed error message</param>
		private void ShowError(string detail)
		{
			// Set viewDataModel.DBErrorLine1
			viewDataModel.DBErrorLine1 = UIConstants.GC_ErrorString;

			// Set viewDataModel.DBErrorLine2
			viewDataModel.DBErrorLine2 = detail;

		}

		/// <summary>
		/// This method is called to generate and retrieve Sample code for 
		/// language selected in Data View tab
		/// </summary>
		/// <param name="selectedFileName">Name of the selected file</param>
		/// <param name="format">Format of the FileType</param>
		/// <returns>returns code</returns>
		private string GetSampleCode(string selectedFileName, FileFormat format, string container, string entitySet)
		{
			string containerAlias = container;
			string entitySetName = entitySet;

			string code = string.Empty;

			// Create server path
			string sampleCodeFilePath = String.Join("\\",
				new string[] { Server.MapPath("~").ToString(), 
					UIConstants.DBPC_SampleCodeFolderName, selectedFileName });

			// Get the sample code
			SampleCodeGenerator codeGen = new SampleCodeGenerator(sampleCodeFilePath);

			var queryString = string.Empty;
			string baseUri = OgdiConfiguration.GetValue("serviceUri") + containerAlias;

			// Generate code depending on passed FileFormat
			if (format == FileFormat.CSharpFormat)
			{
				code = codeGen.GenerateCSharp(baseUri, entitySetName, queryString);
			}
			else if (format == FileFormat.HtmlFormat)
			{
				code = codeGen.GenerateHtml(baseUri, entitySetName, queryString);
			}
			return code;
		}

		/// <summary>
		/// Loads the controls on Data Browser page
		/// </summary>        
		/// <param name="container"> Container Name in string format</param>
		/// <param name="entitySetName">EnitySet Name in string format</param>
		private void LoadControls(string container, string entitySetName)
		{
            viewDataModel.EntitySetName = entitySetName;
			viewDataModel.Container = container;

			SetURLs(container, entitySetName, "");

			// Load Dataview
			LoadDataView();

			// Load Mapview
			LoadMapView();

			EntitySet entitySet = EntitySetRepository.GetEntitySet(container, entitySetName);

			var viewDs = new DatasetInfoDataSource();
			var views = viewDs.GetAnalyticSummary(Helper.GenerateDatasetItemKey(entitySet.ContainerAlias, entitySet.EntitySetName));

			viewDataModel.EntitySetWrapper = new EntitySetWrapper()
			{
				EntitySet = entitySet,
				PositiveVotes = views.PositiveVotes,
				NegativeVotes = views.NegativeVotes,
				Views = views.views_total
			};

			if (!entitySet.IsEmpty)
			{
				EntitySetDetails metaDataDetails = EntitySetDetailsRepository.GetMetaData(container, entitySetName);

				viewDataModel.EntitySetDetails = metaDataDetails;

				LoadBarChart(metaDataDetails);
				LoadPieChart(metaDataDetails);
			}
		}

		/// <summary>
		/// This method returns Full data for the passed criterias
		/// </summary>
		/// <param name="container">container alias</param>
		/// <param name="entitySet">entity set name</param>
		/// <param name="filter">filter criteria</param>
		/// <returns>returns full data in EntitySetDetials object</returns>
		private EntitySetDetails GetFullData(string container, string entitySet, string filter)
		{
			// Fetch the full data in fullEntitySetDetails
			// To get full details, set isFullData parameter to true.
			EntitySetDetails cachedObject = null;
			if (HttpContext.Session["ChartData"] != null)
			{
				cachedObject = HttpContext.Session["ChartData"] as EntitySetDetails;
			}

			string cachedFilter = HttpContext.Session["ChartFilter"] == null ? null :
				HttpContext.Session["ChartFilter"].ToString().Trim();

			// Check if data available in cache
			if (cachedObject != null && cachedObject.ContainerAlias == container &&
				(cachedObject).EntitySetName == entitySet && cachedFilter == filter)
			{
				// Get data from cache
				return cachedObject;
			}
			else
			{
				// Create an object of class EntitySetDetails to get full 
				// details of the selected EntitySet
				EntitySetDetails fullEntitySetDetails = new EntitySetDetails();

				// Get data from service and insert it to cache
				// 1000 is the max results Azure Table Storage allows per query
				fullEntitySetDetails = EntitySetDetailsRepository.GetBrowserData(
											container,
											entitySet,
											filter == UIConstants.DBPC_NoFilterText ? null : filter,
											1000,
											null,
											null,
											true);

				if (HttpContext.Session["ChartData"] != null)
					HttpContext.Session.Remove("ChartData");
				if (HttpContext.Session["ChartFilter"] != null)
					HttpContext.Session.Remove("ChartFilter");

				HttpContext.Session.Add("ChartData", fullEntitySetDetails);
				if (filter == null)
					filter = UIConstants.DBPC_NoFilterText;
				HttpContext.Session.Add("ChartFilter", filter);

				return fullEntitySetDetails;
			}
		}

		#endregion

		#region DataView Private Methods

		/// <summary>
		/// This method creates a filter query name in ViewData using parameters passed to it
		/// </summary>
		/// <param name="containerAlias">container name in string format</param>
		/// <param name="entitySetName">EnitySet name in string format</param>
		/// <param name="filter">Filter name in string format</param>
		private void SetURLs(string containerAlias, string entitySetName, string filter)
		{
			string baseQuery = OgdiConfiguration.GetValue("serviceUri") + containerAlias + "/" + entitySetName + "/";

			// Append filter to viewDataModel.BaseQueryName if it exists
			viewDataModel.BaseQueryName = baseQuery;
			viewDataModel.FilteredQueryName = baseQuery;

			if (!string.IsNullOrEmpty(filter))
			{
				viewDataModel.FilteredQueryName += "?$filter=" + filter;
			}
		}

		/// <summary>
		/// This method fetches the previous page records as per parameters passed to it
		/// </summary>
		/// <param name="containerAlias">Container name in string format</param>
		/// <param name="entitySetName">EntitySet name in string format</param>
		/// <param name="filter"> Filter value in string format</param>
		private void PreviousLinkClicked(string containerAlias, string entitySetName, string filter)
		{
			ContinuationToken nextContinuationToken = null;
			ContinuationToken previousContinuationToken = null;

			var stack = GetContinuationTokenStack();

			// The current token on the stack is the "next" token for 
			// the previous instance of the page.  Since this is a 
			// "previous" click, we don't care about this token so we
			// remove it ("throw it away")
			stack.Pop();

			// The current token on the stack is for the "next" set of 
			// for this instance of the page.  In order to execute the 
			// "previous" query, we need to temporarily pop the "next" 
			// token off the stack.
			previousContinuationToken = stack.Pop();

			// This is the token we need to execute the "previous" query.
			nextContinuationToken = StackTop(stack);

			// We still need the "next" token to execute the "next" query 
			// when the user clicks the NextLink.  Therefore we put it 
			// back on the stack.
			stack.Push(previousContinuationToken);

			EntitySetDetails result;

			if (nextContinuationToken != null)
			{
				result = EntitySetDetailsRepository.GetBrowserData(
										containerAlias,
										entitySetName,
										filter,
										Convert.ToInt32(
										UIConstants.DBPC_EntityDetailsTableSize,
										CultureInfo.InvariantCulture),
										nextContinuationToken.NextPartitionKey,
										nextContinuationToken.NextRowKey,
										false);
			}
			else
			{
				result = EntitySetDetailsRepository.GetBrowserData(
										containerAlias,
										entitySetName,
										filter,
										Convert.ToInt32(
										UIConstants.DBPC_EntityDetailsTableSize,
										CultureInfo.InvariantCulture),
										null,
										null,
										false);
			}

			// Set the parameters to load the entitySet Details
			SetPagingAndBind(result, ButtonClicked.Previous, stack);

		}

		/// <summary>
		/// This method fetches the next page records as per parameters passed to it
		/// </summary>
		/// <param name="containerAlias">Container name in string format</param>
		/// <param name="entitySetName">EntitySet name in string format</param>
		/// <param name="filter"> Filter value in string format</param>
		private void NextLinkClicked(string containerAlias, string entitySetName, string filter)
		{
			// Get nextContinuationToken
			var stack = GetContinuationTokenStack();

			var nextContinuationToken = StackTop(stack);

			// Load the details
			EntitySetDetails result;

			if (nextContinuationToken != null)
			{
				result = EntitySetDetailsRepository.GetBrowserData(
										containerAlias,
										entitySetName,
										filter,
										Convert.ToInt32(
										UIConstants.DBPC_EntityDetailsTableSize,
										CultureInfo.InvariantCulture),
										nextContinuationToken.NextPartitionKey,
										nextContinuationToken.NextRowKey,
										false);
			}
			else
			{
				result = EntitySetDetailsRepository.GetBrowserData(
										containerAlias,
										entitySetName,
										filter,
										Convert.ToInt32(
										UIConstants.DBPC_EntityDetailsTableSize,
										CultureInfo.InvariantCulture),
										null,
										null,
										false);
			}

			// Set the parameters to load the entitySet Details
			SetPagingAndBind(result, ButtonClicked.Next, stack);

		}

		/// <summary>
		/// Returns top value of the stack
		/// </summary>
		/// <param name="stack">Stack used for storing ContinuationTokens</param>
		/// <returns>Top object of the type ContinuationToken</returns>
		private static ContinuationToken StackTop(Stack<ContinuationToken> stack)
		{
			try
			{
				// returns top of the stack
				return stack.Peek();
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		/// This method sets the paging and binds the details table
		/// </summary>
		/// <param name="entitySetDetails">object of type EntitySetDetails</param>
		/// <param name="buttonClicked">value of the type ButtonClicked</param>
		/// <param name="stack">Stack used for storing ContinuationTokens</param>
		private void SetPagingAndBind(EntitySetDetails entitySetDetails,
            ButtonClicked buttonClicked, Stack<ContinuationToken> stack)
		{
			// Initially disable both ViewData 
			viewDataModel.NextEnable = false;
			viewDataModel.PrevEnable = false;

			// If data exists in the DetailsTable
			if (entitySetDetails.DetailsTable.Rows.Count != 0)
			{
				switch (buttonClicked)
				{
					// Run
					case ButtonClicked.Run:
						// Add code to disable Previous Link
						viewDataModel.PrevEnable = false;

						stack = new Stack<ContinuationToken>();

						// If NextPartitionKey and NextRowKey exists
						if (!string.IsNullOrEmpty(entitySetDetails.NextPartitionKey) &&
							!string.IsNullOrEmpty(entitySetDetails.NextRowKey))
						{
							var nextContinuationToken = new ContinuationToken
							{
								NextPartitionKey = entitySetDetails.NextPartitionKey,
								NextRowKey = entitySetDetails.NextRowKey
							};

							// push the ContinuationToken
							stack.Push(nextContinuationToken);

							// Add code to enable Next Link                            
							viewDataModel.NextEnable = true;
						}

						if (stack.Count == 0)
						{
							// Add code to disable Next Link
							viewDataModel.NextEnable = false;
						}
						break;

					// Previous
					case ButtonClicked.Previous:
						if (stack.Count == 1)
						{
							// Add code to disable Previous Link
							viewDataModel.PrevEnable = false;
						}
						else
						{
							// Add code to enable Previous Link
							viewDataModel.PrevEnable = true;
						}

						// Add code to enable Next Link
						viewDataModel.NextEnable = true;

						break;

					// Next
					case ButtonClicked.Next:
						// If NextPartitionKey and NextRowKey exists
						if (!string.IsNullOrEmpty(entitySetDetails.NextPartitionKey) &&
							!string.IsNullOrEmpty(entitySetDetails.NextRowKey))
						{
							var nextContinuationToken = new ContinuationToken
							{
								NextPartitionKey = entitySetDetails.NextPartitionKey,
								NextRowKey = entitySetDetails.NextRowKey
							};

							// push the ContinuationToken
							stack.Push(nextContinuationToken);

							// Add code to enable Next Link
							viewDataModel.NextEnable = true;

							// Add code to enable Previous Link
							viewDataModel.PrevEnable = true;
						}
						else
						{
							// The else condition means there are no more pages of data.
							// The PreviousLink click handler ALWAYS throws away what
							// is on top of the stack.  Therefore, we add a bogus token.
							stack.Push(new ContinuationToken
							{
								NextPartitionKey = "X",
								NextRowKey = "X"
							});

							// Add code to enable Previous Link
							viewDataModel.PrevEnable = true;

							// Add code to disable Next Link
							viewDataModel.NextEnable = false;
						}
						break;
				}

				if (Session["ContinuationTokenStack"] != null)
					Session.Remove("ContinuationTokenStack");

				Session.Add("ContinuationTokenStack", stack);

				viewDataModel.TableBrowserData = entitySetDetails.DetailsTable;
			}
			else
			{
				ShowError(UIConstants.DBPC_ErrorZeroResults,
					UIConstants.DBPC_ErrorChkQuery);
			}
		}

		/// <summary>
		/// This method will load Data View
		/// </summary>
		private void LoadDataView()
		{
			// Load DataView Languages                
			List<SampleCodeLanguage> dataViewLanguageList = 
                (List<SampleCodeLanguage>)Helper.AllDVLanguages;

			LoadDataViewSampleCode(dataViewLanguageList[0].FilePath, viewDataModel.Container, viewDataModel.EntitySetName);
			viewDataModel.DataViewLanguages = new SelectList(dataViewLanguageList, "FilePath", "LanguageName");
		}

		/// <summary>
		/// This method will give stack of ContinuationToken from 
		/// the current session
		/// </summary>
		/// <returns>returns stack of ContinuationTokenStack of
		/// current session</returns>
		private Stack<ContinuationToken> GetContinuationTokenStack()
		{
			// returns stack of ContinuationTokenStack of current session
			return HttpContext.Session["ContinuationTokenStack"] != null ?
				Session["ContinuationTokenStack"] as Stack<ContinuationToken> : null;
		}

		/// <summary>
		/// This method handles CSV format conversion
		/// </summary>
		/// <param name="fullEntitySetDetails">EnititySet details to be converted into CSV format</param>
		/// <returns>returns stack of ContinuationTokenStack of
		/// current session</returns>
		private static string ConvertToCsv(EntitySetDetails fullEntitySetDetails)
		{
			try
			{
				StringBuilder sbAllEntities = new StringBuilder();

				foreach (DataColumn col in fullEntitySetDetails.DetailsTable.Columns)
				{
					sbAllEntities.Append(col.Caption);
					sbAllEntities.Append(",");
				}
				sbAllEntities.Remove(sbAllEntities.Length - 1, 1);
				sbAllEntities.Append(Environment.NewLine);

				foreach (DataRow row in fullEntitySetDetails.DetailsTable.Rows)
				{
					foreach (DataColumn col in fullEntitySetDetails.DetailsTable.Columns)
					{
						if (!string.IsNullOrEmpty(row[col].ToString()))
						{
							row[col] = row[col].ToString().Replace(',', ' ');
							row[col] = row[col].ToString().Replace('\n', ' ');
							sbAllEntities.Append(row[col].ToString());
						}
						else
						{
							try
							{
								row[col] = string.Empty;
								sbAllEntities.Append(row[col].ToString());
							}
							catch
							{ }
						}
						sbAllEntities.Append(",");
					}
					sbAllEntities.Remove(sbAllEntities.Length - 1, 1);
					sbAllEntities.Append(Environment.NewLine);
				}

				return sbAllEntities.ToString();
			}
			catch
			{
				return null;
			}
		}

		#endregion

		#region MapView Private Methods

		/// <summary>
		/// Resets ViewData for MapView
		/// </summary>
		private void ResetMapViewData()
		{
			viewDataModel.Longitude = 0;    // Longitude of the map

			viewDataModel.Latitude = 0;     // Latitude of the map                 

			viewDataModel.MapStyle = "0";   // Style of the map: Aeriel / Road / etc..

			viewDataModel.MapMode = 0;      // Mode of the map: 2D / 3D            

			viewDataModel.IsBookmarked = 'N';   // If page is bookmarked
		}

		/// <summary>
		/// This method loads Map View
		/// </summary>
		private void LoadMapView()
		{
			// Load MapView Languages                
			List<SampleCodeLanguage> mapViewLanguageList =
                (List<SampleCodeLanguage>)Helper.AllMVLanguages;

			SampleCodeLanguage currentLanguage =
                new SampleCodeLanguage(UIConstants.DBPC_DefaultMapViewLanguage,
                    Helper.GetMVLanguagePath(UIConstants.DBPC_DefaultMapViewLanguage));

			LoadMapViewSampleCode(currentLanguage.FilePath, viewDataModel.Container, viewDataModel.EntitySetName);

			viewDataModel.MapViewLanguages = new SelectList(mapViewLanguageList,
                "FilePath", "LanguageName", currentLanguage.FilePath);
		}

		#endregion

		#region BarChart View Private Methods

		/// <summary>
		/// This method loads Bar Chart
		/// </summary>
		/// <param name="metaDataDetails">This object contains column 
		/// details of entitySets.</param>
		private void LoadBarChart(EntitySetDetails metaDataDetails)
		{
			// Create an instance of type SortedDictionary<string, Type> 
			// to store numeric columns for y-axis column combo-box
			SortedDictionary<string, string> dictNumericColumns =
				new SortedDictionary<string, string>();

			// Create an instance of type SortedDictionary<string, Type> to store
			// ALL columns except GUID type for X-axis column combo-box
			SortedDictionary<string, string> dictAllColumns =
				new SortedDictionary<string, string>();

			GetColumnDictionariesForChart(dictNumericColumns, dictAllColumns, metaDataDetails);

			FillBarChartColumnComboBoxes(dictNumericColumns, dictAllColumns);

			FillBarChartDateComboBox();

			SetBarChartOptionsAndOtherParams();

			// Load BarChart Languages   
			SetBarChartViewLanguages();

			// Set initially keyvalue list to null
			viewDataModel.Chart = null;
		}

		/// <summary>
		/// This method sets the parameters for filling BarChart column
		/// comboboxes
		/// </summary>
		/// <param name="dictNumericColumns">SortedDictionary of all numeric 
		/// columns</param>
		/// <param name="dictAllColumns">SortedDictionary of all coulmns
		/// except GUID column</param> 
		private void FillBarChartColumnComboBoxes(
			SortedDictionary<string, string> dictNumericColumns,
			SortedDictionary<string, string> dictAllColumns)
		{
			viewDataModel.BarHorizontal = new SelectList(dictAllColumns,
				"value", "key");
			viewDataModel.BarVertical = new SelectList(dictNumericColumns,
				"value", "key");
		}

		/// <summary>
		/// This method sets the parameters for filling Date Range combobox
		/// of Bar Chart
		/// </summary>
		private void FillBarChartDateComboBox()
		{
			viewDataModel.BarDateRange = new SelectList(GetDateRanges());
		}

		/// <summary>
		/// This method sets SelectOne parameters
		/// of Bar Chart
		/// </summary>
		private void SetBarChartOptionsAndOtherParams()
		{
			viewDataModel.IsSelectOne = false;
		}

		/// <summary>
		/// This method sets the parameters for Language combobox
		/// of Bar Chart
		/// </summary>
		private void SetBarChartViewLanguages()
		{
			List<SampleCodeLanguage> barChartLanguageList =
				(List<SampleCodeLanguage>)Helper.AllBarChartLanguages;

			SampleCodeLanguage currentLanguage =
				new SampleCodeLanguage(UIConstants.DBPC_DefaultBarChartLanguage,
					Helper.GetBarChartLanguagePath(
					UIConstants.DBPC_DefaultBarChartLanguage));
			LoadBarChartSampleCode(currentLanguage.FilePath, viewDataModel.Container, viewDataModel.EntitySetName);
			viewDataModel.BarChartViewLanguages = new SelectList(barChartLanguageList,
			   "FilePath", "LanguageName", currentLanguage.FilePath);
		}

		#endregion

		#region PieChart View Private Methods

		/// <summary>
		/// This method loads Pie Chart
		/// </summary>
		/// <param name="metaDataDetails">This object contains
		/// column details of entitySets.</param>
		private void LoadPieChart(EntitySetDetails metaDataDetails)
		{
			// Create an instance of type SortedDictionary<string, Type>
			// to store numeric columns for y-axis column combo-box
			SortedDictionary<string, string> dictNumericColumns =
				new SortedDictionary<string, string>();

			// Create an instance of type SortedDictionary<string, Type> 
			// to store ALL columns except GUID type for X-axis column combo-box
			SortedDictionary<string, string> dictAllColumns =
				new SortedDictionary<string, string>();

			//  Fill the dictionary objects
			GetColumnDictionariesForChart(dictNumericColumns, dictAllColumns, metaDataDetails);

			FillPieChartColumnComboBoxes(dictNumericColumns, dictAllColumns);

			FillPieChartDateComboBox();

			SetPieChartOptionsAndOtherParams();

			// Load PieChart Languages                
			SetPieChartViewLanguages();

			// Set initially keyvalue list to null
			viewDataModel.Chart = null;
		}

		/// <summary>
		/// This method sets the parameters for filling PieChart column
		/// comboboxes
		/// </summary>
		/// <param name="dictNumericColumns">SortedDictionary of all numeric 
		/// columns</param>
		/// <param name="dictAllColumns">SortedDictionary of all coulmns
		/// except GUID column</param>    
		private void FillPieChartColumnComboBoxes(
			SortedDictionary<string, string> dictNumericColumns,
			SortedDictionary<string, string> dictAllColumns)
		{
			viewDataModel.PieHorizontal = new SelectList(dictAllColumns,
				"value", "key");
			viewDataModel.PieVertical = new SelectList(dictNumericColumns,
				   "value", "key");
		}

		/// <summary>
		/// This method sets the parameters for filling Date Range combobox
		/// of Pie Chart
		/// </summary>
		private void FillPieChartDateComboBox()
		{
			viewDataModel.PieDateRange = new SelectList(GetDateRanges());
		}

		/// <summary>
		/// This method sets the Option parameters and SelectOne parameters
		/// of Pie Chart
		/// </summary>
		private void SetPieChartOptionsAndOtherParams()
		{
			viewDataModel.IsSelectOne = false;
		}

		/// <summary>
		/// This method sets the parameters for Language combobox
		/// of Pie Chart
		/// </summary>
		private void SetPieChartViewLanguages()
		{
			List<SampleCodeLanguage> pieChartLanguageList = (
				List<SampleCodeLanguage>)Helper.AllPieChartLanguages;

			SampleCodeLanguage currentLanguage =
				new SampleCodeLanguage(UIConstants.DBPC_DefaultPieChartLanguage,
					Helper.GetPieChartLanguagePath(
					UIConstants.DBPC_DefaultPieChartLanguage));
			LoadPieChartSampleCode(currentLanguage.FilePath, viewDataModel.Container, viewDataModel.EntitySetName);

			viewDataModel.PieChartViewLanguages =
			  new SelectList(pieChartLanguageList,
			  "FilePath", "LanguageName", currentLanguage.FilePath);
		}

		#endregion

		#region Chart Private Methods

		/// <summary>
		/// This method sets SortedDictionaries for all columns & numeric columns
		/// </summary>
		/// <param name="dictNumericColumns">SortedDictionary of all numeric 
		/// columns</param>
		/// <param name="dictAllColumns">SortedDictionary of all coulmns
		/// except GUID column</param>
		/// <param name="metaDataDetails">metedata details containg column details</param>
		private static void GetColumnDictionariesForChart(
			IDictionary<string, string> dictNumericColumns,
			IDictionary<string, string> dictAllColumns,
			EntitySetDetails metaDataDetails)
		{
			//  Fill the dictionary objects
			//  Add watermarking element 
			dictAllColumns.Add(DataBrowserResources.DataSetData.SelectOne, "X");
			//  Add watermarking element
			dictNumericColumns.Add(DataBrowserResources.DataSetData.SelectOne, "X");
			//  Add other elements
			foreach (System.Data.DataColumn col in metaDataDetails.DetailsTable.Columns)
			{
				// If any Numeric Type
				if (col.DataType.FullName.Contains("Double") ||
					col.DataType.FullName.Contains("Int") ||
					col.DataType.FullName.Contains("Byte") ||
					col.DataType.FullName.Contains("Single"))
				{
					// Add colname and type                    
					dictNumericColumns.Add(col.Caption.ToString(),
						col.DataType.FullName + "^" + col.Caption.ToString());
				}

				// If not Guid type
				if (col.DataType.FullName != "System.Guid")
				{
					// Add colname and type
					dictAllColumns.Add(col.Caption, col.DataType.FullName
						+ "^" + col.Caption.ToString());
				}
			}
		}

		/// <summary>
		/// This method creates keyvalue(count) list satisfying passed criteria
		/// </summary>
		/// <param name="dataTable">dataTable which holds full data</param>
		/// <param name="horizontalColumnName">column name on horizontal axis</param>
		/// <param name="xColType">type of column on horizontal axis</param>
		/// <param name="dateRange">date range selected for column on x-axis</param>
		/// <returns>keyvalue list satisfying selected filter</returns>
		private static Dictionary<string, double> GetKeyValuePairsCount(DataTable dataTable,
			string horizontalColumnName, Type xColType, DateRangeConstants dateRange)
		{
			// Create an object of type Dictionary<string, double> to hold keyValue List
			Dictionary<string, double> keyValueList = new Dictionary<string, double>();

			// if type of column on x-axis is not Datetime
			if (xColType != Type.GetType("System.DateTime"))
			{
				keyValueList = GetKVPairsForCountForNoDate(dataTable, horizontalColumnName);
			}
			// if type of column on x-axis is Datetime
			else
			{
				// Apply the logic according to selected date range
				switch (dateRange)
				{
					case DateRangeConstants.Daily:
						keyValueList = GetKVPairsForCountForDaily(dataTable, horizontalColumnName);
						break;

					case DateRangeConstants.Monthly:
						keyValueList = GetKVPairsForCountForMonthly(dataTable, horizontalColumnName);
						break;

					case DateRangeConstants.Quarterly:
						keyValueList = GetKVPairsForCountForQuarterly(dataTable, horizontalColumnName);
						break;

					case DateRangeConstants.Yearly:
						keyValueList = GetKVPairsForCountForYearly(dataTable, horizontalColumnName);
						break;

				}
			}

			// Returns keyValueList
			return keyValueList;
		}

		/// <summary>
		/// This method creates keyvalue(aggregate) list satisfying passed criteria  
		/// </summary>
		/// <param name="dataTable">dataTable which holds full data</param>
		/// <param name="horizontalColumnName">column name on horizontal axis</param>
		/// <param name="verticalColumnName">column name on vertical axis</param>
		/// <param name="xColType">type of column on horizontal axis</param>
		/// <param name="dateRange">date range selected for column on x-axis</param>
		/// <returns>keyvalue list satisfying selected filter</returns>
		private static Dictionary<string, double> GetKeyValuePairsWithColumnAggregate(DataTable dataTable,
			string horizontalColumnName, string verticalColumnName, Type xColType,
			DateRangeConstants dateRange)
		{
			// Create an object of type Dictionary<string, double> to hold keyValue List
			Dictionary<string, double> keyValueList = new Dictionary<string, double>();

			// if type of column on x-axis is not Datetime
			if (xColType != Type.GetType("System.DateTime"))
			{
				keyValueList = GetKVPairsForAggregateForNoDate(dataTable, horizontalColumnName, verticalColumnName);
			}
			// if type of column on x-axis is Datetime
			else
			{
				// Apply the logic according to selected date range
				switch (dateRange)
				{
					case DateRangeConstants.Daily:
						keyValueList = GetKVPairsForAggregateForDaily(dataTable, horizontalColumnName, verticalColumnName);
						break;
					case DateRangeConstants.Monthly:
						keyValueList = GetKVPairsForAggregateForMonthly(dataTable, horizontalColumnName, verticalColumnName);
						break;
					case DateRangeConstants.Quarterly:
						keyValueList = GetKVPairsForAggregateForQuarterly(dataTable, horizontalColumnName, verticalColumnName);
						break;
					case DateRangeConstants.Yearly:
						keyValueList = GetKVPairsForAggregateForYearly(dataTable, horizontalColumnName, verticalColumnName);
						break;
				}
			}

			// Returns keyValueList
			return keyValueList;
		}

		/// <summary>
		/// This method creates keyvalue(average) list satisfying passed criteria  
		/// </summary>
		/// <param name="dataTable">dataTable which holds full data</param>
		/// <param name="horizontalColumnName">column name on horizontal axis</param>
		/// <param name="verticalColumnName">column name on vertical axis</param>
		/// <param name="xColType">type of column on horizontal axis</param>
		/// <param name="dateRange">date range selected for column on x-axis</param>
		/// <returns>keyvalue list satisfying selected filter</returns>
		private static Dictionary<string, double> GetKeyValuePairsWithColumnAverage(DataTable dataTable,
			string horizontalColumnName, string verticalColumnName, Type xColType,
			DateRangeConstants dateRange)
		{
			// Create an object of type Dictionary<string, double> to hold keyValue List
			Dictionary<string, double> keyValueList = new Dictionary<string, double>();

			// if type of column on x-axis is not Datetime
			if (xColType != Type.GetType("System.DateTime"))
			{
				keyValueList = GetKVPairsForAverageForNoDate(dataTable, horizontalColumnName, verticalColumnName);
			}
			// if type of column on x-axis is Datetime
			else
			{
				// Apply the logic according to selected date range
				switch (dateRange)
				{
					case DateRangeConstants.Daily:
						keyValueList = GetKVPairsForAverageForDaily(dataTable, horizontalColumnName, verticalColumnName);
						break;
					case DateRangeConstants.Monthly:
						keyValueList = GetKVPairsForAverageForMonthly(dataTable, horizontalColumnName, verticalColumnName);
						break;
					case DateRangeConstants.Quarterly:
						keyValueList = GetKVPairsForAverageForQuarterly(dataTable, horizontalColumnName, verticalColumnName);
						break;
					case DateRangeConstants.Yearly:
						keyValueList = GetKVPairsForAverageForYearly(dataTable, horizontalColumnName, verticalColumnName);
						break;
				}
			}

			// Returns keyValueList
			return keyValueList;
		}

		/// <summary>
		/// This method will return quarter number for passed month number
		/// </summary>
		/// <param name="month">month number</param>
		/// <returns>quarter number</returns>
		private static int GetQuarter(int month)
		{
			// First Quarter
			if (month <= 3)
				return 1;

			// Second Quarter
			if (month <= 6)
				return 2;

			// Third Quarter
			if (month <= 9)
				return 3;

			// Fourth Quarter
			if (month <= 12)
				return 4;

			return 0;
		}

		/// <summary>
		/// This method gets key value pairs for the chart according to passed parameters
		/// </summary>
		/// <param name="horizontalColumnName">Column at horizontal axis</param>
		/// <param name="dateRange">Date range on x-axis</param>
		/// <param name="barVerticalOptions">Option on y-axis: Option1 or Option2</param>
		/// <param name="verticalColumnName">Column at horizontal axis</param>
		/// <param name="verticalColumnOptions">Option on y-axis for column 
		/// selection: Aggregate or Average</param>
		/// <param name="container"> Container name in string format</param>
		/// <param name="entitySet">Entity Set name in string format</param>
		/// <param name="filter">Filter value in string format</param>
		/// <returns>boolean value which tells the success in Getting Chart Data</returns>
		private bool GetChartData(string horizontalColumnName, string dateRange,
			string barVerticalOptions, string verticalColumnName,
			string verticalColumnOptions, string container, string entitySet, string filter)
		{
			filter = string.IsNullOrEmpty(filter) ? UIConstants.DBPC_NoFilterText : filter;
			EntitySetDetails fullEntitySetDetails = GetFullData(container, entitySet, filter);

			// Get dateRangeConst
			DateRangeConstants dateRangeConst = DateRangeConstants.None;
			if (!dateRange.Equals("X"))
				dateRangeConst = (DateRangeConstants)Enum.Parse(typeof(DateRangeConstants),
					dateRange);

			// Create an object of type Dictionary<string, double> to store keyvalue pairs
			var keyValueList = new Dictionary<string, double>();

			// Convert VerticalOptions string to enum VerticalOptions
			var optionSelected =
				(VerticalOptions)Enum.Parse(typeof(VerticalOptions),
				barVerticalOptions);

			// Convert VerticalColumnOptions string to enum VerticalColumnOptions
			var colOptionSelected =
				(VerticalColumnOptions)Enum.Parse(typeof(VerticalColumnOptions),
				verticalColumnOptions);

			// If Horizontal column exists
			if (horizontalColumnName != "X")
			{
				// Get type of the horizontal column selected
				Type xColType =
					fullEntitySetDetails.DetailsTable.Columns[horizontalColumnName].DataType;

				// If Option1 selected
				if (optionSelected == VerticalOptions.Option1)
				{
					// Get the keyvalue pairs for Count of X-axis's value
					keyValueList = GetKeyValuePairsCount(fullEntitySetDetails.DetailsTable,
						horizontalColumnName, xColType, dateRangeConst);
				}
				// If Option2 selected
				else
				{
					// If vertical column exists
					if (!verticalColumnName.Equals("X"))
					{
						if (colOptionSelected == VerticalColumnOptions.Aggregate)
						{
							// Get the keyvalue pairs for y-option: "Aggregate"
							keyValueList =
								GetKeyValuePairsWithColumnAggregate(fullEntitySetDetails.DetailsTable,
								horizontalColumnName, verticalColumnName, xColType, dateRangeConst);
						}
						else if (colOptionSelected == VerticalColumnOptions.Average)
						{
							// Get the keyvalue pairs for y-option: "Average"
							keyValueList =
								GetKeyValuePairsWithColumnAverage(fullEntitySetDetails.DetailsTable,
								horizontalColumnName, verticalColumnName, xColType, dateRangeConst);
						}
					}
					// If vertical column does not exists
					else
					{
						ShowError(UIConstants.DBPC_GraphErrorVerticalAxisMissing,
							UIConstants.DBPC_GraphErrorNoGraphAvailable);
						return false;
					}
				}
			}
			// If Horizontal column does not exists
			else
			{
				ShowError(UIConstants.DBPC_GraphErrorNoColumnAvailable,
					UIConstants.DBPC_GraphErrorNoGraphAvailable);
				return false;
			}

			// Set X-axis column name
			viewDataModel.XColName = horizontalColumnName;

			// Set Y-axis column name
			if (optionSelected == VerticalOptions.Option1)
				viewDataModel.YColName = UIConstants.DBPC_GraphOccuranceText + " \""
					+ horizontalColumnName + "\"";

			else
				viewDataModel.YColName = verticalColumnName + " ["
					+ verticalColumnOptions + "]";


			// Set EntitySet Name
			viewDataModel.EntitySetName = entitySet;

			// Set keyValue List for chart
			viewDataModel.Chart = keyValueList;

			// Set count of keys in the keyValue list
			viewDataModel.XCount = keyValueList.Keys.Count;

			return true;
		}

		/// <summary>
		/// This method will get list of date ranges available in DateRangeConstants
		/// </summary>
		/// <returns>Returns list of date ranges</returns>
		private static List<string> GetDateRanges()
		{
			// Create an instance of class List<string>
			List<string> lstDateRanges = new List<string>();

			// Add DateRangeConstants in lstDateRanges
			lstDateRanges.Add(DateRangeConstants.Daily.ToString());
			lstDateRanges.Add(DateRangeConstants.Monthly.ToString());
			lstDateRanges.Add(DateRangeConstants.Quarterly.ToString());
			lstDateRanges.Add(DateRangeConstants.Yearly.ToString());

			// returns list of date ranges
			return lstDateRanges;
		}

		/// <summary>
		/// This method fetches records in a format: value @ x-axis & count of 
		/// occurances of value @ x-axis order by value @ x-axis
		/// </summary>
		/// <param name="dataTable">dataTabl from which x&y values will be 
		/// fetched</param>
		/// <param name="horizontalColumnName">name of the horizontal column
		/// </param>
		/// <returns>keyvalue list of desired data</returns>
		private static Dictionary<string, double> GetKVPairsForCountForNoDate
			(DataTable dataTable, string horizontalColumnName)
		{
			// Fetch the required data by firing LINQ query on DataTable
			// Fetch records in a format: value @ x-axis & count of occurances of
			// value @ x-axis order by value @ x-axis
			var lstData = from DataRow dRow in dataTable.Rows
						  orderby dRow[horizontalColumnName].ToString()
						  group dRow by dRow[horizontalColumnName] into grp
						  select new
						  {
							  xValue = grp.Key.ToString(),
							  yValue = grp.Count()
						  };

			// Convert lstData to Dictionary<string, double> type
			return lstData.ToDictionary(tuple => tuple.xValue.ToString(),
				tuple => Convert.ToDouble(tuple.yValue));
		}

		/// <summary>
		/// This method fetches records in a format: value @ x-axis on daily basis & 
		/// count of occurances of value @ x-axis order by date of value @ x-axis
		/// </summary>
		/// <param name="dataTable">dataTabl from which x&y values will be 
		/// fetched</param>
		/// <param name="horizontalColumnName">name of the horizontal column
		/// </param>
		/// <returns>keyvalue list of desired data</returns>
		private static Dictionary<string, double> GetKVPairsForCountForDaily
			(DataTable dataTable, string horizontalColumnName)
		{
			// Fetch the required data by firing LINQ query on DataTable
			// Fetch records in a format: value @ x-axis on daily basis & 
			// count of occurances of value @ x-axis 
			// order by date of value @ x-axis
			var lstData = from DataRow dRow in dataTable.Rows
						  where (!string.IsNullOrEmpty(dRow[horizontalColumnName].ToString()) && !string.IsNullOrEmpty(Convert.ToDateTime(dRow[horizontalColumnName].ToString(),
						  CultureInfo.InvariantCulture).ToShortDateString()))
						  orderby Convert.ToDateTime(dRow[horizontalColumnName],
						  CultureInfo.InvariantCulture)
						  group dRow by Convert.ToDateTime(dRow[horizontalColumnName],
						  CultureInfo.InvariantCulture).ToShortDateString() into grp
						  select new
						  {
							  xValue = Convert.ToDateTime(grp.Key,
							  CultureInfo.InvariantCulture).ToString("dd MMM yyyy",
							  CultureInfo.InvariantCulture),
							  yValue = grp.Count()
						  };

			// Convert lstData to Dictionary<string, double> type
			return lstData.ToDictionary(tuple => tuple.xValue,
				tuple => Convert.ToDouble(tuple.yValue));
		}

		/// <summary>
		/// This method fetches records in a format: value @ x-axis on monthly basis &
		/// count of occurances of value @ x-axis 
		/// order by year & month of value @ x-axis
		/// </summary>
		/// <param name="dataTable">dataTabl from which x&y values will be 
		/// fetched</param>
		/// <param name="horizontalColumnName">name of the horizontal column
		/// </param>
		/// <returns>keyvalue list of desired data</returns>
		private static Dictionary<string, double> GetKVPairsForCountForMonthly
			(DataTable dataTable, string horizontalColumnName)
		{
			// Fetch the required data by firing LINQ query on DataTable
			// Fetch records in a format: value @ x-axis on monthly basis &
			// count of occurances of value @ x-axis 
			// order by year & month of value @ x-axis
			var lstData = from DataRow dRow in dataTable.Rows
						  where (!string.IsNullOrEmpty(dRow[horizontalColumnName].ToString()) && !string.IsNullOrEmpty(Convert.ToDateTime(dRow[horizontalColumnName].ToString(),
						  CultureInfo.InvariantCulture).ToShortDateString()))
						  orderby Convert.ToDateTime(dRow[horizontalColumnName],
									CultureInfo.InvariantCulture).Year,
								  Convert.ToDateTime(dRow[horizontalColumnName],
									CultureInfo.InvariantCulture).Month
						  group dRow by new
						  {
							  month = Convert.ToDateTime(dRow[horizontalColumnName],
										CultureInfo.InvariantCulture).ToString("MMM",
										CultureInfo.InvariantCulture),
							  year = Convert.ToDateTime(dRow[horizontalColumnName],
										CultureInfo.InvariantCulture).Year
						  } into grp
						  select new
						  {
							  xValue = grp.Key.month + " " + grp.Key.year,
							  yValue = grp.Count()
						  };

			// Convert lstData to Dictionary<string, double> type
			return lstData.ToDictionary(tuple => tuple.xValue,
				tuple => Convert.ToDouble(tuple.yValue));
		}

		/// <summary>
		/// This method fetches records in a format: value @ x-axis on quarterly basis & 
		/// count of occurances of value @ x-axis 
		/// order by year & quarter of value @ x-axis
		/// </summary>
		/// <param name="dataTable">dataTabl from which x&y values will be 
		/// fetched</param>
		/// <param name="horizontalColumnName">name of the horizontal column
		/// </param>
		/// <returns>keyvalue list of desired data</returns>
		private static Dictionary<string, double> GetKVPairsForCountForQuarterly
			(DataTable dataTable, string horizontalColumnName)
		{
			// Fetch the required data by firing LINQ query on DataTable
			// Fetch records in a format: value @ x-axis on quarterly basis & 
			// count of occurances of value @ x-axis 
			// order by year & quarter of value @ x-axis
			var lstData = from DataRow dRow in dataTable.Rows
						  where (!string.IsNullOrEmpty(dRow[horizontalColumnName].ToString()) && !string.IsNullOrEmpty(Convert.ToDateTime(dRow[horizontalColumnName].ToString(),
						  CultureInfo.InvariantCulture).ToShortDateString()))
						  orderby Convert.ToDateTime(dRow[horizontalColumnName],
										CultureInfo.InvariantCulture).Year,
								  GetQuarter(Convert.ToDateTime(dRow[horizontalColumnName],
										CultureInfo.InvariantCulture).Month)
						  group dRow by new
						  {
							  quarter =
									GetQuarter(Convert.ToDateTime(dRow[horizontalColumnName],
									CultureInfo.InvariantCulture).Month),
							  year = Convert.ToDateTime(dRow[horizontalColumnName],
									CultureInfo.InvariantCulture).Year
						  } into grp
						  select new
						  {
							  xValue = "Qtr " + grp.Key.quarter + " " + grp.Key.year,
							  yValue = grp.Count()
						  };

			// Convert lstData to Dictionary<string, double> type
			return lstData.ToDictionary(tuple => tuple.xValue,
				tuple => Convert.ToDouble(tuple.yValue));
		}

		/// <summary>
		/// This method fetches records in a format: value @ x-axis on yearly basis & 
		/// count of occurances of value @ x-axis 
		/// order by year of value @ x-axis
		/// </summary>
		/// <param name="dataTable">dataTabl from which x&y values will be 
		/// fetched</param>
		/// <param name="horizontalColumnName">name of the horizontal column
		/// </param>
		/// <returns>keyvalue list of desired data</returns>
		private static Dictionary<string, double> GetKVPairsForCountForYearly
			(DataTable dataTable, string horizontalColumnName)
		{
			// Fetch the required data by firing LINQ query on DataTable
			// Fetch records in a format: value @ x-axis on yearly basis & 
			// count of occurances of value @ x-axis 
			// order by year of value @ x-axis
			var lstData = from DataRow dRow in dataTable.Rows
						  where (!string.IsNullOrEmpty(dRow[horizontalColumnName].ToString()) && !string.IsNullOrEmpty(Convert.ToDateTime(dRow[horizontalColumnName].ToString(),
						  CultureInfo.InvariantCulture).ToShortDateString()))
						  orderby Convert.ToDateTime(dRow[horizontalColumnName],
						  CultureInfo.InvariantCulture).Year
						  group dRow by new
						  {
							  year = Convert.ToDateTime(dRow[horizontalColumnName],
							  CultureInfo.InvariantCulture).Year
						  } into grp
						  select new
						  {
							  xValue = grp.Key.year.ToString(CultureInfo.InvariantCulture),
							  yValue = grp.Count()
						  };

			// Convert lstData to Dictionary<string, double> type
			return lstData.ToDictionary(tuple => tuple.xValue,
				tuple => Convert.ToDouble(tuple.yValue));
		}

		/// <summary>
		/// This method fetches records in a format: value @ x-axis & aggregate of value 
		/// @ y-axis corresponding to value @ x-axis
		/// order by value @ x-axis
		/// </summary>
		/// <param name="dataTable">dataTabl from which x&y values will be 
		/// fetched</param>
		/// <param name="horizontalColumnName">name of the horizontal column
		/// </param>
		/// <param name="verticalColumnName">name of the vertical column
		/// </param>    
		/// <returns>keyvalue list of desired data</returns>
		private static Dictionary<string, double> GetKVPairsForAggregateForNoDate
			(DataTable dataTable, string horizontalColumnName, string verticalColumnName)
		{
			// Fetch the required data by firing LINQ query on DataTable
			// Fetch records in a format: value @ x-axis & aggregate of value 
			// @ y-axis corresponding to value @ x-axis
			// order by value @ x-axis
			var lstData = from DataRow row in dataTable.Rows
						  orderby row[horizontalColumnName].ToString()
						  group row by row[horizontalColumnName].ToString() into grp
						  select new
						  {
							  xValue = grp.Key.ToString(),
							  yValue =
							  grp.Sum(row => !string.IsNullOrEmpty(row[verticalColumnName].ToString()) ?
									Convert.ToDouble(row[verticalColumnName],
									CultureInfo.InvariantCulture) : 0)
						  };

			// Convert lstData to Dictionary<string, double> type
			return lstData.ToDictionary(tuple => tuple.xValue,
				tuple => tuple.yValue);
		}

		/// <summary>
		/// This method fetches records in a format: value @ x-axis on daily basis &
		/// aggregate of value @ y-axis
		/// corresponding to value @ x-axis order by date of value @ x-axis
		/// </summary>
		/// <param name="dataTable">dataTable from which x&y values will be 
		/// fetched</param>
		/// <param name="horizontalColumnName">name of the horizontal column
		/// </param>
		/// <param name="verticalColumnName">name of the vertical column
		/// </param>
		/// <returns>keyvalue list of desired data</returns>
		private static Dictionary<string, double> GetKVPairsForAggregateForDaily
			(DataTable dataTable, string horizontalColumnName, string verticalColumnName)
		{
			// Fetch the required data by firing LINQ query on DataTable
			// Fetch records in a format: value @ x-axis on daily basis &
			// aggregate of value @ y-axis
			// corresponding to value @ x-axis order by date of value @ x-axis
			var lstData = from DataRow row in dataTable.Rows
						  where (!string.IsNullOrEmpty(row[horizontalColumnName].ToString()) && !string.IsNullOrEmpty(Convert.ToDateTime(row[horizontalColumnName].ToString(),
						  CultureInfo.InvariantCulture).ToShortDateString()))
						  orderby Convert.ToDateTime(row[horizontalColumnName],
						  CultureInfo.InvariantCulture)
						  group row by Convert.ToDateTime(row[horizontalColumnName],
						  CultureInfo.InvariantCulture).ToShortDateString() into grp
						  select new
						  {
							  xValue = Convert.ToDateTime(grp.Key,
							  CultureInfo.InvariantCulture).ToString("dd MMM yyyy",
							  CultureInfo.InvariantCulture),
							  yValue =
							  grp.Sum(row => !string.IsNullOrEmpty(row[verticalColumnName].ToString()) ?
								Convert.ToDouble(row[verticalColumnName].ToString(),
								CultureInfo.InvariantCulture) : 0)
						  };

			// Convert lstData to Dictionary<string, double> type
			return lstData.ToDictionary(tuple => tuple.xValue,
				tuple => Convert.ToDouble(tuple.yValue));
		}

		/// <summary>
		/// This method fetches records in a format: value @ x-axis on monthly basis 
		/// & aggregate of value @ y-axis
		/// corresponding to value @ x-axis order by year & month of value @ x-axis
		/// </summary>
		/// <param name="dataTable">dataTable from which x&y values will be 
		/// fetched</param>
		/// <param name="horizontalColumnName">name of the horizontal column
		/// </param>
		/// <param name="verticalColumnName">name of the vertical column
		/// </param>
		/// <returns>keyvalue list of desired data</returns>
		private static Dictionary<string, double> GetKVPairsForAggregateForMonthly
			(DataTable dataTable, string horizontalColumnName, string verticalColumnName)
		{
			// Fetch the required data by firing LINQ query on DataTable
			// Fetch records in a format: value @ x-axis on monthly basis 
			// & aggregate of value @ y-axis
			// corresponding to value @ x-axis order by year & month of value @ x-axis
			var lstData = from DataRow row in dataTable.Rows
						  where (!string.IsNullOrEmpty(row[horizontalColumnName].ToString()) && !string.IsNullOrEmpty(Convert.ToDateTime(row[horizontalColumnName].ToString(),
						  CultureInfo.InvariantCulture).ToShortDateString()))
						  orderby Convert.ToDateTime(row[horizontalColumnName],
						  CultureInfo.InvariantCulture).Year,
								Convert.ToDateTime(row[horizontalColumnName],
								CultureInfo.InvariantCulture).Month
						  group row by new
						  {
							  month = Convert.ToDateTime(row[horizontalColumnName],
							  CultureInfo.InvariantCulture).ToString("MMM",
							  CultureInfo.InvariantCulture),
							  year = Convert.ToDateTime(row[horizontalColumnName],
							  CultureInfo.InvariantCulture).Year
						  } into grp
						  select new
						  {
							  xValue = grp.Key.month + " " + grp.Key.year,
							  yValue =
								grp.Sum(row => !string.IsNullOrEmpty(row[verticalColumnName].ToString()) ?
									Convert.ToDouble(row[verticalColumnName].ToString(),
									CultureInfo.InvariantCulture) : 0)
						  };

			// Convert lstData to Dictionary<string, double> type
			return lstData.ToDictionary(tuple => tuple.xValue,
				tuple => Convert.ToDouble(tuple.yValue));
		}

		/// <summary>
		/// This method fetches records in a format: value @ x-axis on quarterly basis 
		/// & aggregate of value @ y-axis
		/// corresponding to value @ x-axis order by year & quarter of value @ x-axis
		/// </summary>
		/// <param name="dataTable">dataTable from which x&y values will be 
		/// fetched</param>
		/// <param name="horizontalColumnName">name of the horizontal column
		/// </param>
		/// <param name="verticalColumnName">name of the vertical column
		/// </param>
		/// <returns>keyvalue list of desired data</returns>
		private static Dictionary<string, double> GetKVPairsForAggregateForQuarterly
			(DataTable dataTable, string horizontalColumnName, string verticalColumnName)
		{
			// Fetch the required data by firing LINQ query on DataTable
			// Fetch records in a format: value @ x-axis on quarterly basis 
			// & aggregate of value @ y-axis
			// corresponding to value @ x-axis order by year & quarter of value @ x-axis
			var lstData = from DataRow row in dataTable.Rows
						  where (!string.IsNullOrEmpty(row[horizontalColumnName].ToString()) && !string.IsNullOrEmpty(Convert.ToDateTime(row[horizontalColumnName].ToString(),
						  CultureInfo.InvariantCulture).ToShortDateString()))
						  orderby Convert.ToDateTime(row[horizontalColumnName],
						  CultureInfo.InvariantCulture).Year,
								GetQuarter(Convert.ToDateTime(row[horizontalColumnName],
								CultureInfo.InvariantCulture).Month)
						  group row by new
						  {
							  quarter = GetQuarter(Convert.ToDateTime(row[horizontalColumnName],
							  CultureInfo.InvariantCulture).Month),
							  year = Convert.ToDateTime(row[horizontalColumnName],
							  CultureInfo.InvariantCulture).Year
						  } into grp
						  select new
						  {
							  xValue = "Qtr " + grp.Key.quarter + " " + grp.Key.year,
							  yValue =
								grp.Sum(row => !string.IsNullOrEmpty(row[verticalColumnName].ToString()) ?
									Convert.ToDouble(row[verticalColumnName].ToString(),
									CultureInfo.InvariantCulture) : 0)
						  };

			// Convert lstData to Dictionary<string, double> type
			return lstData.ToDictionary(tuple => tuple.xValue,
				tuple => Convert.ToDouble(tuple.yValue));
		}

		/// <summary>
		/// This method fetches records in a format: value @ x-axis on yearly basis 
		/// & aggregate of value @ y-axis
		/// corresponding to value @ x-axis order by year of value @ x-axis
		/// </summary>
		/// <param name="dataTable">dataTable from which x&y values will be 
		/// fetched</param>
		/// <param name="horizontalColumnName">name of the horizontal column
		/// </param>
		/// <param name="verticalColumnName">name of the vertical column
		/// </param>
		/// <returns>keyvalue list of desired data</returns>
		private static Dictionary<string, double> GetKVPairsForAggregateForYearly
			(DataTable dataTable, string horizontalColumnName, string verticalColumnName)
		{
			// Fetch the required data by firing LINQ query on DataTable
			// Fetch records in a format: value @ x-axis on yearly basis 
			// & aggregate of value @ y-axis
			// corresponding to value @ x-axis order by year of value @ x-axis
			var lstData = from DataRow row in dataTable.Rows
						  where (!string.IsNullOrEmpty(row[horizontalColumnName].ToString()) && !string.IsNullOrEmpty(Convert.ToDateTime(row[horizontalColumnName].ToString(),
						  CultureInfo.InvariantCulture).ToShortDateString()))
						  orderby Convert.ToDateTime(row[horizontalColumnName],
						  CultureInfo.InvariantCulture).Year
						  group row by new
						  {
							  year = Convert.ToDateTime(row[horizontalColumnName],
							  CultureInfo.InvariantCulture).Year
						  } into grp
						  select new
						  {
							  xValue = grp.Key.year.ToString(CultureInfo.InvariantCulture),
							  yValue =
								grp.Sum(row => !string.IsNullOrEmpty(row[verticalColumnName].ToString()) ?
									Convert.ToDouble(row[verticalColumnName].ToString(),
									CultureInfo.InvariantCulture) : 0)
						  };

			// Convert lstData to Dictionary<string, double> type
			return lstData.ToDictionary(tuple => tuple.xValue,
				tuple => Convert.ToDouble(tuple.yValue));
		}

		/// <summary>
		/// This method fetches records in a format: value @ x-axis & average of value
		/// @ y-axis corresponding to value @ x-axis
		/// order by value @ x-axis
		/// </summary>
		/// <param name="dataTable">dataTable from which x&y values will be 
		/// fetched</param>
		/// <param name="horizontalColumnName">name of the horizontal column
		/// </param>
		/// <param name="verticalColumnName">name of the vertical column
		/// </param>
		/// <returns>keyvalue list of desired data</returns>
		private static Dictionary<string, double> GetKVPairsForAverageForNoDate
			(DataTable dataTable, string horizontalColumnName, string verticalColumnName)
		{
			// Fetch the required data by firing LINQ query on DataTable
			// Fetch records in a format: value @ x-axis & average of value
			// @ y-axis corresponding to value @ x-axis
			// order by value @ x-axis
			var lstData = from DataRow row in dataTable.Rows
						  orderby row[horizontalColumnName].ToString()
						  group row by row[horizontalColumnName].ToString() into grp
						  select new
						  {
							  xValue = grp.Key,
							  yValue =
								grp.Average(row => !string.IsNullOrEmpty(row[verticalColumnName].ToString()) ?
									Convert.ToDouble(row[verticalColumnName],
									CultureInfo.InvariantCulture) : 0)
						  };

			// Convert lstData to Dictionary<string, double> type
			return lstData.ToDictionary(tuple => tuple.xValue,
				tuple => tuple.yValue);
		}

		/// <summary>
		/// This method fetches records in a format: value @ x-axis on daily basis
		/// & average of value @ y-axis
		/// corresponding to value @ x-axis order by date of value @ x-axis
		/// </summary>
		/// <param name="dataTable">dataTable from which x&y values will be 
		/// fetched</param>
		/// <param name="horizontalColumnName">name of the horizontal column
		/// </param>
		/// <param name="verticalColumnName">name of the vertical column
		/// </param>
		/// <returns>keyvalue list of desired data</returns>
		private static Dictionary<string, double> GetKVPairsForAverageForDaily
			(DataTable dataTable, string horizontalColumnName, string verticalColumnName)
		{
			// Fetch the required data by firing LINQ query on DataTable
			// Fetch records in a format: value @ x-axis on daily basis
			// & average of value @ y-axis
			// corresponding to value @ x-axis order by date of value @ x-axis
			var lstData = from DataRow row in dataTable.Rows
						  where (!string.IsNullOrEmpty(row[horizontalColumnName].ToString()) && !string.IsNullOrEmpty(Convert.ToDateTime(row[horizontalColumnName].ToString(),
						  CultureInfo.InvariantCulture).ToShortDateString()))
						  orderby Convert.ToDateTime(row[horizontalColumnName],
						  CultureInfo.InvariantCulture)
						  group row by Convert.ToDateTime(row[horizontalColumnName],
						  CultureInfo.InvariantCulture).ToShortDateString() into grp
						  select new
						  {
							  xValue = Convert.ToDateTime(grp.Key,
							  CultureInfo.InvariantCulture).ToString("dd MMM yyyy",
							  CultureInfo.InvariantCulture),
							  yValue =
								grp.Average(row => !string.IsNullOrEmpty(row[verticalColumnName].ToString()) ?
									Convert.ToDouble(row[verticalColumnName].ToString(),
									CultureInfo.InvariantCulture) : 0)
						  };

			// Convert lstData to Dictionary<string, double> type
			return lstData.ToDictionary(tuple => tuple.xValue,
				tuple => Convert.ToDouble(tuple.yValue));
		}

		/// <summary>
		/// This method fetches records in a format: value @ x-axis on monthly basis
		/// & average of value @ y-axis
		/// corresponding to value @ x-axis order by year & month of value @ x-axis
		/// </summary>
		/// <param name="dataTable">dataTable from which x&y values will be 
		/// fetched</param>
		/// <param name="horizontalColumnName">name of the horizontal column
		/// </param>
		/// <param name="verticalColumnName">name of the vertical column
		/// </param>
		/// <returns>keyvalue list of desired data</returns>
		private static Dictionary<string, double> GetKVPairsForAverageForMonthly
			(DataTable dataTable, string horizontalColumnName, string verticalColumnName)
		{
			// Fetch the required data by firing LINQ query on DataTable
			// Fetch records in a format: value @ x-axis on monthly basis
			// & average of value @ y-axis
			// corresponding to value @ x-axis order by year & month of value @ x-axis
			var lstData = from DataRow row in dataTable.Rows
						  where (!string.IsNullOrEmpty(row[horizontalColumnName].ToString()) && !string.IsNullOrEmpty(Convert.ToDateTime(row[horizontalColumnName].ToString(),
						  CultureInfo.InvariantCulture).ToShortDateString()))
						  orderby Convert.ToDateTime(row[horizontalColumnName],
						  CultureInfo.InvariantCulture).Year,
								Convert.ToDateTime(row[horizontalColumnName],
								CultureInfo.InvariantCulture).Month
						  group row by new
						  {
							  month = Convert.ToDateTime(row[horizontalColumnName],
							  CultureInfo.InvariantCulture).ToString("MMM",
							  CultureInfo.InvariantCulture),
							  year = Convert.ToDateTime(row[horizontalColumnName],
							  CultureInfo.InvariantCulture).Year
						  } into grp
						  select new
						  {
							  xValue = grp.Key.month + " " + grp.Key.year,
							  yValue =
								grp.Average(row => !string.IsNullOrEmpty(row[verticalColumnName].ToString()) ?
									Convert.ToDouble(row[verticalColumnName].ToString(),
									CultureInfo.InvariantCulture) : 0)
						  };

			// Convert lstData to Dictionary<string, double> type
			return lstData.ToDictionary(tuple => tuple.xValue,
				tuple => Convert.ToDouble(tuple.yValue));

		}

		/// <summary>
		/// This method fetches records in a format: value @ x-axis on qyarterly basis 
		/// & average of value @ y-axis
		/// corresponding to value @ x-axis order by year & quarter of value @ x-axis
		/// </summary>
		/// <param name="dataTable">dataTable from which x&y values will be 
		/// fetched</param>
		/// <param name="horizontalColumnName">name of the horizontal column
		/// </param>
		/// <param name="verticalColumnName">name of the vertical column
		/// </param>
		/// <returns>keyvalue list of desired data</returns>
		private static Dictionary<string, double> GetKVPairsForAverageForQuarterly
			(DataTable dataTable, string horizontalColumnName, string verticalColumnName)
		{
			// Fetch the required data by firing LINQ query on DataTable
			// Fetch records in a format: value @ x-axis on qyarterly basis 
			// & average of value @ y-axis
			// corresponding to value @ x-axis order by year & quarter of value @ x-axis
			var lstData = from DataRow row in dataTable.Rows
						  where (!string.IsNullOrEmpty(row[horizontalColumnName].ToString()) && !string.IsNullOrEmpty(Convert.ToDateTime(row[horizontalColumnName].ToString(),
						  CultureInfo.InvariantCulture).ToShortDateString()))
						  orderby Convert.ToDateTime(row[horizontalColumnName],
						  CultureInfo.InvariantCulture).Year,
								GetQuarter(Convert.ToDateTime(row[horizontalColumnName],
								CultureInfo.InvariantCulture).Month)
						  group row by new
						  {
							  quarter = GetQuarter(Convert.ToDateTime(row[horizontalColumnName],
							  CultureInfo.InvariantCulture).Month),
							  year = Convert.ToDateTime(row[horizontalColumnName],
							  CultureInfo.InvariantCulture).Year
						  } into grp
						  select new
						  {
							  xValue = "Qtr " + grp.Key.quarter + " " + grp.Key.year,
							  yValue =
								grp.Average(row => !string.IsNullOrEmpty(row[verticalColumnName].ToString()) ?
									Convert.ToDouble(row[verticalColumnName].ToString(),
									CultureInfo.InvariantCulture) : 0)
						  };

			// Convert lstData to Dictionary<string, double> type
			return lstData.ToDictionary(tuple => tuple.xValue,
				tuple => Convert.ToDouble(tuple.yValue));
		}

		/// <summary>
		/// This method fetches records in a format: value @ x-axis on yearly basis 
		/// & average of value @ y-axis
		/// corresponding to value @ x-axis order by year of value @ x-axis
		/// </summary>
		/// <param name="dataTable">dataTable from which x&y values will be 
		/// fetched</param>
		/// <param name="horizontalColumnName">name of the horizontal column
		/// </param>
		/// <param name="verticalColumnName">name of the vertical column
		/// </param>
		/// <returns>keyvalue list of desired data</returns>
		private static Dictionary<string, double> GetKVPairsForAverageForYearly
			(DataTable dataTable, string horizontalColumnName, string verticalColumnName)
		{
			// Fetch the required data by firing LINQ query on DataTable
			// Fetch records in a format: value @ x-axis on yearly basis 
			// & average of value @ y-axis
			// corresponding to value @ x-axis order by year of value @ x-axis
			var lstData = from DataRow row in dataTable.Rows
						  where (!string.IsNullOrEmpty(row[horizontalColumnName].ToString()) && !string.IsNullOrEmpty(Convert.ToDateTime(row[horizontalColumnName].ToString(),
						  CultureInfo.InvariantCulture).ToShortDateString()))
						  orderby Convert.ToDateTime(row[horizontalColumnName],
						  CultureInfo.InvariantCulture).Year
						  group row by new
						  {
							  year = Convert.ToDateTime(row[horizontalColumnName],
							  CultureInfo.InvariantCulture).Year
						  } into grp
						  select new
						  {
							  xValue = grp.Key.year.ToString(CultureInfo.InvariantCulture),
							  yValue =
								grp.Average(row => !string.IsNullOrEmpty(row[verticalColumnName].ToString()) ?
									Convert.ToDouble(row[verticalColumnName].ToString(),
									CultureInfo.InvariantCulture) : 0)
						  };

			// Convert lstData to Dictionary<string, double> type
			return lstData.ToDictionary(tuple => tuple.xValue,
				tuple => Convert.ToDouble(tuple.yValue));
		}

		#endregion
	}
}

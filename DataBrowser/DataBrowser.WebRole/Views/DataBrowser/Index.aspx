<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/OGDIMasterPage.Master" Inherits="System.Web.Mvc.ViewPage<DataBrowserModel>" %>

<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.App_GlobalResources" %>
<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.Models" %>
<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.Models.Comments" %>
<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
	<link href="../../Content/css/csharp.css" rel="stylesheet" type="text/css" />
	<script type="text/javascript" language="javascript" src="<%= Url.Content("http://ajax.googleapis.com/ajax/libs/jqueryui/1.8.0/jquery-ui.min.js") %>"></script>
	<script type="text/javascript" language="javascript" src="<%= Url.Content("../../Scripts/jquery.clipboard.min.js") %>"></script>
	
	<% if (!Model.IsPlanned) { %>
	  <script type="text/javascript" language="javascript" src="<%= Url.Content("../../Scripts/DataBrowser.js") %>"></script>
	  <script type="text/javascript" language="javascript" src="http://ecn.dev.virtualearth.net/mapcontrol/mapcontrol.ashx?v=6.2&mkt=en-us"></script>
	  <script type="text/javascript" language="javascript" src="<%= ResolveUrl("~/Scripts/querybuilder.min.js") %>"></script>
	<%} %>

	<!-- If you are using IE8, then COMMENT / UNCOMMENT this meta tag
		 to demonstrate working in IE7 -->
	<%--<meta http-equiv="X-UA-Compatible" content="IE=EmulateIE7" />--%>
	
	<script src="http://ajax.microsoft.com/ajax/beta/0909/MicrosoftAjax.js" type="text/javascript"></script>  
	
	<script type="text/javascript">
		Sys.Application.set_enableHistory(true);
		<% if (!Model.IsPlanned) { %>
		  Sys.Application.add_init(page_init);
		<%} %>
		
		//  Constant used for storing damaged bookmark error
		var mapControlError = '<%= UIConstants.DBPC_mapControlError %>';
		
		// Separator used for separating bookmarking parameters in URL Hash
		var hashSeparator = '<%= UIConstants.DBPC_hashSeparator %>';
		
		var map;
		var kmlLayer;
		var mapLoaded = false;
		var kmlQuery;
		var ExpanderImage = '<%= UIConstants.GC_ExpandImagePath %>';
		var ExpanderImageDescription = '<%= UIConstants.GC_ExpandImageLongDesc %>';
		var CollapserImage = '<%= UIConstants.GC_CollapseImagePath %>';
		var RunLogoImage = '<%= UIConstants.DBPC_RunLogoImage %>';
		var HelpLogoImage = '<%= UIConstants.DBPC_HelpLogoImage %>';
		var RunLogoImageAlternateText = '<%= UIConstants.DBPC_RunLogoImageAlternateText %>';
		var HelpLogoImageAlternateText = '<%= UIConstants.DBPC_HelpLogoImageAlternateText %>';
		var MapViewNote = '<%= UIConstants.DBPC_MapViewNote %>';
		var MapViewNoteLink = '<%= UIConstants.DBPC_MapViewNoteLink %>';
		var MapViewNoteLinkText = '<%= UIConstants.DBPC_MapViewNoteLinkText %>';
		var container = '<%= Html.Encode(ViewData.Model.Container) %>';
		var entitySet = '<%= Html.Encode(ViewData.Model.EntitySetName) %>';
		var mainFilter = '';
		var defaultFilterText = '<%= UIConstants.DBPC_DefaultFilterText %>';
		var defaultSelectOneText = '<%= UIConstants.DBPC_SelectOneText %>';

		var scheme = '<%= HttpContext.Current.Request.Url.Scheme %>';
		var host = '<%= HttpContext.Current.Request.Url.Host %>';
		var port = '<%= HttpContext.Current.Request.Url.Port %>';
		var appPath = '<%= HttpContext.Current.Request.ApplicationPath %>';
		var isBookmarked = '<%= ViewData.Model.IsBookmarked %>';
		var viewType = '';
		var tagType = '';
		var sampleCodeLanguage = '';       
		var longitude = 0;
		var latitude = 0;
		var zoomLevel = 11;
		var mapMode = 1;
		var mapStyle;
		var sceneId = 0;
		var birdseyeSceneOrientation = 0;
		var isSelectOne = '<%= ViewData.Model.IsSelectOne %>';        
		var noFilterText = '<%= UIConstants.DBPC_NoFilterText %>';
		var clientSideError = '<%= UIConstants.GC_BadRequestErrorString %>';
		var noRunClickedError = '<%= UIConstants.GC_NoRunClickedError %>';
		var horizontalAxisColText = '<%= UIConstants.DBPC_HorizontalAxisColumnText %>';
		var tooLongURLtoBookmark = '<%= UIConstants.DBPC_TooLongURLForBookmark %>';
		var filterRunned = noFilterText;
		var siteTitle = '<%= UIConstants.SiteTitle %>';
	</script>
	
	<style type="text/css">
		.hidepanle
		{
			width: 100%;
			border-bottom: solid 1px #c4c4c4;
		}
		.hidepanle h1
		{
			background: #ffffff url(/Content/images/arrow-square.gif) no-repeat right -51px;
			padding: 7px 15px;
			margin: 0;
			font: bold 16px Arial;
			border: solid 1px #cccccc;
			border-bottom: none;
			cursor: pointer;
		}
		.hidepanle h1.active
		{
			background-position: right 5px;
			background-color: #e3e2e2;
		}
		.hidepanle div.hidepanle
		{
			background: #ffffff;
			margin: 0;
			padding: 10px 15px 20px;
			border-left: solid 1px #cccccc;
			border-right: solid 1px #cccccc;
		}

		td.dslistheader
		{
			padding: 4px;
			background: url(/Content/images/sort.jpg);
			background-repeat: no-repeat;
			background-position: left;
			height: 34px;
			text-align: left;
			text-indent: 20px;
			font-family: Verdana;
			font-weight: bold;
			font-size: 12px;
			cursor: pointer;
		}
		
		td.desc
		{
			background: url(/Content/images/sorted_desc.jpg);
			background-repeat: no-repeat;
			background-position: left;
		} 

		td.asc
		{
			background: url(/Content/images/sorted_asc.jpg);
			background-repeat: no-repeat;
			background-position: left;
		} 


		table.dslistrow
		{
			background: url(/Content/images/listrow.jpg);
			height: 34px;
			border: 1px double #cccccc;
			margin-top: 2px;
			margin-bottom: 2px;
		}
		table.dslistrowselected
		{
			background: url(/Content/images/listrowh.jpg);
			height: 34px;
			border: 1px double #cccccc;
			margin-top: 2px;
			margin-bottom: 2px;
		}
		td.dslistrow
		{
			padding: 4px;
			text-align: left;
			font-family: Verdana;
			font-size: 11px;
			color: #404040;
		}
		
		.canvas .sheet {border-left-width:5px;}
	</style>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">

	<div class="tabs">
		<% if (!Model.IsPlanned) { %>
		<div class="tab tc1 t1" title="Data"><div class="active" onclick='q.Tabs.switchTabs(this, "eidDataTabContent")'></div></div>
		<% } %>
		<div class="tab tc2 t2" title="Comments"><div onclick='q.Tabs.switchTabs(this, "eidCommentsTabContent")'></div></div>
		<div class="tab tc3 t3" title="Details"><div <%if(Model.IsPlanned){%>class="active"<%}%> onclick='q.Tabs.switchTabs(this, "eidDetailsTabContent")'></div></div>
		<% if (!Model.IsPlanned) { %>
		<div class="tab tc4 t4" title="Developers"><div onclick='q.Tabs.switchTabs(this, "eidDevelopersTabContent")'></div></div>
		<% } %>
	</div>
	
	<iframe id="__historyFrame" src="<%= Url.Action("Index", "HistoryIframeHelper") %>" style="display:none;"></iframe>

	<% Html.RenderPartial("DataSetView", Model.EntitySetWrapper); %>

	<div id="divError" style="display: none">
		<% Html.RenderPartial("ErrorView", ViewData); %>
	</div>
	
	<% if (!Model.IsPlanned) { %>
	  <div class="tab-content" id="eidDataTabContent" style="display:block;">
		  <% Html.RenderPartial("DataSetData", Model); %>
	  </div>
	<% } %>
	
	<div class="tab-content" id="eidDetailsTabContent" <%if(Model.IsPlanned){%>style="display:block;"<%}%> >
		<% Html.RenderPartial("DataSetDetails", Model.EntitySetWrapper); %>
	</div>
  
	<div class="tab-content" id="eidCommentsTabContent">
		<% Html.RenderPartial("Comments", CommentInfo.CreateDatasetCommentInfo(Model.Container, Model.EntitySetName, Model.EntitySetWrapper.EntitySet.Name)); %>
	</div>
	
	<% if (!Model.IsPlanned) { %>
		<div class="tab-content" id="eidDevelopersTabContent">
			<% Html.RenderPartial("SampleCode", Model); %>
		</div>
	<% } %>

	<% Html.RenderPartial("Bookmark"); %>
	 
</asp:Content>
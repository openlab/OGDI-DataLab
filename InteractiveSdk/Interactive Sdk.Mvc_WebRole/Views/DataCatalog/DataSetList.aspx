<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/OGDIMasterPage.Master"
    Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.Models"%>
<%@ Import Namespace="System.Web.Mvc"%>
<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.App_GlobalResources"%>

<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">

    <script type="text/javascript" language="javascript" src="<%= Url.Content("../../Scripts/DataBrowser.min.js") %>"></script>
    <script type="text/javascript" language="javascript" src="<%= Url.Content("../../Scripts/ogdi/list.js") %>"></script>
          
    <script type="text/javascript">
        var fieldNames = ["Name", "Description", "Category", "Status", "Date", "Rating", "Views"];
        setListParameters("/DataCatalog/DataSets", "/DataCatalog/GetListDataJSON", fieldNames);

        $(document).ready(function() {
          $("#SubmitFilter").click(function() {
            filter = submitFilter();
            setPage(1);
            updateListData();
          });
          $("#ClearFilter").click(function() {
            clearFilter();
            filter = submitFilter();
            setPage(1);
            updateListData();
          });
        });

    </script>

</asp:Content>
<asp:Content ID="mainContent" ContentPlaceHolderID="MainContent" runat="server">
    
    <div class="top5">
		<table cellpadding="0" cellspacing="0" border="0"><tr>
		<td style="border-width:0px;"><% Html.RenderPartial("TopList", Model, new ViewDataDictionary(ViewData){{"TopOf", Field.Date}}); %></td>
		<td><% Html.RenderPartial("TopList", Model, new ViewDataDictionary(ViewData){{"TopOf", Field.Rating}}); %></td>
		<td><% Html.RenderPartial("TopList", Model, new ViewDataDictionary(ViewData){{"TopOf", Field.Views}}); %></td>
		</tr></table>
    </div>
        
	<div class="block">
		<% Html.RenderPartial("DataSetsFilter", Model); %>
    </div>
    <% var sortImageSrc = ResolveUrl("~/Images/t.gif"); %>
	<div class="dataset-list block">
		<table cellpadding="0" cellspacing="0" border="0">
		<thead><tr>
			<td id="Name" width="37.5%">Name<img src="<%= sortImageSrc %>" /></td>
			<td id="Category" width="12.5%" class="ascna">Category<img src="<%= sortImageSrc %>" /></td>
			<td id="Status" width="12.5%" class="ascna">Status<img src="<%= sortImageSrc %>" /></td>
			<td id="Date" width="12.5%" class="descna">Date<img src="<%= sortImageSrc %>" /></td>
			<td id="Rating" width="12.5%" class="descna">Rating<img src="<%= sortImageSrc %>" /></td>
			<td id="Views" width="12.5%" class="descna">Views<img src="<%= sortImageSrc %>" /></td>
		</tr></thead>
		<tbody class="rows"></tbody>
		<tfoot>
		<tr>
			<td colspan="2" >
				Have an idea about the data you would like to see here? Request it <a href="/Request/Index/">here</a>
			</td>
			<td colspan="4">
        <% Html.RenderPartial("PageControl"); %>
				<div class="clear"></div>
			</td>
		</tr>
		</tfoot>
		</table>
	</div>
    <div id="BackGroundLoadingIndicator" class="bgLoadingIndicator" style="display: none">
    </div>
    <div id="LoadingIndicatorPanel"  style="display: none; position:  ">
        <img id="imgLoading" class="loader" alt='<%= UIConstants.GC_LoadingAltText %>' style="display: none" src='<%=UIConstants.GC_LoadingImagePath %>' longdesc='<%= UIConstants.GC_LoadingLongDesc %>' />
    </div>

	<% Html.RenderPartial("Bookmark"); %>
</asp:Content>

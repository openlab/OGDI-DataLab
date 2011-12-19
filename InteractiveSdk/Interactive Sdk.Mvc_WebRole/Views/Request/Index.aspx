<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/OGDIMasterPage.Master" Inherits="System.Web.Mvc.ViewPage<Ogdi.InteractiveSdk.Mvc.Models.Request.RequestListModel>" %>
<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.App_GlobalResources" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
	<script type="text/javascript" language="javascript" src="<%= Url.Content("../../Scripts/DataBrowser.min.js") %>"></script>
	<script type="text/javascript" language="javascript" src="<%= Url.Content("../../Scripts/ogdi/list.js") %>"></script>

	<script type="text/javascript">
		var fieldNames = ["Name", "Description", "Category", "Status", "Date", "Rating", "Views"];
		setListParameters("/Request/List", "/Request/ListDataJSON", fieldNames);
	</script>
	
</asp:Content>

<asp:Content ID="mainContent" ContentPlaceHolderID="MainContent" runat="server">


<div class="block">
	<div class="requests">
		<div class="bar">New Data Requests</div>
		<p>See what others looking for. Need something different? Ask <a href="/Request/New/">here</a>. </p>
	</div>
</div>
<% var sortImageSrc = ResolveUrl("~/Images/t.gif"); %>
<div class="dataset-list block">
	<table cellpadding="0" cellspacing="0" border="0">
	<thead>
		<tr>
			<td id="Name" width="50%">Name <img src="<%= sortImageSrc %>" /></td>
			<td id="Status" width="12.5%">Status <img src="<%= sortImageSrc %>" /></td>
			<td id="Date" width="12.5%">Date <img src="<%= sortImageSrc %>" /></td>
			<td id="Rating" width="12.5%">Rating <img src="<%= sortImageSrc %>" /></td>
			<td id="Views" width="12.5%">Views <img src="<%= sortImageSrc %>" /></td>
		</tr>
	</thead>
	<tbody class="rows">
	</tbody>
	<tfoot>
		<tr>
			<td colspan="2"></td>
			<td colspan="3">
				<% Html.RenderPartial("PageControl"); %>
				<div class="clear"></div>
			</td>
		</tr>
	</tfoot>
	</table>
</div>
<div id="BackGroundLoadingIndicator" class="bgLoadingIndicator" style="display: none"></div>
<div id="LoadingIndicatorPanel" style="display: none; position: ">
	<img id="imgLoading" class="loader" alt='<%= UIConstants.GC_LoadingAltText %>' style="display: none" src='<%=UIConstants.GC_LoadingImagePath %>' longdesc='<%= UIConstants.GC_LoadingLongDesc %>' />
</div>
</asp:Content>

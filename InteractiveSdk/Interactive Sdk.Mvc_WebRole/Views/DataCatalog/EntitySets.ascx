<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Ogdi.InteractiveSdk.Mvc.Models.DataCatalogModel>" %>
<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.App_GlobalResources" %>
<% if (ViewData.Model.EntitySet.Count != 0)
   {%>
   
<h3 id="CategoryNamepanel">
<%= Html.Encode(ViewData.Model.CategoryName) %>
</h3>
<div class="sets">
	<table border="0" cellpadding="0" cellspacing="0">
	<tr id="trError" style="display: none">
		<td>
			<table>
			<tr><td style="color: Red"><%= Html.Encode(ViewData.Model.ErrorLine1)%></td></tr>
			<tr><td><%= Html.Encode(ViewData.Model.ErrorLine2)%></td></tr>
			</table>
		</td>
	</tr>
	<tr>
		<th>Name</th>
		<th>Source</th>
		<th>Description</th>
	</tr>
	<% try
	{
		var odd = true;
		foreach (Ogdi.InteractiveSdk.Mvc.Models.EntitySet item in ViewData.Model.EntitySet)
		{ %>
	<tr class="<%= odd ? "co" : "ce" %>">
		<td>
			<% string link = "../DataBrowser/" + item.ContainerAlias + "/" + item.EntitySetName + "#param=NOFILTER--DataView--Results"; %>
			<a href="<%= link %>"><%= Html.Encode(item.Name)%></a>
		</td>
		<td><%= Html.Encode(item.Source)%></td>
		<td>
			<%= Html.Encode(item.Description)%>
			<br />
			<a href="<%= Html.Encode(item.MetadataUrl) %>" target="_blank">More Information</a>
			<img src="<%= UIConstants.GC_ExternalLinkImagePath %>" title='<%= Html.Encode(item.Name) %>' alt="<%= UIConstants.GC_ExternalLinkAltText %>" longdesc="<%= UIConstants.GC_ExternalLinkLongDesc %>" />
		</td>
	</tr>
	<% odd = !odd; } } catch (Exception) { } %>
	</table>
</div>
<% } %>
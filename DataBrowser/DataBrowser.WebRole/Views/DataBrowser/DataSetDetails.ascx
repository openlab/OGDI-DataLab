<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.Models" %>
<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<EntitySetWrapper>" %>



<div class="dataset-details">
	<% var model = this.Model.EntitySet; %>
	<div class="bar">
		<%--onclick="$(this).toggleClass('expanded');$('#eidDatasetDetails').toggle();" title="Click to see details">--%>
		Dataset Details
		<%--<div class="toggler">
			<img src="<%= this.ResolveUrl("~/Content/ico.png") %>" title="Expand" class="ico icoPlus" />
			<img src="<%= this.ResolveUrl("~/Content/ico.png") %>" title="Collapse" class="ico icoMinus" />
		</div>--%>
	</div>
	<div class="content" id="eidDatasetDetails">
		<table class="record" cellpadding="0" cellspacing="0" border="0">
		
		<tr class="field">
			<td class="label">Dataset name</td>
			<td class="value"><%= Html.Encode(model.Name) %></td>
		</tr>
		
		<tr>
			<td class="label">Data source</td>
			<td class="value"><%= Html.Encode(model.Source) %></td>
		</tr>
		
		<tr>
			<td class="label">Category</td>
			<td class="value"><%= Html.Encode(model.CategoryValue) %></td>
		</tr>		
		
		<tr>
			<td class="label">Released Date</td>
			<td class="value"><%= Html.Encode(EntitySet.GetEntityDateAsString(model.ReleasedDate))%></td>
		</tr>

		<tr>
			<td class="label">Last Updated Date</td>
			<td class="value"><%= Html.Encode(EntitySet.GetEntityDateAsString(model.LastUpdateDate))%></td>
		</tr>

		<tr>
			<td class="label">Expired Date</td>
			<td class="value"><%= Html.Encode(EntitySet.GetEntityDateAsString(model.ExpiredDate))%></td>
		</tr>
		
		<tr>
			<td class="label">Update frequency</td>
			<td class="value"><%= Html.Encode(model.UpdateFrequency) %></td>
		</tr>
		
		<tr>
			<td class="label">Description</td>
			<td class="value"><%= Html.Encode(model.Description) %></td>
		</tr>
		
		<tr>
			<td class="label">Status</td>
			<td class="value">
			    <% if(model.IsEmpty) { %>
			        <%= Html.Encode("Planned") %>
			    <% } else { %>
			        <%= Html.Encode("Published") %>
			    <% } %>			
			</td>
		</tr>				

		<tr>
			<td class="label">Time period covered</td>
			<td class="value"><%= Html.Encode(model.PeriodCovered) %></td>
		</tr>
		
		<tr>
			<td class="label">Geographic area covered</td>
			<td class="value"><%= Html.Encode(model.GeographicCoverage) %></td>
		</tr>
		
		<tr>
			<td class="label">Collection Mode</td>
			<td class="value"><%= Html.Encode(model.CollectionMode) %></td>
		</tr>		
		
		<tr>
			<td class="label">Metadata Url</td>
			<td class="value"><%= Html.Encode(model.MetadataUrl) %></td>
		</tr>		
		
		<tr>
			<td class="label">Keywords</td>
			<td class="value"><%= Html.Encode(model.Keywords) %></td>
		</tr>
		
		<tr>
			<td class="label">Links and references</td>
			<td class="value"><a href="<%=model.Links %>"><%= Html.Encode(model.Links) %></a></td>
		</tr>
		
		<tr>
			<td class="label">Collection Instruments</td>
			<td class="value"><%= Html.Encode(model.CollectionInstruments) %></td>
		</tr>
		
		<tr>
			<td class="label">Technical Documentation</td>
			<td class="value"><%= Html.Encode(model.TechnicalInfo) %></td>
		</tr>				
		
		<tr>
			<td class="label">Data Dictinary/Variables</td>
			<td class="value"><%= Html.Encode(model.DataDictionaryVariables) %></td>
		</tr>
		
		<tr>
			<td class="label">Additional information</td>
			<td class="value"><%= Html.Encode(model.AdditionalInformation) %></td>
		</tr>		
		
		</table>
	</div>
</div>

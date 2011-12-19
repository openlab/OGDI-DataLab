<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.Models" %>
<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.Models.Rating" %>
<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.Models.Request" %>
<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Ogdi.InteractiveSdk.Mvc.Models.Request.RequestListModel>" %>
<% var odd = true;
	foreach (Request request in ViewData.Model.List)
   {%>
<tr class="<%= odd ? "co" : "ce" %>">
	<td>
		<%
	string link = "/Request/Details/?id=" + request.RequestID;
		%>
		<a href="<%=link%>"><b><%= Html.Encode(request.Subject) %></b></a>
		<div class="description"><%= Html.Encode(request.Description) %></div>
	</td>
	<td><%= Html.Encode(request.Status) %></td>
	<td><%= request.PostedDate.HasValue ? request.PostedDate.Value.ToString("MM/dd/yyyy") : ""%></td>
	<td>
		<% Html.RenderPartial("Rates", new RateInfo(request.ItemKey, request.PositiveVotes, request.NegativeVotes) ); %>
	</td>
	<td><%= request.Views.ToString()%></td>
</tr>
<% odd = !odd; } %>
<input id="total" type="hidden" value="<%= ViewData.Model.TotalPages %>" /> 
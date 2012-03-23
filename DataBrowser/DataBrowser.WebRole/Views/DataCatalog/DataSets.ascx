<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.Models" %>
<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.Models.Rating" %>
<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Ogdi.InteractiveSdk.Mvc.Models.DatasetListModel>" %>
<% 
	var odd = true;
	foreach (EntitySetWrapper entity in ViewData.Model.MainList)
   {%>
<tr class="<%= odd ? "co" : "ce" %>">
	<td>
		<%
			string link = this.ResolveUrl("~/DataBrowser/" + entity.EntitySet.ContainerAlias + "/" + entity.EntitySet.EntitySetName + "#param=NOFILTER--DataView--Results");
		%>
		<a href="<%=link%>"><b><%=entity.EntitySet.Name%></b></a>
		<div class="description"><%=entity.EntitySet.Description%></div>
	</td>
	<td>
		<%=entity.EntitySet.CategoryValue%>
	</td>
	<td>
	  <%=entity.EntitySet.IsEmpty ? "Planned" : "Published"%>
	</td>
	<td>
		<%=entity.EntitySet.LastUpdateDate.ToString("MM/dd/yyyy")%>
	</td>
	<td>
		<% Html.RenderPartial("Rates", new RateInfo(entity.EntitySet.ItemKey , entity.PositiveVotes, entity.NegativeVotes) ); %>	</td>
	<td>
		<%=entity.Views.ToString()%>
		<input id="total" type="hidden" value="<%=ViewData.Model.PageCount%>" /> 
	</td>
</tr>
<% odd = !odd; } %>

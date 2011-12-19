<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<DatasetListModel>" %>
<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.Models" %>
<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.Models.Rating" %>

<%
	var topOfField = (Field) ViewData["TopOf"];
    string title;
	switch (topOfField)
    {
        case Field.Date:
            title = "Recently Published";
            break;
        case Field.Rating:
            title = "Most Rated";
            break;
        case Field.Views:
            title = "Most Viewed";
            break;
        default:
            title = "by " + Model.OrderBy.Field;
            break;
    }
%>
<div class="bar"><%=title%></div>
<table class="items" cellpadding="0" cellspacing="0" border="0">
<% foreach (var item in Model.GetTopList(topOfField))
   {
       if (topOfField == Field.Rating && item.PositiveVotes == 0 && item.NegativeVotes == 0)
           continue;

       if (topOfField == Field.Views && item.Views == 0)
           continue;
       
	   string link = ResolveUrl("~/DataBrowser/" + item.EntitySet.ContainerAlias + "/" + item.EntitySet.EntitySetName + "#param=NOFILTER--DataView--Results");
%>
    <tr class="item">
		<td class="name"><a href="<%= link %>"><%= Html.Encode(item.EntitySet.Name)%></a></td>
       <%
		   switch (topOfField)
           {
			   case Field.Date: {
				%>
					<td class="value" title="<%= Html.Encode(item.EntitySet.LastUpdateDate.ToString("D")) %>">
						<%= Html.Encode(item.EntitySet.LastUpdateDate.ToString("MM/dd/yyyy"))%>
					</td>
				<%
		   break;
				   }
			   case Field.Rating:
				   {
				%>
					<td class="value">
						<% Html.RenderPartial("Rates", new RateInfo(item.EntitySet.ItemKey, item.PositiveVotes, item.NegativeVotes) ); 
                       %>
					</td>
				<%
					break;
				   }
			   case Field.Views:
				   {
				%>
					<td class="value">
						<%= item.Views.ToString()%>
					</td>
				<%
					break;
				   }
               default:
                   break;
           }
       %>
	</tr>
<%} %>
</table>

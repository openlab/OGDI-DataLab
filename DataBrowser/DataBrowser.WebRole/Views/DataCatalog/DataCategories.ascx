<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Ogdi.InteractiveSdk.Mvc.Models.DataCatalogModel>" %>
<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.App_GlobalResources"  %>
<td valign="top">
    <div class="leftmenu">
        <div id="CategoryPanel">
            <table cellpadding="0" cellspacing="0">
            <tr>
                <td>
                <% if ((string.IsNullOrEmpty(ViewData.Model.CurrentCategory)) || (ViewData.Model.CurrentCategory.Equals(UIConstants.DCPC_AllDataSetsText))) { %>
                    <a id="All" href="javascript:categoryLinkClicked('All');" style="font-weight: bold">All</a>
                <% } else { %> 
                    <a id="All" href="javascript:categoryLinkClicked('All');">All</a>
                <% } %>
                </td>
            </tr>
            <% try
               {
                   foreach (string item in ViewData.Model.CategoryList)
                   { %>
            <tr>
                <td>
                    <% if (!string.IsNullOrEmpty(ViewData.Model.CurrentCategory) && item.Equals(ViewData.Model.CurrentCategory)) { %>
						<a id="<%= item %>" href="javascript:categoryLinkClicked('<%= item %>');" style="font-weight: bold"><%= item%></a>
                    <% } else { %>                    
						<a id="<%= item %>" href="javascript:categoryLinkClicked('<%= item %>');"><%= item%></a>
                    <% } %>
                </td>
            </tr>
            <%		}
               }
               catch (Exception) { } %>
            </table>
        </div>
    </div>
</td>
<td valign="top" id="rightPanelDiv"><% Html.RenderPartial("EntitySets", ViewData); %></td>

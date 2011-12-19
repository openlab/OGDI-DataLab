<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/OGDIMasterPage.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<table width="100%">
<tr>
<td width="30%">
</td>
<td width="40%">
    <ul>
        <li><%= Html.ActionLink("Comments", "AgencyComments", "Comments")%></li>
        <li><%= Html.ActionLink("Requests", "RequestsManage", "Request")%></li>
        <li><a href="/Reports/Reports.aspx">Reports</a></li>
    </ul>
</td>
<td width="30%">
</td>
</tr>
</table>

</asp:Content>


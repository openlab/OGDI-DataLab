<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Ogdi.InteractiveSdk.Mvc.Models.EntitySet>" %>
<table>
    <tr>
        <td align="right">
            Dataset name
        </td>
        <td align="left">
            <%=Model.Name %>
        </td>
    </tr>
    
    <tr>
        <td align="right">
            Data source
        </td>
        <td align="left">
            <%=Model.Source%>
        </td>
    </tr>
    
    <tr>
        <td align="right">
            Category
        </td>
        <td align="left">
            <%=Model.CategoryValue %>
        </td>
    </tr>
    
    <tr>
        <td align="right">
            Date to be released
        </td>
        <td align="left">
            <%=Model.ReleasedDate %>
        </td>
    </tr>
    
    <tr>
        <td align="right">
            Update frequency
        </td>
        <td align="left">
            <%=Model.UpdateFrequency %>
        </td>
    </tr>
    
    <tr>
        <td align="right">
            Description
        </td>
        <td align="left">
            <%=Model.Description %>
        </td>
    </tr>
    
    <tr>
        <td align="right">
            Keywords
        </td>
        <td align="left">
            <%=Model.Keywords %>
        </td>
    </tr>
    
    <tr>
        <td align="right">
            Links and references
        </td>
        <td align="left">
            <a href="<%=Model.Links %>"><%=Model.Links %></a>
        </td>
    </tr>
    
    <tr>
        <td align="right">
            Time period covered
        </td>
        <td align="left">
            <%=Model.PeriodCovered %>
        </td>
    </tr>
    
    <tr>
        <td align="right">
            Geographic area covered
        </td>
        <td align="left">
            <%=Model.GeographicCoverage %>
        </td>
    </tr>
    
    <tr>
        <td align="right">
            Additional information
        </td>
        <td align="left">
            <%=Model.AdditionalInformation %>
        </td>
    </tr>
</table>


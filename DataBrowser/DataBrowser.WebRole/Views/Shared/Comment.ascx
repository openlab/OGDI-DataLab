<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Ogdi.InteractiveSdk.Mvc.Models.Comments.Comment>" %>
<div class="parameters">
    <table cellpadding="0" cellspacing="0" width="100%">    
        <tr>
            <td width="330px">                        
            </td>
            <td width="250px">            
                <span class="label">POSTED ON</span><%= Html.Encode(Model.PostedOnAsText)%>            
            </td>
            <td width="290px">
                <span class="label">TYPE</span><%= Html.Encode(Model.Type)%>
            </td>
            <td align="right">
                <span class="addresed">
                    <% if ((Model.Status == "Addresed")||
                           (Model.Status == "Replied")||
                           (Model.Type == "General Comment (reply required)"))
                       { %>
                    Addresed
                <% } %>
                </span>
            </td>
            <td align="right">
                <span class="delete"><a href="javascript:DeleteComment('<%= Html.Encode(Model.RowKey) %>')">
                    <img src="<%= ResolveUrl("~/Content/ico.png") %>" title="Delete this comment" class="ico icoRemove"
                        alt="delete" />
                </a></span>
            </td>
        </tr>
    </table>
</div>
    
<div>
    <span class="author">From
        <%= Html.Encode(Model.Author)%>: </span><span class="subject">
            <%= Html.Encode(Model.Subject)%></span>
</div>
<div class="comment">
    <%= Html.Encode(Model.Body)%></div>

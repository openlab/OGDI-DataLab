<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/OGDIMasterPage.Master" Inherits="System.Web.Mvc.ViewPage<Ogdi.InteractiveSdk.Mvc.Models.ManageRolesModel>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

<div style="text-align:center">
    <table style="text-align:center; margin-left:auto; margin-right:auto; width:440px;">
    <tr>
    <td>
     <div class="boxTopMedia" style="margin-left:-6px;"></div>  
     <div class="boxMidMedia" style="margin-left:-6px;">
       <div class="boxMidPadMedia" style="margin-left:-6px;">
          <% using (Html.BeginForm()) { %>
          <%= Html.Hidden("Operation")%>
          <table style="width:95%;">
                    <tr style="height:40px">
                        <td style="width:100%;text-align:center" colspan="2">
                            <div style="font-size:24px; height:38px; color:#a3a3a3; line-height:14pt;">
                                <b>Manage Roles</b>
                            </div>
                            <b><%= Html.ValidationMessage("_FORM")%></b>
                        </td>
                    </tr>
                    <tr>
                        <td style="width:50%;text-align:right;vertical-align:top">
                            <label for="UserName">Account login:</label>
                        </td>
                        <td style="width:50%;text-align:left;font-size:smaller">
                            <%= Html.TextBox("UserName",Model.UserName)%><br />
                            <%= Html.ValidationMessage("UserName")%>
                        </td>
                    </tr>
                    <tr>
                        <td style="width:50%;text-align:right;vertical-align:top">
                            <label for="RoleNames">Roles:</label>
                        </td>
                        <td style="width:50%;text-align:left;font-size:smaller">
                            <%= Html.DropDownList("RoleName", Model.RoleNames)%>
                            <%= Html.ValidationMessage("RoleName") %>
                        </td>
                    </tr>
                    <tr>
                        <td style="width:50%;text-align:right">
                            <input type="button" value="Assign" onclick="javascript:$('#Operation').val('Add');$('form').submit();" />
                        </td>
                        <td style="width:50%;text-align:left">
                            <input type="button" value="Unassign" onclick="javascript:$('#Operation').val('Remove');$('form').submit();"/>
                        </td>
                    </tr>
                </table>       
            <% } %> 
       </div>  
     </div> 
    <div class="boxBottomMedia" style="margin-left:-6px;"></div>
    </td>
    </tr>
    </table>
</div>
</asp:Content>

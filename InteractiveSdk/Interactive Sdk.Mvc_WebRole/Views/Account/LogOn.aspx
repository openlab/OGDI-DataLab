<%@ Page Language="C#" MasterPageFile="~/Views/Shared/OGDIMasterPage.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="loginContent" ContentPlaceHolderID="MainContent" runat="server">
<div style="text-align:center">
    <table style="text-align:center; margin-left:auto; margin-right:auto; width:440px;">
    <tr>
    <td>
     <div class="boxTopMedia" style="margin-left:-6px;"></div>  
     <div class="boxMidMedia" style="margin-left:-6px;">
       <div class="boxMidPadMedia" style="margin-left:-6px;">
          <% using (Html.BeginForm()) { %>
          <table style="width:95%;">
                    <tr style="height:40px">
                        <td style="width:100%;text-align:center" colspan="2">
                            <div style="font-size:24px; height:38px; color:#a3a3a3; line-height:14pt;">
                                <b>Log On</b>
                            </div>
                            <b><%= Html.ValidationMessage("_FORM")%></b>
                        </td>
                    </tr>
                    <tr>
                        <td style="width:50%;text-align:right;vertical-align:top">
                            <label for="username">Login:</label>
                        </td>
                        <td style="width:50%;text-align:left;font-size:smaller">
                            <%= Html.TextBox("username") %><br/>
                            <%= Html.ValidationMessage("username") %>
                        </td>
                    </tr>
                    <tr>
                        <td style="width:50%;text-align:right;vertical-align:top">
                            <label for="password">Password:</label>
                        </td>
                        <td style="width:50%;text-align:left;font-size:smaller">
                            <%= Html.Password("password") %><br/>
                            <%= Html.ValidationMessage("password") %>
                        </td>
                    </tr>
                    <tr>
                        <td style="width:50%;text-align:right">
                        </td>
                        <td style="width:50%;text-align:left">
                            <%= Html.CheckBox("rememberMe") %> 
                            <label class="inline" for="rememberMe">Remember me?</label>
                        </td>
                    </tr>
                    <tr>
                        <td style="width:50%;text-align:right">
                        </td>
                        <td style="width:50%;text-align:left">
                            <input type="submit" value="Login" />
                        </td>
                    </tr>
<%--
                     <tr>
                        <td style="width:50%;text-align:right">
                        </td>
                        <td style="width:50%;text-align:left">
                            <a href="/Account/Register">Not registered?</a>
                        </td>
                    </tr>
                    <tr>
                        <td style="width:50%;text-align:right">
                        </td>
                        <td style="width:50%;text-align:left">
                            <a href="/Account/ForgetPassword">Forget Password?</a>
                        </td>
                    </tr>
--%>
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

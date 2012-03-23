<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%
    if (Request.IsAuthenticated) 
    {
        if("Administrator".Equals(Session["UserRole"]))
        {
%>
        [ <%= Html.ActionLink("Manage roles", "ManageRoles", "Account") %> ]
<%            
        }
%>
        [ <%= Html.ActionLink("Registration Info", "UpdateUserInfo", "Account") %> ]
        [ <%= Html.ActionLink("Log Off", "LogOff", "Account") %> ]
<%
    }
    else {
%> 
        [ <%= Html.ActionLink("Log On", "LogOn", "Account") %> ]
<%
    }
%>
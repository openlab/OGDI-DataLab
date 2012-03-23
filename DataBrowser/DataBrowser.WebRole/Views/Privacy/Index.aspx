<%@ Page Language="C#" MasterPageFile="~/Views/Shared/OGDIMasterPage.Master" 
Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.App_GlobalResources"  %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    
    <%= Ogdi.Azure.Configuration.OgdiConfiguration.GetValue("PrivacyPage")%>
    
    <% Html.RenderPartial("Bookmark"); %>
</asp:Content>

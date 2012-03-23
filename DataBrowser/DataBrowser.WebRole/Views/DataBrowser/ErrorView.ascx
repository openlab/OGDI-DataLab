<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Ogdi.InteractiveSdk.Mvc.Models.DataBrowserModel>" %>
<p id="pError" class="big" style="margin-left: 25px">
    <strong style="color: Red">
        <%= Html.Encode(ViewData.Model.DBErrorLine1)%>
    </strong>
    <br />
    <%= Html.Encode(ViewData.Model.DBErrorLine2)%>
</p>

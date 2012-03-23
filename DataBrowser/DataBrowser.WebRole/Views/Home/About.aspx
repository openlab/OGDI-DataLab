<%@ Page Language="C#" MasterPageFile="~/Views/Shared/OGDIMasterPage.Master" Inherits="System.Web.Mvc.ViewPage<Ogdi.InteractiveSdk.Mvc.Models.HomeModel>" %>
<asp:Content ID="indexContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="welcome">
        <h3>About DataLab</h3>
        <p>
            <strong>DataLab</strong> is an open source cloud-based Open Data Catalogue. This
            codebase represents an update to the original open source framework released by
            Microsoft Open Government Data Initiative (OGDI). It’s an easy way to get started
            with your open data project, providing:</p>
        <ul>
            <li><strong>Data Service</strong>: a fully-functional API allowing programmatic access
                to the data. It serves up data using a number of different formats such as XML (oData/ATOM),
                KML, JSON, and JSONP that can be queried by variety of technologies & software.</li>
            <li><strong>Data Browser</strong>: a web interface to the data service with a visual
                way to browse, query, interact with and download data. It includes mapping & charting
                capabilities for slicing/dicing & visualizing geographic datasets.</li>
            <li><strong>Data Loader</strong>: a software utility used to import the data into the
                catalogue from CSV and KML formats. Users can load data from a client machine or
                also create a command-line access to load datasets dynamically from databases</li>
        </ul>
        <p>
            DataLab leverages the scalability, flexibility and reliability of the <a href="http://www.windowsazure.com/">
                Windows Azure Platform</a> and can lower up-front infrastructure costs of your
            open data initiative.</p>
        <% Html.RenderPartial("Bookmark"); %>
    </div>
    <div style="clear: both; position: relative">
    </div>
</asp:Content>

<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Ogdi.InteractiveSdk.Mvc.Models.DataBrowserModel>" %>

<% 
    chart.Controls.Add(Model.BarChart);
%>

<asp:Panel ID="chart" runat="server" />
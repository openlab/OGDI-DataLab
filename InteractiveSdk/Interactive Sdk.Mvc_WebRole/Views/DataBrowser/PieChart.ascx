<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Ogdi.InteractiveSdk.Mvc.Models.DataBrowserModel>" %>

<script type="text/javascript">
    var h = '<%= Model.PieChartHeight %>';
    document.getElementById("divPieResultHeight").style.height = parseInt(h) + "px";  
</script>

<% chart.Controls.Add(Model.PieChart); %>

<asp:Panel ID="chart" runat="server" />
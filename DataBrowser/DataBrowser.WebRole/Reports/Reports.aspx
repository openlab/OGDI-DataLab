<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Reports.aspx.cs" MasterPageFile="~/Reports/Reports.Master" Inherits="Ogdi.InteractiveSdk.Reports.Reports" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>
<asp:Content ContentPlaceHolderID="TitleContent" runat="server">
	<style type="text/css">
		.canvas .sheet {border-left-width:5px;}
	</style>
</asp:Content>
<asp:Content Id="MainContent" ContentPlaceHolderID="MainContent" runat="server">

<div class="tabs">
	<div class="tab tc2 t2" title="Comments"><div onclick='q.Tabs.switchTabs(this, null, "<%= this.ResolveUrl("~/Comments/AgencyComments") %>")'></div></div>
	<div class="tab tc6 t6" title="Requests"><div onclick='q.Tabs.switchTabs(this, null, "<%= this.ResolveUrl("~/Request/RequestsManage") %>")'></div></div>
	<div class="tab tc5 t5" title="Reports"><div class="active" onclick='q.Tabs.switchTabs(this, null, "<%= this.ResolveUrl("~/Reports/Reports.aspx") %>")'></div></div>
</div>

<div class="block reports">
		<div class="form">
			<div class="bar">
				Reports</div>
			<div class="content">
				<div class="left">
					<div class="label">
						Report type</div>
					<div>
						<asp:DropDownList ID="DropDownList1" runat="server" />
					</div>
				</div>
				<div class="left">
					<div class="label">
						From</div>
					<div>
						<asp:Calendar ID="From" runat="server" />
					</div>
				</div>
				<div class="left">
					<div class="label">
						To</div>
					<div>
						<asp:Calendar ID="To" runat="server" />
					</div>
					<div class="buttons">
						<input type="submit" name="Run" value="Run" /></div>
				</div>
				<div class="clear">
				</div>
			</div>
		</div>
		<div class="block">
			<rsweb:ReportViewer ID="ReportViewer1" runat="server" Font-Names="Verdana" Font-Size="8pt"
				Width="800px" Height="600px" ProcessingMode="Local">
				<LocalReport>
					<DataSources>
						<rsweb:ReportDataSource DataSourceId="ObjectDataSource1" Name="Data" />
					</DataSources>
				</LocalReport>
			</rsweb:ReportViewer>
			<asp:ObjectDataSource ID="ObjectDataSource1" runat="server">
				<SelectParameters>
					<asp:ControlParameter ControlID="From" Name="datefrom" PropertyName="SelectedDate"
						Type="DateTime" />
					<asp:ControlParameter ControlID="To" Name="todate" PropertyName="SelectedDate" Type="DateTime" />
				</SelectParameters>
			</asp:ObjectDataSource>
		</div>
	</div>
</asp:Content>
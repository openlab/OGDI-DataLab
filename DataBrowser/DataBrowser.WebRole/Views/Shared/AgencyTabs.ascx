<%@ Control Language="C#" Inherits="Ogdi.InteractiveSdk.Mvc.OgdiViewUserControl<Ogdi.InteractiveSdk.Mvc.Models.ViewData.AgencyTabsViewModel>" %>
<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.Models.ViewData" %>

<div class="tabs">
	<div class="tab tc2 t2" title="Comments"><div <%= this.Model.Active == AgencyTab.Comments ? " class=\"active\"" : "" %> onclick='q.Tabs.switchTabs(this, null, "<%= this.ResolveUrl("~/Comments/AgencyComments") %>")'></div></div>
	<div class="tab tc6 t6" title="Requests"><div <%= this.Model.Active == AgencyTab.Requests ? " class=\"active\"" : "" %> onclick='q.Tabs.switchTabs(this, null, "<%= this.ResolveUrl("~/Request/RequestsManage") %>")'></div></div>
	<div class="tab tc5 t5" title="Reports"><div <%= this.Model.Active == AgencyTab.Reports ? " class=\"active\"" : "" %> onclick='q.Tabs.switchTabs(this, null, "<%= this.ResolveUrl("~/Reports/Reports.aspx") %>")'></div></div>
</div>
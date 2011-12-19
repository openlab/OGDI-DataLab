<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.App_GlobalResources" %>
<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.Models" %>
<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc" %>
<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.Models.Comments" %>
<%@ Control Language="C#" Inherits="OgdiViewUserControl<DataBrowserModel>" %>

<div class="query">
	<div class="label">Base Query</div>
	<div>
		<%= ViewData.Model.BaseQueryName %>
		<label id="labelBaseQuery" title="<%= ViewData.Model.BaseQueryName %>" visible="false"></label>
	</div>
	
		<div class="label">Filter Expression</div>
		<div class="switch">
		  <span id="eidQbVisual" class="active">Query Builder</span>
			<span id="eidQbTextual">Text Query</span>
		</div>
		<div>
			<div class="expression left" id="eidQueryBoxContainer" style="display:none;">
				<div class="left"><textarea id="queryBox" rows="3" cols="20" ><%=  ViewData.Model.FilterText %></textarea></div>
				<div class="left"><a onclick="javascript:ShowFilterHintsDialog()"><img src="<%= this.ResolveUrl("~/Content/ico.png") %>" class="ico icoQuestion" title="Show Filter Hints" /></a></div>
				<div class="clear"></div>
			</div>
			<div id="eidBuilder" class="query-builder left" ></div>
			<div class="left">
			<a href="javascript:runClicked()"><%= Html.NiceButton(this, "run", 0, null) %></a>
			<a href="javascript:clearClicked()"><%= Html.NiceButton(this, "clear", 0, null) %></a>
			</div>
			<div class="clear"></div>
		</div>
</div>

<script type="text/javascript">

	var meta = new Array();
	
<% foreach (System.Data.DataColumn column in Model.EntitySetDetails.DetailsTable.Columns) { %>
meta.push({ name: "<%= column.ColumnName %>", type: "<%= column.DataType %>" });
<% } %>

    var builder = new Ogdi.QueryBuilder($("#eidBuilder"), meta, $("#queryBox"), $("#eidQueryBoxContainer"));
	$(function() {
		
		builder.addFilter();
		builder.dump();
	});
	
	function clearClicked()
	{
	    builder.clearFilter();
		builder.addFilter();
		builder.dump();	    
	}
	
</script>


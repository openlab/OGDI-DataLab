<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Ogdi.InteractiveSdk.Mvc.Models.DataBrowserModel>" %>
<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.App_GlobalResources"  %>
<%@ Import Namespace="System.Web.Mvc" %>
<%@ Import Namespace="System.Collections.Generic" %>

<label style="display:none" id="labelNextEnable" title="<%= Html.Encode(Convert.ToInt16(ViewData.Model.NextEnable).ToString()) %>"></label>
<label style="display:none" id="labelPrevEnable" title="<%= Html.Encode(Convert.ToInt16(ViewData.Model.PrevEnable).ToString()) %>"></label>
<label style="display:none" id="labelError" title="<%= Html.Encode(ViewData.Model.DBErrorLine1) %>"></label>

    <table cellpadding="0" cellspacing="0" width="100%" >
    <tr>
    <td width="100%">
        <div>Full query URL:  <a id="dataFullQueryUrlHyperlink" target="_blank" href="<%= Html.Encode(ViewData.Model.FilteredQueryName) %>"><%= Html.Encode(ViewData.Model.FilteredQueryName)%></a></div>
        <div>(Click to view results as XML/Atom)</div>    
    </td>    
    <td align="right">    
        <div id="eidDaisyDownloadDiv">
           <%= Html.ActionLink("DOWNLOAD", "Download", "DataBrowser", 
                               new { container = ViewData.Model.Container,
                                     entitySet = ViewData.Model.EntitySetName,
                                     filter = string.IsNullOrEmpty(Html.Encode(ViewData.Model.FilterText)) ? UIConstants.DBPC_NoFilterText :
                                                Html.Encode(ViewData.Model.FilterText).ToString().Trim().Equals(UIConstants.DBPC_DefaultFilterText) ?
                                                UIConstants.DBPC_NoFilterText : Html.Encode(ViewData.Model.FilterText).ToString().Trim()}, null)%>           
        </div>
        <div id="eidCSVDownloadDiv">
            <%= Html.ActionLink("DOWNLOAD", "DownloadCsv", "DataBrowser",
                                  new
                                  {  container = ViewData.Model.Container,
                                     entitySet = ViewData.Model.EntitySetName,
                                     filter = string.IsNullOrEmpty(Html.Encode(ViewData.Model.FilterText)) ? UIConstants.DBPC_NoFilterText :
                                                Html.Encode(ViewData.Model.FilterText).ToString().Trim().Equals(UIConstants.DBPC_DefaultFilterText) ?
                                                UIConstants.DBPC_NoFilterText : Html.Encode(ViewData.Model.FilterText).ToString().Trim()}, null)%>
            
        </div>
        <div id="eidExcelDownloadDiv">
            <%= Html.ActionLink("DOWNLOAD", "DownloadCsv", "DataBrowser",
                                  new
                                  {  container = ViewData.Model.Container,
                                     entitySet = ViewData.Model.EntitySetName,
                                     filter = string.IsNullOrEmpty(Html.Encode(ViewData.Model.FilterText)) ? UIConstants.DBPC_NoFilterText :
                                                Html.Encode(ViewData.Model.FilterText).ToString().Trim().Equals(UIConstants.DBPC_DefaultFilterText) ?
                                                UIConstants.DBPC_NoFilterText : Html.Encode(ViewData.Model.FilterText).ToString().Trim()}, null)%>
            
        </div>
        <td>&nbsp;as&nbsp;</td>                                     
        </td>
        <td align="right">        
            <select id="eidDownloadType" onchange="javascript:downloadTypeChange()">
              <option>CSV</option>
              <option>Excel</option>
              <option>Daisy</option>
            </select>        
        </td>                       
    </tr>
    </table>

<script language="javascript">    

    function downloadTypeChange() {

        $("#eidDaisyDownloadDiv").hide();
        $("#eidCSVDownloadDiv").hide();
        $("#eidExcelDownloadDiv").hide();
    
        switch($("#eidDownloadType")[0].selectedIndex)
        {
            case 0:
                $("#eidCSVDownloadDiv").show();
                break;
            case 1:                
                $("#eidExcelDownloadDiv").show();
               break;
           case 2:
                $("#eidDaisyDownloadDiv").show();
                
               break;
        }               
    }

    downloadTypeChange();
</script>

<div class="data-sample">

	<% if (this.Model.TableBrowserData != null && this.Model.TableBrowserData.Columns != null) { %>

    <table cellspacing="1" cellpadding="2" border="0">
        <% System.Data.DataTable detailsTable = ViewData.Model.TableBrowserData; %>
        <thead>
            <tr>
                <% foreach (System.Data.DataColumn col in detailsTable.Columns) { %>
                <th><%= col.ColumnName%></th>
                <% } %>
            </tr>
        </thead>
        <tbody>
            <%	var odd = true; foreach (System.Data.DataRow row in detailsTable.Rows) { %>
				<tr class="<%= odd ? "co" : "ce" %>">
					<% bool first = true; foreach (System.Data.DataColumn col in detailsTable.Columns) { %>
                    <td style="white-space: nowrap; text-align: left">
                        <% if (first) { first = false; %>
							<img src='<%= UIConstants.DBPC_KeyImagePath %>' alt='<%= row[col].ToString() %>' title='<%= row[col].ToString() %>' longdesc='<%= UIConstants.DBPC_KeyLongDesc %>' style="height: 15px; border-width: 0px;" />
                        <% } else { %>
							<% var value = row[col]; %>
							<%= Object.ReferenceEquals(value, null) ? "&nbsp;" : Html.Encode(value) %>
                        <% } %>
                    </td>
                    <% } %>
                </tr>
			<% odd = !odd; } %>
        </tbody>
    </table>
    <% } %>
</div>
<div class="data-sample">
<table cellspacing="1" cellpadding="2" border="0">
        <tfoot>
            <tr>
                <td>
                    <div class="page-size">
                        30 Items per page
                    </div>
                    <div id="eidPagingControl" class="paging">
                    </div>
                    <div class="clear">
                    </div>
                </td>
            </tr>
        </tfoot>
</table>
</div>
<script type="text/javascript">

    $(document).ready(function() {

    var container = $("#eidPagingControl");

        $("<img src='../../Content/ico.png' title='Previous page' class='ico icoPrev' id='prevLink' onclick='previousClicked();' />").appendTo(container);
        q.tag("span", "paginglinks").appendTo(container);
        $("<img src='../../Content/ico.png' title='Next page' class='ico icoNext' id='nextLink' onclick='nextClicked();' />").appendTo(container);
        
        SetPagingLinkVisibility();
        ShowHideError();
    });
    
    
</script>
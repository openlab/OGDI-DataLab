<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<DataBrowserModel>" %>

<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.App_GlobalResources" %>
<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.Models" %>

<div class="dataset-data">

	<div class="content">
<%--	<% Html.RenderPartial("Query", this.Model); %>--%>
    
     <div id="sampleCodeTabs" style="visibility: hidden;">
        <ul>
            <li id="liCodeDataView"><a href="#code-tabs-1">Data View</a></li>
            <li id="liCodeMapView"><a href="#code-tabs-2">Map View</a></li>
            <li id="liCodeBarChart"><a href="#code-tabs-3">Bar Chart</a></li>
            <li id="liCodePieChart"><a href="#code-tabs-4">Pie Chart</a></li>
        </ul>
        <div id="code-tabs-1">
              <div id="divOuterDataViewSampleCode" >
                  <div style="vertical-align: bottom;
                       margin-top: 5px; float: left;">
                      Language/Environment:&nbsp;
                      <%= Html.DropDownList("languagesDataView", ViewData.Model.DataViewLanguages)%>
                  </div>
                  &nbsp;&nbsp;<img id="copyDataCodeButton" 
                      src='<%= UIConstants.DBPC_CopyDataImagePath %>'
                      class="copyicon" title="Copy to Clipboard" 
                      alt='<%= UIConstants.DBPC_CopyDataAltText %>'
                      longdesc="<%= UIConstants.DBPC_CopyDataLongDesc %>" />
                  <br />
                  <div id="divDataViewSampleCode" class="codesamplescroll">
                      <div><% Html.RenderPartial("SampleCodeDataView", ViewData); %></div>
                  </div>
              </div>
        </div>
        <div id="code-tabs-2">
              <div id="divOuterMapViewSampleCode">
                  <p>
                      NOTE: This is a working sample. You can copy / paste this code into an html file.
                      However, you must download the dependent jquery-1.4.2.min.js file for successful
                      execution. You can download jquery-1.4.2.min.js from <a 
                          href="http://ajax.microsoft.com/ajax/jquery/jquery-1.4.2.min.js">
                          here</a>.</p>
                  <div style="vertical-align: bottom; margin-top: 5px; float: left;">
                      Language/Environment:&nbsp;
                      <%= Html.DropDownList("languagesMapView", 
                          ViewData.Model.MapViewLanguages)%>
                  </div>
                  &nbsp;<img id="copyMapCodeButton" 
                      src='<%= UIConstants.DBPC_CopyDataImagePath %>'
                      class="copyicon" title="Copy to Clipboard" 
                      alt='<%= UIConstants.DBPC_CopyDataAltText %>'
                      longdesc='<%= UIConstants.DBPC_CopyDataLongDesc %>' />
                  <br />
                  <div id="divMapViewSampleCode" class="codesamplescroll">
                      <% Html.RenderPartial("SampleCodeMapView", ViewData); %>
                  </div>
              </div>
        </div>
        <div id="code-tabs-3">
              <div id="divOuterBarChartViewSampleCode">
                  <div style="vertical-align: bottom; margin-top: 5px; float: left;">
                      Language/Environment:&nbsp;
                      <%= Html.DropDownList("languagesBarChartView",
                          ViewData.Model.BarChartViewLanguages)%>
                  </div>
                  &nbsp;<img id="copyBarChartCodeButton" 
                      src='<%= UIConstants.DBPC_CopyDataImagePath %>'
                      class="copyicon" title="Copy to Clipboard" 
                      alt='<%= UIConstants.DBPC_CopyDataAltText %>'
                      longdesc='<%= UIConstants.DBPC_CopyDataLongDesc %>' />
                  <br />
                  <div id="divBarChartViewSampleCode" class="codesamplescroll">
                      <% Html.RenderPartial("SampleCodeBarChartView", ViewData); %>
                  </div>
              </div>
        </div>
        <div id="code-tabs-4">
              <div id="divOuterPieChartViewSampleCode">
                  <div style="vertical-align: bottom; margin-top: 5px; float: left;">
                      Language/Environment:&nbsp;
                      <%= Html.DropDownList("languagesPieChartView", 
                          ViewData.Model.PieChartViewLanguages)%>
                  </div>
                  &nbsp;<img id="copyPieChartCodeButton" 
                      src='<%= UIConstants.DBPC_CopyDataImagePath %>'
                      class="copyicon" title="Copy to Clipboard" 
                      alt='<%= UIConstants.DBPC_CopyDataAltText %>'
                      longdesc='<%= UIConstants.DBPC_CopyDataLongDesc %>' />
                  <br />
                  <div id="divPieChartViewSampleCode" class="codesamplescroll">
                      <% Html.RenderPartial("SampleCodePieChartView", ViewData); %>
                  </div>
              </div>
        </div>
    </div>
 	</div>
</div>

<%@ Control Language="C#" Inherits="OgdiViewUserControl<DataBrowserModel>" %>

<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.App_GlobalResources" %>
<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.Models" %>
<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc" %>


<div class="dataset-data">
	<div class="content">
	
	<div id="BackGroundLoadingIndicator" class="bgLoadingIndicator" style="display: none;"></div>
	<div id="LoadingIndicatorPanel" style="display: none; position:  ">
		<img id="imgLoading" class="loader" 
			alt='<%= UIConstants.GC_LoadingAltText %>'
			style="display: none"
			src='<%=UIConstants.GC_LoadingImagePath %>'
			longdesc='<%= UIConstants.GC_LoadingLongDesc %>'
			onload="showLoadingIndicator();" />
	</div>

	<% Html.RenderPartial("Query", Model); %>
	
	 <div id="tabs" style="visibility: hidden;">
		<ul>
			<li id="liDataView"><a href="#tabs-1">Data View</a></li>
			<li id="liMapView"><a href="#tabs-2">Map View</a></li>
			<li id="liBarChart"><a href="#tabs-3">Bar Chart</a></li>
			<li id="liPieChart"><a href="#tabs-4">Pie Chart</a></li>
		</ul>
		<div id="tabs-1">
				<div id="divDataViewResults">
					<% Html.RenderPartial("DataView", ViewData); %>
				</div>
		</div>
		<div id="tabs-2">
				<div id="divMapViewResults">
					<div id="mapFullQueryPanel" style="display: none;">
						<span>Full query URL:</span> &nbsp;<a 
							id="mapFullQueryUrlHyperlink" target="_blank"></a>
						<br />
						<div>
							(Click to view results as XML/KML)</div>
					</div>
					<div id="mapNoResultsDiv" style="display: none;">
						<span>Selected data does not have location information.</span>
					</div>
					<div id="mapManyPlacemarksDiv" style="display: none;">
						<span>Number of placemarks in a result set exceeds the pin display count limit (50 placemarks)</span>
					</div>                    
					<br />
					<div id='myMap' class="veMapStyle">
					</div>
					<br />
				</div>
		</div>
		<div id="tabs-3">
				<div id="divOuterBarChartResults" style="height: 630px;">
					<div>
						<table style="width: 100%;">
							<tr style="height: 100%;">
								<td>
									<fieldset style="height: 130px; padding: 0px; 
										border-style: solid; border-color: #eeeeee;">
										<legend style="text-align: left;"><b>Horizontal Axis</b>
										</legend>
										<br />
										<table>
											<tr style="padding: 10px">
												<td style="text-align: left;">
													<label id="labelBarHorizontalComboBox">
														Entityset
														<br />
														Column:&nbsp;&nbsp;</label>
												</td>
												<td>
													<%= Html.DropDownList("ddlBarHorizontal", 
														ViewData.Model.BarHorizontal, 
														new { style = "width:175px;", 
															onchange = "horizontalBarComboBoxSelected(this.value)" })%>
												</td>
											</tr>
											<tr style="padding: 10px">
												<td style="text-align: left;">
													<label id="labelBarDateRange" 
													style="display: none;">
														Date
														<br />
														Range:</label>
												</td>
												<td>
													<%= Html.DropDownList("ddlBarDateRange",
														ViewData.Model.BarDateRange,
														new { style = "display:none;" })%>
												</td>
											</tr>
										</table>
									</fieldset>
								</td>
								<td>
									<fieldset style="height: 130px; text-align: center; padding:
										 0px; border-style: solid;
										border-color: #eeeeee;">
										<legend style="text-align: left;"><b>Vertical Axis</b>
										</legend>
										<table>
											<tr>
												<td style="width: 150px;">
													<fieldset style="height: 100px; width: 150px; 
														padding: 0px; border-style: solid;
														border-color: #eeeeee;">
														<legend style="text-align: left;">
															<%= Html.RadioButton("BarVerticalOptions", 
																"Option1", true,
																new { name = "BarVerticalOptions", 
																	onclick = "barOption1Selected();" })%>
																	Option 1 </legend>
														<table>
															<tr>
																<td>
																	<label id="labelBarCount">
																		No. of occurrences of
																	</label>
																</td>
															</tr>
														</table>
													</fieldset>
												</td>
												<td>
													<fieldset style="height: 100px; padding: 0px; 
														border-style: solid; border-color: #eeeeee;">
														<legend style="text-align: left;">
															<% if (ViewData.Model.BarYOption != null && 
																   ViewData.Model.BarYOption == "Option2")
															   { %>
															<%= Html.RadioButton("BarVerticalOptions", 
																			"Option2", true,
																			new { name = "BarVerticalOptions", 
																				onclick = "barOption2Selected();" })%>
																				Option 2
															<% }
															   else
															   { %>
															<%= Html.RadioButton("BarVerticalOptions", 
																			"Option2", false,
																			new { name = "BarVerticalOptions",
																				onclick = "barOption2Selected();" })%>
																				Option 2
															<% } %>
														</legend>
														<div>
															<table>
																<tr style="padding: 10px;">
																	<td style="text-align: left;">
																		<label id="VerticalComboBoxLabel" 
																		style="color: Gray;">
																			Entityset
																			<br />
																			Column:&nbsp;&nbsp;</label>
																	</td>
																	<td>
																		<%= Html.DropDownList("ddlBarVertical",
																			ViewData.Model.BarVertical, 
																			new { style = "width:175px;", 
																				disabled = "disabled" })%>
																	</td>
																</tr>
																</table>
														</div>
														<div>
															<%= Html.RadioButton("BarVerticalColumnOptions", 
																"Aggregate", true, new { disabled = "disabled" })%>
															<label id="labelBarAggregate" style="color: Gray;">
																Aggregate</label>
															<% if (ViewData.Model.BarYColOption != null &&
																   ViewData.Model.BarYColOption == "Average")
															   { %>
															<%= Html.RadioButton("BarVerticalColumnOptions", 
																"Average", true, new { disabled = "disabled" })%>
															<% }
															   else
															   { %>
															<%= Html.RadioButton("BarVerticalColumnOptions", 
																"Average", false, new { disabled = "disabled" })%>
															<% } %>
															<label id="labelBarAverage" style="color: Gray;">
																Average</label>
														</div>
													</fieldset>
												</td>
											</tr>
										</table>
									</fieldset>
								</td>
								<td valign="top" style="padding-top: 5px;">
									<a href="javascript:getBarChart()" title='Refresh Chart' >
										 <%= Html.NiceButton(this, "refresh-chart", 0, "RefreshChart")%>
									</a>                                    
								</td>
							</tr>
						</table>
					</div>
					<div id="divBarChartError" style="display: none; 
						background-color: Red; color: White;
						text-align: center; font-weight: bold; font-size: 17px;">
						<%= UIConstants.DBPC_ChartErrorText %>
					</div>
					<div id="divBarChartResults">
						<% if (ViewData.Model.Chart != null)
						   {
							   Html.RenderPartial("BarChart", ViewData);
						   } %>
					</div>
				</div>
		</div>
		<div id="tabs-4">
				<div id="divPieResultHeight">
					<div>
						<table style="width: 100%;">
							<tr style="height: 100%;">
								<td>
									<fieldset style="height: 130px; padding: 0px;
										 border-style: solid; border-color: #eeeeee;">
										<legend style="text-align: left;"><b>Horizontal Axis</b>
										</legend>
										<br />
										<table>
											<tr style="padding: 10px">
												<td style="text-align: left;">
													<label id="labelPieHorizontalComboBox">
														Entityset
														<br />
														Column:&nbsp;&nbsp;</label>
												</td>
												<td>
													<%= Html.DropDownList("ddlPieHorizontal", 
														ViewData.Model.PieHorizontal,
														new { style = "width:175px;",
															onchange = "horizontalPieComboBoxSelected(this.value)" })%>
												</td>
											</tr>
											<tr style="padding: 10px">
												<td style="text-align: left;">
													<label id="labelPieDateRange" style="display: none;">
														Date<br />
														Range:</label>
												</td>
												<td>
													<%= Html.DropDownList("ddlPieDateRange", 
														ViewData.Model.PieDateRange, 
														new { style = "display:none;" })%>
												</td>
											</tr>
										</table>
									</fieldset>
								</td>
								<td>
									<fieldset style="height: 130px; text-align: center; 
										padding: 0px; border-style: solid;
										border-color: #eeeeee;">
										<legend style="text-align: left;"><b>Vertical Axis</b>
										</legend>
										<table>
											<tr>
												<td style="width: 150px;">
													<fieldset style="height: 100px; width: 150px; 
														padding: 0px; border-style: solid;
														border-color: #eeeeee;">
														<legend style="text-align: left;">
															<%= Html.RadioButton("PieVerticalOptions", 
																"Option1", true, 
																	new { name="PieVerticalOptions", 
																		onclick = "pieOption1Selected();" })%>
																		Option 1 </legend>
														<table>
															<tr>
																<td>
																	<label id="labelPieCount">
																		No. of occurrences of
																	</label>
																</td>
															</tr>
														</table>
													</fieldset>
												</td>
												<td>
													<fieldset style="height: 100px; padding: 0px; 
														border-style: solid; border-color: #eeeeee;">
														<legend style="text-align: left;">
															<% if (ViewData.Model.PieYOption != null && 
																   ViewData.Model.PieYOption == "Option2")
															   { %>
															<%= Html.RadioButton("PieVerticalOptions",
																		"Option2", true, 
																		 new { name = "PieVerticalOptions", 
																			 onclick = "pieOption2Selected();" })%>
																			 Option 2
															<% }
															   else
															   { %>
															<%= Html.RadioButton("PieVerticalOptions", 
																		"Option2", false, 
																		new { name = "PieVerticalOptions", 
																			onclick = "pieOption2Selected();" })%>
																			Option 2
															<% } %>
														</legend>
														<div>
															<table>
																<tr style="padding: 10px;">
																	<td style="text-align: left;">
																		<label id="labelPieVerticalComboBox" 
																		style="color: Gray">
																			Entityset
																			<br />
																			Column:&nbsp;&nbsp;</label>
																	</td>
																	<td>
																		<%= Html.DropDownList("ddlPieVertical", 
																			ViewData.Model.PieVertical, 
																			new { style = "width:175px;", disabled = "disabled" })%>
																	</td>
																</tr>
																</table>
														</div>
														<div>
															<%= Html.RadioButton("PieVerticalColumnOptions",
																"Aggregate", true, new { disabled = "disabled" })%>
															<label id="labelPieAggregate" style="color: Gray">
																Aggregate</label>
															<% if (ViewData.Model.PieYColOption != null && 
																   ViewData.Model.PieYColOption == "Average")
															   { %>
															<%= Html.RadioButton("PieVerticalColumnOptions", 
																"Average", true, new { disabled = "disabled" })%>
															<% }
															   else
															   { %>
															<%= Html.RadioButton("PieVerticalColumnOptions", 
																"Average", false, new { disabled = "disabled" })%>
															<% } %>
															<label id="labelPieAverage" style="color: Gray">
																Average</label>
														</div>
													</fieldset>
												</td>
											</tr>
										</table>
									</fieldset>
								</td>
								<td valign="top" style="padding-top: 5px;">
									<a href="javascript:getPieChart()" title='Refresh Chart' >
										 <%= Html.NiceButton(this, "refresh-chart", 0, "RefreshChart")%>
									</a>                                      
								</td>
							</tr>
						</table>
					</div>
					<div id="divPieChartError" style="display: none; 
						background-color: Red; color: White;
						text-align: center; font-weight: bold; font-size: 17px;">
						<%= UIConstants.DBPC_ChartErrorText %>
					</div>
					<div id="divPieChartResults" style="height:849px;">
						<% if (ViewData.Model.Chart != null)
						   {
							   Html.RenderPartial("PieChart", ViewData);
						   } %>
					</div>
				</div>
		</div>
	</div>
	
	<div id="filterDialog" title="$filter hints" class="filterdialog" style="display: none;">
		<p>
			OGDI uses a subset of the <a 
			href="http://msdn.microsoft.com/en-us/library/cc668784(VS.100).aspx">
				WCF Data Services query syntax</a><img
				src='<%= UIConstants.GC_ExternalLinkImagePath %>'
				alt='<%= UIConstants.DBPC_ADOqueryAltText %>'
				title='<%= UIConstants.DBPC_ADOqueryTitle %>'
				longdesc='<%= UIConstants.DBPC_ADOqueryLongDesc %>' />. The basic format
			of a filter expression is <i>property</i>&nbsp;<i>comparison</i>&nbsp;<i>expression</i>,
			where <i>property</i> is the name of a property, <i>comparison</i> is a comparison
			operator, and <i>expression</i> is the expression with which the property&#39;s
			value should be compared.
		</p>
		<p>
			Some examples of filter expressions using the CrimeIncidents data set from the DC
			container are:</p>
		<table width="100%">
			<tr>
				<td style="vertical-align: top">
					method eq 2
				</td>
				<td style="vertical-align: top">
					<p>
						Crime incidents where the method code is equal to 2. Other comparison operators
						include &quot;gt&quot; for greater than, &quot;lt&quot; for less than, &quot;ge&quot;
						for greater than or equal to, and &quot;le&quot; for less than or equal to.</p>
				</td>
			</tr>
			<tr>
				<td style="vertical-align: top">
					shift eq &#39;EVN&#39;
				</td>
				<td style="vertical-align: top">
					<p>
						Crime incidents where the &quot;shift&quot; property is equal to &quot;EVN&quot;.
						String literals must be enclosed in single quotes. Boolean literals are represented
						as &quot;true&quot; or &quot;false&quot; (without the double quotes).</p>
				</td>
			</tr>
			<tr>
				<td style="vertical-align: top">
					reportdatetime ge datetime&#39;2008-06-01T00:00:00Z&#39;
				</td>
				<td style="vertical-align: top">
					<p>
						Crime incidents where the report date/time is on or after midnight, June 1, 2008,
						UTC. Date/time literals are expressed in <a 
						href="http://en.wikipedia.org/wiki/ISO_8601" target="_blank">ISO 8601</a><img 
						src="<%= UIConstants.GC_ExternalLinkImagePath %>"
						alt="<%= UIConstants.DBPC_ISO8601AltText %>" 
						title="<%= UIConstants.DBPC_ISO8601Title %>"
						longdesc="<%= UIConstants.DBPC_ISO8601LongDesc %>" />
						format, <i>yyyy</i>-<i>mm</i>-<i>dd</i>T<i>HH</i>:<i>MM</i>:<i>SS</i>, where <i>yyyy</i>
						is the four-digit year, <i>mm</i> is the two-digit month number, <i>dd</i> is the
						two-digit day of the month, <i>HH</i> is the two-digit hour (in 24-hour format),
						<i>MM</i> is the two-digit minute, and <i>SS</i> is the two-digit second.</p>
				</td>
			</tr>
			<tr>
				<td style="vertical-align: top">
					(shift eq &#39;EVN&#39;) or (shift eq &#39;MID&#39;)
				</td>
				<td style="vertical-align: top">
					<p>
						Crime incidents where the shift is equal to &quot;EVN;&quot; or &quot;MID&quot;.
						The filter syntax supports the &quot;and&quot;, &quot;or&quot;, and &quot;not&quot;
						logical operators. Expressions can grouped with parentheses, as in many popular
						programming languages.</p>
				</td>
			</tr>
		</table>
	</div>
	
	</div>
</div>

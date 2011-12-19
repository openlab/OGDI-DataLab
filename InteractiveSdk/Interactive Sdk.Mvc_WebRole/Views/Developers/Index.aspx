<%@ Page Language="C#" MasterPageFile="~/Views/Shared/OGDIMasterPage.Master" 
Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.App_GlobalResources"  %>
<asp:Content ID="DevelopersContent2" ContentPlaceHolderID="MainContent" runat="server">

	<div class="block">
		<div class="developers">
			<div class="bar">For Developers</div>
		<h3>
			The Windows Azure Platform</h3>
		<p class="big">
			The Open Government Data Initiative is based on Microsoft's Windows Azure Platform, a cloud-computing 
			platform that helps developers quickly and easily create, deploy, manage, and run Web applications
			 and services at Internet scale. To learn more please visit the 
			 <a href="<%= UIConstants.DPC_HomePageLink %>"
				target="_blank">Windows Azure Platform</a><img 
					src="<%= UIConstants.GC_ExternalLinkImagePath %>"
					alt="<%= UIConstants.DPC_HomeLinkAltText %>" 
					title="<%= UIConstants.DPC_HomeLinkTitle %>"
					longdesc="<%= UIConstants.DPC_HomeLinkLongDescPath %>" /> home page.
		</p>
		<h3>
			Querying OGDI</h3>
		<p class="big">
			OGDI exposes data through <a href="<%= UIConstants.DPC_RESTLink %>" target="_blank">
				REST</a><img src="<%= UIConstants.GC_ExternalLinkImagePath %>"
					alt="<%= UIConstants.DPC_RestLinkAltText %>" 
					title="<%= UIConstants.DPC_RestLinkTitle %>"
					longdesc="<%= UIConstants.DPC_RestLinkLongDesc %>" /> Web services. 
					The basic format of an OGDI service call is 
					http://ogdi.cloudapp.net/v1/<i>container</i>/<i>dataset</i>?<i>query</i>,
			where:
		</p>
		<ul class="big">
			<li><i>container</i> is the name of the container (for example, "dc" for the District
				of Columbia's data sets). </li>
			<li><i>dataset</i> is the name of the data set (for example, "CrimeIncidents" for the
				Crime Incidents data set in the DC container). </li>
			<li><i>query</i> is your set of query parameters, expressed using a subset of the <a
				href="<%= UIConstants.DPC_ADOLink %>" target="_blank">WCF
				Data Services query syntax.</a><img src="<%= UIConstants.GC_ExternalLinkImagePath %>"
					alt="<%= UIConstants.DPC_ADOLinkAltText %>" 
					title="<%= UIConstants.DPC_ADOLinkTitle %>" 
					longdesc="<%= UIConstants.DPC_ADOLinkLongDescPath %>" />
			</li>
		</ul>
		<p class="big">
			Note that OGDI currently only supports  the $filter and $top query options in the
			WCF Data Services query syntax. Example queries using the Crime Incidents data
			set are available on the <a href="<%= UIConstants.DPC_DataBrowserPage %>">
				Data Browser</a> page of our Interactive SDK.
		</p>
		<p class="big">
			Also note that if a property has a null value for a particular entity in the data
			set, it will be omitted entirely from the result set returned by OGDI. For example,
			in the Crime Incidents data set, the "method" property is only returned for records
			that have a "method" value in the underlying data set. Your application design should
			take this into account and handle potentially missing properties.
		</p>
		<h3>
			Data Formats</h3>
		<h4>AtomPub</h4>
		<p class="big">
			By default, OGDI returns data in the <a href="<%= UIConstants.DPC_ODataLink %>"
				target="_blank">Open Data Protocol (OData)</a><img
					src="<%= UIConstants.GC_ExternalLinkImagePath %>" 
					alt="<%= UIConstants.DPC_ODataAltText %>"
					longdesc="<%= UIConstants.DPC_OdataLongDesc %>" />
			format. This format extends the broadly adopted <a href="<%= UIConstants.DPC_AtomPublishingLink %>"
				target="_blank">Atom Publishing Protocol</a><img
				src="<%= UIConstants.GC_ExternalLinkImagePath %>"
					alt="<%= UIConstants.DPC_AtomPublishingLinkAltText %>"
					title="<%= UIConstants.DPC_AtomPublishingLinkTitle %>"
					longdesc="<%= UIConstants.DPC_AtomPublishingLinkLongDescPath %>" />
			and can be easily consumed by a variety of platforms, including
			Microsoft .NET, Java, Ruby, PHP, and Python. Refer to the code samples on the <a
				href="<%= UIConstants.DPC_DataBrowserPage %>">Data Browser</a>
			page for examples.
		</p>
		<h4>JSON</h4>
		<p class="big">
			OGDI can also return data in the <a href="<%= UIConstants.DPC_JSONLink %>"
				target="_blank">JavaScript Object Notation (JSON)</a><img
				src="<%= UIConstants.GC_ExternalLinkImagePath %>"
					alt="<%= UIConstants.DPC_JSONLinkAltText %>" 
					title="<%= UIConstants.DPC_JSONLinkTitle %>" 
					longdesc="<%= UIConstants.DPC_JSONLinkLongDescPath %>" />
			format, which can be conveniently consumed using JavaScript and other technologies.
			To return data in JSON format, simply append the format=json parameter to your query.
			For example, to retrieve crime incidents in Washington, DC that occurred during the
			police department's evening shift in JSON format:
		</p>
		<p class="big" style="margin-left: 40px">
			<a href="<%= UIConstants.DPC_EquationLink %>">
			<%= UIConstants.DPC_EquationLink %></a>
		</p>
		<h4>
			JSONP</h4>    
		<p class="big">
			To mitigate security vulnerabilities associated with cross-site scripting attacks,
			Web browsers generally prevent client-side JavaScript applications originating in
			one network domain (for example, yourdomain.com) from making HTTP requests to other
			network domains (for example, the ogdi.cloudapp.net network domain that hosts the
			OGDI data services). This can prevent JavaScript applications hosted in another
			domain from making straight-forward calls to the OGDI data services, but there a
			variety of techniques that can be used, such as this widely-used <a
				 href="<%= UIConstants.DPC_IFrameLink %>"
					target="_blank">IFRAMES-based technique</a><img 
					src="<%= UIConstants.GC_ExternalLinkImagePath %>"
					alt="<%= UIConstants.DPC_IFrameAltText %>" 
					title="<%= UIConstants.DPC_IFrameTitle %>" 
					longdesc="<%= UIConstants.DPC_IFrameLongDescPath %>" />
			described by Michael Mahemoff.
		</p>
		<p class="big">
			OGDI's data services also provide direct support for the <a
			 href="<%= UIConstants.DPC_JSONPLink %>"
					target="_blank">JSONP</a><img 
					src="<%= UIConstants.GC_ExternalLinkImagePath %>"
					alt="<%= UIConstants.DPC_JSONPAltText %>" 
					title="<%= UIConstants.DPC_JSONPTitle %>"
					longdesc="<%= UIConstants.DPC_JSONPLongDescPath %>" />
			technique. Using this technique, OGDI's data services will call a callback function
			that you specify, passing in the results of your query in JSON format as an input
			format. To use this technique, issue a query with the following additional parameters:
			format=json&callback=<i>yourCallback</i>, where <i>yourCallback</i> is the name
			of a JavaScript callback function defined on the Web page issuing the request.
		</p>
		<p class="big">
			Refer to the JavaScript sample on the <a href="<%= UIConstants.DPC_DataBrowserPage %>">
				Data Browser</a> page for an example of using JSONP with OGDI. In that sample,
			the AdditionalDataLoaded() function is the JSONP callback function.
		</p>
		<h4>
			Geospatial Data</h4>
		<p class="big">
			Many of the data sets in OGDI also include geospatial data, which is returned in
			the <a href="<%= UIConstants.DPC_KMLLink %>" target="_blank">
				Keyhole Markup Language (KML)</a><img src="<%= UIConstants.GC_ExternalLinkImagePath %>"
					alt="<%= UIConstants.DPC_KMLLinkAltText %>" 
					title="<%= UIConstants.DPC_KMLLinkTitle %>"
					longdesc="<%= UIConstants.DPC_KMLLinkLongDescPath %>" />
			format. This format is compatible with popular desktop and Web-based mapping technologies
			including Microsoft <a href="<%= UIConstants.DPC_VirtualEarthLink %>"
				target="_blank">Bing Maps</a><img src="<%= UIConstants.GC_ExternalLinkImagePath %>"
					alt="<%= UIConstants.DPC_VirtualEarthAltText %>" 
					title="<%= UIConstants.DPC_VirtualEarthTitle %>"
					longdesc="<%= UIConstants.DPC_VirtualEarthLongDescPath %>" />,
			<a href="<%= UIConstants.DPC_GoogleMapLink %>" target="_blank">
				Google Maps</a><img src="<%= UIConstants.GC_ExternalLinkImagePath %>"
					alt="<%= UIConstants.DPC_GoogleMapsAltText %>" 
					title="<%= UIConstants.DPC_GoogleMapsTitle %>" 
					longdesc="<%= UIConstants.DPC_GoogleMapLongDescPath %>" />,
			<a href="<%= UIConstants.DPC_YahooMapLink %>" target="_blank">
				Yahoo! Maps</a><img src="<%= UIConstants.GC_ExternalLinkImagePath %>"
					alt="<%= UIConstants.DPC_YahooMapsAltText %>" 
					title="<%= UIConstants.DPC_YahooMapsTitle %>" 
					longdesc="<%= UIConstants.DPC_YahooMapsLongDescPath %>" />,
			and <a href="<%= UIConstants.DPC_GoogleEarthLink %>" target="_blank">
				Google Earth</a><img src="<%= UIConstants.GC_ExternalLinkImagePath %>"
					alt="<%= UIConstants.DPC_GoogleEarthAltText %>" 
					title="<%= UIConstants.DPC_GoogleEarthTitle %>" 
					longdesc="<%= UIConstants.DPC_GoogleEarthLongDescPath %>" />.
		</p>
		<p class="big">
			To return geospatial data in KML format, append the format=kml parameter to your
			query. For example, to retrieve geospatial points in KML format for crime incidents
			in DC that occurred during the police department's evening shift:
		</p>
		<p class="big" style="margin-left: 40px">
			<a href="http://ogdi.cloudapp.net/v1/dc/CrimeIncidents?$filter=shift eq 'EVN'&format=kml">
				http://ogdi.cloudapp.net/v1/dc/CrimeIncidents?$filter=shift eq 'EVN'&format=kml</a>
		</p>
		<p class="big">
			Note that if the data set that you are using does not include any geospatial data,
			a KML query to OGDI will return an empty result set.
		</p>
	    
	    
		<h3>Client Libraries</h3>
		<p class="big">
			Developers using <a href="<%= UIConstants.DPC_VStudio %>"
				target="_blank">Microsoft Visual Studio 2008, Service Pack 1</a><img
				 src="<%= UIConstants.GC_ExternalLinkImagePath %>"
					alt="<%= UIConstants.DPC_VStudioAltText %>" 
					title="<%= UIConstants.DPC_VStudioTitle %>" 
					longdesc="<%= UIConstants.DPC_VStudioLongDescPath %>" />
			(or later) can use <a href="<%= UIConstants.DPC_ADONetLink %>"
				target="_blank">WCF Data Services</a><img
				 src="<%= UIConstants.GC_ExternalLinkImagePath %>"
					alt="<%= UIConstants.DPC_ADONetAltText %>" 
					title="<%= UIConstants.DPC_ADONetTitle %>"
					longdesc="<%= UIConstants.DPC_ADONetLongDescPath %>" />
			to access data from OGDI through easy-to-use .NET classes.
			Within Visual Studio, this is accomplished by using the <a 
			href="<%= UIConstants.DPC_AddServiceRefLink %>" 
			target="_blank">Add Service Reference</a><img
					src="<%= UIConstants.GC_ExternalLinkImagePath %>"
					alt="<%= UIConstants.DPC_AddServiceRefAltText %>"
					longdesc="<%= UIConstants.DPC_AddServiceRefLongDesc %>" /> 
				feature (see .NET samples on the <a href="<%= UIConstants.DPC_DataBrowserPage %>">
				Data Browser</a> page).
			To make accessing OGDI data easier, Java developers can use <a 
			href="<%= UIConstants.DPC_RestletExtensionLink %>" 
			target="_blank">Restlet Extension for WCF Data Services</a><img
					src="<%= UIConstants.GC_ExternalLinkImagePath %>"
					alt="<%= UIConstants.DPC_RestletExtensionAltText %>"
					longdesc="<%= UIConstants.DPC_RestletExtensionLongDesc %>" />.
			PHP developers can take advantage of the Toolkit for <a 
			href="<%= UIConstants.DPC_PHPToolkitLink %>" 
			target="_blank">Toolkit for PHP with WCF Data Services</a><img
					src="<%= UIConstants.GC_ExternalLinkImagePath %>"
					alt="<%= UIConstants.DPC_PHPToolkitAltText %>"
					longdesc="<%= UIConstants.DPC_PHPToolkitLongDesc %>" />.
		</p>    
		<h3>
			Paging</h3>
		<p class="big">
			OGDI and the underlying Windows Azure Table Storage service support paging through
			large sets of query results. The documentation for <a
			 href="<%= UIConstants.DPC_QueryTimeoutLink %>"
				target="_blank">Query Timeout and Pagination</a><img
				 src="<%= UIConstants.GC_ExternalLinkImagePath %>"
					alt="<%= UIConstants.DPC_QueryTimeoutAltText %>" 
					title="<%= UIConstants.DPC_QueryTimeoutTitle %>" 
					longdesc="<%= UIConstants.DPC_QueryTimeoutLongDescPath %>" />
			in the Windows Azure Table Storage service provides a complete description of how
			OGDI and the underlying Azure platform support paging. You can also refer to the
			"C#/ASP.NET Paging" sample on the <a href="<%= UIConstants.DPC_DataBrowserPage %>">
				Data Browser</a> page, which demonstrates how to perform paged queries using
			the WCF Data Services client library.
		</p>
		</div>    
	</div>
    <% Html.RenderPartial("Bookmark"); %>
    
</asp:Content>

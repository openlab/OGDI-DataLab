<%@ Page Language="C#" MasterPageFile="~/Views/Shared/OGDIMasterPage.Master" Inherits="System.Web.Mvc.ViewPage" %>
<asp:Content id="DevelopersContent2" contentplaceholderid="MainContent" runat="server">
    <div class="block">
        <div class="developers">
            <div class="bar">
                For Developers</div>
            <h3>
                Overview</h3>
            <p>
                DataLab OGDI is written using C# and the .NET Framework, targeted for Microsoft's
                <a href="http://www.windowsazure.com/">Windows Azure Platform</a> cloud-computing
                platform. Data Service.</p>
            <p>
                The DataService uses RESTful Web service to expose data for programmatic access
                via a number of formats, including Open Data Protocol (OData)**, an extension to
                **Atom Publishing Protocol (AtomPub), Keyhole Markup Language (KML), JSON and JSONP.</p>
            <p>
                The Data Browser is written an ASP.NET MVC 1.0 and uses jQuery and a variety of
                other open source components and enables users to browse and query published data.</p>
            <p>
                The Data Loader is desktop client tool that includes both GUI-based and console-based
                data loader tools.</p>
            <h3>
                Querying DataLab</h3>
            The basic format of DataLab <a href="http://en.wikipedia.org/wiki/Representational_State_Transfer">
                REST</a> service call is http://ogdi.cloudapp.net/v1/container/dataset?query,
            where:
            <ul>
                <li>container is the name of the container (for example, "dc" for the District of Columbia's
                    data sets).</li>
                <li>dataset is the name of the data set (for example, "CrimeIncidents" for the Crime
                    Incidents data set in the DC container).</li>
                <li>query is your set of query parameters, expressed using a subset of the <a href="http://msdn.microsoft.com/en-us/library/cc668784(VS.100).aspx">
                    WCF Data Services query syntax</a>.</li>
            </ul>
            <p>
                Note that DataLab currently only supports the $filter and $top query options in
                the WCF Data Services query syntax. Also note that if a property has a null value
                for a particular entity in the data set, it will be omitted entirely from the result
                set returned by OGDI. For example, in the Crime Incidents data set, the "method"
                property is only returned for records that have a "method" value in the underlying
                data set. Your application design should take this into account and handle potentially
                missing properties.</p>
            <h3>
                Data Formats</h3>
            <h4>
                AtomPub</h4>
            <p>
                By default, DataLab returns data in the <a href="http://odata.org/">Open Data Protocol
                    (OData)</a> format. This format extends the broadly adopted <a href="http://bitworking.org/projects/atom/rfc5023.html">
                        Atom Publishing Protocol</a> and can be easily consumed by a variety of
                platforms, including Microsoft .NET, Java, Ruby, PHP, and Python. Refer to the code
                samples on the <a href="http://datadotgc.cloudapp.net/DataBrowser/dc/CrimeIncidents#param=NOFILTER--DataView--Results">
                    Data Browser</a> page for examples.</p>
            <h4>
                JSON</h4>
            <p>
                DataLab can also return data in the <a href="http://en.wikipedia.org/wiki/JSON">JavaScript
                    Object Notation (JSON)</a> format, which can be conveniently consumed using
                JavaScript and other technologies. To return data in JSON format, simply append
                the format=json parameter to your query. For example, to retrieve crime incidents
                in Washington, DC that occurred during the police department's evening shift in
                JSON format:</p>
            <blockquote>
                http://ogdi.cloudapp.net/v1/dc/CrimeIncidents?$filter=shift eq 'EVN'&format=json</blockquote>
            <h4>
                JSONP</h4>
            <p>
                To mitigate security vulnerabilities associated with cross-site scripting attacks,
                Web browsers generally prevent client-side JavaScript applications originating in
                one network domain (for example, yourdomain.com) from making HTTP requests to other
                network domains (for example, the ogdi.cloudapp.net network domain that hosts the
                DataLab data services). This can prevent JavaScript applications hosted in another
                domain from making straight-forward calls to the DataLab data services, but there
                a variety of techniques that can be used, such as this widely-used <a href="http://softwareas.com/cross-domain-communication-with-iframes">
                    IFRAMES-based technique</a> described by Michael Mahemoff.</p>
            <p>
                DataLab’s data services also provide direct support for the <a href="http://en.wikipedia.org/wiki/JSON#JSONP">
                    JSONP</a> technique. Using this technique, the data services will call a callback
                function that you specify, passing in the results of your query in JSON format as
                an input format. To use this technique, issue a query with the following additional
                parameters: format=json&callback=yourCallback, where yourCallback is the name of
                a JavaScript callback function defined on the Web page issuing the request.</p>
            <p>
                Refer to the JavaScript sample on the <a href="http://datadotgc.cloudapp.net/DataBrowser/dc/CrimeIncidents#param=NOFILTER--DataView--Results">
                    Data Browser</a> page for an example of using JSONP with DataLab. In that sample,
                the AdditionalDataLoaded() function is the JSONP callback function.</p>
            <h4>
                Geospatial Data</h4>
            <p>
                Many of the data sets in DataLab also include geospatial data, which is returned
                in the <a href="http://en.wikipedia.org/wiki/Keyhole_Markup_Language">Keyhole Markup
                    Language (KML)</a> format. This format is compatible with popular desktop and
                Web-based mapping technologies including Microsoft <a href="http://dev.live.com/virtualearth">
                    Bing Maps</a>, <a href="http://maps.google.com/">Google Maps</a>, <a href="http://maps.yahoo.com/">
                        Yahoo! Maps</a> and <a href="http://earth.google.com/">Google Earth</a>.</p>
            <p>
                To return geospatial data in KML format, append the format=kml parameter to your
                query. For example, to retrieve geospatial points in KML format for crime incidents
                in DC that occurred during the police department's evening shift:</p>
            <blockquote>
                http://ogdi.cloudapp.net/v1/dc/CrimeIncidents?$filter=shift eq 'EVN'&format=kml</blockquote>
            <p>
                Note that if the data set that you are using does not include any geospatial data,
                a KML query to DataLab will return an empty result set.</p>
            <h3>
                Client Libraries</h3>
            <p>
                Developers using <a href="http://www.microsoft.com/vstudio">Microsoft Visual Studio
                    2008, Service Pack 1</a> (or later) can use <a href="http://msdn.microsoft.com/en-us/data/bb931106.aspx">
                        WCF Data Services</a> to access data from DataLab through easy-to-use .NET
                classes. Within Visual Studio, this is accomplished by using the <a href="http://msdn.microsoft.com/en-us/data/cc974504.aspx">
                    Add Service Reference</a> feature (see .NET samples on the <a href="http://datadotgc.cloudapp.net/DataBrowser/dc/CrimeIncidents#param=NOFILTER--DataView--Resultshttp://datadotgc.cloudapp.net/DataBrowser/dc/CrimeIncidents#param=NOFILTER--DataView--Results">
                        Data Browser</a> page). To make accessing DataLab data easier, Java developers
                can use <a href="http://www.interoperabilitybridges.com/projects/restlet-extension-for-adonet-data-services.aspx">
                    Restlet Extension for WCF Data Services</a> . PHP developers can take advantage
                of the Toolkit for Toolkit for <a href="http://www.interoperabilitybridges.com/projects/toolkit-for-php-with-adonet-data-services-.aspx">
                    PHP with WCF Data Services</a>.</p>
            <h3>
                Paging</h3>
            <p>
                DataLab and the underlying Windows Azure Table Storage service support paging through
                large sets of query results. The documentation for <a href="http://msdn.microsoft.com/en-us/library/dd135718.aspx">
                    Query Timeout and Pagination</a> in the Windows Azure Table Storage service
                provides a complete description of how DataLab and the underlying Azure platform
                support paging. You can also refer to the "C#/ASP.NET Paging" sample on the <a href="http://datadotgc.cloudapp.net/DataBrowser/dc/CrimeIncidents#param=NOFILTER--DataView--Results">
                    Data Browser</a> page, which demonstrates how to perform paged queries using
                the WCF Data Services client library.</p>
        </div>
    </div>
    <% Html.RenderPartial("Bookmark"); %> </asp:Content> 
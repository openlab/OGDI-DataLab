/* 
*   Copyright (c) Microsoft Corporation.  All rights reserved.
* 
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions are met:
*
*     * Redistributions of source code must retain the above copyright
*       notice, this list of conditions and the following disclaimer.
*     * Redistributions in binary form must reproduce the above copyright
*       notice, this list of conditions and the following disclaimer in the
*       documentation and/or other materials provided with the distribution.
*     * Neither the name of Microsoft Corporation nor the names of its contributors 
*       may be used to endorse or promote products derived from this software
*       without specific prior written permission.
*
* THIS SOFTWARE IS PROVIDED BY THE REGENTS AND CONTRIBUTORS ``AS IS'' AND ANY
* EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
* WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
* DISCLAIMED. IN NO EVENT SHALL THE REGENTS AND CONTRIBUTORS BE LIABLE FOR ANY
* DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
* (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
* LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
* ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
* (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
* SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

// Variable used as an indicator to run onStateChanged method or not.
var isStateChangeExecute = true;

// **** CURRENT *****************
var current_filter;
var current_viewType;
var current_tagType;
//  Sample Code accordion
var current_language
//  Result Accordion
//  MapView
var current_mapStyle;
var current_zoomLevel;
var current_longitude;
var current_latitude;
var current_sceneId = 0;
var current_birdseyeSceneOrientation = 0;
var current_mapDetails = "";
//  Bar/Pie-ChartView 
var current_horizontalColumnName;
var current_dateRange;
var current_verticalOptions;
var current_verticalColumnName;
var current_verticalColumnOptions;
//********************************

// **** HASH *****************
var hash_columns = null;
var hash_filter;
var hash_viewType;
var hash_tagType;
//  Sample Code accordion
var hash_language
//  Result Accordion
//  MapView
var hash_mapStyle;
var hash_zoomLevel = 11;
var hash_longitude;
var hash_latitude;
var hash_sceneId = 0;
var hash_birdseyeSceneOrientation = 0;
//  Bar/Pie-ChartView 
var hash_horizontalColumnName;
var hash_dateRange;
var hash_verticalOptions;
var hash_verticalColumnName;
var hash_verticalColumnOptions;
//********************************

//  This is default zoomLevel of map set when non bookmarked map view
//  is loaded using Loadkml function
var defaultZoomLevelOfMapSet = 11;

//  This is the INDEX in param(URL hash) value
//  where the map style value is placed
var indexInHashWhereMapStyleStarts = 3;

var isBookMarkedMapView = 'N';
var isHashChangeRoute = false;
var isBookmarkedUrl = false;

var map;
var birdsEyeViewSceneIdChange = false;
var centerPixel;
var birdsEyeViewZoomLevel = 11;
var birdsEyeSceneGlobal = 'North';




/// <summary>
/// This function gets the current status of map 
/// in variables with prefix "current_"
/// </summary>
function getCurrentMapStatus() {
    try {
        current_mapStyle = map.getMapTypeId();
        var center = map.getCenter(); current_zoomLevel = map.getZoom(); current_longitude = center.Longitude; current_latitude = center.Latitude;
        current_mapDetails = current_mapStyle + "/" + current_zoomLevel + "/" + current_longitude + "/" + current_latitude;

    } catch (e) {
        errorFunction(mapControlError, true);
        hideLoadingIndicator();
    }
}

/// <summary>
/// This function handles the things when map view changes
/// It changes the URL hash value according to changed map view
/// </summary>
function viewHandler() {
    if (current_viewType == "MapView" && current_tagType == "Results") {
        var hashValue = createHashValue();
        Sys.Application.addHistoryPoint({ param: hashValue }, siteTitle);
    }
}
/// <summary>
/// This function sets the Map view according to
/// current URL hash value 
/// </summary>
function setMapViewByHash() {
    try {
        Microsoft.Maps.Events.removeHandler("onchangeview");
        showLoadingIndicator();
        map.setMapType(hash_mapStyle);
        getCurrentMapStatus();
        if ((current_zoomLevel != hash_zoomLevel) || (current_latitude != hash_latitude) || (current_longitude != hash_longitude)) {
            Microsoft.Maps.Events.addHandler(map, "onchangeview", hideLoadingIndicatorAndDetach);
            map.setOptions({ center: new Microsoft.Maps.Location(hash_latitude, hash_longitude), zoom: hash_zoomLevel });
        }
        else {
            hideLoadingIndicatorAndDetach();
        }
    } catch (e) {
        errorFunction(mapControlError, true);
        hideLoadingIndicator();
    }
}


/// <summary>
/// Everything inside this function will load as soon as 
/// the DOM is loaded and before the page contents are loaded
/// </summary>
$(document).ready(
function () {

    //  This is executed when Ajax calls begin
    $("#LoadingIndicatorPanel").ajaxStart(function () {
        //  Call to showLoadingIndicator function
        showLoadingIndicator();
    });

    //  This is executed when Ajax calls Stop
    $("#LoadingIndicatorPanel").ajaxStop(function () {
        //  Call to hideLoadingIndicator function
        hideLoadingIndicator();
    });

    //  This is executed when selected index for dropdown list is changed
    $("select#languagesDataView").change(function () {

        //  Call to languageDataViewSelectedIndexChanged function
        languageDataViewSelectedIndexChanged();

    });

    //  This is executed when selected index for dropdown list is changed
    $("select#languagesMapView").change(function () {

        //  Call to languageMapViewSelectedIndexChanged function
        languageMapViewSelectedIndexChanged();

    });

    //  This is executed when selected index for dropdown list is changed
    $("select#languagesBarChartView").change(function () {

        //  Call to languageBarChartViewSelectedIndexChanged function
        languageBarChartViewSelectedIndexChanged();

    });

    //  This is executed when selected index for dropdown list is changed
    $("select#languagesPieChartView").change(function () {

        //  Call to languagePieChartViewSelectedIndexChanged function
        languagePieChartViewSelectedIndexChanged();

    });

    //  This is executed when user clicks on 1st paragraph on this page
    $("#Para1").click(function () {

        //  Slide toggle Para2 panel with 350 ms delay
        $(this).next("#Para2").slideToggle(350);

        return false;

    });

    //  Does things when Para1 toggles
    $('#Para1').toggle(

       function () {

           //  Change Expand Collapse image to CollapserImage
           $('#ExpandCollapseImage').attr('src', CollapserImage);

           //  Change title of that image to "Hide Details"
           $('#ExpandCollapseImage').attr('title', "Hide Details");

           //  Change alt of that image to "Hide Details"
           $('#ExpandCollapseImage').attr('alt', "Hide Details");

       }, function () {

           //  Change Expand/Collapse image to ExpanderImage
           $('#ExpandCollapseImage').attr('src', ExpanderImage);

           //  Change title of that image to "Show Details"
           $('#ExpandCollapseImage').attr('title', "Show Details");

           //  Change alt of that image to "Show Details"
           $('#ExpandCollapseImage').attr('alt', "Show Details");

       });
});

/// <summary>
/// This function initializes the page elements
/// </summary>
function initializePage() {

    //  Set filter dialog properties
    $("#filterDialog").dialog({

        //  The width of the dialog, in pixels.
        width: 530,

        //  When autoOpen is true the dialog will open 
        //  automatically when dialog is called. 
        //  If false it will stay hidden until .dialog("open") is called on it.
        autoOpen: false,

        //  The effect to be used when the dialog is opened.
        show: 'slide',

        //  If set to true, the dialog will have modal behavior; 
        //  other items on the page will be disabled
        modal: true,

        //  If set to true, the dialog will be resizeable.
        resizable: false,

        //  Specifies where the dialog should be displayed. Possible values:
        position: 'center'

    });

    //  Set filter dialog CSS properties
    $("#filterDialog").css({ visibility: "visible", cursor: "pointer" });

    //  Attach function with select event(triggered when clicking a tab).
    $('#tabs').tabs({ select: tabsSelected });

    //  Set visibility of CSS of tabs to "visible"
    //  (Set a key/value object as style properties to all matched elements.)
    $('#tabs').css({ visibility: "visible" });

    //  Attach function with select event(triggered when clicking a tab).
    $('#sampleCodeTabs').tabs({ select: tabsSelected });

    //  Set visibility of CSS of tabs to "visible"
    //  (Set a key/value object as style properties to all matched elements.)
    $('#sampleCodeTabs').css({ visibility: "visible" });

    //  Bind copyDataCode function to the click event of matched element.
    $("#copyDataCodeButton").click(copyDataCode);

    //  Bind copyMapCode function to the click event of matched element.
    $("#copyMapCodeButton").click(copyMapCode);

    //  Bind copyBarChartCode function to the click event of matched element.
    $("#copyBarChartCodeButton").click(copyBarChartCode);

    //  Bind copyPieChartCode function to the click event of matched element.
    $("#copyPieChartCodeButton").click(copyPieChartCode);

    //  Attach clipboardReady function to the clipboardReady event
    $.clipboardReady(clipboardReady, { swfpath: "~/Scripts/jquery.clipboard.swf" });

    //  Add event which occurs when the user clicks the browser's Back or Forward button.
    Sys.Application.add_navigate(onStateChanged);

}



/// <summary>
/// This function sets the elements on the page when page gets loaded
/// </summary>
function page_init() {

    //  Normal page load URL is considered as bookmarked URL
    isBookmarkedUrl = true;
    isHashChangeRoute = false;
    //  Create a new instance of the Map object.
    var tmp = document.getElementById("bmc");
    map = new Microsoft.Maps.Map(document.getElementById("myMap"), { credentials: tmp.textContent, enableSearchLogo: false, height: 450, width: 862 });
    Microsoft.Maps.Events.addHandler(map, "onerror", showMapControlError);
    initializePage();

    if (hash_columns == null) {
        var nbCol = document.getElementsByTagName("th").length;
        hash_columns = new Array(nbCol);
        $.each(hash_columns, function (ndx) {
            hash_columns[ndx] = true;
        });
    }
    readHashValueIntoHashVariables();

    setPageAccordingToHashVariables();

}

function btn_click_column(column) {

    if (isHold(column)) {
        document.getElementById(column).className = "myround secondary";
        hash_columns[column] = false;
        $("[name=" + column + "]").each(function (ndx) {
            this.style.display = "none";
        });
    }
    else {
        document.getElementById(column).className = "myround secondary-hold";
        hash_columns[column] = true;
        $("[name=" + column + "]").each(function (ndx) {
            this.style.display = "table-cell";
        });
    }

}

function isHold(data) {
    if (document.getElementById(data).className.indexOf("hold", 0) != -1)
        return true;
    else return false;
}


/// <summary>
/// This function replaces the current variables with hash variables
/// </summary>
function replaceCurrentVariablesWithHashVariables() {
    current_filter = hash_filter;
    current_viewType = hash_viewType;
    current_tagType = hash_tagType;
    current_language = hash_language;
    current_mapStyle = hash_mapStyle;
    current_zoomLevel = hash_zoomLevel;
    current_longitude = hash_longitude;
    current_latitude = hash_latitude;
    current_sceneId = hash_sceneId;
    current_birdseyeSceneOrientation = hash_birdseyeSceneOrientation;
    current_horizontalColumnName = hash_horizontalColumnName;
    current_dateRange = hash_dateRange;
    current_verticalOptions = hash_verticalOptions;
    current_verticalColumnName = hash_verticalColumnName;
    current_verticalColumnOptions = hash_verticalColumnOptions;
}

/// <summary>
/// This function Sets the data view using hash variables
/// </summary>
function setDataView() {
    switch (hash_tagType) {
        case "SampleCode":
            if (isHashChangeRoute) {
                if (current_viewType != hash_viewType) {
                    isBookmarkedUrl = true;
                    if (!$('#liDataView').hasClass('ui-state-active')) {
                        $('#tabs').tabs('select', 0);
                    }
                }
                if (current_tagType != hash_tagType) {
                    isBookmarkedUrl = true;
                    $("#dataViewAccordion").accordion("activate", 0);
                }
                if (current_language != hash_language) {
                    isBookmarkedUrl = true;
                    document.getElementById("languagesDataView").value = hash_language;
                    languageDataViewSelectedIndexChanged();
                }
                if (current_filter != hash_filter) {
                    isBookmarkedUrl = true;
                    SetFilter(hash_filter);
                    runClicked();
                }
            }
            else {
                if (!$('#liDataView').hasClass('ui-state-active')) {
                    $('#tabs').tabs('select', 0);
                }
                $("#dataViewAccordion").accordion("activate", 0);
                document.getElementById("languagesDataView").value = hash_language;
                languageDataViewSelectedIndexChanged();
                SetFilter(hash_filter);
                runClicked();
            }
            break;

        case 'Results':
            if (isHashChangeRoute) {
                if (current_viewType != hash_viewType) {
                    isBookmarkedUrl = true;
                    if (!$('#liDataView').hasClass('ui-state-active')) {
                        $('#tabs').tabs('select', 0);
                    }
                }
                if (current_tagType != hash_tagType) {
                    isBookmarkedUrl = true;
                    $("#dataViewAccordion").accordion("activate", 1);
                }
                if (current_filter != hash_filter) {
                    isBookmarkedUrl = true;
                    SetFilter(hash_filter);
                    runClicked();
                }
            } else {
                SetFilter(hash_filter);
                runClicked();
            }
            break;


    }
}

/// <summary>
/// This function sets the map view using hash variables
/// </summary>
function setMapView() {
    switch (hash_tagType) {
        case "SampleCode":
            if (isHashChangeRoute) {
                if (current_viewType != hash_viewType) {
                    isBookmarkedUrl = true;
                    if (!$('#liMapView').hasClass('ui-state-active')) {
                        $('#tabs').tabs('select', 1);
                    }
                }
                if (current_tagType != hash_tagType) {
                    isBookmarkedUrl = true;
                    $("#mapViewAccordion").accordion("activate", 0);
                }
                if (current_language != hash_language) {
                    isBookmarkedUrl = true;
                    document.getElementById("languagesMapView").value = hash_language;
                    languageMapViewSelectedIndexChanged();
                }
                if (current_filter != hash_filter) {
                    isBookmarkedUrl = true;
                    SetFilter(hash_filter);
                    runClicked();
                }
            } else {
                if (!$('#liMapView').hasClass('ui-state-active')) {
                    $('#tabs').tabs('select', 1);
                }
                $("#mapViewAccordion").accordion("activate", 0);
                document.getElementById("languagesMapView").value = hash_language;
                languageMapViewSelectedIndexChanged();
                SetFilter(hash_filter);
                runClicked();
            }
            break;
        case "Results":
            if (isHashChangeRoute) {
                if (current_viewType != hash_viewType) {
                    isBookmarkedUrl = true;
                    if (!$('#liMapView').hasClass('ui-state-active')) {
                        $('#tabs').tabs('select', 1);
                    }
                }
                if (current_tagType != hash_tagType) {
                    isBookmarkedUrl = true;
                    $("#mapViewAccordion").accordion("activate", 1);
                }
                if (current_filter != hash_filter) {
                    isBookMarkedMapView = 'N';
                    isBookmarkedUrl = true;
                    SetFilter(hash_filter);
                    runClicked();
                } else {
                    setMapViewByHash();
                }
            } else {
                if (!$('#liMapView').hasClass('ui-state-active')) {
                    $('#tabs').tabs('select', 1);
                }
                isBookMarkedMapView = 'Y';
                SetFilter(hash_filter);
                runClicked();
            }
            break;


    }
}

/// <summary>
/// This function sets the bar chart view using hash variables
/// </summary>
function setBarChartView() {
    switch (hash_tagType) {
        case "SampleCode":
            if (isHashChangeRoute) {
                if (current_viewType != hash_viewType) {
                    isBookmarkedUrl = true;
                    if (!$('#liBarChart').hasClass('ui-state-active')) {
                        $('#tabs').tabs('select', 2);
                    }
                }
                if (current_tagType != hash_tagType) {
                    isBookmarkedUrl = true;
                    $("#barChartAccordion").accordion("activate", 0);
                }
                if (current_language != hash_language) {
                    isBookmarkedUrl = true;
                    document.getElementById("languagesBarChartView").value = hash_language;
                    languageBarChartViewSelectedIndexChanged();
                }
                if (current_filter != hash_filter) {
                    isBookmarkedUrl = true;
                    SetFilter(hash_filter);
                    runClicked();
                }

            } else {
                if (!$('#liBarChart').hasClass('ui-state-active')) {
                    $('#tabs').tabs('select', 2);
                }
                $("#barChartAccordion").accordion("activate", 0);
                document.getElementById("languagesBarChartView").value = hash_language;
                languageBarChartViewSelectedIndexChanged();
                SetFilter(hash_filter);
                runClicked();
            }
            break;
        case "Results":
            if (isHashChangeRoute) {
                if (current_viewType != hash_viewType) {
                    isBookmarkedUrl = true;
                    if (!$('#liBarChart').hasClass('ui-state-active')) {
                        $('#tabs').tabs('select', 2);
                    }
                }
                if (current_tagType != hash_tagType) {
                    isBookmarkedUrl = true;
                    $("#barChartAccordion").accordion("activate", 1);
                }
                if ((current_filter != hash_filter) || (current_horizontalColumnName != hash_horizontalColumnName) || (current_dateRange != hash_dateRange) || (current_verticalOptions != hash_verticalOptions) || (current_verticalColumnName != hash_verticalColumnName) || (current_verticalColumnOptions != hash_verticalColumnOptions)) {
                    isBookmarkedUrl = true;
                    SetFilter(hash_filter);
                    runClicked();
                    isBookmarkedUrl = true;
                    ResetBarChartFilter();
                    SetBarChartFilter();
                    drawBarChart(hash_filter, hash_horizontalColumnName, hash_dateRange, hash_verticalOptions, hash_verticalColumnName, hash_verticalColumnOptions)
                }
            }
            else {
                if (!$('#liBarChart').hasClass('ui-state-active')) {
                    $('#tabs').tabs('select', 2);
                }
                SetFilter(hash_filter);
                runClicked();
                ResetBarChartFilter();
                SetBarChartFilter();
                drawBarChart(hash_filter, hash_horizontalColumnName, hash_dateRange, hash_verticalOptions, hash_verticalColumnName, hash_verticalColumnOptions)
            }
            break;

    }
}

/// <summary>
/// This function sets the pie chart view using hash variables
/// </summary>
function setPieChartView() {
    switch (hash_tagType) {
        case "SampleCode":
            if (isHashChangeRoute) {
                if (current_viewType != hash_viewType) {
                    isBookmarkedUrl = true;
                    if (!$('#liPieChart').hasClass('ui-state-active')) {
                        $('#tabs').tabs('select', 3);
                    }
                }
                if (current_tagType != hash_tagType) {
                    isBookmarkedUrl = true;
                    $("#pieChartAccordion").accordion("activate", 0);
                }
                if (current_language != hash_language) {
                    isBookmarkedUrl = true;
                    document.getElementById("languagesPieChartView").value = hash_language;
                    languagePieChartViewSelectedIndexChanged();
                }
                if (current_filter != hash_filter) {
                    isBookmarkedUrl = true;
                    SetFilter(hash_filter);
                    runClicked();
                }

            } else {
                if (!$('#liPieChart').hasClass('ui-state-active')) {
                    $('#tabs').tabs('select', 3);
                }
                $("#pieChartAccordion").accordion("activate", 0);
                document.getElementById("languagesPieChartView").value = hash_language;
                languagePieChartViewSelectedIndexChanged();
                SetFilter(hash_filter);
                runClicked();
            }
            break;
        case "Results":
            if (isHashChangeRoute) {
                if (current_viewType != hash_viewType) {
                    isBookmarkedUrl = true;
                    if (!$('#liPieChart').hasClass('ui-state-active')) {
                        $('#tabs').tabs('select', 3);
                    }
                }
                if (current_tagType != hash_tagType) {
                    isBookmarkedUrl = true;
                    $("#pieChartAccordion").accordion("activate", 1);
                }
                if ((current_filter != hash_filter) || (current_horizontalColumnName != hash_horizontalColumnName) || (current_dateRange != hash_dateRange) || (current_verticalOptions != hash_verticalOptions) || (current_verticalColumnName != hash_verticalColumnName) || (current_verticalColumnOptions != hash_verticalColumnOptions)) {
                    isBookmarkedUrl = true;
                    SetFilter(hash_filter);
                    runClicked();
                    isBookmarkedUrl = true;
                    ResetPieChartFilter();
                    SetPieChartFilter();
                    drawPieChart(hash_filter, hash_horizontalColumnName, hash_dateRange, hash_verticalOptions, hash_verticalColumnName, hash_verticalColumnOptions)
                }
            }
            else {
                if (!$('#liPieChart').hasClass('ui-state-active')) {
                    $('#tabs').tabs('select', 3);
                }
                SetFilter(hash_filter);
                runClicked();
                ResetPieChartFilter();
                SetPieChartFilter();
                drawPieChart(hash_filter, hash_horizontalColumnName, hash_dateRange, hash_verticalOptions, hash_verticalColumnName, hash_verticalColumnOptions)
            }
            break;

    }
}

/// <summary>
/// This function sets/resets the page using hash variables
/// </summary>
function setPageAccordingToHashVariables() {
    if (isBookmarkedUrl) {
        replaceCurrentVariablesWithHashVariables();
    }
    switch (hash_viewType) {
        case 'DataView':
            setDataView();
            isHashChangeRoute = false;
            isBookmarkedUrl = false;
            break;

        case 'MapView':
            setMapView();
            isHashChangeRoute = false;
            isBookmarkedUrl = false;
            break;

        case 'BarChartView':  //  When Bar Chart View ...
            setBarChartView();
            isHashChangeRoute = false;
            isBookmarkedUrl = false;
            break;

        case 'PieChartView':  //  When Pie Chart View ...
            setPieChartView();
            isHashChangeRoute = false;
            isBookmarkedUrl = false;
            break;


    }
}

/// <summary>
/// This function shows map control error
/// </summary>
function showMapControlError() {
    alert(mapControlError);
}

/// <summary>
/// This function avoids mouse wheel scrolling on map view
/// </summary>
function wheelCallback(e) {
    window.scrollBy(0, -5 * e.mouseWheelChange);
    //  true for canceling default action
    return true;
}

/// <summary>
/// This function changes status of DataBrowser page if 
/// URL hash status is different than the current status of page
/// </summary>
function onStateChanged(sender, e) {
    if (isDeltaInHashAndCurrent()) {
        isBookmarkedUrl = false;
        isHashChangeRoute = true;
        setPageAccordingToHashVariables();
    }
}

/// <summary>
/// This function checks whether current page status is different that 
/// the page status mentioned in URL hash or not
/// </summary>
function isDeltaInHashAndCurrent() {
    readHashValueIntoHashVariables();
    var fillCurrentVaribles = createHashValue();
    switch (hash_viewType) {
        case "DataView":
            switch (hash_tagType) {
                case "SampleCode":
                    if ((hash_filter != current_filter) || (hash_viewType != current_viewType) || (hash_tagType != current_tagType) || (hash_language != current_language)) {

                        return true;
                    } else {
                        return false;
                    }
                    break;
                case "Results":
                    if ((hash_filter != current_filter) || (hash_viewType != current_viewType) || (hash_tagType != current_tagType)) {
                        return true;
                    } else {
                        return false;
                    }
                    break;

            }
            break;
        case "MapView":
            switch (hash_tagType) {
                case "SampleCode":
                    if ((hash_filter != current_filter) || (hash_viewType != current_viewType) || (hash_tagType != current_tagType) || (hash_language != current_language)) {
                        return true;
                    } else {
                        return false;
                    }
                    break;
                case "Results": break;
            }
            break;

        case "BarChartView":
        case "PieChartView":
            switch (hash_tagType) {
                case "SampleCode":
                    if ((hash_filter != current_filter) || (hash_viewType != current_viewType) || (hash_tagType != current_tagType) || (hash_language != current_language)) {
                        return true;
                    } else {
                        return false;
                    }
                    break;
                case "Results":
                    if ((hash_filter != current_filter) || (hash_viewType != current_viewType) || (hash_tagType != current_tagType) || (hash_horizontalColumnName != current_horizontalColumnName) || (hash_dateRange != current_dateRange) || (hash_verticalOptions != current_verticalOptions) || (hash_verticalColumnName != current_verticalColumnName) || (hash_verticalColumnOptions != current_verticalColumnOptions)) {
                        return true;
                    } else {
                        return false;
                    }
                    break;

            }
            break;


    }
}

/// <summary>
/// This function shows the error on the Databrowser page
/// if isString==false then error is object  else it is considered as object
/// </summary>
function errorFunction(error, isString) {
    if (!isString) {
        error = error.statusText;
    }

    //  This call takes one argument, an object of key/value pairs, 
    //  that are used to initalize and handle the request, 
    //  and returns the XMLHttpRequest object for 
    //  post-processing (like manual aborting) if needed. 
    $.ajax(
                {

                    //  The type of request to make ("POST" or "GET"), default is "GET".
                    type: "POST",

                    //  The URL to request is set here
                    //  MapRoute name --> DataBrowserError
                    url: "../DataBrowserError/",

                    data: { error: error },

                    //  The type of data that you're expecting 
                    //  back from the server is set to html
                    dataType: 'html',

                    //  A function to be called if the request succeeds.
                    success: function (html) {
                        //  Set display Style for tabs to block
                        //  (Set matched element with id == divError VISIBLE)
                        document.getElementById("divError").style.display = "block";

                        //  Set html content in the tag(id == divError) with 
                        //  html in "html" variable passed to this function
                        $("#divError").html(html);

                        //  Set display Style for tabs to none
                        //  (Set matched element with id == tabs INVISIBLE)
                        document.getElementById("tabs").style.display = "none";

                    }
                }
                );
}

/// <summary>
/// This function gets the data to be displayed in DataView  
/// from server and shows that data in the data view
/// </summary>
function getData(filter) {
    $.ajax(
        {

            //  The type of request to make ("POST" or "GET"), default is "GET".
            type: "POST",

            //  The URL to request is set here
            //  MapRoute name --> DataBrowserRun
            url: "../DataBrowserRun/",

            data: { container: container, entitySet: entitySet, filter: filter },

            //  The type of data that you're expecting 
            //  back from the server is set to html
            dataType: 'html',

            //  A function to be called if the request fails.
            error: function (error) {
                errorFunction(error, false);
            },

            //  A function to be called if the request succeeds.
            success: function (html) {
                //  If substring 'id="pError"' is present in 
                //  html data  returned from server then ...

                if (html.toString().indexOf('id="pError"', 0) >= 0) {
                    //  Set display Style for tabs to block
                    //  (Set matched element with id == divError VISIBLE)
                    document.getElementById("divError").style.display = "block";

                    //  Set html content in the tag(id == divError) with 
                    //  html in "html" variable passed to this function
                    $("#divError").html(html);

                    //  Set display Style for tabs to none
                    //  (Set matched element with id == tabs INVISIBLE)
                    document.getElementById("tabs").style.display = "none";

                }
                else {

                    //  Set display Style for divError to none
                    //  (Set matched element with id == divError INVISIBLE)
                    document.getElementById("divError").style.display = "none";

                    //  Set display Style for tabs to block
                    //  (Set matched element with id == tabs VISIBLE)
                    document.getElementById("tabs").style.display = "block";

                    //  Set visibility Style for tabs to visible
                    document.getElementById("tabs").style.visibility = "visible";

                    //  Set html content in the tag(id == divDataViewResults) with 
                    //  html in "html" variable passed to this function
                    $("#divDataViewResults").html(html);
                }
            }
        }
        );
}

/// <summary>
/// This function sets visibility of "Start", "< Prev" & "Next >" link
/// </summary>
function SetPagingLinkVisibility() {

    var prevEnable = document.getElementById("labelPrevEnable").title;
    var nextEnable = document.getElementById("labelNextEnable").title;

    //  If prevEnable value is '1' then ...
    if (prevEnable == "1") {

        //  Enable prevLink
        document.getElementById("prevLink").disabled = false;

    } //  If prevEnable value is NOT '1' then ...
    else {

        //  Disable prevLink
        document.getElementById("prevLink").disabled = true;

        //  Set prevLink class to "disabled"
        document.getElementById("prevLink").className = "ico icoPrevDisabled";
    }

    //  If nextEnable value is '1' then ...
    if (nextEnable == "1") {

        //  Enable nextLink
        document.getElementById("nextLink").disabled = false;

    } //  If nextEnable value is NOT '1' then ...
    else {

        //  Disable nextLink
        document.getElementById("nextLink").disabled = true;

        //  Set nextLink class to "disabled"
        document.getElementById("nextLink").className = "ico icoNextDisabled";
    }
}

/// <summary>
/// This function opens the Filter hint's dialog
/// </summary>
function ShowFilterHintsDialog() {

    //  Open filter Dialog
    $('#filterDialog').dialog('open');

}

/// <summary>
/// This function shows/hides error panel
/// </summary>
function ShowHideError() {
    var error = document.getElementById("labelError").title;
    //  If there is no ERROR in "error" variable then ...
    if (error == '') {

        //  Set display Style for divError to none
        //  (Set matched element with id == divError INVISIBLE)
        document.getElementById("divError").style.display = "none";

    } //  If there is ERROR in "error" variable then ...
    else {

        //  Set display Style for divError to block
        //  (Set matched element with id == divError VISIBLE)
        document.getElementById("divError").style.display = "block";

    }
}


/// <summary>
/// This function loads the sample code for the selected language 
/// in combo box in Data View
/// </summary>
function languageDataViewSelectedIndexChanged() {
    current_language = readSampleCodeLanguageFromActiveViewType();
    var hashValue = createHashValue();
    changeDataViewLanguage(current_language);
    if (isBookmarkedUrl == false) {
        Sys.Application.addHistoryPoint({ param: hashValue }, siteTitle);
    } else {
        isBookmarkedUrl = false;
    }
}

/// <summary>
/// This function changes the sample code in Data View according to new selected language
/// </summary>
function changeDataViewLanguage(selectedLanguage) {

    //  This call takes one argument, an object of key/value pairs, 
    //  that are used to initalize and handle the request, 
    //  and returns the XMLHttpRequest object for 
    //  post-processing (like manual aborting) if needed. 
    $.ajax(
        {

            //  The type of request to make ("POST" or "GET"), default is "GET".
            type: "POST",

            //  The URL to request is set here using selectedLanguage
            //  MapRoute name --> LoadSampleCodeDataView
            url: "../LoadSampleCodeDataView/",

            data: { selectedFileName: selectedLanguage, container: container, entitySet: entitySet },

            //  The type of data that you're expecting 
            //  back from the server is set to html
            dataType: 'html',

            //  A function to be called if the request succeeds.
            success: function (html) {

                //  Set html content in the tag(id == divDataViewSampleCode) with 
                //  html in "html" variable passed to this function
                $("#divDataViewSampleCode").html(html);

            }
        }
        );
}

/// <summary>
/// This function changes the sample code in Map View according to new selected language
/// </summary>
function changeMapViewLanguage(selectedLanguage) {

    //  This call takes one argument, an object of key/value pairs, 
    //  that are used to initalize and handle the request, 
    //  and returns the XMLHttpRequest object for 
    //  post-processing (like manual aborting) if needed. 
    $.ajax(
        {

            //  The type of request to make ("POST" or "GET"), default is "GET".
            type: "POST",

            //  The URL to request is set here using selectedLanguage
            //  MapRoute name --> LoadSampleCodeMapView
            url: "../LoadSampleCodeMapView/",


            data: { selectedFileName: selectedLanguage, container: container, entitySet: entitySet },

            //  The type of data that you're expecting 
            //  back from the server is set to html
            dataType: 'html',

            //  A function to be called if the request succeeds.
            success: function (html) {

                //  Set html content in the tag(id == divMapViewSampleCode) with 
                //  html in "html" variable passed to this function
                $("#divMapViewSampleCode").html(html);

            }
        }
        );
}

/// <summary>
/// This function loads the sample code for the selected language 
/// in combo box in Map View
/// </summary>
function languageMapViewSelectedIndexChanged() {
    current_language = readSampleCodeLanguageFromActiveViewType();
    var hashValue = createHashValue();
    changeMapViewLanguage(current_language);
    if (isBookmarkedUrl == false) {
        Sys.Application.addHistoryPoint({ param: hashValue }, siteTitle);
    } else {
        isBookmarkedUrl = false;
    }
}

/// <summary>
/// This function loads the sample code for the selected language 
/// in combo box in Bar Chart View
/// </summary>
function languageBarChartViewSelectedIndexChanged() {
    current_language = readSampleCodeLanguageFromActiveViewType();
    var hashValue = createHashValue();
    changeBarChartViewLanguage(current_language);
    if (isBookmarkedUrl == false) {
        Sys.Application.addHistoryPoint({ param: hashValue }, siteTitle);
    } else {
        isBookmarkedUrl = false;
    }
}

/// <summary>
/// This function changes the sample code in Bar Chart View according to new selected language
/// </summary>
function changeBarChartViewLanguage(selectedLanguage) {

    //  This call takes one argument, an object of key/value pairs, 
    //  that are used to initalize and handle the request, 
    //  and returns the XMLHttpRequest object for 
    //  post-processing (like manual aborting) if needed. 
    $.ajax(
        {

            //  The type of request to make ("POST" or "GET"), default is "GET".
            type: "POST",

            //  The URL to request is set here using selectedLanguage
            //  MapRoute name --> LoadSampleCodeBarChartView
            url: "../LoadSampleCodeBarChartView/",

            data: { selectedFileName: selectedLanguage, container: container, entitySet: entitySet },

            //  The type of data that you're expecting 
            //  back from the server is set to html
            dataType: 'html',

            //  A function to be called if the request succeeds.
            success: function (html) {

                //  Set html content in the 
                //  tag(id == divBarChartViewSampleCode) with 
                //  html in "html" variable passed to this function
                $("#divBarChartViewSampleCode").html(html);

            }
        }
        );
}

/// <summary>
/// This function loads the sample code for the selected language 
/// in combo box in Pie Chart View
/// </summary>
function languagePieChartViewSelectedIndexChanged() {
    current_language = readSampleCodeLanguageFromActiveViewType();
    var hashValue = createHashValue();
    changePieChartViewLanguage(current_language);
    if (isBookmarkedUrl == false) {
        Sys.Application.addHistoryPoint({ param: hashValue }, siteTitle);
    } else {
        isBookmarkedUrl = false;
    }
}

/// <summary>
/// This function changes the sample code in Pie Chart View according to new selected language
/// </summary>
function changePieChartViewLanguage(selectedLanguage) {

    //  This call takes one argument, an object of key/value pairs, 
    //  that are used to initalize and handle the request, 
    //  and returns the XMLHttpRequest object for 
    //  post-processing (like manual aborting) if needed. 
    $.ajax(
        {

            //  The type of request to make ("POST" or "GET"), default is "GET".
            type: "POST",

            //  The URL to request is set here using selectedLanguage
            //  MapRoute name --> LoadSampleCodePieChartView
            url: "../LoadSampleCodePieChartView/",

            data: { selectedFileName: selectedLanguage, container: container, entitySet: entitySet },

            //  The type of data that you're expecting 
            //  back from the server is set to html
            dataType: 'html',

            //  A function to be called if the request succeeds.
            success: function (html) {

                //  Set html content in the 
                //  tag(id == divPieChartViewSampleCode) with 
                //  html in "html" variable passed to this function
                $("#divPieChartViewSampleCode").html(html);

            }
        }
        );
}

/// <summary>
/// This function loads previous 10(=pageSize) records in Data View
/// </summary>
function previousClicked() {

    //  If prevLink is NOT disabled then ...
    if ($("#prevLink").hasClass("disabled") == false) {

        var filter = current_filter;
        SetFilter(filter);

        //  This call takes one argument, an object of key/value pairs, 
        //  that are used to initalize and handle the request, 
        //  and returns the XMLHttpRequest object for 
        //  post-processing (like manual aborting) if needed.
        $.ajax(
            {

                //  The type of request to make ("POST" or "GET"), default is "GET".
                type: "POST",

                //  The URL to request is set here
                //  MapRoute name --> DataBrowserPaging
                url: "../DataBrowserPaging/",

                data: { container: container, entitySet: entitySet, filter: filter, link: "Previous" },

                //  The type of data that you're expecting 
                //  back from the server is set to html
                dataType: 'html',

                //  A function to be called if the request fails.
                error: function (error) {
                    errorFunction(error, false);
                },

                //  A function to be called if the request succeeds.
                success: function (html) {

                    //  If substring 'id="pError"' is present in 
                    //  html data  returned from server then ...
                    if (html.toString().indexOf('id="pError"', 0) >= 0) {

                        //  Set display Style for divError to block
                        //  (Set matched element with id == divError VISIBLE)
                        document.getElementById("divError").style.display = "block";

                        //  Set html content in the tag(id == divError) with 
                        //  html in "html" variable passed to this function
                        $("#divError").html(html);

                        //  Set display Style for tabs to none
                        //  (Set matched element with id == tabs INVISIBLE)
                        document.getElementById("tabs").style.display = "none";

                    } //  If substring 'id="pError"' is NOT present in 
                        //  html data  returned from server then ...
                    else {

                        //  Set display Style for divError to none
                        //  (Set matched element with id == divError INVISIBLE)
                        document.getElementById("divError").style.display = "none";

                        //  Set display Style for tabs to block
                        //  (Set matched element with id == tabs VISIBLE)
                        document.getElementById("tabs").style.display = "block";

                        //  Set visibility Style for tabs to visible
                        document.getElementById("tabs").style.visibility = "visible";

                        //  Set html content in the tag(id == divDataViewResults) with 
                        //  html in "html" variable passed to this function
                        $("#divDataViewResults").html(html);

                        $.each(hash_columns, function (ndx) {
                            if (!hash_columns[ndx])
                                btn_click_column(ndx);
                        });
                    }
                }
            }
            );
    }
}

/// <summary>
/// This function loads next 10(=pageSize) records in Data View
/// </summary>
function nextClicked() {

    //  If nextLink is NOT disabled then ...
    if ($("#nextLink").hasClass("disabled") == false) {

        var filter = current_filter;
        SetFilter(filter);

        //  This call takes one argument, an object of key/value pairs, 
        //  that are used to initalize and handle the request, 
        //  and returns the XMLHttpRequest object for 
        //  post-processing (like manual aborting) if needed.
        $.ajax(
            {

                //  The type of request to make ("POST" or "GET"), default is "GET".
                type: "POST",

                //  The URL to request is set here
                //  MapRoute name --> DataBrowserPaging
                url: "../DataBrowserPaging/",

                data: { container: container, entitySet: entitySet, filter: filter, link: "Next" },


                //  The type of data that you're expecting 
                //  back from the server is set to html
                dataType: 'html',

                //  A function to be called if the request fails.
                error: function (error) {
                    errorFunction(error, false);
                },

                //  A function to be called if the request succeeds.
                success: function (html) {

                    //  If substring 'id="pError"' is present in 
                    //  html data  returned from server then ...
                    if (html.toString().indexOf('id="pError"', 0) >= 0) {

                        //  Set display Style for divError to block
                        //  (Set matched element with id == divError VISIBLE)
                        document.getElementById("divError").style.display = "block";

                        //  Set html content in the tag(id == divError) with 
                        //  html in "html" variable passed to this function
                        $("#divError").html(html);

                        //  Set display Style for tabs to none
                        //  (Set matched element with id == tabs INVISIBLE)
                        document.getElementById("tabs").style.display = "none";

                    } //  If substring 'id="pError"' is NOT present in 
                        //  html data  returned from server then ...
                    else {

                        //  Set display Style for divError to none
                        //  (Set matched element with id == divError INVISIBLE)
                        document.getElementById("divError").style.display = "none";

                        //  Set display Style for tabs to block
                        //  (Set matched element with id == tabs VISIBLE)
                        document.getElementById("tabs").style.display = "block";

                        //  Set visibility Style for tabs to visible
                        document.getElementById("tabs").style.visibility = "visible";

                        //  Set html content in the tag(id == divDataViewResults) with 
                        //  html in "html" variable passed to this function
                        $("#divDataViewResults").html(html);

                        $.each(hash_columns, function (ndx) {
                            if (!hash_columns[ndx])
                                btn_click_column(ndx);
                        });
                    }
                }
            }
            );
    }
}

/// <summary>
/// This function sets hash variables according to View and Tag type passed to it
/// </summary>
function getDefaultHashValueFor(defaultViewToGo, defaultTagToGo) {
    hash_filter = noFilterText;
    switch (defaultViewToGo) {
        case "DataView":
            hash_viewType = "DataView";
            switch (defaultTagToGo) {

                //  NOFILTER--DataView--SampleCode--SampleCode_CS.txt    
                case "SampleCode":
                    hash_tagType = "SampleCode";
                    hash_language = $("[id =" + "languages" + hash_viewType + "]")[0].value;
                    break;

                    //  NOFILTER--DataView--Results    
                case "Results":
                    hash_tagType = "Results";
                    break;
            }
            break;


        case "MapView":
            hash_viewType = "MapView";
            switch (defaultTagToGo) {

                //  NOFILTER--MapView--SampleCode--SampleCode_MAP.txt   
                case "SampleCode":
                    hash_tagType = "SampleCode";
                    hash_language = $("[id =" + "languages" + hash_viewType + "]")[0].value;
                    break;

                    //  NOFILTER--MapView--Results--r--4---99.49218750000001--37.02009820136811 
                case "Results":
                    hash_tagType = "Results";
                    hash_mapStyle = "r";
                    hash_zoomLevel = 11;
                    hash_longitude = "0";
                    hash_latitude = "0";
                    break;
            }
            break;

        case "BarChartView":
            hash_viewType = "BarChartView";
            switch (defaultTagToGo) {

                //  NOFILTER--BarChartView--SampleCode--SampleCode_CS_BarChart.txt   
                case "SampleCode":
                    hash_tagType = "SampleCode";
                    hash_language = $("[id =" + "languages" + hash_viewType + "]")[0].value;
                    break;

                    //  NOFILTER--BarChartView--Results--X--X--Option1--X--Aggregate   
                case "Results":
                    hash_tagType = "Results";
                    hash_horizontalColumnName = "X";
                    hash_dateRange = "X";
                    hash_verticalOptions = "Option1";
                    hash_verticalColumnName = "X";
                    hash_verticalColumnOptions = "Aggregate";
                    break;
            }
            break;

        case "PieChartView":
            hash_viewType = "PieChartView";
            switch (defaultTagToGo) {

                //  NOFILTER--PieChartView--SampleCode--SampleCode_CS_PieChart.txt        
                case "SampleCode":
                    hash_tagType = "SampleCode";
                    hash_language = $("[id =" + "languages" + hash_viewType + "]")[0].value;
                    break;

                    //  NOFILTER--PieChartView--Results--X--X--Option1--X--Aggregate   
                case "Results":
                    hash_tagType = "Results";
                    hash_horizontalColumnName = "X";
                    hash_dateRange = "X";
                    hash_verticalOptions = "Option1";
                    hash_verticalColumnName = "X";
                    hash_verticalColumnOptions = "Aggregate";
                    break;
            }
            break;
    }
}


/// <summary>
/// This function checks for given view whether given language is present in the sample code language 
/// dropdownlist or not
/// </summary>
function isValuePresentInLanguageComboBox(languageInHash, viewType) {
    var dropDownList = $("[id =" + "languages" + viewType + "]");
    for (var i = 0; i < dropDownList[0].length; i++) {
        if (dropDownList[0][i].value == languageInHash) {
            return true;
        }
    }
    return false;
}

/// <summary>
/// This function reads the hash URL and separate outs the parametes in it
/// into hash variables
/// </summary>
function readHashValueIntoHashVariables() {

    var hashURL = window.location.hash;

    if (hashURL == "" || hashURL == "#") {
        getDefaultHashValueFor("DataView", "Results");
        return;
    }

    //  Since length of string "#param=" is 7 ...
    var hashParametersArray = decodeURI((hashURL)).substr(7).split(hashSeparator);
    if (hashParametersArray.length <= 2) {
        getDefaultHashValueFor("DataView", "Results");
        return;
    }

    if (hashParametersArray.length >= 2) {
        hash_filter = unescape(hashParametersArray[0]);
        hash_viewType = hashParametersArray[1];
        hash_tagType = hashParametersArray[2];
        switch (hash_viewType) {
            case 'DataView':
                switch (hash_tagType) {
                    case 'SampleCode':
                        hash_language = hashParametersArray[3];
                        if (hash_language == "" || (!isValuePresentInLanguageComboBox(hash_language, "DataView"))) {
                            getDefaultHashValueFor("DataView", "SampleCode");
                        }
                        break;

                    case 'Results':
                        break;

                    default:
                        getDefaultHashValueFor("DataView", "Results");
                        break;
                }
                break;

            case 'MapView':
                switch (hash_tagType) {
                    case 'SampleCode':
                        hash_language = hashParametersArray[3];
                        if (hash_language == "" || (!isValuePresentInLanguageComboBox(hash_language, "MapView"))) {
                            getDefaultHashValueFor("MapView", "SampleCode");
                        }
                        break;

                    case 'Results':
                        // indexInHashWhereMapStyleStarts ~ param=FILTER(0)/MapView(1)/Result(2)/MapStyle(3)/ZoomLevel(4)...
                        hash_mapStyle = hashParametersArray[indexInHashWhereMapStyleStarts];
                        if (hash_mapStyle != 'r' && hash_mapStyle != 'a' && hash_mapStyle != 'h' && hash_mapStyle != 'o' && hash_mapStyle != 'b') {
                            getDefaultHashValueFor("MapView", "Results");
                            return;
                        }
                        hash_zoomLevel = parseInt(hashParametersArray[indexInHashWhereMapStyleStarts + 1]);
                        if (isNaN(hash_zoomLevel)) {
                            getDefaultHashValueFor("MapView", "Results");
                            return;
                        }

                        hash_longitude = hashParametersArray[indexInHashWhereMapStyleStarts + 2];
                        if (isNaN(hash_longitude)) {
                            getDefaultHashValueFor("MapView", "Results");
                            return;
                        }

                        hash_latitude = hashParametersArray[indexInHashWhereMapStyleStarts + 3];
                        if (isNaN(hash_latitude)) {
                            getDefaultHashValueFor("MapView", "Results");
                            return;
                        }

                        switch (hash_mapStyle) {
                            case 'b': case 'o':
                                hash_sceneId = hashParametersArray[indexInHashWhereMapStyleStarts + 4];
                                if (isNaN(hash_sceneId)) {
                                    getDefaultHashValueFor("MapView", "Results");
                                    return;
                                }
                                hash_birdseyeSceneOrientation = hashParametersArray[indexInHashWhereMapStyleStarts + 5];
                                if (hash_birdseyeSceneOrientation != 'North' && hash_birdseyeSceneOrientation != 'South' && hash_birdseyeSceneOrientation != 'East' && hash_birdseyeSceneOrientation != 'West') {
                                    getDefaultHashValueFor("MapView", "Results");
                                    return;
                                }
                                break;
                        }
                        break;

                    default:
                        getDefaultHashValueFor("MapView", "Results");
                        break;
                }
                break;

            case 'BarChartView':
                switch (hash_tagType) {
                    case 'SampleCode':
                        hash_language = hashParametersArray[3];
                        if (hash_language == "" || (!isValuePresentInLanguageComboBox(hash_language, "BarChartView"))) {
                            getDefaultHashValueFor("BarChartView", "SampleCode");
                        }
                        break;

                    case 'Results':
                        hash_horizontalColumnName = hashParametersArray[3];
                        hash_dateRange = hashParametersArray[4];
                        hash_verticalOptions = hashParametersArray[5];
                        hash_verticalColumnName = hashParametersArray[6];
                        hash_verticalColumnOptions = hashParametersArray[7];
                        break;
                    default:
                        getDefaultHashValueFor("BarChartView", "Results");
                        break;
                }
                break;

            case 'PieChartView':
                switch (hash_tagType) {
                    case 'SampleCode':
                        hash_language = hashParametersArray[3];
                        if (hash_language == "" || (!isValuePresentInLanguageComboBox(hash_language, "PieChartView"))) {
                            getDefaultHashValueFor("PieChartView", "SampleCode");
                        }
                        break;

                    case 'Results':
                        hash_horizontalColumnName = hashParametersArray[3];
                        hash_dateRange = hashParametersArray[4];
                        hash_verticalOptions = hashParametersArray[5];
                        hash_verticalColumnName = hashParametersArray[6];
                        hash_verticalColumnOptions = hashParametersArray[7];
                        break;
                    default:
                        getDefaultHashValueFor("PieChartView", "Results");
                        break;
                }
                break;

            default:
                getDefaultHashValueFor("DataView", "Results");
                break;
        }

    }
}

/// <summary>
/// This function gets the data for applied filter and sets the json accordingly 
/// into the map
/// </summary>
function runClicked() {
    current_filter = GetFilter();
    getData(current_filter);
    loadJson(current_filter);
    if (isBookmarkedUrl == false) {
        switch (current_viewType) {
            case 'DataView':
            case 'MapView':
                ResetBarChartFilter();
                ResetPieChartFilter();
                setMapViewByHash();
                break;
            case 'BarChartView':
                if (current_tagType == "Results") {
                    getBarChart();
                    ResetPieChartFilter();
                }
                break;

            case 'PieChartView':
                if (current_tagType == "Results") {
                    getPieChart();
                    ResetBarChartFilter();
                }
                break;
        }
        var hashValue = createHashValue();
        Sys.Application.addHistoryPoint({ param: hashValue }, siteTitle);
    } else {
        isBookmarkedUrl = false;
    }
}

/// <summary>
/// This function reads the active sample code language from current view
/// </summary>
function readSampleCodeLanguageFromActiveViewType() {
    switch (current_viewType) {
        case 'DataView':
            return $("#languagesDataView > option:selected").attr("value");
        case 'MapView':
            return $("#languagesMapView > option:selected").attr("value");
        case 'BarChartView':
            return $("#languagesBarChartView > option:selected").attr("value");
        case 'PieChartView':
            return $("#languagesPieChartView > option:selected").attr("value");
    }
}

/// <summary>
/// This function sets the curent variables with current status of page and
/// returns the created hash value
/// </summary>
function createHashValue() {
    switch (current_tagType) {
        case 'SampleCode':
            current_language = readSampleCodeLanguageFromActiveViewType();
            return escape(current_filter) + hashSeparator + current_viewType + hashSeparator + current_tagType + hashSeparator + current_language;

        case 'Results':
            switch (current_viewType) {
                case 'DataView':
                    return escape(current_filter) + hashSeparator + current_viewType + hashSeparator + current_tagType;

                case 'MapView':
                    try {
                        current_mapStyle = map.getImageryId();
                        var center = map.getCenter(); current_zoomLevel = map.getZoom(); current_longitude = center.Longitude; current_latitude = center.Latitude;
                        current_mapDetails = current_mapStyle + hashSeparator + current_zoomLevel + hashSeparator + current_longitude + hashSeparator + current_latitude;

                    } catch (e) {
                        errorFunction(mapControlError, true);
                        hideLoadingIndicator();
                    }
                    return escape(current_filter) + hashSeparator + current_viewType + hashSeparator + current_tagType + hashSeparator + current_mapDetails;

                case 'BarChartView':
                    getBarChartParametersInCurrentVariables();
                    return escape(current_filter) + hashSeparator + current_viewType + hashSeparator + current_tagType
                        + hashSeparator + current_horizontalColumnName
                        + hashSeparator + current_dateRange
                        + hashSeparator + current_verticalOptions
                        + hashSeparator + current_verticalColumnName
                        + hashSeparator + current_verticalColumnOptions;


                case 'PieChartView':
                    getPieChartParametersInCurrentVariables();
                    return escape(current_filter) + hashSeparator + current_viewType + hashSeparator + current_tagType
                        + hashSeparator + current_horizontalColumnName
                        + hashSeparator + current_dateRange
                        + hashSeparator + current_verticalOptions
                        + hashSeparator + current_verticalColumnName
                        + hashSeparator + current_verticalColumnOptions;
            }
            break;
    }

}

/// <summary>
/// Reset Bar Chart filter & hide the graph if any
/// </summary>
function ResetBarChartFilter() {

    // Set Horizontal Column Name combobox to : Select One
    document.getElementById("ddlBarHorizontal")[0].selected = true;

    // Set Vertical Column Name combobox to : Select One
    document.getElementById("ddlBarVertical")[0].selected = true;

    $("#labelBarDateRange").attr("style", "display:none;");
    document.getElementById("ddlBarDateRange").style.display = "none";

    // Select option 1.reset
    var barVerticalOptions = document.getElementsByName("BarVerticalOptions");
    barVerticalOptions[0].checked = 'checked';
    barOption1Selected();

    //  Set columnNameSelected to "Horizontal Axis column"
    columnNameSelected = horizontalAxisColText;

    //  Append the new <span> tag containing new string based on 
    //  value selected in dropdownlist inside the tag
    //  with id == labelBarCount
    $("#labelBarCount > span").remove();
    $("#labelBarCount").append("<span>&ldquo;"
            + columnNameSelected + "&rdquo;</span>");

    // Select Aggregate
    var barVerticalColumnOptions = document.getElementsByName("BarVerticalColumnOptions");
    barVerticalColumnOptions[0].checked = 'checked';

    // Hide Bar Chart
    document.getElementById("divBarChartResults").style.display = "none";
}

/// <summary>
/// Reset Pie Chart filter & hide the graph if any
/// </summary>
function ResetPieChartFilter() {

    // Set Horizontal Column Name combobox to : Select One
    document.getElementById("ddlPieHorizontal")[0].selected = true;

    // Set Vertical Column Name combobox to : Select One
    document.getElementById("ddlPieVertical")[0].selected = true;

    $("#labelPieDateRange").attr("style", "display:none;");
    document.getElementById("ddlPieDateRange").style.display = "none";

    // Select option 1
    var pieVerticalOptions = document.getElementsByName("PieVerticalOptions");
    pieVerticalOptions[0].checked = 'checked';
    pieOption1Selected();

    //  Set columnNameSelected to "Horizontal Axis column"
    columnNameSelected = horizontalAxisColText;

    //  Append the new <span> tag containing new string based on 
    //  value selected in dropdownlist inside the tag
    //  with id == labelPieCount
    $("#labelPieCount > span").remove();
    $("#labelPieCount").append("<span>&ldquo;"
            + columnNameSelected + "&rdquo;</span>");

    // Select Aggregate
    var pieVerticalColumnOptions = document.getElementsByName("PieVerticalColumnOptions");
    pieVerticalColumnOptions[0].checked = 'checked';

    // Hide Pie Chart
    document.getElementById("divPieChartResults").style.display = "none";
}

/// <summary>
/// This function gets the current filter options set in the 
/// bar Chart view and finally shows the bar Chart
/// </summary>
function getBarChart() {
    var hashValue = createHashValue();
    drawBarChart(current_filter, current_horizontalColumnName, current_dateRange, current_verticalOptions, current_verticalColumnName, current_verticalColumnOptions)
    SetFilter(current_filter);
    if (isBookmarkedUrl == false) {
        Sys.Application.addHistoryPoint({ param: hashValue }, siteTitle);
    } else {
        isBookmarkedUrl = false;
    }
}

/// <summary>
/// Ajax call to draw Bar Chart
/// </summary>
function drawBarChart(filter, xCol, xRange, yOption, yCol, yColOption) {

    SetFilter(filter);

    if (xCol == "X" || (yOption == 'Option2' && yCol == "X")) {
        ShowBarChartError();
        return;
    }

    //  Get matched element(in variable divChart) with id == divBarChartResults
    var divChart = document.getElementById("divBarChartResults");

    //  Set overflow style for divchart to scroll
    //divChart.style.overflow = "scroll";

    //  This call takes one argument, an object of key/value pairs, 
    //  that are used to initalize and handle the request, 
    //  and returns the XMLHttpRequest object for 
    //  post-processing (like manual aborting) if needed.
    $.ajax(
    {

        //  The type of request to make ("POST" or "GET"), default is "GET".
        type: "POST",

        //  The URL to request is set here
        //  MapRoute name --> DataBrowserRunBarChart
        url: "../DataBrowserRunBarChart/",

        data: { horizontalColumnName: xCol, dateRange: xRange, verticalOptions: yOption, verticalColumnName: yCol, verticalColumnOptions: yColOption, container: container, entitySet: entitySet, filter: filter },


        //  The type of data that you're expecting 
        //  back from the server is set to html
        dataType: 'html',

        //  A function to be called if the request fails.
        error: function (error) {
            alert("drawBarChart");
            errorFunction(error, false);
        },

        //  A function to be called if the request succeeds.
        success: function (html) {
            //  If substring 'id="pError"' is present in 
            //  html data  returned from server then ...
            if (html.toString().indexOf('id="pError"', 0) >= 0) {

                //  Set display Style for tabs to block
                //  (Set matched element with id == divError VISIBLE)
                document.getElementById("divError").style.display = "block";

                //  Set html content in the tag(id == divError) with 
                //  html in "html" variable passed to this function
                $("#divError").html(html);

                //  Set display Style for tabs to none
                //  (Set matched element with id == tabs INVISIBLE)
                document.getElementById("tabs").style.display = "none";

            }
            else {

                //  Set display Style for divError to none
                //  (Set matched element with id == divError INVISIBLE)
                document.getElementById("divError").style.display = "none";

                //  Set display Style for tabs to block
                //  (Set matched element with id == tabs VISIBLE)
                document.getElementById("tabs").style.display = "block";

                //  Set visibility Style for tabs to visible
                document.getElementById("tabs").style.visibility = "visible";

                //  Set html content in the tag(id == divBarChartResults) with 
                //  html in "html" variable passed to this function
                var test = html.replace("DataBrowser/DataBrowserRunBarChart/", "");
                $("#divBarChartResults").html(test);

            }
        }
    }
    );

    //  Call to function showBarChartGraph which shows Bar Chart drawn
    showBarChartGraph();
}

/// <summary>
/// Set Bar Chart filter parameters using hash variables
/// </summary>
function SetBarChartFilter() {

    // Set ddlBarHorizontal
    var currentHzText = $("#ddlBarHorizontal > option:selected").attr("text");
    var optionsHz = $("#ddlBarHorizontal > option");
    if (hash_horizontalColumnName == undefined || hash_horizontalColumnName == defaultSelectOneText || hash_horizontalColumnName == "X") {
        optionsHz[0].selected = true;
    }
    else {
        if (currentHzText == hash_horizontalColumnName) {
            if (optionsHz[$("#ddlBarHorizontal > option:selected").attr("index")].value.split("^")[0] == "System.DateTime") {

                //  (Set matched element with id == labelBarDateRange VISIBLE)
                $("#labelBarDateRange").attr("style", "display:block;");

                //  (Set matched element with id == labelBarDateRange VISIBLE)
                $("#ddlBarDateRange").attr("style", "display:block;");

            } // If  datatype is other than "System.DateTime" then ...
            else {

                //  (Set matched element with id == labelBarDateRange INVISIBLE)
                $("#labelBarDateRange").attr("style", "display:none;");

                //  (Set matched element with id == ddlBarDateRange INVISIBLE)
                $("#ddlBarDateRange").attr("style", "display:none;");

            }
        }
        else {
            for (i = 0; i < optionsHz.length; i++) {
                if (optionsHz[i].innerText == hash_horizontalColumnName) {
                    optionsHz[i].selected = true;
                    if (optionsHz[i].value.split("^")[0] == "System.DateTime") {

                        //  (Set matched element with id == labelBarDateRange VISIBLE)
                        $("#labelBarDateRange").attr("style", "display:block;");

                        //  (Set matched element with id == labelBarDateRange VISIBLE)
                        $("#ddlBarDateRange").attr("style", "display:block;");

                    } // If  datatype is other than "System.DateTime" then ...
                    else {

                        //  (Set matched element with id == labelBarDateRange INVISIBLE)
                        $("#labelBarDateRange").attr("style", "display:none;");

                        //  (Set matched element with id == ddlBarDateRange INVISIBLE)
                        $("#ddlBarDateRange").attr("style", "display:none;");

                    }

                } else {
                    optionsHz[i].selected = false;
                }
            }
        }
    }

    // Set Label
    SetBarChartLabel();

    // Set Range
    var currentDtText = $("#ddlBarDateRange > option:selected").attr("text");
    var optionsDt = $("#ddlBarDateRange > option");
    if (currentDtText == hash_dateRange) {

    } else {
        for (i = 0; i < optionsDt.length; i++) {
            if (optionsDt[i].innerText == hash_dateRange) {
                optionsDt[i].selected = true;
            } else {
                optionsDt[i].selected = false;
            }
        }
    }

    // Set BarVerticalOptions
    var barVerticalOptions = document.getElementsByName("BarVerticalOptions");
    if (hash_verticalOptions == 'Option1') {
        barVerticalOptions[0].checked = 'checked';
        barOption1Selected();
    }
    else if (hash_verticalOptions == 'Option2') {
        barVerticalOptions[1].checked = 'checked';
        barOption2Selected();
    }

    // Set ddlBarVertical
    var currentVtText = $("#ddlBarVertical > option:selected").attr("text");
    var optionsVt = $("#ddlBarVertical > option");
    if (hash_verticalColumnName == undefined || hash_verticalColumnName == defaultSelectOneText || hash_verticalColumnName == "X") {
        optionsVt[0].selected = true;
    }
    else {
        if (currentVtText == hash_verticalColumnName) {

        }
        else {
            for (i = 0; i < optionsVt.length; i++) {
                if (optionsVt[i].innerText == hash_verticalColumnName) {
                    optionsVt[i].selected = true;
                    break;
                }
            }
        }
    }

    // Set BarVerticalColumnOptions
    var barVerticalColumnOptions = document.getElementsByName("BarVerticalColumnOptions");
    if (hash_verticalColumnOptions == 'Aggregate') {
        barVerticalColumnOptions[0].checked = 'checked';
    }
    else if (hash_verticalColumnOptions == 'Average') {
        barVerticalColumnOptions[1].checked = 'checked';
    }

    // Disable divBarChartResults
    document.getElementById("divBarChartResults").style.display = "none";
}

/// <summary>
/// Set Pie Chart filter parameters using hash variables
/// </summary>
function SetPieChartFilter() {

    var currentHzText = $("#ddlPieHorizontal > option:selected").attr("text");
    var optionsHz = $("#ddlPieHorizontal > option");
    if (hash_horizontalColumnName == undefined || hash_horizontalColumnName == defaultSelectOneText || hash_horizontalColumnName == "X") {
        optionsHz[0].selected = true;

    }
    else {
        if (currentHzText == hash_horizontalColumnName) {
            if (optionsHz[$("#ddlPieHorizontal > option:selected").attr("index")].value.split("^")[0] == "System.DateTime") {

                //  (Set matched element with id == labelBarDateRange VISIBLE)
                $("#labelPieDateRange").attr("style", "display:block;");

                //  (Set matched element with id == labelBarDateRange VISIBLE)
                $("#ddlPieDateRange").attr("style", "display:block;");

            } // If  datatype is other than "System.DateTime" then ...
            else {

                //  (Set matched element with id == labelBarDateRange INVISIBLE)
                $("#labelPieDateRange").attr("style", "display:none;");

                //  (Set matched element with id == ddlBarDateRange INVISIBLE)
                $("#ddlPieDateRange").attr("style", "display:none;");

            }
        }
        else {
            for (i = 0; i < optionsHz.length; i++) {
                if (optionsHz[i].innerText == hash_horizontalColumnName) {
                    optionsHz[i].selected = true;
                    if (optionsHz[i].value.split("^")[0] == "System.DateTime") {

                        //  (Set matched element with id == labelBarDateRange VISIBLE)
                        $("#labelPieDateRange").attr("style", "display:block;");

                        //  (Set matched element with id == labelBarDateRange VISIBLE)
                        $("#ddlPieDateRange").attr("style", "display:block;");

                    } // If  datatype is other than "System.DateTime" then ...
                    else {

                        //  (Set matched element with id == labelBarDateRange INVISIBLE)
                        $("#labelPieDateRange").attr("style", "display:none;");

                        //  (Set matched element with id == ddlBarDateRange INVISIBLE)
                        $("#ddlPieDateRange").attr("style", "display:none;");

                    }

                } else {
                    optionsHz[i].selected = false;
                }
            }
        }
    }

    /// Set Label
    SetPieChartLabel();

    // Set ddlPieDateRange
    var currentDtText = $("#ddlPieDateRange > option:selected").attr("text");
    var optionsDt = $("#ddlPieDateRange > option");
    if (currentDtText == hash_dateRange) {
    } else {
        for (i = 0; i < optionsDt.length; i++) {
            if (optionsDt[i].innerText == hash_dateRange) {
                optionsDt[i].selected = true;
            } else {
                optionsDt[i].selected = false;
            }
        }
    }

    // Set PieVerticalOptions
    var pieVerticalOptions = document.getElementsByName("PieVerticalOptions");
    if (hash_verticalOptions == 'Option1') {
        pieVerticalOptions[0].checked = 'checked';
        pieOption1Selected();
    }
    else if (hash_verticalOptions == 'Option2') {
        pieVerticalOptions[1].checked = 'checked';
        pieOption2Selected();
    }

    // Set ddlPieVertical
    var currentVtText = $("#ddlPieVertical > option:selected").attr("text");
    var optionsVt = $("#ddlPieVertical > option");
    if (hash_verticalColumnName == undefined || hash_verticalColumnName == defaultSelectOneText || hash_verticalColumnName == "X") {
        optionsVt[0].selected = true;
    }
    else {
        if (currentVtText == hash_verticalColumnName) {
        }
        else {
            for (i = 0; i < optionsVt.length; i++) {
                if (optionsVt[i].innerText == hash_verticalColumnName) {
                    optionsVt[i].selected = true;
                    break;
                }
            }
        }
    }

    // Set PieVerticalColumnOptions
    var pieVerticalColumnOptions = document.getElementsByName("PieVerticalColumnOptions");
    if (hash_verticalColumnOptions == 'Aggregate') {
        pieVerticalColumnOptions[0].checked = 'checked';
    }
    else if (hash_verticalColumnOptions == 'Average') {
        pieVerticalColumnOptions[1].checked = 'checked';
    }

    // Disable divPieChartResults
    document.getElementById("divPieChartResults").style.display = "none";
}


/// <summary>
/// This function gets the current filter options set in the 
/// Pie Chart view and finally shows the Pie Chart
/// </summary>
function getPieChart() {
    var hashValue = createHashValue();
    drawPieChart(current_filter, current_horizontalColumnName, current_dateRange, current_verticalOptions, current_verticalColumnName, current_verticalColumnOptions)
    SetFilter(current_filter);
    if (isBookmarkedUrl == false) {
        Sys.Application.addHistoryPoint({ param: hashValue }, siteTitle);
    } else {
        isBookmarkedUrl = false;
    }
}

/// <summary>
/// Ajax call to draw Pie Chart
/// </summary>
function drawPieChart(filter, xCol, xRange, yOption, yCol, yColOption) {

    SetFilter(filter);

    if (xCol == "X" || (yOption == 'Option2' && yCol == "X")) {
        ShowPieChartError();
        return;
    }

    //  This call takes one argument, an object of key/value pairs, 
    //  that are used to initalize and handle the request, 
    //  and returns the XMLHttpRequest object for 
    //  post-processing (like manual aborting) if needed.
    $.ajax(
        {

            //  The type of request to make ("POST" or "GET"), default is "GET"
            type: "POST",

            //  The URL to request is set here
            //  MapRoute name --> DataBrowserRunPieChart
            url: "../DataBrowserRunPieChart/",

            data: { horizontalColumnName: xCol, dateRange: xRange, verticalOptions: yOption, verticalColumnName: yCol, verticalColumnOptions: yColOption, container: container, entitySet: entitySet, filter: filter },

            //  The type of data that you're expecting 
            //  back from the server is set to html
            dataType: 'html',

            //  A function to be called if the request fails.
            error: function (error) {
                errorFunction(error, false);
            },

            //  A function to be called if the request succeeds.
            success: function (html) {

                //  If substring 'id="pError"' is present in 
                //  html data  returned from server then ...
                if (html.toString().indexOf('id="pError"', 0) >= 0) {

                    //  Set display Style for tabs to block
                    //  (Set matched element with id == divError VISIBLE)
                    document.getElementById("divError").style.display = "block";

                    //  Set html content in the tag(id == divError) with 
                    //  html in "html" variable passed to this function
                    $("#divError").html(html);

                    //  Set display Style for tabs to none
                    //  (Set matched element with id == tabs INVISIBLE)
                    document.getElementById("tabs").style.display = "none";

                }
                else {

                    //  Set display Style for divError to none
                    //  (Set matched element with id == divError INVISIBLE)
                    document.getElementById("divError").style.display = "none";

                    //  Set display Style for tabs to block
                    //  (Set matched element with id == tabs VISIBLE)
                    document.getElementById("tabs").style.display = "block";

                    //  Set visibility Style for tabs to visible
                    document.getElementById("tabs").style.visibility = "visible";

                    //  Set html content in the tag(id == divPieChartResults) with 
                    //  html in "html" variable passed to this function
                    var test = html.replace("DataBrowser/DataBrowserRunPieChart/", "")
                    $("#divPieChartResults").html(test);

                }
            }
        }
        );

    //  Call to function showPieChartGraph which shows Pie Chart drawn
    showPieChartGraph();
}

/// <summary>
/// This function makes copy code button visible in Data View & in Map View
/// </summary>
function clipboardReady() {

    //  Set copyDataCodeButton visible
    $("#copyDataCodeButton").css({ visibility: "visible", cursor: "pointer" });

    //  Set copyMapCodeButton visible
    $("#copyMapCodeButton").css({ visibility: "visible", cursor: "pointer" });

    //  Set copyBarChartCodeButton visible
    $("#copyBarChartCodeButton").css({ visibility: "visible", cursor: "pointer" });

    //  Set copyPieChartCodeButton visible
    $("#copyPieChartCodeButton").css({ visibility: "visible", cursor: "pointer" });

}

/// <summary>
/// This function activates Result accordion by default when 
/// user selects a tab  
/// </summary>
function tabsSelected(event, ui) {
    switch (ui.index) {
        case 0:
            current_viewType = "DataView";
            Microsoft.Maps.Events.removeHandler("onchangeview");
            break;

        case 1:
            current_viewType = "MapView";
            Microsoft.Maps.Events.addHandler(map, "onchangeview", viewHandler);
            break;

        case 2:
            current_viewType = "BarChartView";
            Microsoft.Maps.Events.removeHandler("onchangeview");
            break;

        case 3:
            current_viewType = "PieChartView";
            Microsoft.Maps.Events.removeHandler("onchangeview");
            break;
    }
    current_tagType = "Results";
    if (isBookmarkedUrl == false) {
        var hashValue = createHashValue();
        Sys.Application.addHistoryPoint({ param: hashValue }, siteTitle);
    }
}

/// <summary>
/// This function loads data in Json format for displaying it in the map
/// </summary>
function loadJson(filter) {
    Microsoft.Maps.Events.removeHandler("onchangeview");
    if (filter == noFilterText) {
        filter = '';
    }
    map.entities.clear();

    JsonQuery = document.getElementById("labelBaseQuery").title + "?";

    if (filter != defaultFilterText
        && document.getElementById("queryBox") != "") {
        JsonQuery += "$filter=" + filter;
    }

    JsonQuery += "&format=json";
    $.getJSON(JsonQuery + '&callback=?', function (data) {
        data = data.d;
        if (data.length > 0 && data[0].latitude && data[0].longitude) {
            if (whatDecimalSeparator(data[0].latitude) == ',')
                for (var i = 0; i < data.length; i++) {
                    map.entities.push(new Microsoft.Maps.Pushpin(new Microsoft.Maps.Location(parseFloat(data[i].latitude.replace(',', '.')), parseFloat(data[i].longitude.replace(',', '.')))));
                }
            else
                for (var i = 0; i < data.length; i++) {
                    map.entities.push(new Microsoft.Maps.Pushpin(new Microsoft.Maps.Location(parseFloat(data[i].latitude), parseFloat(data[i].longitude))));
                }
        }
        jsonLoaded(map.entities);
    });
}

/// <summary>
/// This function find the decimal separator used in the json file
/// </summary>
function whatDecimalSeparator(data) {
    if (data.indexOf(',') != -1)
        return ',';
    else return '.';
}

/// <summary>
/// This function
/// 1. sets custom pushpind on shape layer
/// 2. sets Map View state
/// </summary>
function jsonLoaded(data) {

    //  Get shape count
    var shapeCount = data.getLength();

    //  If shapecount is not zero(is +ve) then ...
    if (shapeCount > 0) {
        map.setView({ center: data.get(0).getLocation(), zoom: hash_zoomLevel });

        //  Set mapFullQueryUrlHyperlink text on UI with value in JsonQuery
        $("#mapFullQueryUrlHyperlink").html(JsonQuery);

        //  Set mapFullQueryUrlHyperlink HYPERLINK on UI with value in JsonQuery
        $("#mapFullQueryUrlHyperlink").attr({ href: JsonQuery });

        //  Show mapFullQueryPanel
        $("#mapFullQueryPanel").show();

        //  Hide mapNoResultsDiv
        $("#mapNoResultsDiv").hide();

    }
    else {

        //  Hide mapFullQueryPanel
        $("#mapFullQueryPanel").hide();

        //  Show mapNoResultsDiv
        $("#mapNoResultsDiv").show();

        // Hide mapManyPlacemarksDiv
        $("#mapManyPlacemarksDiv").show();

        // Hide map tab
        $("#tabs-2").hide();
        $("#liMapView").hide();
    }

    //alert("Attach");
    Microsoft.Maps.Events.addHandler(map, 'onchangeview', viewHandler);


}






/// <summary>
/// This function hides loading indicator and detaches 
/// required map control events.
/// </summary>
function hideLoadingIndicatorAndDetach() {

    try {
        //  Detach the onchangeview map control event so
        //  that it no longer calls the hideLoadingIndicatorAndDetach function.
        Microsoft.Maps.Events.removeHandler("onchangeview");

        //  Setting map style again is required for BirdsEye view since
        //  processing on BirdsEye view removes labels
        //  By resetting map style we ensure labels on the map if labels
        //  are expected to be seen on the map
        map.setMapType(hash_mapStyle);

        setTimeout(hideLoadingIndicatorAndAttachViewHandler, 3000);
    } catch (e) {
        errorFunction(mapControlError, true);
        hideLoadingIndicator();
    }


}

function hideLoadingIndicatorAndAttachViewHandler() {
    //  Hide Loading Indicator
    hideLoadingIndicator();

    Microsoft.Maps.Events.addHandler(map, "onchangeview", viewHandler);
}

/// <summary>
/// This function does nothing.
/// Its an empty function used in call map.SetBirdseyeScene(...,callback)
/// </summary>
function dummyCallback() {
}

/// <summary>
/// This function copies the sample code in the clipboard for Data View
/// </summary>
function copyDataCode() {

    //  Get inner html(in variable htmlContent) 
    //  of matched element(id == divDataViewSampleCode)
    htmlContent = $("#divDataViewSampleCode").html();

    //  Get html content with stripped HTML tags
    textContent = stripHTML(htmlContent);
    $.clipboard(textContent);

    //  Hide copyDataCodeButton in fast speed
    $('#copyDataCodeButton').animate({ opacity: "hide" }, "fast");

    // Show copyDataCodeButton in slow speed
    $('#copyDataCodeButton').animate({ opacity: "show" }, "slow");

}

/// <summary>
/// This function copies the sample code in the clipboard for Map View
/// </summary>
function copyMapCode() {

    //  Get inner html(in variable htmlContent)
    //  of matched element(id == divMapViewSampleCode)
    htmlContent = $("#divMapViewSampleCode").html();

    //  Get html content with stripped HTML tags
    textContent = stripHTML(htmlContent);
    $.clipboard(textContent);

    //  Hide copyMapCodeButton in fast speed
    $('#copyMapCodeButton').animate({ opacity: "hide" }, "fast");

    // Show copyMapCodeButton in slow speed
    $('#copyMapCodeButton').animate({ opacity: "show" }, "slow");

}

/// <summary>
/// This function copies the sample code in the clipboard for Bar Chart View
/// </summary>
function copyBarChartCode() {

    //  Get inner html(in variable htmlContent)
    //  of matched element(id == divBarChartViewSampleCode)
    htmlContent = $("#divBarChartViewSampleCode").html();

    //  Get html content with stripped HTML tags
    textContent = stripHTML(htmlContent);
    $.clipboard(textContent);

    //  Hide copyBarChartCodeButton in fast speed
    $('#copyBarChartCodeButton').animate({ opacity: "hide" }, "fast");

    // Show copyBarChartCodeButton in slow speed
    $('#copyBarChartCodeButton').animate({ opacity: "show" }, "slow");

}

/// <summary>
/// This function copies the sample code in the clipboard for Pie Chart view
/// </summary>
function copyPieChartCode() {

    //  Get inner html(in variable htmlContent)
    //  of matched element(id == divPieChartViewSampleCode)
    htmlContent = $("#divPieChartViewSampleCode").html();

    //  Get html content with stripped HTML tags
    textContent = stripHTML(htmlContent);
    $.clipboard(textContent);

    //  Hide copyPieChartCodeButton in fast speed
    $('#copyPieChartCodeButton').animate({ opacity: "hide" }, "fast");

    // Show copyPieChartCodeButton in slow speed
    $('#copyPieChartCodeButton').animate({ opacity: "show" }, "slow");

}

/// <summary>
/// This function strips HTML tags from the given stream
/// ( Based on code 
/// from http://www.javascriptkit.com/script/script2/removehtml.shtml )
/// </summary>
function stripHTML(s) {

    //  Replace closing <PRE> tags with CR/LF pair
    s = s.replace(/<\/PRE>/g, "\r\n");

    //  Replace non-breaking space with CR/LF pair
    s = s.replace(/&nbsp;/g, "\r\n");

    //  Remove double-spaced lines
    s = s.replace(/\r\n\r\n/g, "\r\n");

    //  Remove all remaining HTML tags
    var re = /<\S[^><]*>/g;
    s = s.replace(re, "");

    s = s.replace(/&amp;/g, "&");
    s = s.replace(/&lt;/g, "<");
    s = s.replace(/&gt;/g, ">");
    s = s.replace(/&nbsp;&nbsp;&nbsp;/g, "\t");
    s = s.replace(/<br \/>/g, "\r\n");

    return s;
}

/// <summary>
/// This function gets executed when user
/// selects Option 1 from Vertical Axis group
/// for Bar Chart View
/// </summary>
function barOption1Selected() {
    enableOption1Controls();
}

/// <summary>
/// This function gets executed when user 
/// selects Option 2 from Vertical Axis group
/// for Bar Chart View
/// </summary>
function barOption2Selected() {
    enableOption2Controls();
}

/// <summary>
/// This function gets executed when user 
/// selects Option 1 from Vertical Axis group
/// for Pie Chart View
/// </summary>
function pieOption1Selected() {
    enablePieOption1Controls();
}

/// <summary>
/// This function gets executed when user
/// selects Option 2 from Vertical Axis group
/// for Pie Chart View
/// </summary>
function pieOption2Selected() {
    enablePieOption2Controls();
}

/// <summary>
/// This function enables Option 1 controls 
/// for Bar Chart View
/// </summary>
function enableOption1Controls() {

    //  Get matched element(in variable VerticalColumnOptions) 
    //  with Name = BarVerticalColumnOptions  
    var VerticalColumnOptions =
        document.getElementsByName("BarVerticalColumnOptions");

    //  For i less than number of vertical column options do ...
    for (i = 0; i < VerticalColumnOptions.length; i++) {
        VerticalColumnOptions[i].disabled = true;
    }

    //  Disable dropdownlist with id == ddlBarVertical
    $("#ddlBarVertical").attr('disabled', 'disabled');

    //  Make color of label(with id == labelBarAggregate) GRAY.
    $("#labelBarAggregate").attr('style', 'color:Gray');

    //  Make color of label(with id == labelBarAverage) GRAY.
    $("#labelBarAverage").attr('style', 'color:Gray');

    //  Make color of label(with id == VerticalComboBoxLabel) GRAY.
    $("#VerticalComboBoxLabel").attr('style', 'color:Gray');

    //  Remove style applied to label with id == labelBarCount
    $("#labelBarCount").removeAttr('style');
}

/// <summary>
/// This function enables Option 1 controls
/// for Pie Chart View
/// </summary>
function enablePieOption1Controls() {

    //  Get matched element(in variable pieVerticalColumnOptions) 
    //  with Name = PieVerticalColumnOptions
    var pieVerticalColumnOptions
        = document.getElementsByName("PieVerticalColumnOptions");

    //  For i less than number of vertical column options do ...
    for (i = 0; i < pieVerticalColumnOptions.length; i++) {
        pieVerticalColumnOptions[i].disabled = true;
    }

    //  Disable dropdownlist with id == ddlPieVertical
    $("#ddlPieVertical").attr('disabled', 'disabled');


    //  Make color of label(with id == labelPieAggregate) GRAY.
    $("#labelPieAggregate").attr('style', 'color:Gray');

    //  Make color of label(with id == labelPieAverage) GRAY.
    $("#labelPieAverage").attr('style', 'color:Gray');

    //  Make color of label(with id == labelPieVerticalComboBox) GRAY.
    $("#labelPieVerticalComboBox").attr('style', 'color:Gray');

    //  Remove style applied to label with id == labelPieCount
    $("#labelPieCount").removeAttr('style');

}

/// <summary>
/// This function Option 2 controls
/// for Bar Chart View
/// </summary>
function enableOption2Controls() {

    //  Get matched element(in variable VerticalColumnOptions) 
    //  with Name = BarVerticalColumnOptions
    var VerticalColumnOptions
        = document.getElementsByName("BarVerticalColumnOptions");

    //  For i less than number of vertical column options do ...
    for (i = 0; i < VerticalColumnOptions.length; i++) {
        VerticalColumnOptions[i].disabled = false;
    }

    //  Remove 'disabled' attribute of matched element
    $("#ddlBarVertical").removeAttr('disabled');

    //  Remove style applied to label with id == labelBarAggregate
    $("#labelBarAggregate").removeAttr('style');

    //  Remove style applied to label with id == labelBarAverage
    $("#labelBarAverage").removeAttr('style');

    //  Remove style applied to label with id == VerticalComboBoxLabel
    $("#VerticalComboBoxLabel").removeAttr('style');

    //  Make color of label(with id == labelBarCount) GRAY.
    $("#labelBarCount").attr('style', 'color:Gray');

}

/// <summary>
/// This function enables Option 2 controls
/// for Pie Chart View
/// </summary>
function enablePieOption2Controls() {

    //  Get matched element(in variable pieVerticalColumnOptions) 
    //  with Name == PieVerticalColumnOptions
    var pieVerticalColumnOptions
        = document.getElementsByName("PieVerticalColumnOptions");

    //  For i less than number of vertical column options do ...
    for (i = 0; i < pieVerticalColumnOptions.length; i++) {
        pieVerticalColumnOptions[i].disabled = false;
    }

    //  Remove 'disabled' attribute of matched element
    $("#ddlPieVertical").removeAttr('disabled');

    //  Remove style applied to label with id == labelPieAggregate
    $("#labelPieAggregate").removeAttr('style');

    //  Remove style applied to label with id == labelPieAverage
    $("#labelPieAverage").removeAttr('style');

    //  Remove style applied to label with id == labelPieVerticalComboBox
    $("#labelPieVerticalComboBox").removeAttr('style');

    //  Make color of label(with id == labelPieCount) GRAY.
    $("#labelPieCount").attr('style', 'color:Gray');

}

/// <summary>
/// This function enables/disables Date Range combo box 
/// according to selectedvalue passed to it
/// </summary>
function horizontalBarComboBoxSelected(selectedValue) {

    var index = selectedValue.toString().indexOf("^", 0);

    //  Get the datatype of value selected in dropdownlist
    var datatype = selectedValue.toString().substring(0, index);

    //  If datatype is  "System.DateTime" then ...
    if (datatype == "System.DateTime") {

        //  (Set matched element with id == labelBarDateRange VISIBLE)
        $("#labelBarDateRange").attr("style", "display:block;");

        //  (Set matched element with id == labelBarDateRange VISIBLE)
        $("#ddlBarDateRange").attr("style", "display:block;");

    } // If  datatype is other than "System.DateTime" then ...
    else {

        //  (Set matched element with id == labelBarDateRange INVISIBLE)
        $("#labelBarDateRange").attr("style", "display:none;");

        //  (Set matched element with id == ddlBarDateRange INVISIBLE)
        $("#ddlBarDateRange").attr("style", "display:none;");

    }
    SetBarChartLabel();
}

/// <summary>
/// This function set Bar Chart label of Number Of occurances of "<ColumnName>" 
/// to respective value.
/// </summary>
function SetBarChartLabel() {
    //  Remove the <span> tag from tag with id == labelBarCount
    $("#labelBarCount > span").remove();

    //  Get selected value for the dropdownlist(id == ddlBarHorizontal)
    var columnNameSelected = $("#ddlBarHorizontal > option:selected").text();

    //  If dropdownlist has selected watermark then ...
    if (columnNameSelected == defaultSelectOneText) {

        //  Set columnNameSelected to "Horizontal Axis column"
        columnNameSelected = horizontalAxisColText;

    } //  If dropdownlist has selected value other than watermark
        // then ...
    else {

        // If columnNameSelected has length more than 20 chars then ...
        if (columnNameSelected.length > 20) {

            //  Select those 20 chars and append two dots to that string
            columnNameSelected = columnNameSelected.substring(0, 20) + "..";

        }
    }

    //  Append the new <span> tag containing new string based on 
    //  value selected in dropdownlist inside the tag
    //  with id == labelBarCount
    $("#labelBarCount").append("<span>&ldquo;"
            + columnNameSelected + "&rdquo;</span>");
}

/// <summary>
/// This function enables/disables Date Range combo box 
/// according to selectedvalue passed to it
/// </summary>
function horizontalPieComboBoxSelected(selectedValue) {
    var index = selectedValue.toString().indexOf("^", 0);

    //  Get the datatype of value selected in dropdownlist
    var datatype = selectedValue.toString().substring(0, index);

    //  If datatype is  "System.DateTime" then ...
    if (datatype == "System.DateTime") {

        //  (Set matched element with id == labelPieDateRange VISIBLE)
        $("#labelPieDateRange").attr("style", "display:block;");

        //  (Set matched element with id == ddlPieDateRange VISIBLE)
        $("#ddlPieDateRange").attr("style", "display:block;");

    } // If  datatype is other than "System.DateTime" then ... 
    else {

        //  (Set matched element with id == labelPieDateRange INVISIBLE)
        $("#labelPieDateRange").attr("style", "display:none;");

        //  (Set matched element with id == ddlPieDateRange INVISIBLE)
        $("#ddlPieDateRange").attr("style", "display:none;");

    }
    SetPieChartLabel();
}

/// <summary>
/// This function sets Pie Chart label of Number Of occurances of "<ColumnName>" 
/// to respective value.
/// </summary>
function SetPieChartLabel() {
    //  Remove the <span> tag from tag with id == labelPieCount
    $("#labelPieCount > span").remove();


    //  Get selected value for the dropdownlist(id == ddlPieHorizontal)
    var columnNameSelected = $("#ddlPieHorizontal > option:selected").text();

    //  If dropdownlist has selected watermark then ...
    if (columnNameSelected == defaultSelectOneText) {

        //  Set columnNameSelected to "Horizontal Axis column"
        columnNameSelected = horizontalAxisColText;

    } //  If dropdownlist has selected value other than watermark
        // then ...
    else {

        // If columnNameSelected has length more than 20 chars then ...
        if (columnNameSelected.length > 20) {

            //  Select those 20 chars and append two dots to that string
            columnNameSelected = columnNameSelected.substring(0, 20) + "..";

        }

    }

    //  Append the new <span> tag containing new string based on 
    //  value selected in dropdownlist inside the tag
    //  with id == labelPieCount
    $("#labelPieCount").append("<span>&ldquo;"
        + columnNameSelected + "&rdquo;</span>");

}

/// <summary>
/// This function shows the ERROR in Bar Chart view when 
/// user input for Bar chart creation is invalid
/// </summary>
function ShowBarChartError() {

    //  Show bar chart ERROR <div>    
    document.getElementById("divBarChartError").style.display = "block";

    //  Hide bar chart RESULT <div>
    document.getElementById("divBarChartResults").style.display = "none";

}

/// <summary>
/// This function shows the ERROR in Pie Chart view when user 
/// input for Pie chart creation is invalid
/// </summary>
function ShowPieChartError() {

    //  Show pie chart ERROR <div>
    document.getElementById("divPieChartError").style.display = "block";

    //  Hide pie chart RESULT <div>
    document.getElementById("divPieChartResults").style.display = "none";
}

/// <summary>
/// This function shows Bar Chart graph and before that it hides 
/// bar chart ERROR <div>
/// </summary>
function showBarChartGraph() {

    //  Hide bar chart ERROR <div>
    document.getElementById("divBarChartError").style.display = "none";

    //  Show bar chart RESULT <div>    
    document.getElementById("divBarChartResults").style.display = "block";

}

/// <summary>
/// This function shows Pie Chart graph. and before that it hides 
/// pie chart ERROR <div>
/// </summary>
function showPieChartGraph() {

    //  Hide pie chart ERROR <div>
    document.getElementById("divPieChartError").style.display = "none";

    //  Show pie chart RESULT <div>
    document.getElementById("divPieChartResults").style.display = "block";

    //  If browser is Netscape then ...
    if (navigator.appName == "Netscape") {

        //  Set height style to 100%( This is a hack where height=100% 
        //  does not work for Netscape browsers )
        document.getElementById("divPieResultHeight").style.height = "100%";

    }

}

/// <summary>
/// This function gets & returns current Filter from the filter text area
/// </summary>
function GetFilter() {

    //  Set value of filter variable to value in textarea with id == queryBox
    var filter = $("#queryBox").val().replace(/^\s+|\s+$/g, "");

    //  If substring 'additional $filter parameters...' is present in 
    //  filter query ...
    if (filter.indexOf(defaultFilterText, 0) >= 0) {

        //  Set filter variable to empty string
        filter = noFilterText;

    }
    return filter;
}

/// <summary>
/// This function sets current Filter in the filter textarea
/// </summary>
function SetFilter(filter) {
    if (filter == noFilterText) {
        filter = defaultFilterText;
    }
    document.getElementById("queryBox").value = filter;
}

/// <summary>
/// This function shows Loading Indicator on the page
/// </summary>
function showLoadingIndicator() {

    //  Get height of body of this page
    var bodyHeight = getBodyHeight();

    //  Get width of body of this page
    var bodyWidth = getBodyWidth();

    //  Set a 'style' property to a value, for all matched 
    //  elements(id == BackGroundLoadingIndicator).
    $("#BackGroundLoadingIndicator").attr(
                    "style",
                    "display:block;height:" + bodyHeight + "px;width:" + bodyWidth + "px");

    //  Set a 'style' property to a value, for all matched 
    //  elements(id == LoadingIndicatorPanel).
    //  (Set matched element with id == LoadingIndicatorPanel VISIBLE)
    $("#LoadingIndicatorPanel").attr("style", "display:block;");

    //  Set a 'style' property to a value, for all matched 
    //  elements(id == imgLoading).
    //  (Set matched element with id == imgLoading VISIBLE)
    $("#imgLoading").attr("style", "display:block;");

}

/// <summary>
/// This function hides Loading Indicator on the page
/// </summary>
function hideLoadingIndicator() {

    //  Set a 'style' property to a value, for all matched 
    //  elements(id == LoadingIndicatorPanel).
    $("#LoadingIndicatorPanel").attr("style", "display:none;");

    //  Set a 'style' property to a value, for all matched 
    //  elements(id == BackGroundLoadingIndicator).
    $("#BackGroundLoadingIndicator").attr("style", "display:none;");

    //  Set a 'style' property to a value, for all matched 
    //  elements(id == imgLoading).
    $("#imgLoading").attr("style", "display:none;");

}

/// <summary>
/// This function returns the calculated width of body 
/// of this page
/// </summary>
function getBodyWidth() {

    //  Initialize width to zero
    var width = 0;

    //  Initialize scrollWidth to zero
    var scrollWidth = 0;

    //  Initialize offsetWidth to zero
    var offsetWidth = 0;

    //  Initialize maxWidth to zero
    var maxWidth = 0;

    //  If document.width has some value then ...
    if (document.width) {

        //  Set width to document.width
        width = document.width;

    } //  If document.body has some value then ...
    else if (document.body) {

        //  If document.body.scrollWidth has some value then ...
        if (document.body.scrollWidth) {

            //  Set width & scrollWidth to document.body.scrollWidth
            width = scrollWidth = document.body.scrollWidth;

        }

        //  If document.body.offsetWidth has some value then ...
        if (document.body.offsetWidth) {

            //  Set width & offsetWidth to document.body.offsetWidth
            width = offsetWidth = document.body.offsetWidth;

        }

        //  If scrollWidth & offsetWidth 
        //  both have some value then ...
        if (scrollWidth && offsetWidth) {

            //  Set width & maxWidth to maximum value 
            //  among scrollWidth & offsetWidth
            width = maxWidth = Math.max(scrollWidth, offsetWidth);

            // If scrollWidth & offsetWidth are different then ...
            if (scrollWidth != offsetWidth) {

                // Manupulate width to cover entire body with mask
                width = width * (1003 / 936);

            }
            // If screen.width has some value then ...
            if (screen.width) {

                //  Set width to maximum value
                //  among width & screen.width
                width = maxWidth = Math.max(width, screen.width);
            }

        }
    }

    //  If browser is Microsoft Internet Explorer then ...
    if (navigator.appName == "Microsoft Internet Explorer" || navigator.appName == "Opera") {
        return width;
    } //  If browser is Netscape then ...
    else if (navigator.appName == "Netscape") {

        //  Set width to maximum value
        //  among screen.width & window.innerWidth
        return Math.max(screen.width, window.innerWidth);

    }
    else {
        return width;
    }
}

/// <summary>
/// This function returns the calculated height of body 
/// of this page
/// </summary>
function getBodyHeight() {

    //  Initialize height to zero
    var height = 0;

    //  Initialize scrollHeight to zero
    var scrollHeight = 0;

    //  Initialize offsetHeight to zero
    var offsetHeight = 0;

    //  Initialize maxHeight to zero
    var maxHeight;

    //  If document.height has some value then ...
    if (document.height) {

        //  Set height to document.height
        height = document.height;

    } //  If document.body has some value then ...
    else if (document.body) {

        //  If document.body.scrollHeight has some value then ...
        if (document.body.scrollHeight) {

            //  Set height & scrollHeight to document.body.scrollHeight
            height = scrollHeight = document.body.scrollHeight;
        }

        //  If document.body.offsetHeight has some value then ...
        if (document.body.offsetHeight) {

            //  Set height & offsetHeight to document.body.offsetHeight
            height = offsetHeight = document.body.offsetHeight;
        }


        //  If scrollHeight & offsetHeight 
        //  both have some value then ...
        if (scrollHeight && offsetHeight) {

            //  Set height & maxHeight to maximum value
            //  among scrollHeight & offsetHeight
            height = maxHeight = Math.max(scrollHeight, offsetHeight);

        }
    }

    //  If browser is Microsoft Internet Explorer then ...
    if (navigator.appName == "Microsoft Internet Explorer") {
        return height;
    } //  If browser is NOT Microsoft Internet Explorer then ...
    else {

        //  width to be returned is the available height
        //  multiplied by
        //  ratio of device-logical YDPIs
        return (window.screen.availHeight) * (screen.deviceYDPI / screen.logicalYDPI);

    }

}

//  On window resize call function setLayerPosition
window.onresize = setLayerPosition;

/// <summary>
/// This function resets the position of loading Indicator mask
/// using new height & width values
/// </summary>
function setLayerPosition() {

    //  Get matched element(in variable backGroundLoadingIndicator) 
    //  with id == BackGroundLoadingIndicator
    var backGroundLoadingIndicator
        = document.getElementById("BackGroundLoadingIndicator");

    //  Set width style of backGroundLoadingIndicator to new width
    backGroundLoadingIndicator.style.width = getBodyWidth() + "px";

    //  Set height style of backGroundLoadingIndicator to new height
    backGroundLoadingIndicator.style.height = getBodyHeight() + "px";

}

/// <summary>
/// This function reads Bar chart filter parameters into current variables
/// </summary>
function getBarChartParametersInCurrentVariables() {
    current_horizontalColumnName = $("#ddlBarHorizontal > option:selected").attr("text");
    if (current_horizontalColumnName == undefined || current_horizontalColumnName == defaultSelectOneText) {
        current_horizontalColumnName = "X";
    }
    var xDateRange = document.getElementById("ddlBarDateRange");
    if (xDateRange.style.display != "block") {
        current_dateRange = "X";
    }
    else {
        current_dateRange = $("#ddlBarDateRange > option:selected").attr("text");
    }
    var VerticalOptions = document.getElementsByName("BarVerticalOptions");
    current_verticalOptions = "";
    for (i = 0; i < VerticalOptions.length; i++) {
        if (VerticalOptions[i].checked) {
            current_verticalOptions = VerticalOptions[i].value;
            break;
        }
    }
    current_verticalColumnName = $("#ddlBarVertical > option:selected").attr("text");
    if (current_verticalColumnName == undefined || current_verticalColumnName == defaultSelectOneText) {
        current_verticalColumnName = "X";
    }
    var VerticalColumnOptions = document.getElementsByName("BarVerticalColumnOptions");
    current_verticalColumnOptions = "";
    for (i = 0; i < VerticalColumnOptions.length; i++) {
        if (VerticalColumnOptions[i].checked) {
            current_verticalColumnOptions = VerticalColumnOptions[i].value;
            break;
        }
    }
}

/// <summary>
/// This function reads Pie chart filter parameters into current variables
/// </summary>
function getPieChartParametersInCurrentVariables() {
    current_horizontalColumnName = $("#ddlPieHorizontal > option:selected").attr("text");
    if (current_horizontalColumnName == undefined || current_horizontalColumnName == defaultSelectOneText) {
        current_horizontalColumnName = "X";
    }
    var xDateRange = document.getElementById("ddlPieDateRange");
    if (xDateRange.style.display != "block") {
        current_dateRange = "X";
    }
    else {
        current_dateRange = $("#ddlPieDateRange > option:selected").attr("text");
    }
    var pieVerticalOptions = document.getElementsByName("PieVerticalOptions");
    current_verticalOptions = "";
    for (i = 0; i < pieVerticalOptions.length; i++) {
        if (pieVerticalOptions[i].checked) {
            current_verticalOptions = pieVerticalOptions[i].value;
            break;
        }
    }
    current_verticalColumnName = $("#ddlPieVertical > option:selected").attr("text");
    if (current_verticalColumnName == undefined || current_verticalColumnName == defaultSelectOneText) {
        current_verticalColumnName = "X";
    }
    var pieVerticalColumnOptions
        = document.getElementsByName("PieVerticalColumnOptions");
    current_verticalColumnOptions = "";
    for (i = 0; i < pieVerticalColumnOptions.length; i++) {
        if (pieVerticalColumnOptions[i].checked) {
            current_verticalColumnOptions = pieVerticalColumnOptions[i].value;
            break;
        }
    }
}

/// <summary>
/// This function can be used by developers for checking the the status of current and 
/// hash variables at any point of time 
/// </summary>
function debug() {
    alert("\nhash_filter-->" + hash_filter
    + "\nhash_viewType-->" + hash_viewType
    + "\nhash_tagType-->" + hash_tagType
    + "\nhash_language-->" + hash_language
    + "\nhash_horizontalColumnName-->" + hash_horizontalColumnName
    + "\nhash_dateRange-->" + hash_dateRange
    + "\nhash_verticalOptions-->" + hash_verticalOptions
    + "\nhash_verticalColumnName-->" + hash_verticalColumnName
    + "\nhash_verticalColumnOptions-->" + hash_verticalColumnOptions
    + "\n\n"
    + "\nhash_mapStyle-->" + hash_mapStyle
    + "\nhash_zoomLevel-->" + hash_zoomLevel
    + "\nhash_longitude-->" + hash_longitude
    + "\nhash_latitude-->" + hash_latitude
    + "\nhash_sceneId-->" + hash_sceneId
    + "\nhash_birdseyeSceneOrientation-->" + hash_birdseyeSceneOrientation
    + "\n\n"
    + "\ncurrent_filter-->" + current_filter
    + "\ncurrent_viewType-->" + current_viewType
    + "\ncurrent_tagType-->" + current_tagType
    + "\ncurrent_language-->" + current_language
    + "\ncurrent_horizontalColumnName-->" + current_horizontalColumnName
    + "\ncurrent_dateRange-->" + current_dateRange
    + "\ncurrent_verticalOptions-->" + current_verticalOptions
    + "\ncurrent_verticalColumnName-->" + current_verticalColumnName
    + "\ncurrent_verticalColumnOptions-->" + current_verticalColumnOptions
    + "\n\n"
    + "\ncurrent_mapStyle-->" + current_mapStyle
    + "\ncurrent_zoomLevel-->" + current_zoomLevel
    + "\ncurrent_longitude-->" + current_longitude
    + "\ncurrent_latitude-->" + current_latitude
    + "\ncurrent_sceneId-->" + current_sceneId
    + "\ncurrent_birdseyeSceneOrientation-->" + current_birdseyeSceneOrientation
    );
}
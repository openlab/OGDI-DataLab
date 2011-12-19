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

/// <summary>
/// Everything inside this function will load as soon as 
/// the DOM is loaded and before the page contents are loaded
/// </summary>
$(document).ready(function() {

    //  This is executed when Ajax calls begin
    $("#LoadingIndicatorPanel").ajaxStart(function() {

        //  Call to showLoadingIndicator function
        showLoadingIndicator();

    });

    //  This is executed when Ajax calls Stop
    $("#LoadingIndicatorPanel").ajaxStop(function() {

        //  Call to hideLoadingIndicator function
        hideLoadingIndicator();

    });

    //  This is executed when selected index for dropdown list is changed
    $("select#Containers").change(function() {

        //  Call to function dropDownListSelectedIndexChanged which 
        //  loads categories in left panel for selected container
        dropDownListSelectedIndexChanged();

    });

    //  This is executed when user clicks on 1st paragraph on this page
    $("#Para1").click(function() {

        //  Slide toggle Para2 panel with 350 ms delay
        $(this).next("#Para2").slideToggle(350)

        return false;

    });

    //  This is executed when slideToggle function for 
    //  1st paragraph is executed
    $('#Para1').toggle(
               function() {

                   //  Change Expand Collapse image to CollapserImage
                   $('#ExpandCollapseImage').attr('src', CollapserImage);

                   //  Change title of that image to "Hide Details"
                   $('#ExpandCollapseImage').attr('title', "Hide Details");

                   //  Change alt of that image to "Hide Details"
                   $('#ExpandCollapseImage').attr('alt', "Hide Details");

               }, function() {

                   //  Change Expand/Collapse image to ExpanderImage
                   $('#ExpandCollapseImage').attr('src', ExpanderImage);

                   //  Change title of that image to "Show Details"
                   $('#ExpandCollapseImage').attr('title', "Show Details");

                   //  Change alt of that image to "Show Details"
                   $('#ExpandCollapseImage').attr('alt', "Show Details");

               });
});

/// <summary>
/// This function initialises the elements on the page
/// </summary>
function page_init() {

    //  Add event which occurs when the user clicks the browser's Back or Forward button.
    Sys.Application.add_navigate(onStateChanged);

    //  Get current URL hash value
    var currentHash = window.location.hash;

    //  If hash value is "#" or empty then ...
    if (currentHash == "#" || currentHash == "") {
        var containerAlias = $("#Containers > option:selected").attr("value");

        // Create a history point and add it to the browser's history stack.
        Sys.Application.addHistoryPoint({ Container: containerAlias, Category: 'All' }, siteTitle);
        return;
    }

    var hash = currentHash.split("&");
    var containerIndex = hash[0].indexOf("=");
    var containerAlias = hash[0].substr(containerIndex + 1);

    var categoryIndex = hash[1].indexOf("=");
    var categoryName = hash[1].substr(categoryIndex + 1);

    var loadedCategory = getLoadedCategory();
    var loadedContainber = document.getElementById("Containers").value;

    if (loadedContainber != containerAlias || loadedCategory != categoryName) {
        loadCategoriesAndEntitySets(containerAlias, categoryName);
    }
}

/// <summary>
/// This function gets the category loaded on page..
/// </summary>
function getLoadedCategory() {
    var loadedCategory = "";
    if (document.getElementById("CategoryNamepanel")) {
        loadedCategory = document.getElementById("CategoryNamepanel").innerHTML;
        if (loadedCategory.indexOf("All Available Data Sets") > -1) {
            loadedCategory = "All";
        }
    }
    return loadedCategory;
}

/// <summary>
/// This function loads categories and Entitysets on the page
/// </summary>
function loadCategoriesAndEntitySets(containerAlias, categoryName) {
    document.getElementById("Containers").value = containerAlias;
    // Load categories
    $.ajax({

        //  The type of request to make ("POST" or "GET"), default is "GET".
        type: "POST",

        //  The URL to request is set here
        url: "DataCatalog/LoadDataCatalogMapRoute/" + containerAlias + "/" + categoryName,

        //  The type of data that you're expecting
        //  back from the server is set to html
        dataType: 'html',

        //  A function to be called if the request succeeds.
        success: function(html) {

            //  Set html content in the tag(id == leftPanelDiv) with 
            //  html in "html" variable passed to this function
            $("#leftPanelDiv").html(html);

            //  Call to function categoryLinkBold which makes clicked category bold
            categoryLinkBold(categoryName);
        }
    });
}

/// <summary>
/// This function shows Legal Disclaimer dialog 
/// with Legal Disclaimer content in it
/// </summary>
function legalDisclaimerLinkClicked() {

    //  Get selected value for the dropdownlist(id == Containers)
    var containerAlias = $("#Containers > option:selected").attr("value");

    //  This call takes one argument, an object of key/value pairs, 
    //  that are used to initalize and handle the request, 
    //  and returns the XMLHttpRequest object for 
    //  post-processing (like manual aborting) if needed.
    $.ajax({

        //  The type of request to make ("POST" or "GET"), default is "GET".
        type: "GET",

        //  For sending data to the server, we use this content-type.
        contentType: "application/json; charset=utf-8",

        //  The URL to request is set here
        url: "DataCatalog/LoadLegalDisclaimerMapRoute/" + containerAlias,
        data: "{}",

        //  The type of data that you're expecting
        //  back from the server is set to json
        dataType: "json",

        //  A function to be called if the request succeeds.
        success: function(data) {

            //  Remove <p> tag(s) from tag with id == disclaimerBody
            $('#disclaimerBody > p').remove();

            //  If data is not empty then ...
            if (data.length > 0) {

                //  For each legalDisclaimer in Data
                //  (POINT TO BE NOTED : Every Container will always 
                //  have only one Legal Disclaimer)
                for (legalDisclaimer in data) {

                    //  Append content to the inside of 
                    //  every matched element(id == disclaimerBody).
                    $("#disclaimerBody").append(
                            "<p>" + data[legalDisclaimer] + "</p>");

                }
            }

            //  Set disclaimer popup dialog properties
            $("#disclaimerPopup").dialog({

                //  The width of the dialog, in pixels.
                width: 530,

                //  When autoOpen is true the dialog will open 
                //  automatically when dialog is called. 
                //  If false it will stay hidden 
                //  until .dialog("open") is called on it.
                autoOpen: false,

                //  The effect to be used when the dialog is opened.
                show: 'slide',

                //  If set to true, the dialog will have modal behavior; 
                //  other items on the page will be disabled
                modal: true,

                //  Specifies where the dialog should be displayed.
                position: 'center',

                //  If set to true, the dialog will be resizable.
                resizable: false,

                //  This event is triggered when a dialog attempts to close. 
                //  If the beforeclose event handler (callback function) 
                //  returns false, the close will be prevented.
                beforeclose: function(event, ui) {

                    //  Remove 'disabled' attribute of matched 
                    //  element(id == Containers)
                    //  (Enable Containers dropdownlist)
                    //$('#Containers').removeAttr('disabled');

                }
            });

            //  Set disclaimerPopup dialog CSS properties
            $("#disclaimerPopup").css({ visibility: "visible", cursor: "pointer" });

            //  Open disclaimerPopup Dialog
            $('#disclaimerPopup').dialog('open');

            //  Disable Containers dropdownlist
            //$('#Containers').attr('disabled', true);

        }
    });
}

/// <summary>
/// This function changes status of DataCatalog page if 
/// URL hash status is different than the current status of page
/// </summary>
function onStateChanged(sender, e) {
    var containerAlias = e.get_state().Container;
    var categoryName = e.get_state().Category;

    if (containerAlias == "" || containerAlias == undefined) {
        containerAlias = $("#Containers > option:selected").attr("value");
    }

    if (categoryName == "" || categoryName == undefined) {
        categoryName = "All";
    }

    var loadedCategory = getLoadedCategory();
    var loadedContainer = document.getElementById("Containers").value;

    if (loadedContainer != containerAlias || loadedCategory == "") {
        loadCategoriesAndEntitySets(containerAlias, categoryName);
    }
    else if (loadedCategory != categoryName) {

        //  This call takes one argument, an object of key/value pairs, 
        //  that are used to initalize and handle the request, 
        //  and returns the XMLHttpRequest object for 
        //  post-processing (like manual aborting) if needed.
        $.ajax({

            //  The type of request to make ("POST" or "GET"), default is "GET".
            type: "POST",

            //  The URL to request is set here
            url: "DataCatalog/LoadDataCatalogEntitySets/" + containerAlias + "/" + categoryName,

            //  The type of data that you're expecting
            //  back from the server is set to html
            dataType: 'html',

            //  A function to be called if the request succeeds.
            success: function(html) {

                //  Set html content in the tag(id == rightPanelDiv) with 
                //  html in "html" variable passed to this function
                $("#rightPanelDiv").html(html);

                //  Call to function categoryLinkBold which makes clicked category bold
                categoryLinkBold(categoryName);
            }
        });
    }
}


/// <summary>
/// This function loads categories in left panel for selected container & it loads
/// all entitysets for that container in respective panel and finally it
/// adds new page status to the browser's history stack
/// </summary>
function dropDownListSelectedIndexChanged() {

    //  Get selected value for the dropdownlist(id == Containers)
    var containerAlias = $("#Containers > option:selected").attr("value");

    //  This call takes one argument, an object of key/value pairs, 
    //  that are used to initalize and handle the request,
    //  and returns the XMLHttpRequest object for
    //  post-processing (like manual aborting) if needed.
    $.ajax({

        //  The type of request to make ("POST" or "GET"), default is "GET".
        type: "POST",

        //  The URL to request is set here
        url: "DataCatalog/LoadDataCatalogMapRoute/" + containerAlias,

        //  The type of data that you're expecting
        //  back from the server is set to html
        dataType: 'html',

        //  A function to be called if the request succeeds.
        success: function(html) {

            //  Set html content in the tag(id == leftPanelDiv) with 
            //  html in "html" variable passed to this function
            $("#leftPanelDiv").html(html);

            // Create a history point and add it to the browser's history stack.
            Sys.Application.addHistoryPoint({ Container: containerAlias, Category: 'All' }, siteTitle);
        }
    }
    );
}

/// <summary>
/// This function adds new page status to the browser's history stack when user clicks on any category
/// </summary>
function categoryLinkClicked(categoryName) {

    //  Get selected value for the dropdownlist(id == Containers)
    var containerAlias = $("#Containers > option:selected").attr("value");

    // Create a history point and add it to the browser's history stack.
    Sys.Application.addHistoryPoint({ Container: containerAlias, Category: categoryName }, siteTitle);
}

/// <summary>
/// This function makes the link of the category name passed to it BOLD
/// </summary>
function categoryLinkBold(categoryName) {

    categoryName = unescape(categoryName);

    //  Get matched element(in variable anchorTagCollection) 
    //  with id == CategoryPanel
    var anchorTagCollection = document.getElementById("CategoryPanel");

    //  Get all <a> tag(s) inside tag with id == CategoryPanel
    var anchorTag = anchorTagCollection.getElementsByTagName("a");

    //  For all <a> tags do ...
    for (var i = 0; i < anchorTag.length; i++) {

        //  If id of this <a> tag is NOT same as that 
        //  of categoryName(parameter to this function) then ...
        if (anchorTag[i].id != categoryName) {

            //  Set fontWeight style of this <a> tag to 'normal'
            document.getElementById(anchorTag[i].id).style.fontWeight = 'normal';

        } //  If id of this <a> tag is  same as that 
        //  of categoryName(parameter to this function) then ...
        else {

            //  Set fontWeight style of this <a> tag to 'bold'
            document.getElementById(anchorTag[i].id).style.fontWeight = 'bold';

        }
    }
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

    /*alert("\n\n\nscrollWidth:" + scrollWidth
    + "\n" + "offsetWidth:" + offsetWidth
    + "\n" + "Math.max(scrollWidth, offsetWidth):" + maxWidth
    + "\n" + "ClientWidth:" + document.body.clientWidth
    + "\n\nDocument width:" + document.width
    + "\n\nWindow.innerWidth:" + window.innerWidth
    + "\n\nWindow.screen.availWidth:" + window.screen.availWidth
    + "\n\nscreen.width:" + screen.width
    + "\n\nwindow.screen.availWidth) * (screen.deviceXDPI / screen.logicalXDPI:)" + ((window.screen.availWidth) * (screen.deviceXDPI / screen.logicalXDPI))
    + "\n\nwidth:" + width
    + "\n\n\n\nnavigator.appName:" + navigator.appName
    );*/

    //  If browser is Microsoft Internet Explorer then ...
    if (navigator.appName == "Microsoft Internet Explorer" || navigator.appName == "Opera") {
        return width;
    } //  If browser is Mozilla then ...
    else if (navigator.appName == "Mozilla") {

        //  width to be returned is the available width
        //  multiplied by
        //  ratio of device-logical XDPIs
        return (window.screen.availWidth) * (screen.deviceXDPI / screen.logicalXDPI); ;

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
        return (window.screen.availHeight) * (screen.deviceYDPI / screen.logicalYDPI); ;

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
/// This function shows/Hides error on DataCatalog page
/// </summary>
function ShowHideError() {
    var trError = document.getElementById("trError");
    if (trError) {
        var error = document.getElementById("labelError").title;
        if (error == '') {
            document.getElementById("trError").style.display = "none";
        }
        else {
            document.getElementById("trError").style.display = "block";
        }
    }
}
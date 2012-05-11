/**
* common list variables
*/
var pageSize;
var pageNumber;
var orderField;
var orderType;
var totalPages;

var sortOreders = ["Asc", "Desc"];

var filter = null;

$(document).ready(function() {

    $(".dataset-list thead td").click(function() {
        var c = $(this).attr('class');
        var supportDefaultOrdering = (c == 'ascna' || c == 'descna');
        changeOrder($(this));
        // If list supports default ordering
        if (supportDefaultOrdering) {
            $(this).siblings().each(
                function() {
                    if ($(this).attr('class') == 'asc') {
                        $(this).removeClass('asc');
                        $(this).toggleClass('ascna');
                    } else {
                        if ($(this).attr('class') == 'desc') {
                            $(this).removeClass('desc');
                            $(this).toggleClass('descna');
                        }
                    }
                })
        } else {
            $(this).siblings().removeClass("asc");
            $(this).siblings().removeClass("desc");
        }
        updateListData();
    });

    var container = $("#eidPagingControl");
    $("<img  style='display: none;' src='../../Content/ico.png' id='eidFirstDisabled' title='First page' class='ico icoFirstDisabled' />").appendTo(container);
    q.icon("first", "First page", function() { setPage(1); }).appendTo(container);
    $("<img style='display: none;' src='../../Content/ico.png' id='eidLeftDisabled' title='Previous page' class='ico icoLeftDisabled' />").appendTo(container);
    q.icon("left", "Previous page", function() { setPage(pageNumber - 1); }).appendTo(container);
    q.tag("span", "paginglinks").appendTo(container);
    q.icon("right", "Next page", function() { setPage(pageNumber + 1); }).appendTo(container);
    $("<img style='display: none;' src='../../Content/ico.png' id='eidRightDisabled' title='Next page' class='ico icoRightDisabled' />").appendTo(container);
    q.icon("last", "Last page", function() { setPage(totalPages); }).appendTo(container);
    $("<img  style='display: none;' src='../../Content/ico.png' id='eidLastDisabled' title='Last page' class='ico icoLastDisabled' />").appendTo(container);
    q.div("clear").appendTo(container);

    $("#perpage").change(function() {
        pageSize = $("select[@id=perpage] option:selected").val();
        setPage(1);
        updateListData();
    });


    getInitialListData();    
    setOrder();
    $("#option" + pageSize).attr('selected', 'selected');
    updateListData();    
});


/**
* context list variables 
*/
var fieldNames;
var dataURL;
var contextURL;

function setListParameters(listDataURL, listInitDataURL, fields) {
    dataURL = listDataURL;
    contextURL = listInitDataURL;
    fields = fieldNames;
}

/**
* Common functions
*/

function getInitialListData() {
    $.ajax({
        async: false,
        url: contextURL,
        dataType: "json",
        success: function(data) {
            pageSize = data.PageSize;
            pageNumber = data.PageNumber;
            orderField = fieldNames[data.OrderBy.Field];
            orderType = sortOreders[data.OrderBy.Direction];
        },
        error: function(XMLHttpRequest, textStatus, errorThrown) {
            alert(XMLHttpRequest.responseText);
        }
    });
}

function updateListData() {
    showLoadingIndicator();
    $.ajax({
        async: true,
        type: "POST",
        url: dataURL,
        data: postify({ pageSize: pageSize, pageNumber: pageNumber, orderField: orderField, orderType: orderType, filter: filter }),
        dataType: "html",
        success: function(data) {
            $(".dataset-list tbody.rows").html(data);
            totalPages = $('#total').attr("value");
            updatePagingControl();
            hideLoadingIndicator();
        },
        error: function(XMLHttpRequest, textStatus, errorThrown) {
            alert(XMLHttpRequest.responseText);
            hideLoadingIndicator();
        }
    });
}

/**
* Ordering
*/
function setOrder() {
    if ('Desc' == orderType) {
        $("#" + orderField).toggleClass('desc');
    }
    else {
        $("#" + orderField).toggleClass('asc');
    }
}

function changeOrder(header) {
    var c = header.attr('class');
    // If list supports default ordering
    if (c == 'ascna' || c == 'descna') {
        if (c == 'ascna') {
            header.removeClass('ascna');
            header.toggleClass('asc');
            orderType = 'Asc';
        } else {
            header.removeClass('descna');
            header.toggleClass('desc');
            orderType = 'Desc';
        }
    } else {
        if (header.attr('id') == orderField) {
            if ('Desc' == orderType) {
                header.removeClass('desc');
                header.toggleClass('asc');
                orderType = 'Asc';
            }
            else {
                header.removeClass('asc');
                header.toggleClass('desc');
                orderType = 'Desc';
            }
        }
        else {
            header.toggleClass('desc');
            orderType = 'Desc';
        }
    }
    orderField = header.attr('id');
}


/**
* paging
*/

function showIcons(number) {
    var ld = $("#eidLeftDisabled");
    var rd = $("#eidRightDisabled");
    var l = $("img[title='Previous page'][class='icon']");
    var r = $("img[title='Next page'][class='icon']");
    var first = $("img[title='First page'][class='icon']");
    var last = $("img[title='Last page'][class='icon']");    
    var firstDisabled = $("#eidFirstDisabled");
    var lastDisabled = $("#eidLastDisabled");

    if (number >= 1 && number <= totalPages) {

        ld.hide();
        rd.hide();
        firstDisabled.hide();
        lastDisabled.hide();
        l.show();
        r.show();
        first.show();
        last.show();

        if (number == 1) {
            ld.show();
            firstDisabled.show();
            l.hide();
            first.hide();
        }
        
        if (number == totalPages) {
            rd.show();
            lastDisabled.show();
            r.hide();
            last.hide();
        } 
    }
}

function setPage(number) {

    if (number >= 1 && number <= totalPages) {
        showIcons(number);
        pageNumber = number;
        updateListData();
        updatePagingControl();
    }
}

function updatePagingControl() {
    showIcons(pageNumber);
    var l = navigator.userLanguage;
    if (l == "fr")
        $(".paginglinks").html("&nbsp;" + pageNumber + "&nbsp;sur&nbsp;" + totalPages);
    else
        $(".paginglinks").html("&nbsp;" + pageNumber + "&nbsp;of&nbsp;" + totalPages);
}

/**
* Filtering
*/

/**
* Utils
*/

function postify(value) {
    var result = {};
    var buildResult = function(object, prefix) {
        for (var key in object) {
            var postKey = isFinite(key)
					? (prefix != "" ? prefix : "") + "[" + key + "]"
					: (prefix != "" ? prefix + "." : "") + key;
            switch (typeof (object[key])) {
                case "number": case "string": case "boolean":
                    result[postKey] = object[key];
                    break;
                case "object":
                    if (object[key]) {
                        if (object[key].toUTCString)
                            result[postKey] = object[key].toUTCString().replace("UTC", "GMT");
                        else {
                            buildResult(object[key], postKey != "" ? postKey : key);
                        }
                    }
                    else {
                        result[postKey] = null;
                    }
            }
        }
    }
    buildResult(value, "");
    return result;
}

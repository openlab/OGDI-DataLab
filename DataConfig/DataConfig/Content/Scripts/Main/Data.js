/*
** OnLoad
*/
$(function () {
    // Bind row click event
    $("tbody tr").click(Utils.RowClickEvent);

    // Bind catalogs events
    $("#selectAllCatalogs").click(Catalogs.SelectAll);
    $("#addCatalog").click(Catalogs.Add);
    $("#deleteCatalogs").click(Catalogs.Delete);

    // Bind datasets events
    $("#selectAllDatasets").click(Datasets.SelectAll);
    $("#addDataset").click(Datasets.Add);
    $("#deleteDatasets").click(Datasets.Delete);

    // Bind filter events
    $(".displayAll").click(Filter.DisplayAll);
    $("a.filter").click(Filter.Filter);

    // Bind sort events
    $("a.sort").click(Sort.Sort);
});


/*
** Catalogs object
*/
var Catalogs = {
    _selectAll: true,
    SelectAll: function () {
        // Select or unselect all catalogs
        $("#catalogs tbody .deleteBox input").prop("checked", Catalogs._selectAll);

        // Switch flag
        Catalogs._selectAll = !Catalogs._selectAll;

        // Change button value
        $("#selectAllCatalogs").html(Catalogs._selectAll ? Strings.SelectAll : Strings.DeselectAll);
    },
    Add: function () {
        // Close modal
        $('#addCatalogModal').trigger('reveal:close');

        // Show AJAX loader
        $("#ajaxLoader").css("display", "inline-block");

        // Build form
        var form = {
            "ConfigStorageName": Config.StorageName,
            "ConfigStorageKey": Config.StorageKey,
            "Alias": $("#addCatalogModal #alias").val(),
            "Description": $("#addCatalogModal #description").val(),
            "Disclaimer": $("#addCatalogModal #disclaimer").val(),
            "DataStorageName": $("#addCatalogModal #dataStorageName").val(),
            "DataStorageKey": $("#addCatalogModal #dataStorageKey").val()
        };

        // Send AJAX request
        $.ajax({
            type: "POST",
            url: "/Catalog/Add",
            data: form,
            success: function (data) {
                if (data.Error) {
                    $("#alertBoxContainer").append(Utils.GetAlertBox("alert", data.Error));
                    $("#ajaxLoader").css("display", "none");
                } else {
                    // Reload page
                    Utils.ReloadPage(true, "#displayByCatalog");
                }
            }
        });
    },
    Delete: function () {
        // Retrieve all checked catalogs
        var checkboxes = $("#catalogs tbody .deleteBox input:checked");

        // Check if at least one catalog has been selected
        if ($(checkboxes).length == 0) {
            alert(Strings.NoCatalogSelected);
            return;
        }

        // Deletion confirmation message
        if (!confirm(Strings.ConfirmDeleteCatalogs)) {
            return;
        }

        // Show AJAX loader
        $("#ajaxLoader").css("display", "inline-block");

        // Build form
        var form = {
            "ConfigStorageName": Config.StorageName,
            "ConfigStorageKey": Config.StorageKey
        };

        // Initialize done counter
        var doneCnt = 0;

        // Delete each selected catalog
        $.each(checkboxes, function () {
            // Retrieve current row
            var tr = $(this).parents("tr");

            // Complete form
            form.PartitionKey = $(tr).find(".partitionKey").html();
            form.RowKey = $(tr).find(".rowKey").html();

            // Send AJAX request
            $.ajax({
                type: "POST",
                url: "/Catalog/Delete",
                data: form,
                success: function () {
                    if (++doneCnt == $(checkboxes).length) {
                        // Reload page
                        Utils.ReloadPage(true, "#displayByCatalog");
                    }
                }
            });
        });
    }
};


/*
** Datasets object
*/
var Datasets = {
    _selectAll: true,
    SelectAll: function () {
        // Select or unselect all datasets
        $("#datasets tbody .deleteBox input").prop("checked", Datasets._selectAll);

        // Switch flag
        Datasets._selectAll = !Datasets._selectAll;

        // Change button value
        $("#selectAllDatasets").html(Datasets._selectAll ? Strings.SelectAll : Strings.DeselectAll);
    },
    Add: function () {
        // Close modal
        $('#addDatasetModal').trigger('reveal:close');

        // Alert not implemented
        alert('This feature is not implemented yet');

        //TODO: Implement here
    },
    Delete: function () {
        // Retrieve all checked datasets
        var checkboxes = $("#datasets tbody .deleteBox input:checked");

        // Check if at least one dataset has been selected
        if ($(checkboxes).length == 0) {
            alert(Strings.NoDatasetSelected);
            return;
        }

        // Deletion confirmation message
        if (!confirm(Strings.ConfirmDeleteDatasets)) {
            return;
        }

        // Show AJAX loader
        $("#ajaxLoader").css("display", "inline-block");

        // Build form
        var form = {
            "DataStorageName": Config.StorageName,
            "DataStorageKey": Config.StorageKey
        };

        // Initiate done counter
        var doneCnt = 0;

        // Delete each selected dataset
        $.each(checkboxes, function () {
            // Retrieve current row
            var tr = $(this).parents("tr");

            // Complete form
            form.PartitionKey = $(tr).find(".partitionKey").html();
            form.RowKey = $(tr).find(".rowKey").html();

            // Send AJAX request
            $.ajax({
                type: "POST",
                url: "/Dataset/Delete",
                data: form,
                success: function () {
                    if (++doneCnt == $(checkboxes).length) {
                        // Reload page
                        Utils.ReloadPage(true, "#displayByDataset");
                    }
                }
            });
        });
    }
};

/*
** Filter object
*/
var Filter = {
    DisplayAll: function () {
        // Retrieve current filter list
        var filterList = $(this).parents(".filterBlock").find(".filterList");

        // Animate filter list
        if ($(this).html() == Strings.Hide) {
            filterList.animate({ "maxHeight": "270px" }, 1000);
            $(this).html(Strings.DisplayAll);
        } else {
            filterList.animate({ "maxHeight": filterList[0].scrollHeight }, 1000);
            $(this).html(Strings.Hide);
        }
    },
    Filter: function () {
        // Retrieve current filter
        var filter = $(this).attr("title");

        // Set filter value to the form
        if ($(this).hasClass("catalog")) {
            if ($(this).hasClass("clear")) {
                $("#dataForm > #catalogFilter").val("");
            } else {
                $("#dataForm > #catalogFilter").val(filter);
            }
        } else if ($(this).hasClass("category")) {
            if ($(this).hasClass("clear")) {
                $("#dataForm > #categoryFilter").val("");
            } else {
                $("#dataForm > #categoryFilter").val(filter);
            }
        } else if ($(this).hasClass("keyword")) {
            if ($(this).hasClass("clear")) {
                $("#dataForm > #keywordFilter").val("");
            } else {
                $("#dataForm > #keywordFilter").val(filter);
            }
        } else return;

        // Submit form
        Utils.ReloadPage(false, "#displayByDataset");
    }
};

/*
** Sort object
*/
var Sort = {
    Sort: function () {
        // Set sort param to the form
        if ($(this).hasClass("entityset")) {
            $("#dataForm > #sortParam").val("entityset");
        } else if ($(this).hasClass("source")) {
            $("#dataForm > #sortParam").val("source");
        } else if ($(this).hasClass("category")) {
            $("#dataForm > #sortParam").val("category");
        } else return;

        // Set sort order to the form
        $("#dataForm > #sortOrder").val($("#dataForm > #sortOrder").val() == "asc" ? "desc" : "asc");

        // Submit form
        Utils.ReloadPage(false, "#displayByDataset");
    }
};


/*
** Utils object
*/
var Utils = {
    RowClickEvent: function (e) {
        if (e.target.type !== 'checkbox') {
            var checkbox = $(this).find("input[type=checkbox]");
            $(checkbox).prop("checked", $(checkbox).is(":checked") ? false : true);
        }
    },
    ReloadPage: function (withReset, urlSuffix) {
        if (withReset) {
            $("#dataForm > #sortOrder").val("");
            $("#dataForm > #sortParam").val("");
            $("#dataForm > #catalogFilter").val("");
            $("#dataForm > #categoryFilter").val("");
            $("#dataForm > #keywordFilter").val("");
        }

        $("#dataForm").attr("action", "/Main/Data" + urlSuffix).submit();
    },
    GetAlertBox: function (type, message) {
        var alertBox = $("<div/>");
        alertBox.addClass("alert-box " + type);
        alertBox.html(message);
        alertBox.append("<a href='' class='close'>&times;</a>");
        return alertBox;
    }
};

/*
** Onload
*/
$(function () {
    // Bind row click event
    $("tbody tr").click(Utils.RowClickEvent);

    // Bind catalogs buttons
    $("#selectAllCatalogs").click(Catalogs.SelectAll);
    $("#addCatalog").click(Catalogs.Add);
    $("#deleteCatalogs").click(Catalogs.Delete);

    // Bind datasets buttons
    $("#selectAllDatasets").click(Datasets.SelectAll);
    $("#addDataset").click(Datasets.Add);
    $("#deleteDatasets").click(Datasets.Delete);

    // Bind displayAll buttons
    $(".displayAll").click(function () {
        var filterList = $(this).parents(".filterBlock").find(".filterList");
        if ($(this).html() == Strings.Hide) {
            filterList.animate({ "maxHeight": "270px" }, 1000);
            $(this).html(Strings.DisplayAll);
        } else {
            filterList.animate({ "maxHeight": filterList[0].scrollHeight }, 1000);
            $(this).html(Strings.Hide);
        }
    });
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
                    // Display error
                    $("#alertBoxContainer").append(Utils.GetAlertBox("alert", data.Error));
                } else {
                    // Generate new catalog row
                    var tr = Utils.GetCatalogRow(data.Result);

                    // Display new catalog row
                    $("#catalogs tbody").append(tr);

                    // Bind row click event
                    $(tr).click(Utils.RowClickEvent);

                    // Display success message
                    $("#alertBoxContainer").append(Utils.GetAlertBox("success", Strings.CatalogAdded));
                }
            },
            error: function (data) {
                // Display error
                $("#alertBoxContainer").append(Utils.GetAlertBox("alert", data.responseText));
            },
            complete: function () {
                // Hide AJAX loader
                $("#ajaxLoader").css("display", "none");
            }
        });
    },
    Delete: function () {
        var checkboxes = $("#catalogs tbody .deleteBox input:checked");

        // Check if at least one catalog has been selected
        if ($(checkboxes).length == 0) {
            alert(Strings.NoCatalogSelected);
            return;
        }

        // Deletion confirmation message
        if (confirm(Strings.ConfirmDeleteCatalogs)) {
            // Show AJAX loader
            $("#ajaxLoader").css("display", "inline-block");

            // Delete each selected catalog
            for (var ndx = 0; ndx < checkboxes.length; ++ndx) {
                // Retrieve current row
                var tr = $(checkboxes[ndx]).parents("tr");

                // Build form
                var form = {
                    "ConfigStorageName": Config.StorageName,
                    "ConfigStorageKey": Config.StorageKey,
                    "PartitionKey": $(tr).find(".partitionKey").html(),
                    "RowKey": $(tr).find(".rowKey").html()
                };

                // Send AJAX request
                $.ajax({
                    type: "POST",
                    url: "/Catalog/Delete",
                    data: form,
                    success: function (data) {
                        if (data.Error) {
                            // Display error
                            $("#alertBoxContainer").append(Utils.GetAlertBox("alert", data.Error));
                        } else {
                            // Remove current catalog row
                            $("#catalogs tbody tr").each(function () {
                                if ($(this).find(".partitionKey").html() == data.PartitionKey && $(this).find(".rowKey").html() == data.RowKey) {
                                    $(this).fadeOut("slow", function () { $(this).remove(); });
                                }
                            });
                        }
                    },
                    error: function (data) {
                        // Display error
                        $("#alertBoxContainer").append(Utils.GetAlertBox("alert", data.responseText));
                    },
                    complete: function () {
                        // Hide AJAX loader only when all catalogs have been deleted
                        if (ndx == checkboxes.length) {
                            $("#ajaxLoader").css("display", "none");
                        }
                    }
                });
            }
        }
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
    Delete: function () {
        var checkboxes = $("#datasets tbody .deleteBox input:checked");

        // Check if at least one dataset has been selected
        if ($(checkboxes).length == 0) {
            alert(Strings.NoDatasetSelected);
            return;
        }

        // Deletion confirmation message
        if (confirm(Strings.ConfirmDeleteDatasets)) {
            // Show AJAX loader
            $("#ajaxLoader").css("display", "inline-block");

            // Delete each selected dataset
            for (var ndx = 0; ndx < checkboxes.length; ++ndx) {
                // Retrieve current row
                var tr = $(checkboxes[ndx]).parents("tr");

                // Build form
                var form = {
                    "DataStorageName": Config.StorageName,
                    "DataStorageKey": Config.StorageKey,
                    "PartitionKey": $(tr).find(".partitionKey").html(),
                    "RowKey": $(tr).find(".rowKey").html()
                };

                // Send AJAX request
                $.ajax({
                    type: "POST",
                    url: "/Dataset/Delete",
                    data: form,
                    success: function (data) {
                        if (data.Error) {
                            // Display error
                            $("#alertBoxContainer").append(Utils.GetAlertBox("alert", data.Error));
                        } else {
                            // Remove current dataset row
                            $("#datasets tbody tr").each(function () {
                                if ($(this).find(".partitionKey").html() == data.PartitionKey && $(this).find(".rowKey").html() == data.RowKey) {
                                    $(this).fadeOut("slow", function () { $(this).remove(); });
                                }
                            });
                        }
                    },
                    error: function (data) {
                        // Display error
                        $("#alertBoxContainer").append(Utils.GetAlertBox("alert", data.responseText));
                    },
                    complete: function () {
                        // Hide AJAX loader only when all datasets have been deleted
                        if (ndx == checkboxes.length) {
                            $("#ajaxLoader").css("display", "none");
                        }
                    }
                });
            }
        }
    }
};


/*
** Utils object
*/
var Utils = {
    RowClickEvent: function(e) {
        if (!$(e.toElement).is("input")) {
            var checkbox = $(this).find("input[type=checkbox]");
            $(checkbox).prop("checked", $(checkbox).is(":checked") ? false : true);
        }
    },
    GetAlertBox: function (type, message) {
        var alertBox = $("<div/>");
        alertBox.addClass("alert-box " + type);
        alertBox.html(message);
        alertBox.append("<a href='' class='close'>&times;</a>");
        return alertBox;
    },
    GetCatalogRow: function (item) {
        var tr = $("<tr/>");
        tr.append(Utils.GetTableCell("deleteBox", "<input type='checkbox' />", -1));
        tr.append(Utils.GetTableCell("hidden partitionKey", item["PartitionKey"], -1));
        tr.append(Utils.GetTableCell("hidden rowKey", item["RowKey"], -1));
        tr.append(Utils.GetTableCell(null, item["alias"], 20));
        tr.append(Utils.GetTableCell(null, item["description"], 32));
        tr.append(Utils.GetTableCell(null, item["disclaimer"], 32));
        tr.append(Utils.GetTableCell(null, item["storageaccountname"], 32));
        return tr;
    },
    GetDatasetRow: function (item) {
        var tr = $("<tr/>");
        tr.append(Utils.GetTableCell("deleteBox", "<input type='checkbox' />", -1));
        tr.append(Utils.GetTableCell("hidden partitionKey", item["PartitionKey"], -1));
        tr.append(Utils.GetTableCell("hidden rowKey", item["RowKey"], -1));
        tr.append(Utils.GetTableCell("entityset", item["entityset"], 30));
        tr.append(Utils.GetTableCell("name", item["name"], 30));
        tr.append(Utils.GetTableCell("source", item["source"], 30));
        tr.append(Utils.GetTableCell("category", item["category"], 30));
        return tr;
    },
    GetTableCell: function (classes, content, maxLenght) {
        var cell = $("<td/>").addClass(classes);
        if (maxLenght == -1 || content == null) {
            cell.html(content);
        } else {
            cell.attr("title", content);
            cell.html(content.length > maxLenght ? content.substr(0, maxLenght - 2) + "..." : content);
        }
        return cell;
    }
};

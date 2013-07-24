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
});


/*
** Catalogs object
*/
var Catalogs = {
    _selectAll: true,
    SelectAll: function () {
        // Select or unselect all catalogs
        $("#catalogs .deleteBox input").prop("checked", Catalogs._selectAll);

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
        var checkboxes = $("#catalogs .deleteBox input:checked");

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
    _storageName: null,
    _storageKey: null,
    Load: function (catalogAlias, storageName, storageKey) {
        // Build form
        var form = {
            "StorageName": storageName,
            "StorageKey": storageKey
        };

        // Save current credentials
        this._storageName = storageName;
        this._storageKey = storageKey;

        // Send AJAX request
        $.ajax({
            type: "POST",
            url: "/Dataset/Load",
            data: form,
            success: function (data) {
                if (data.Error) {
                    // Show previous content
                    $("#catalogContent").toggle("slide");

                    // Display error
                    $("#alertBoxContainer").append(Utils.GetAlertBox("alert", data.Error));
                } else {
                    // Delete old content
                    $("#datasetContent table tbody tr").remove();

                    // Display each Dataset
                    $.each(data.Result, function (ndx, item) {
                        $("#datasetContent table tbody").append(Utils.GetDatasetRow(item));
                    });

                    // Set catalog name
                    $("#datasetContent #catalogAlias").html("(" + catalogAlias + ")");

                    // Display content
                    $("#datasetContent").toggle("slide", { "direction": "right" });
                }

                // Hide AJAX loader
                $("#ajaxLoader").css("display", "none");
            },
            error: function (data) {
                // Show previous content
                $("#catalogContent").toggle("slide");

                // Display error
                $("#alertBoxContainer").append(Utils.GetAlertBox("alert", data.responseText));

                // Hide AJAX loader
                $("#ajaxLoader").css("display", "none");
            }
        });
    },
    Delete: function (partitionKey, rowKey) {
        // Deletion confirmation
        if (confirm(Strings.ConfirmDeleteDataset)) {
            // Show AJAX loader
            $("#ajaxLoader").css("display", "inline-block");

            // Build form
            var form = {
                "StorageName": this._storageName,
                "StorageKey": this._storageKey,
                "PartitionKey": partitionKey,
                "RowKey": rowKey
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
                        $("#ajaxLoader").css("display", "none");
                    } else {
                        // Delete dataset (UI)
                        $.each($("#datasetContent table tbody tr"), function (ndx, item) {
                            if ($(item).find(".partitionKey").html() == partitionKey && $(item).find(".rowKey").html() == rowKey) {
                                $(item).fadeOut("slow", function () {
                                    $(item).remove();
                                    $("#alertBoxContainer").append(Utils.GetAlertBox("success", Strings.DatasetDeleted));
                                });
                                return false;
                            }
                        });

                        // Hide AJAX loader
                        $("#ajaxLoader").css("display", "none");
                    }
                },
                error: function (data) {
                    $("#alertBoxContainer").append(Utils.GetAlertBox("alert", data.responseText));
                    $("#ajaxLoader").css("display", "none");
                }
            });
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
        tr.append(Utils.GetTableCell(null, item["description"], 28));
        tr.append(Utils.GetTableCell(null, item["disclaimer"], 28));
        tr.append(Utils.GetTableCell(null, item["storageaccountname"], 20));
        return tr;
    },
    GetDatasetRow: function (item) {
        var tr = $("<tr/>");
        tr.append(Utils.GetTableCell("deleteBox", "<input type='checkbox' />", -1));
        tr.append(Utils.GetTableCell("hidden partitionKey", item["PartitionKey"], -1));
        tr.append(Utils.GetTableCell("hidden rowKey", item["RowKey"], -1));
        tr.append(Utils.GetTableCell("entityset", item["entityset"], 20));
        tr.append(Utils.GetTableCell("name", item["name"], 32));
        tr.append(Utils.GetTableCell("source", item["source"], 20));
        tr.append(Utils.GetTableCell("category", item["category"], 20));
        return tr;
    },
    GetTableCell: function (classes, content, maxLenght) {
        var cell = $("<td/>").addClass(classes);
        if (maxLenght == -1 || content == null) {
            cell.html(content);
        } else {
            cell.attr("title", content);
            cell.html(content.length > maxLenght ? content.substr(0, maxLenght) + "..." : content);
        }
        return cell;
    }
};

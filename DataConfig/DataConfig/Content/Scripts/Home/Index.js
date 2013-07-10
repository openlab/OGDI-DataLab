/*
** Onload
*/
$(function () {
    $("#showCatalogs > button").click(Catalogs.Load);
});

/*
** Catalogs object
*/
var Catalogs = {
    Load: function () {
        // Remove start message
        $("#startMessage").remove();

        // Hide content
        $("#content").fadeOut("fast", function () {
            // Show AJAX loader
            $("#ajaxLoader").css("visibility", "visible");

            // Build form
            var form = {
                "ConfigStorageName": $("#configStorageName").val(),
                "ConfigStorageKey": $("#configStorageKey").val()
            };

            // Send AJAX request
            $.ajax({
                type: "POST",
                url: "/Catalog/Load",
                data: form,
                success: function (data) {
                    // Hide AJAX loader
                    $("#ajaxLoader").css("visibility", "hidden");

                    if (data.Error) {
                        // Display error
                        $("#alertBox").append(Utils.GetAlertBox("alert", data.Error));
                    } else {
                        console.log(data);

                        // Delete old AvailableEndpoints
                        $("#content table tbody tr").remove();

                        // Display each AvailableEndpoint
                        $.each(data.Result, function (ndx, item) {
                            $("#content table tbody").append(Utils.GetCatalogRow(item));
                        });

                        // Display content
                        $("#content").fadeIn("fast");
                    }
                },
                error: function (data) {
                    // Hide AJAX loader
                    $("#ajaxLoader").css("visibility", "hidden");

                    // Display error
                    $("#alertBox").append(Utils.GetAlertBox("alert", data.responseText));
                }
            });
        });
    },
    Add: function () {
        // Close modal
        $('#addCatalog').trigger('reveal:close');

        // Show AJAX loader
        $("#ajaxLoader").css("visibility", "visible");

        // Build form
        var form = {
            "ConfigStorageName": $("#configStorageName").val(),
            "ConfigStorageKey": $("#configStorageKey").val(),
            "Alias": $("#addCatalog #alias").val(),
            "Description": $("#addCatalog #description").val(),
            "Disclaimer": $("#addCatalog #disclaimer").val(),
            "StorageName": $("#addCatalog #storageName").val(),
            "StorageKey": $("#addCatalog #storageKey").val()
        };

        // Send AJAX request
        $.ajax({
            type: "POST",
            url: "/Catalog/Add",
            data: form,
            success: function (data) {
                // Hide AJAX loader
                $("#ajaxLoader").css("visibility", "hidden");

                if (data.Error) {
                    // Display error
                    $("#alertBox").append(Utils.GetAlertBox("alert", data.Error));
                } else {
                    console.log(data);

                    // Display catalog
                    $("#content table tbody").append(Utils.GetCatalogRow(data.Result));
                    $("#alertBox").append(Utils.GetAlertBox("success", Strings.CatalogAdded));
                }
            },
            error: function (data) {
                // Hide AJAX loader
                $("#ajaxLoader").css("visibility", "hidden");

                // Display error
                $("#alertBox").append(Utils.GetAlertBox("alert", data.responseText));
            }
        });
    },
    Delete: function (partitionKey, rowKey) {
        if (confirm(Strings.ConfirmDeleteCatalog)) {
            // Show AJAX loader
            $("#ajaxLoader").css("visibility", "visible");

            // Build form
            var form = {
                "ConfigStorageName": $("#configStorageName").val(),
                "ConfigStorageKey": $("#configStorageKey").val(),
                "PartitionKey": partitionKey,
                "RowKey": rowKey
            };

            // Send AJAX request
            $.ajax({
                type: "POST",
                url: "/Catalog/Delete",
                data: form,
                success: function (data) {
                    // Hide AJAX loader
                    $("#ajaxLoader").css("visibility", "hidden");

                    if (data.Error) {
                        // Display error
                        $("#alertBox").append(Utils.GetAlertBox("alert", data.Error));
                    } else {
                        // Delete catalog (UI)
                        $.each($("#content table tbody tr"), function (ndx, item) {
                            if ($(item).find(".partitionkey").html() == partitionKey && $(item).find(".rowkey").html() == rowKey) {
                                $(item).fadeOut("slow", function () {
                                    $(item).remove();
                                    $("#alertBox").append(Utils.GetAlertBox("success", Strings.CatalogDeleted));
                                });
                                return false;
                            }
                        });
                    }
                },
                error: function (data) {
                    // Hide AJAX loader
                    $("#ajaxLoader").css("visibility", "hidden");

                    // Display error
                    $("#alertBox").append(Utils.GetAlertBox("alert", data.responseText));
                }
            });
        }
    }
};

/*
** Utils object
*/
var Utils = {
    GetAlertBox: function (type, message) {
        var alertBox = $("<div/>");
        alertBox.addClass("alert-box " + type);
        alertBox.html(message);
        alertBox.append("<a href='' class='close'>&times;</a>");
        return alertBox;
    },
    GetCatalogRow: function (item) {
        var tr = $("<tr/>");
        tr.append($("<td/>").html(Utils.GetDeleteCatalogButton(item)));
        tr.append(Utils.GetTableCell("partitionkey hidden", item["PartitionKey"], -1));
        tr.append(Utils.GetTableCell("rowkey hidden", item["RowKey"], -1));
        tr.append(Utils.GetTableCell("alias", item["alias"], 20));
        tr.append(Utils.GetTableCell("description", item["description"], 32));
        tr.append(Utils.GetTableCell("disclaimer", item["disclaimer"], 32));
        tr.append(Utils.GetTableCell("storagename", item["storageaccountname"], 20));
        tr.append(Utils.GetShowDatasetsLink());
        return tr;
    },
    GetTableCell: function (classes, content, maxLenght) {
        var text = (maxLenght != -1 && content.length > maxLenght ? content.substr(0, maxLenght) + "..." : content);
        var cell = $("<td/>");
        cell.addClass(classes);
        cell.attr("title", content);
        cell.html(text);
        return cell;
    },
    GetDeleteCatalogButton: function (item) {
        var button = $("<button/>");
        button.addClass("tiny radius alert button");
        button.attr("title", Strings.DeleteCatalog);
        button.attr("onclick", "Catalogs.Delete('" + item["PartitionKey"] + "', '" + item["RowKey"] + "')");
        button.html($("<i/>").addClass("icon-general-remove"));
        return button;
    },
    GetShowDatasetsLink: function () {
        var cell = $("<td/>");
        var link = $("<a/>");
        link.html(Strings.ShowDatasets + " <i class='icon-general-right-arrow'></i>");
        cell.addClass("datasetsLink");
        cell.html(link);
        return cell;
    }
};

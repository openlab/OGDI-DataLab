/*
** Catalogs object
*/
var Catalogs = {
    Load: function () {
        // Remove start message
        $("#startMessage").remove();

        // Hide current content
        $(".content").hide();

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
                if (data.Error) {
                    // Display error
                    $("#alertBox").append(Utils.GetAlertBox("alert", data.Error));
                } else {
                    // Delete old AvailableEndpoints
                    $("#catalogContent table tbody tr").remove();

                    // Display each AvailableEndpoint
                    $.each(data.Result, function (ndx, item) {
                        $("#catalogContent table tbody").append(Utils.GetCatalogRow(item));
                    });

                    // Set datasets link event
                    Utils.SetDatasetsLinkEvent();

                    // Display content
                    $("#catalogContent").fadeIn("fast");
                }

                // Hide AJAX loader
                $("#ajaxLoader").css("visibility", "hidden");
            },
            error: function (data) {
                // Display error
                $("#alertBox").append(Utils.GetAlertBox("alert", data.responseText));

                // Hide AJAX loader
                $("#ajaxLoader").css("visibility", "hidden");
            }
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
                if (data.Error) {
                    // Display error
                    $("#alertBox").append(Utils.GetAlertBox("alert", data.Error));
                } else {
                    // Display catalog
                    $("#catalogContent table tbody").append(Utils.GetCatalogRow(data.Result));

                    // Set datasets link event
                    Utils.SetDatasetsLinkEvent();

                    // Display message
                    $("#alertBox").append(Utils.GetAlertBox("success", Strings.CatalogAdded));
                }

                // Hide AJAX loader
                $("#ajaxLoader").css("visibility", "hidden");
            },
            error: function (data) {
                // Display error
                $("#alertBox").append(Utils.GetAlertBox("alert", data.responseText));

                // Hide AJAX loader
                $("#ajaxLoader").css("visibility", "hidden");
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
                    if (data.Error) {
                        // Display error
                        $("#alertBox").append(Utils.GetAlertBox("alert", data.Error));
                    } else {
                        // Delete catalog (UI)
                        $.each($("#catalogContent table tbody tr"), function (ndx, item) {
                            if ($(item).find(".partitionKey").html() == partitionKey && $(item).find(".rowKey").html() == rowKey) {
                                $(item).fadeOut("slow", function () {
                                    $(item).remove();
                                    $("#alertBox").append(Utils.GetAlertBox("success", Strings.CatalogDeleted));
                                });
                                return false;
                            }
                        });
                    }

                    // Hide AJAX loader
                    $("#ajaxLoader").css("visibility", "hidden");
                },
                error: function (data) {
                    // Display error
                    $("#alertBox").append(Utils.GetAlertBox("alert", data.responseText));

                    // Hide AJAX loader
                    $("#ajaxLoader").css("visibility", "hidden");
                }
            });
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
                    $("#alertBox").append(Utils.GetAlertBox("alert", data.Error));
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
                $("#ajaxLoader").css("visibility", "hidden");
            },
            error: function (data) {
                // Show previous content
                $("#catalogContent").toggle("slide");

                // Display error
                $("#alertBox").append(Utils.GetAlertBox("alert", data.responseText));

                // Hide AJAX loader
                $("#ajaxLoader").css("visibility", "hidden");
            }
        });
    },
    Delete: function (partitionKey, rowKey) {
        if (confirm(Strings.ConfirmDeleteDataset)) {
            // Show AJAX loader
            $("#ajaxLoader").css("visibility", "visible");

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
                        $("#alertBox").append(Utils.GetAlertBox("alert", data.Error));
                    } else {
                        // Delete dataset (UI)
                        $.each($("#datasetContent table tbody tr"), function (ndx, item) {
                            if ($(item).find(".partitionKey").html() == partitionKey && $(item).find(".rowKey").html() == rowKey) {
                                $(item).fadeOut("slow", function () {
                                    $(item).remove();
                                    $("#alertBox").append(Utils.GetAlertBox("success", Strings.DatasetDeleted));
                                });
                                return false;
                            }
                        });
                    }

                    // Hide AJAX loader
                    $("#ajaxLoader").css("visibility", "hidden");
                },
                error: function (data) {
                    // Display error
                    $("#alertBox").append(Utils.GetAlertBox("alert", data.responseText));

                    // Hide AJAX loader
                    $("#ajaxLoader").css("visibility", "hidden");
                }
            });
        }
    }
};

/*
** Utils object
*/
var Utils = {
    SetDatasetsLinkEvent: function () {
        $(".datasetsLink > a").click(function () {
            var tr = $(this).parent().parent();
            var catalogAlias = $(tr).find("td.alias").attr("title");
            var storageName = $(tr).find("td.storageName").attr("title");
            var storageKey = $(tr).find("td.storageKey").html();
            $("#ajaxLoader").css("visibility", "visible");
            $("#catalogContent").toggle("slide", function () {
                Datasets.Load(catalogAlias, storageName, storageKey);
            });
        });
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
        tr.append($("<td/>").html(Utils.GetDeleteButton(item, "Catalogs", Strings.DeleteCatalog)));
        tr.append(Utils.GetTableCell("partitionKey hidden", item["PartitionKey"], -1));
        tr.append(Utils.GetTableCell("rowKey hidden", item["RowKey"], -1));
        tr.append(Utils.GetTableCell("alias", item["alias"], 20));
        tr.append(Utils.GetTableCell("description", item["description"], 28));
        tr.append(Utils.GetTableCell("disclaimer", item["disclaimer"], 28));
        tr.append(Utils.GetTableCell("storageName", item["storageaccountname"], 20));
        tr.append(Utils.GetTableCell("storageKey hidden", item["storageaccountkey"], -1));
        tr.append(Utils.GetShowDatasetsLink());
        return tr;
    },
    GetDatasetRow: function (item) {
        var tr = $("<tr/>");
        tr.append($("<td/>").html(Utils.GetDeleteButton(item, "Datasets", Strings.DeleteDataset)));
        tr.append(Utils.GetTableCell("partitionKey hidden", item["PartitionKey"], -1));
        tr.append(Utils.GetTableCell("rowKey hidden", item["RowKey"], -1));
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
    },
    GetDeleteButton: function (item, type, title) {
        var button = $("<button/>");
        button.addClass("tiny radius alert button");
        button.attr("title", title);
        button.attr("onclick", type + ".Delete('" + item["PartitionKey"] + "', '" + item["RowKey"] + "')");
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

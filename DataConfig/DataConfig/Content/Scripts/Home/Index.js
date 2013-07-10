/*
** Onload
*/
$(function () {
    $("#showCatalogs > button").click(Catalogs.Load);
});

var Catalogs = {
    Load: function () {
        // Remove start message
        $("#startMessage").remove();

        // Empty alert box
        $("#alertBox").empty();

        // Hide content
        $("#content").fadeOut("fast", function () {
            // Show AJAX loader
            $("#ajaxLoader").show();

            // Build form
            var form = {
                "storageName": $("#storageName").val(),
                "storageKey": $("#storageKey").val()
            };

            // Send AJAX request
            $.ajax({
                type: "POST",
                url: "/Catalog/Load",
                data: form,
                success: function (data) {
                    // Hide AJAX loader
                    $("#ajaxLoader").hide();

                    if (data.Error) {
                        // Display error
                        $("#alertBox").html(Utils.GetAlertBox("alert", data.Error));
                    } else {
                        console.log(data);

                        // Display each AvailableEndpoint
                        $.each(data.Result, function (ndx, item) {
                            var tr = $("<tr/>");

                            tr.append($("<td/>").html(Utils.GetDeleteCatalogButton(item["PartitionKey"], item["RowKey"])));
                            tr.append($("<td/>").html(item["alias"]));
                            tr.append($("<td/>").html(item["description"]));
                            tr.append($("<td/>").html(item["disclaimer"]));
                            tr.append($("<td/>").html(item["storageaccountname"]));
                            tr.append($("<td/>").html("<a href='Index'>voir les jeux de données</a>"));

                            $("#content table tbody").append(tr);
                        });

                        // Display content
                        $("#content").fadeIn("fast");
                    }
                },
                error: function (data) {
                    // Hide AJAX loader
                    $("#ajaxLoader").hide();

                    // Display error
                    $("#alertBox").html(Utils.GetAlertBox("alert", data.responseText));
                }
            });
        });
    },
    Delete: function (partitionKey, rowKey) {
        if (confirm(Global.ConfirmDeleteCatalog)) {
            // Delete catalog
            // Remove corresponding row (UI)
            alert("Delete catalog: pk=" + partitionKey + ", rk=" + rowKey);
        }
    }
};

var Utils = {
    GetAlertBox: function (type, message) {
        var alertBox = $("<div/>").addClass("alert-box").addClass(type);
        alertBox.append(message);
        alertBox.append("<a href='' class='close'>&times;</a>");
        return alertBox;
    },
    GetDeleteCatalogButton: function (partitionKey, rowKey) {
        var button = $("<button/>").addClass("tiny radius alert button");
        button.attr("title", Global.DeleteCatalog);
        button.attr("onclick", "Catalogs.Delete('" + partitionKey + "', '" + rowKey + "')");
        button.html($("<i/>").addClass("icon-general-remove"));
        return button;
    }
};

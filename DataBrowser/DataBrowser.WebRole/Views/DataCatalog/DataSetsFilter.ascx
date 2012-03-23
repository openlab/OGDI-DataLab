<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.Models" %>
<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc" %>
<%@ Control Language="C#" Inherits="OgdiViewUserControl<Ogdi.InteractiveSdk.Mvc.Models.DatasetListModel>" %>

<script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jqueryui/1.8/jquery-ui.min.js"></script>

<link href="http://ajax.googleapis.com/ajax/libs/jqueryui/1.8/themes/base/jquery-ui.css"
    rel="stylesheet" type="text/css" />

<script type="text/javascript">

    $(function() {
        var options = { altFormat: 'mm/dd/yy', showAnim: '' };
        $("#DateFromFilter").datepicker(options);
        $("#DateToFilter").datepicker(options);
    });

    function submitFilter() {
        var result = new Object();

        result.PublishingDates = new Object();

        var fromFormat = $("#DateFromFilter").datepicker("option", "altFormat");
        var fromValue = $("#DateFromFilter").attr("value");
        result.PublishingDates.From = $.datepicker.parseDate(fromFormat, fromValue);

        var toFormat = $("#DateToFilter").datepicker("option", "altFormat");
        var toValue = $("#DateToFilter").attr("value");
        result.PublishingDates.To = $.datepicker.parseDate(toFormat, toValue);

        result.Categories = new Array();
        var categorychecks = $("#CategoryGroup input:checkbox[checked]");
        for (var i = 0; i < categorychecks.length; i++) {
            result.Categories[i] = $(categorychecks[i]).attr("name");
        }

        result.DataSources = new Array();
        var datasetschecks = $("#DataSourceGroup input:checkbox[checked]");
        for (var i = 0; i < datasetschecks.length; i++) {
            result.DataSources[i] = $(datasetschecks[i]).attr("name");
        }

        result.Keywords = $("#KeywordsFilter").val();

        result.Statuses = new Array();
        var statuschecks = $("#StatusesGroup input:checkbox[checked]");
        for (var i = 0; i < statuschecks.length; i++) {
            result.Statuses[i] = $(statuschecks[i]).attr("name");
        }

        result.FileTypes = new Array();
        var filetypechecks = $("#FileTypesGroup input:checkbox[checked]");
        for (var i = 0; i < filetypechecks.length; i++) {
            result.FileTypes[i] = $(filetypechecks[i]).attr("name");
        }

        return result;
    }

    function clearFilter() {
        $("#DateFromFilter").attr("value", "");
        $("#DateToFilter").attr("value", "");
        $("#KeywordsFilter").val("");
        $("#CategoryGroup input:checkbox").attr("checked", false);
        $("#DataSourceGroup input:checkbox").attr("checked", false);
        $("#StatusesGroup input:checkbox").attr("checked", false);
        $("#FileTypesGroup input:checkbox").attr("checked", false);
    }
</script>

<div class="dataset-filter form">
    <div class="bar">
        Datasets</div>
    <div class="content">
        <table cellpadding="0" cellspacing="0" width="100%">
            <tr>
                <td width="33%" align="left" valign="top">
                    <div class="category-block">
                        <div class="label">
                            Category</div>
                        <div id="CategoryGroup" class="items">
                            <%	var index = 0; foreach (String category in ViewData.Model.Categories)
                                {
                                    var id = "cat" + (index++);
                            %>
                            <div class="item">
                                <input type="checkbox" name="<%=category%>" value="<%=category%>" id="<%= id %>" /><label
                                    for="<%= id %>"><%= Html.Encode(category) %></label></div>
                            <% } %>
                        </div>
                    </div>
                </td>
                <td width="33%" align="left" valign="top">
                    <%if (ViewData.Model.AllContainers.GetEnumerator().MoveNext())
                      {%>
                    <div class="data-source-block">
                        <div class="label">
                            Data Source</div>
                        <div id="DataSourceGroup" class="items" style="overflow-y:scroll;">
                            <% index = 0; foreach (Container container in ViewData.Model.AllContainers)
                               {
                                   var id = "ds" + (index++);
                            %>
                            <div class="item">
                                <input type="checkbox" name="<%=container.Alias%>" value="<%=container.Alias%>" id="<%= id %>" /><label
                                    for="<%= id %>"><%= Html.Encode(container.Description) %></label></div>
                            <% } %>
                        </div>
                    </div>
                    <% } %>
                </td>
                <td width="33%" align="left" valign="top">
                    <div class="rest-block">
                        <table cellpadding="0" cellspacing="0" width="100%">
                            <tr class="field">
                                <td class="label">
                                    Dates
                                </td>
                                <td />
                                <td />
                            </tr>
                            <tr>
                                <td class="value published" >
                                    from <input id="DateFromFilter" maxlength="10" size="10" class="calendar"   />
                                </td>
                                <td align="center" >
                                    
                                </td>
                                <td class="value published" >
                                    to <input id="DateToFilter" maxlength="10" size="10" class="calendar"   />
                                </td>
                            </tr>
                            <tr class="field">
                                <td class="label">
                                    Keywords
                                </td>
                                <td />
                                <td />
                            </tr>
                            <tr>
                                <td class="value keywords" colspan="3">
                                    <input id="KeywordsFilter" value=""  style="width:272px;" />
                                </td>
                            </tr>
                            <tr class="field">
                                <td class="label">
                                    Status
                                </td>
                                <td />
                                <td />
                            </tr>
                            <tr id="StatusesGroup">
                                <td class="value">
                                    <input type="checkbox" class="checkbox" name="Published" id="stPublished" /><label
                                        for="stPublished">Published</label>
                                 </td>
                                 <td></td>
                                 <td class="value">
                                    <input type="checkbox" class="checkbox" name="Planned" id="stPlanned" /><label for="stPlanned">Planned</label>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="3" >
                                    <div class="buttons" style="text-align: left;">
                                        <%= Html.NiceButton(this, "clear", 0, "ClearFilter") %>
                                        <%= Html.NiceButton(this, "filter", 0, "SubmitFilter") %>
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </div>
                </td>
            </tr>
        </table>
    </div>
    <div class="clear">
    </div>
</div>

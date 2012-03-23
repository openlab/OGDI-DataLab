<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc" %>
<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/OGDIMasterPage.Master" Inherits="OgdiViewPage<Ogdi.InteractiveSdk.Mvc.Models.Request.AgencyRequestsViewModel>" %>
<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.Models.ViewData" %>
<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.Models" %>
<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.Models.Rating" %>
<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.Models.Request" %>
<%@ Import Namespace="System.Web.Mvc" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    <link href="../../Content/css/list.css" rel="stylesheet" type="text/css" />         
</asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="HeadContent">
	<style type="text/css">
		.canvas .sheet {border-left-width:5px;}
	</style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

	<% Html.RenderPartial("AgencyTabs", new AgencyTabsViewModel(AgencyTab.Requests)); %>
    
    <script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jqueryui/1.8/jquery-ui.min.js"></script>
    
    <div class="block">
		<div class="requests">
			<div class="bar">Requests</div>


    <% Html.BeginForm(); %>
    <div class="form">
        <%
            List<string> list = new List<string>();
            list.Add("All");
            list.Add("Submitted");                        
        %>
        <table>
        <tr>
            <td align="left">Show: <%= Html.DropDownList("ShowStatus", new SelectList(list, ViewData.Model.Filter.Status))%></td>
            <td align="left">From: <input id="From" maxlength="10" size="10" class="calendar" /></td>
            <td align="left">To: <input id="To" maxlength="10" size="10" class="calendar" /></td>
            <td align="left">
				<%= Html.NiceInputButton(this, "filter", "javascript:filter()") %>
			</td>
        </tr>
        </table>
        <%= Html.Hidden("FromHidden") %>
        <%= Html.Hidden("ToHidden")%>
    </div>
    <% Html.EndForm(); %>

    <script type="text/javascript">
        $(function() {
            var options = { altFormat: 'mm/dd/yy', showAnim: '' };
            $("#From").datepicker(options);
            $("#To").datepicker(options);

            $("#From")[0].value = "<%= ViewData.Model.Filter.FromAsString%>";
            $("#To")[0].value = "<%= ViewData.Model.Filter.ToAsString%>";
        });

        function filter() {
            
            var fromValue = $("#From").attr("value");            
            var toValue = $("#To").attr("value");            

            $("#FromHidden")[0].value = fromValue;
            $("#ToHidden")[0].value = toValue;
            $("form").submit();
        }
        
    </script>

    <% foreach (Request request in ViewData.Model.List)
       {%>
    <div id="Request_<%= request.RequestID %>" class="request">
        <table width='100%'>
            <tr>
                <td width="7%" class="label">Author</td>
                <td width="33%"><% Html.RenderPartial("AuthorInfo", new AuthorInfo(request.Name, request.Email)); %></td>
                <td width="20%" class="label">Comments</td>
                <td width="10%"><%= request.Comments %></td>
                <td width="10%" class="label">Posted</td>
                <td align="left" width="20%">
                    <% if (request.PostedDate.HasValue)
                       {
                           Response.Write(request.PostedDate.ToString());
                       }
                    %>
                </td>
            </tr>
            <tr>
                <td class="label">Request</td>
                <td><%= Html.ActionLink(!String.IsNullOrEmpty(request.Subject) ? request.Subject : "Empty", "Details", new { id = request.RequestID})%></td>
                <td class="label">Raiting</td>
                <td><% Html.RenderPartial("RatesPlusMinus", new VoteResults() { Positive = request.PositiveVotes, Negative = request.NegativeVotes }); %></td>
                <td class="label">Status</td>
                <td><div id="Status_<%= request.RequestID %>"><%= request.Status %></div></td>
            </tr>
            <tr>
                <td align="left" class="description" colspan="6"><%= request.Description %></td>
            </tr>
            <tr>
                <td colspan="6" class="admin-console">
					<span>[<a href="javascript:deleteRequest('<%= request.RequestID %>')" title="Hide from Public" class="delete"><%--<%= Html.NiceButton(this, "delete", 0, "") %>--%>Hide from Public</a>]</span>
                    <span>[<a href="javascript:showDiv('Complete_' +'<%= request.RequestID %>')" title="Mark as completed" class="mark-as-completed"><%--<%= Html.NiceButton(this, "mark-as-completed", 0, "") %>--%>Mark as Completed</a>]</span>
                </td>
            </tr>
        </table>
        
		<div id="Complete_<%= request.RequestID %>" style="display: none; float:left;" class="form">
			<div class="bar">Complete Request</div>
			<div class="field">
				<div class="label">Released Date</div>
				<div class=""><input id="ReleaseDate_<%= request.RequestID %>" maxlength="10" size="10" class="calendar" /></div>
				<div class="clear"></div>
			</div>
			<div class="field">
				<div class="label">Links and references</div>
				<div class=""><%= Html.TextBox("Link_" + request.RequestID) %></div>
				<div class="clear"></div>
			</div>
			<div class="field">
				<div class="label">Dataset link</div>
				<div class=""><%= Html.TextBox("DatasetLink_" + request.RequestID) %></div>
				<div class="clear"></div>
			</div>
			<div class="field">
				<div class="label"></div>
				<div class=""></div>
				<div class="clear"></div>
			</div>
			<div class="buttons">
				<%= Html.NiceInputButton(this, "ok", "javascript:completeRequest('" + request.RequestID + "')") %>			
			</div>
		</div>
		<div class="clear"></div>

        
    </div>
    <script type="text/javascript">
        $(function() {
            var options = { altFormat: 'mm/dd/yy', showAnim: '' };
            $("#ReleaseDate_<%= request.RequestID %>").datepicker(options);
        });
    </script>

    <% }%>
    
    	</div>
    </div>

    <script type="text/javascript">
        function deleteRequest(rowKey) {

            $.ajax({
                async: true,
                type: "POST",
                url: "/Request/DeleteRequest",
                data: { rowKey: rowKey },
                dataType: "json",
                success: function(data) {
                    
                    if (data.Show) {
                        $('div#Status_' + rowKey).html(data.Status);
                    }

                },
                error: function(XMLHttpRequest, textStatus, errorThrown) {
                    alert(XMLHttpRequest.responseText);

                }
            });
        }

        function showDiv(divId) {
            var divs = $("div#" + divId);
            if (divs.length > 0) {
                if (divs[0].style.display == 'none')
                    divs[0].style.display = 'inline';
                else
                    divs[0].style.display = 'none';
            }
        }

        function completeRequest(rowKey) {
            
            var link = $("input#Link_" + rowKey)[0].value;
            var datasetLink = $("input#DatasetLink_" + rowKey)[0].value;            
            var releasedDate = $("#ReleaseDate_" + rowKey).attr("value");
            


            $.ajax({
                async: true,
                type: "POST",
                url: "/Request/CompleteRequest",
                data: { rowKey: rowKey, link: link, datasetLink: datasetLink, releasedDate: releasedDate },
                dataType: "json",
                success: function(data) {
                    $("div#Request_" + rowKey)[0].innerHTML = data;
                    $("div#Complete_" + rowKey)[0].innerHTML = "";                    
                },
                error: function(XMLHttpRequest, textStatus, errorThrown) {
                    alert(XMLHttpRequest.responseText);                    
                }
            });

        }       
          
    </script>

</asp:Content>
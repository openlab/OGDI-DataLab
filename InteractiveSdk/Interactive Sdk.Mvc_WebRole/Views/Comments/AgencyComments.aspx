<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc" %>
<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/OGDIMasterPage.Master" Inherits="OgdiViewPage<Ogdi.InteractiveSdk.Mvc.Models.Comments.AgencyCommentViewModel>" %>
<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.Models.ViewData" %>
<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.Models" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	<link href="http://ajax.googleapis.com/ajax/libs/jqueryui/1.8/themes/base/jquery-ui.css" rel="stylesheet" type="text/css" />
	<script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jqueryui/1.8/jquery-ui.min.js"></script>
</asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="HeadContent">
	<style type="text/css">
		.canvas .sheet {border-left-width:5px;}
	</style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
 
 <% Html.RenderPartial("AgencyTabs", new AgencyTabsViewModel(AgencyTab.Comments)); %>
 
 <script type="text/javascript">
     $(function() {
         var options = { altFormat: 'mm/dd/yy', showAnim: '' };
         $("#From").datepicker(options);
         $("#To").datepicker(options);

         $("#From")[0].value = "<%= ViewData.Model.Filter.FromAsString%>";
         $("#To")[0].value = "<%= ViewData.Model.Filter.ToAsString%>";
     });

     function filter() {

         var fromFormat = $("#From").datepicker("option", "altFormat");
         var fromValue = $("#From").attr("value");
         var from = $.datepicker.parseDate(fromFormat, fromValue);

         var toValue = $("#To").attr("value");
         var to = $.datepicker.parseDate(fromFormat, toValue);

         $("#FromHidden")[0].value = fromValue;
         $("#ToHidden")[0].value = toValue;
         $("form").submit();
     }
     function DeleteComment(commentid,element) {
         var globVPath = '<%= this.ResolveUrl("~/") %>';
         var result;
         $.ajax(
            {
                type: "POST",
                async: false,
                url: globVPath + "Comments/DeleteComment/?id=" + commentid,
                data: result,
                dataType: "_default",
                success: function(data) {
                if (data == 'True') {
                        $('div#comment' + commentid).remove();
                    }
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

     function PostNewComment(rowKey,container,parentid,parenttype) {

         var subject = $("input#NewCommentSubject_" + rowKey)[0].value;
         var comment = $("textarea#NewComment_" + rowKey)[0].value;

         if (!subject || !comment) {
             $("#ReplyToCommentAlert_" + rowKey).text("Please write a comment.");
             return;
         }

         $.ajax({
             async: true,
             type: "POST",
             url: "/Comments/Reply",
             data:
             {
                 subject: subject,
                 comment: comment,
                 email: "",
                 type: "General Comment (no reply required)",
                 datasetId: parentid,
                 parentType: parenttype,
                 container: container,
                 origRowKey: rowKey
             },
             dataType: "json",
             success: function(data) {
                 if(data=="Replied"){
                     $("#ReplyToCommentAlert_" + rowKey).text("Posted successfully.");
                     $("textarea#NewComment_" + rowKey).val("");
                 }else{
                     $("#ReplyToCommentAlert_" + rowKey).text("An error occured.");
                 }
             },
             error: function(XMLHttpRequest, textStatus, errorThrown) {
                 $("#ReplyToCommentAlert_" + rowKey).text("An error occured.");
             }
         });
     }

     function UpdateCommentStatus(rowKey, status, element) {
         $.ajax({
             async: true,
             type: "POST",
             url: "/Comments/UpdateStatus",
             data:
             {
                 rowKey: rowKey,
                 status: status
             },
             dataType: "json",
             success: function(data) {
             if (data.Show) {
                 $('div#statusValue' + rowKey).html(data.Status);
                 }
                 else {
                     $('div#comment' + rowKey).remove();
                 }
             },
             error: function(XMLHttpRequest, textStatus, errorThrown) {
                 alert(XMLHttpRequest.responseText);
             }
         });
     }
        
</script>

<div class="block">
	<div class="agency-comments">
		<div class="bar">Manage Comments</div>

		<div class="form middle">
		<% using (Html.BeginForm()){ %>
			Show:                                       
			<%= Html.DropDownList("ShowStatus", Model.Statuses) %>
			 From:
			<input id="From" maxlength="10" size="10" class="calendar" />
			To:
			<input id="To" maxlength="10" size="10" class="calendar" />
			<%= Html.NiceInputButton(this, "filter", "javascript:filter()") %>
			<%= Html.Hidden("FromHidden") %>
			<%= Html.Hidden("ToHidden")%>
			<%} %>
		</div>
    
    <% foreach (var item in Model.Comments)
	   { %>
    
        <div class="commentItem comment" id="comment<%= item.RowKey %>">
        
			<div class="parameters">
				<div class="left" style="width:300px;">
					<div class="below">
						<div class="label"><%= item.ParentType%></div>
						<div class="value"><a href="<%=this.ResolveUrl(item.ParentLink) %>"><%=item.ParentDisplay%></a></div>
						<div class="clear"></div>
					</div>
					<div class="below">
						<div class="label">Author</div>
						<div class="value"><% Html.RenderPartial("AuthorInfo", new AuthorInfo(item.Author, item.Email)); %></div>
						<div class="clear"></div>
					</div>
				</div>
				<div class="left" style="width:300px;">
					<div class="below">
						<div class="label">Type</div>
						<div class="value"><%=item.Type%></div>
						<div class="clear"></div>
					</div>
					<div class="below">
						<div class="label">Posted</div>
						<div class="value"><%=item.Posted.ToString(Ogdi.InteractiveSdk.Mvc.Globals.ShortDateFormat)%></div>
						<div class="clear"></div>
					</div>
				</div>
				<div class="left">
					<div class="below">
						<div class="label">Status</div>
						<div class="value" id="statusValue<%=item.RowKey %>"><%=item.Status%></div>
						<div class="clear"></div>
					</div>
				</div>
				<div class="clear"></div>
			</div>
        
			<div class="message">
				<div class="name"><%--<a href="<%=this.ResolveUrl(item.ParentLink) %>">--%><%=item.Subject%><%--</a>--%></div>
				<div class="description"><%=item.Body%></div>
			</div>        
		
			<div class="controls middle admin-console">
				<span>[<a href="javascript:UpdateCommentStatus('<%= item.RowKey %>', 'Hidden', this)" class="delete">Hide from Public<%--<%= Html.NiceButton(this, "delete", 0, null) %>--%></a>]</span>
				<span>[<a href="javascript:showDiv('Reply_' +'<%= item.RowKey %>')" class="reply">Reply<%--<%= Html.NiceButton(this, "reply", 0, null) %>--%></a>]</span>
				<span>[<a href="javascript:UpdateCommentStatus('<%= item.RowKey %>', 'Addresed', this)" class="mark-as-addressed">Mark as Addressed<%--<%= Html.NiceButton(this, "mark-as-addressed", 0, null) %>--%></a>]</span>
				<span>[<a href="javascript:UpdateCommentStatus('<%= item.RowKey %>', 'NoActionRequired', this)" class="no-action-required">Archive<%--<%= Html.NiceButton(this, "no-action-required", 0, null) %>--%></a>]</span>
			</div>

			<div id="Reply_<%= item.RowKey %>" style="display: none; float:left;" class="form">
				<div class="bar">Reply</div>
				<div class="field">
					<div class="label">Subject</div>
					<div class=""><input id="NewCommentSubject_<%= item.RowKey %>" /></div>
					<div class="clear"></div>
				</div>
				<div class="field">
					<div class="label">Comment</div>
					<div class=""><%= Html.TextArea("NewComment_" + item.RowKey)%></div>
					<div class="clear"></div>
				</div>
				<div class="field">
					<div class="label"></div>
					<div Id="ReplyToCommentAlert_<%=item.RowKey%>"></div>
					<div class="clear"></div>
				</div>
				<div class="buttons">
					<%= Html.NiceInputButton(this, "post-comment", "javascript:PostNewComment('" + item.RowKey + "', '" + item.ParentContainer + "', '" + item.ParentName + "', '" + item.ParentType + "')") %>
				</div>
			</div>
			<div class="clear"></div>
			
        </div>
    <% 
	   } %>

    </div>
</div>

</asp:Content>


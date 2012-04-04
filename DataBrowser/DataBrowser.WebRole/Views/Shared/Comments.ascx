<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc" %>
<%@ Control Language="C#" Inherits="OgdiViewUserControl<Ogdi.InteractiveSdk.Mvc.Models.Comments.CommentInfo>" %>
<%@ Import Namespace="System.Web.Mvc" %>

<% String link = Ogdi.Azure.Configuration.OgdiConfiguration.GetValue("serviceUri") + "Comments" + "/";
link += "?$filter=" + "DatasetId eq '" + Model.DatasetId + "' and PartitionKey eq '" + Model.Container + "'"; %>

<link rel="alternate" type="application/atom+xml" title="Atom" href="<%=link%>" />

<script type="text/javascript">

	function Comments() { }
	Comments.vpath = '<%= this.ResolveUrl("~/") %>';
	Comments.months = ["Jan", "Feb", "Mar", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];


	function renderDate(date) { return Comments.months[date.getMonth()] + ""; }

	function showCaptcha() {
	  Recaptcha.create('<%=Ogdi.Azure.Configuration.OgdiConfiguration.GetValue("RecaptchaPublicKey")%>', 'eidCommentsRecaptcha', RecaptchaOptions);
	}

	$().ready(function() {
	  showCaptcha();
	  function isEmailRequired(element) {
	    return $("#eidCommentNotify")[0].checked || ($("#eidCommentType").val() == "General Comment (reply required)");
	  }
	  $("#commentForm").validate({
		  rules: {
		    CommentUserName: "required",
		    CommentSubject: "required",
		    CommentBody: "required",
			  CommentEmail: {
			    required: { depends: isEmailRequired },
			    email: true
			  }
		  }});
	});

	Comments.AddComment = function() {
	  if ($("#eidCommentsRecaptcha div").length == 0) {
	    showCaptcha();
	    $("#eidCaptchaMessage").empty();
	    return;
	  }

	  if (!$("#commentForm").valid()) {
	    $("#eidCaptchaMessage").text("Please, fill all fields correctly.");
	    return;
	  }

	  var name = $("#eidCommentUserName");
	  var subject = $("#eidCommentSubject");
	  var comment = $("#eidCommentBody");
	  var email = $("#eidCommentEmail");
	  var notify = $("#eidCommentNotify");
	  var type = $("#eidCommentType");
	  var method = "Comments/Add";
	  var datasetId = "<%= Model.DatasetId %>";
	  var datasetName = "<%= Model.DatasetName %>";
	  var parentType = "<%= Model.ParentType %>";
	  var container = "<%= Model.Container %>";	  

	  var data = {
	    name: name.val(),
	    subject: subject.val(),
	    comment: comment.val(),
	    email: email.val(),
	    type: type.val(),
	    notify: notify[0].checked,
	    datasetId: datasetId,
	    datasetName: datasetName,
	    parentType: parentType,
	    container: container,
	    captchaChallenge: Recaptcha.get_challenge(),
	    captchaResponse: Recaptcha.get_response()
	  }

	  if (!data.subject || !data.comment || !data.name) {
	    $("#eidCaptchaMessage").text("Please, enter your name and comment.");
	    return;
	  }

	  var isEmailRequired = data.notify || (data.type == "General Comment (reply required)");

	  if (isEmailRequired && !data.email) {
	    $("#eidCaptchaMessage").text("Please, enter your e-mail.");
	    return;
	  }

	  var errorHandler = function(error, status, xhr) {
	    if (confirm("Error occured, debug?")) {
	      debugger;
	    }
	  }

	  var dataHandler = function(data) {
	    Recaptcha.reload();
	    if (data == "") {
	      $("#eidCaptchaMessage").text("Please, try again.");
	      return;
	    }
	    $("#eidCaptchaMessage").text("Your comment was added successfully.");

	    var container = $("#eidComments");
	    var item = q.div("item").appendTo(container);
	    item.html(data);
	    var odd = container.find("div.item").length % 2;
	    item.addClass(odd ? "co" : "ce");

	    subject.val('');
	    comment.val('');
	    type.val('');
	  }

	  var options = {
	    url: Comments.vpath + method,
	    cache: false,
	    data: data,
	    dataType: "html",
	    error: errorHandler,
	    success: dataHandler,
	    type: "POST"
	  };
	  $.ajax(options);
	  return false; // don't really want to submit
	}

	function DeleteComment(commentid) {
		var globVPath = '<%= this.ResolveUrl("~/") %>';
		var result;
		$.ajax(
            {
            	type: "POST",
            	async: false,
            	url: globVPath + "Comments/DeleteComment/?id=" + commentid,
            	data: result,
            	dataType: "json"
            });
		return false;
	}
	
</script>


<div class="comments">
    <table cellpadding="0" cellspacing="0" width="100%">
        <tr>
            <td>
                <div class="bar">
                    <div class="header">
                        Comments</div>
                    <div class="clear">
                    </div>
                </div>
            </td>
        </tr>
    </table>
<div class="list" id="eidComments">
	<%
		var comments = this.Model.Comments.GetEnumerator();
		if (comments.MoveNext())
		{
			bool odd = true;
			do
			{
				var comment = comments.Current;
	%>
	<div class="item <%= odd ? "co" : "ce" %>">
		<% Html.RenderPartial("comment", comment); %>
	</div>
	<%
				odd = !odd;
			} while (comments.MoveNext());
		}
		else
		{%>
	<% } %>
</div>

<form id="commentForm" method="post">
<div class="bar">New Comment</div>
<table class="form" cellpadding="0" cellspacing="2" border="0">

	<tr class="field">
		<td class="label">Name<span class="field-validation-error">*</span></td>
		<td class="value">
			<input type="text" id="eidCommentUserName" name="CommentUserName" />
			
		</td>
	</tr>
	
	<tr class="field" id="eidCommentEmailBar">
		<td class="label">E-mail<span id="eidEmailStarSpan" class="field-validation-error">*</span></td>
		<td class="value">
			<input type="text" id="eidCommentEmail" name="CommentEmail"/>
			<div style="font-size:7pt; color:gray; ">(Will not be shown to public)</div>
		</td>
	</tr>

	<tr class="field">
		<td class="label">Subject<span class="field-validation-error">*</span></td>
		<td class="value">
			<input type="text" id="eidCommentSubject" name="CommentSubject"/>
		</td>
	</tr>
	
	<tr class="field">
		<td class="label">Comment<span class="field-validation-error">*</span></td>
		<td class="value">
			<textarea cols="40" rows="10" id="eidCommentBody" name="CommentBody" ></textarea>
		</td>
	</tr>
	
	<tr class="field">
		<td class="label">Mark as</td>
		<td class="value">
			<select id="eidCommentType" onchange="javascript:onEmailChange()">
			<% foreach (var type in this.Model.CommentTypes) { %>
				<option value="<%= Html.Encode(type) %>"><%= Html.Encode(type) %></option>
			<% } %>
			</select>
		</td>
	</tr>
	
	<tr class="field">
		<td class="label"><div style="padding: 4px 0px;">Subscription</div></td>
		<td class="value">
    	<a href="<%=link%>">RSS/ATOM feed</a> <a href="<%=link%>"><img src="/Content/Images/rss.gif" alt="Atom feed" style="vertical-align:middle;padding: 4px 0px;"/></a>
		</td>
	</tr>
	
	<tr class="field">
		<td class="label">&nbsp;</td>
		<td class="value">
			<input type="checkbox" class="checkbox" id="eidCommentNotify" onclick="javascript:onEmailChange()"/>
			<label for="eidCommentNotify">Notify me about replies by e-mail</label>
		</td>
	</tr>

	<tr class="field">
		<td class="label">Enter Text<span class="field-validation-error">*</span></td>
		<td class="value">
		    <table cellpadding="0" cellspacing="0">
		    <tr>
		    <td>
			    <div id="eidCommentsRecaptcha"></div>			
			</td>
			<td valign="top">
			    
			</td>
			</tr>
			</table>
		</td>
	</tr>
	
	<tr>
		<td class="value" colspan="2">
			<div id="eidCaptchaMessage"></div>
		</td>
    </tr>
	<tr>
		<td  colspan="2" align="right" class="buttons"><%= Html.NiceInputButton(this, "post-comment", "return !!Comments.AddComment();") %></td>
	</tr>
</table>	
</form>
<script type="text/javascript">
    function onEmailChange() {        
	    var emailStar = $("#eidEmailStarSpan");
	    var type = $("#eidCommentType");
	    var notify = $("#eidCommentNotify");
	    if ((type[0].selectedIndex == 1)||(notify[0].checked)) {
	        emailStar.show();
	    } else {
	        emailStar.hide();
	    }
	}
	onEmailChange();	
</script>

</div>
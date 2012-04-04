<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Ogdi.InteractiveSdk.Mvc.Models.Rating.RateInfo>" %>
<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc" %>

	<%
		if (Model.ReadOnly || Model.HasUserVoted(Request))
		{
            Html.RenderPartial("RatesPlusMinus", new Ogdi.InteractiveSdk.Mvc.Models.Rating.VoteResults() { Negative = this.Model.NegativeRates, Positive = this.Model.PositiveRates });
		}
		else
		{
	%>
	
	
<script type="text/javascript">
	var globalVote;
	function Vote(voteResult, itemKey, element) {
		var globVPath = '<%= ResolveUrl("~/") %>';
		var vote = voteResult > 0 ? "ThumbUp" : "ThumbDown";
		
		var data = {
		    itemKey: itemKey,
			captchaChallenge: Recaptcha.get_challenge(),
			captchaResponse: Recaptcha.get_response()
		};

		$.ajax(
            {
            	type: "POST",
            	async: false,
            	url: globVPath + "Rates/" + vote + "/",
            	data: data,
            	dataType: "html",
            	success: function(data) {
            		if (data == "") {
            			ShowRecaptcha();
            			return;
            		}
            		$('#eidRates').html(data);
            	},
            	error: function(xhr, textStatus, errorThrown) {
            		alert(textStatus + ": " + xhr.responseText);
            	}
            });
	}

	function IsAuthenticated() {
		var globVPath = '<%= ResolveUrl("~/") %>';
		var isAuth;
		$.ajax(
            {
            	type: "POST",
            	async: false,
            	url: globVPath + "Rates/IsAuthenticated",
            	data: isAuth,
            	dataType: "_default",
            	success: function(data) {
            		isAuth = data;
            	},
            	error: function(XMLHttpRequest, textStatus, errorThrown) {
            		alert(textStatus);
            		alert(errorThrown);
            	}
            });

		if (isAuth == 'False') {
			return false;
		}
		return true;
	}

	function ShowRecaptcha() {
		Recaptcha.create('<%= Ogdi.Azure.Configuration.OgdiConfiguration.GetValue("RecaptchaPublicKey")%>', "RecaptchaDiv", { callback: Recaptcha.focus_response_field });
		document.getElementById('RecaptchaButtons').style.display = "block";
	}

	function CheckRecaptcha(element) {
	    Vote(globalVote, '<%= Model.ItemKey %>', element);
	}

	function CloseRecaptcha(element) {
		Recaptcha.destroy();
		document.getElementById('RecaptchaButtons').style.display = "none";
	}
</script>
	<div id="eidRates">	
		<div class="thumbs">
			<img src="<%= ResolveUrl("~/Content/ico.png") %>" title="Like it!" class="ico icoThumbUp" onclick="globalVote=1;if(IsAuthenticated()){Vote(1, '<%=Model.ItemKey%>', this);}else{ShowRecaptcha();}return false;" />
			<span id="eidPositiveRate">
				<%= Model.PositiveRates.ToString() %></span>
			<img src="<%= ResolveUrl("~/Content/ico.png") %>" title="Don't like it!" class="ico icoThumbDown" onclick="globalVote=-1;if(IsAuthenticated()){Vote(-1, '<%=Model.ItemKey%>', this);}else{ShowRecaptcha();}return false;" />
			<span id="eidNegativeRate">
				<%= Model.NegativeRates.ToString() %></span>
		</div>
		
		<div id="RecaptchaDiv"></div>
		<div style="white-space: nowrap; display: none; text-align: right; background-color: White;" id="RecaptchaButtons">
			<input type="button" id="CheckRecaptcha" onclick="CheckRecaptcha(this)" value="Ok" />
			<input type="button" id="CloseRecaptcha" onclick="CloseRecaptcha(this)" value="Cancel" />
		</div>
	</div>		
	<%
		}
	%>


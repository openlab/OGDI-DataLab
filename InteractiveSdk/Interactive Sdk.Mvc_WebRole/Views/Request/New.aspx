<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc" %>
<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/OGDIMasterPage.Master" Inherits="OgdiViewPage<Ogdi.InteractiveSdk.Mvc.Models.Request.Request>" %>

<asp:Content ID="mainContent" ContentPlaceHolderID="MainContent" runat="server">

<script type="text/javascript">

    function showCaptcha() {
      Recaptcha.create('<%=Ogdi.Azure.Configuration.OgdiConfiguration.GetValue("RecaptchaPupblicKey")%>', 'eidCommentsRecaptcha', RecaptchaOptions);
    }

    $().ready(function() {
        showCaptcha();
    });


</script>

<div class="block form">
	<div class="bar">New Request</div>
    <%= Html.ValidationSummary("Create was unsuccessful. Please correct the errors and try again.") %>

    <% using (Html.BeginForm()) {%>
    <table class="form" cellpadding="5" cellspacing="0" border="0">		
		<tr class="field">
			<td class="label">Name</td>
			<td class="value"> <%= Html.TextBox("Name") %>
                <%= Html.ValidationMessage("Name", "*")%></div>
			<td class="clear"></td>
		</tr>				
		<tr class="field">
			<td class="label">Email</div>
			<td class="value"> <%= Html.TextBox("Email")%> ( will not be published )
                <%= Html.ValidationMessage("Email", "*")%></td>
			<td class="clear"></td>
		</tr>						
		<tr class="field">
			<td class="label">Subject</div>
			<td class="value"> <%= Html.TextBox("Subject") %>
                <%= Html.ValidationMessage("Subject", "*") %></td>
			<td class="clear"></td>
		</tr>
		<tr class="field">
			<td class="label">Description</td>
			<td class="value"><%= Html.TextArea("Description", "", 10, 40, null) %>
                <%= Html.ValidationMessage("Description", "*") %></td>
			<td class="clear"></td>
		</tr>
		
	    <tr class="field">
		    <td class="label">Enter Text</td>
		    <td class="value">
			    <div id="eidCommentsRecaptcha"></div>
			    <%= Html.ValidationMessage("eidCommentsRecaptcha", "Words mismatch")%></td>
		    </td>
	    </tr>
		
		<tr class="buttons">
		    <td></td>	
		    <td>
		    <input id = "RecaptchaChallenge" type="hidden" />
		    <input id = "RecaptchaResponse" type="hidden" />
			    <%= Html.NiceInputButton(this, "create", "") %>			    
	        </td>
		</tr>
		 
		</table>
    <% } %>
    
    
	</div>
	<div class="clear"></div>
	
</div>


</asp:Content>



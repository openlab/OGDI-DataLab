<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<% if (!Request.IsAuthenticated) { %>
  <script type="text/javascript">

    var isOpenIdInitialized = false;

    function toggleOpenId() {
      if ($('#showOpenIdButton').attr('checked')) {
        if (!isOpenIdInitialized) {
          $('form.openid').openid();
          isOpenIdInitialized = true;
        }
        $('#openIdContainer').show();
      } else {
        $('#openIdContainer').hide();
      }
    }
    
  </script>
  <input id="showOpenIdButton" type="checkbox" class="checkbox" onclick="toggleOpenId()" style="float:left;cursor:pointer"/>
  <label for="showOpenIdButton" style="cursor:pointer">Login using Open ID</label>
  <div id="openIdContainer" style="display:none">
    <% Html.RenderPartial("OpenId"); %>
  </div>
<% } else { %>
  <%=Html.ActionLink("Sign-out?", "LogOut", "CustomAccount", new {returnUrl = Request.Url+"#login"}, null)%>
<% } %>

<a name="login"> </a>

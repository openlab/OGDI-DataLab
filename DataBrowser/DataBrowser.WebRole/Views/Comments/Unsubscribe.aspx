<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/OGDIMasterPage.Master" Inherits="System.Web.Mvc.ViewPage<Ogdi.InteractiveSdk.Mvc.Models.Comments.Comment>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<script type="text/javascript">
    function Unsubscribe() {
        var globVPath = '<%= ResolveUrl("~/") %>';
        var parentId = '<%=Model.ParentName%>';
        var user = '<%=Model.Email%>';
        var parentType = '<%=Model.ParentType%>';
        var container = '<%=Model.ParentContainer%>';
        var result;
        $.ajax(
            {
                type: "POST",
                async: false,
                url: globVPath + "Comments/Unsubscribe/?id=" + parentId + "&type=" + parentType + "&user=" + user + "&container=" + container + "&accept=true",
                data: result,
                dataType: "json",
                success: function(data) {
                    $("div#unsubscribeInfo").hide();
                    $("div#success").show();
                },
                error: function(XMLHttpRequest, textStatus, errorThrown) {
                    alert(textStatus);
                    alert(errorThrown);
                }
            });
    }
</script>
<div style="padding:20px;">
    <div id="unsubscribeInfo">
        To unsubscribe from posts about <%=Model.ParentDisplay%> please click <a href="javascript:Unsubscribe();" style="font-size:14pt;">here</a>.
    </div>
    <div id="success" style="display:none">
        You are succesfully unsubscribed! Click <%= Html.ActionLink("here", "Index", "Home")%> to leave this page.
    </div>
</div>
</asp:Content>

<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.App_GlobalResources"  %>
<div class="bookmark">
<script type="text/javascript">
        var addthis_pub = '<%= UIConstants.BMC_AddThisPubKey %>';
</script>

<a href="<%= UIConstants.BMC_BookmarkPath %>" onclick="return addthis_sendto()" onmouseout="addthis_close()" onmouseover="return addthis_open(this, '', '[URL]', '[TITLE]')">
    <img alt="<%= UIConstants.BMC_ImageAltText %>" height="16" title="<%= UIConstants.BMC_BookmarkTitle %>" src="<%= UIConstants.BMC_ImagePath %>" style="border: 0" width="125" longdesc="<%= UIConstants.BMC_LongDescPath %>" />
</a>

<script type="text/javascript" src="http://s7.addthis.com/js/200/addthis_widget.js"></script>
</div>
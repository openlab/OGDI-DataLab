<%@ Page Language="C#" MasterPageFile="~/Views/Shared/OGDIMasterPage.Master"
 Inherits="System.Web.Mvc.ViewPage<Ogdi.InteractiveSdk.Mvc.Models.DataCatalogModel>" %>

<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.App_GlobalResources" %>
<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.Models" %>

<asp:Content ID="dataCatalogContent" ContentPlaceHolderID="MainContent" runat="server">

    <script type="text/javascript" language="javascript" src="http://ajax.googleapis.com/ajax/libs/jqueryui/1.7.1/jquery-ui.min.js "></script>
    <script type="text/javascript" language="javascript" src="<%= Url.Content("../../Scripts/DataCatalog.min.js") %>"></script>
    
    <!-- If you are using IE8, then COMMENT / UNCOMMENT this meta tag
     to demonstrate working in IE7 -->
    <%--<meta http-equiv="X-UA-Compatible" content="IE=EmulateIE7" />--%>
    
    <script src="http://ajax.microsoft.com/ajax/beta/0909/MicrosoftAjax.js" type="text/javascript"></script>  
    
    <script type="text/javascript">
        Sys.Application.set_enableHistory(true);        
        
        var ExpanderImage = '<%= UIConstants.GC_ExpandImagePath %>';
        var ExpanderImageDescription = '<%= UIConstants.GC_ExpandImageLongDesc %>';
        var CollapserImage = '<%= UIConstants.GC_CollapseImagePath %>';
        var siteTitle = '<%= UIConstants.SiteTitle %>';
    </script>
    <form id="form1">
        <div>
            <iframe id="__historyFrame" src="<%= Url.Action("Index", "HistoryIframeHelper") %>"
             style="display:none;"></iframe>
             
             <script type="text/javascript">
                 Sys.Application.add_init(page_init);        
             </script>
            <h1>
                Data Catalog</h1>
            <div>
                <div id="Para1" style="cursor: hand" title="Click for more details..." 
                class="collapsePanelHeader">
                    <p class="big">
                        <%= UIConstants.DCPC_DataCatalogIntro1 %> <img
                            id="ExpandCollapseImage" 
                            alt='<%= UIConstants.GC_ExpandCollapseImageAltText %>'
                            src='<%= UIConstants.GC_ExpandImagePath %>' 
                            title="Show Details" 
                            longdesc='<%=  UIConstants.GC_ExpandImageLongDesc %>' />
                    </p>
                </div>
                <div id="Para2" style="margin: 0px; padding-top: 0px;
                     display: none; padding-bottom: 15px;">
                    <p class="big" style="padding-top: 0px; margin: 0px">
                        <%= UIConstants.DCPC_DataCatalogIntro2 %>
                        <br />
                        <%= UIConstants.DCPC_DataCatalogIntro3 %>
                    </p>
                </div>
                <p class="big" style="margin: 0px; padding-top: 0px;">
                    Select Container:
                    <%= Html.DropDownList("Containers", 
                        ViewData.Model.ContainerList)%>
                </p>
                <div class="legallink">
                    <a id="legalDisclaimerLink"
                     href="javascript:legalDisclaimerLinkClicked()">LEGAL DISCLAIMER</a>
                </div>
                <div style="height: 8px;">
                    &nbsp;
                </div>
            </div>
            <div class="catalog">
                <table>
                    <tbody>
                        <tr id="leftPanelDiv" class="leftmenu">
                            <%--<% Html.RenderPartial("DataCategories", ViewData); %>--%>
                        </tr>
                        <tr>
                            <td valign="top">
                                <% Html.RenderPartial("Bookmark"); %>
                            </td>
                            <td valign="top">
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
        <div id="disclaimerPopup" title="LEGAL DISCLAIMER" 
        class="filterdialog" style="visibility: hidden;">
            <div id="disclaimerBody">
            </div>
        </div>
        <label visible="false" id="labelError" 
        title="<%= Html.Encode(ViewData.Model.ErrorLine1) %>">
        </label>
        <div id="BackGroundLoadingIndicator" class="bgLoadingIndicator"
             style="display: none">
            </div>
            <div id="LoadingIndicatorPanel"  style="display: none; position:  ">
                <img id="imgLoading" class="loader" alt='<%= UIConstants.GC_LoadingAltText %>'
                    style="display: none"
                    src='<%=UIConstants.GC_LoadingImagePath %>'
                    longdesc='<%= UIConstants.GC_LoadingLongDesc %>'
                     />
            </div>

        <script type="text/javascript">
            ShowHideError();       
        </script>
    </form>
</asp:Content>

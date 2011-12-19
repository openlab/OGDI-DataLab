<%@ Page Language="C#" MasterPageFile="~/Views/Shared/OGDIMasterPage.Master" 
Inherits="System.Web.Mvc.ViewPage<Ogdi.InteractiveSdk.Mvc.Models.HomeModel>" %>
<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.App_GlobalResources"  %>
<asp:Content ID="indexContent" ContentPlaceHolderID="MainContent" runat="server">
    
    <div class="welcome">
		
		<%= Ogdi.Azure.Configuration.OgdiConfiguration.GetValue("HomePageHeading")%>
        <%= Ogdi.Azure.Configuration.OgdiConfiguration.GetValue("HomePageContent")%>        
        
        <table style="width:100%;">
            <tr>
                <td style="width:50%;">
                    <h2>
                        <a href='<%= UIConstants.HPC_DataCatalogPagePath %>'>Browse the Data</a>
                    </h2>
                    <p class="smalldescription">
                        Browse, query, and view sample datasets.
                    </p>
                </td>
                <td>
                    <h2>
                        <a href="<%= UIConstants.HPC_DeveloperPagePath %>">For Developers</a>
                    </h2>
                    <p class="smalldescription">
                        Read brief developer notes and learn how to use the OGDI API.
                    </p>
                </td>
            </tr>
            <tr style="height:350px;">
                <td align="left">
                    <% Html.RenderPartial("BlogsAndAnnouncements"); %>    
                </td>
                <td>
                    <div class="boxTopMedia" style="margin-left: -6px;">
                    </div>
                    <div class="boxMidMedia" style="width: 440px; height: 350px; margin-left: -6px;">
                        <div class="boxMidPadMedia" style="margin-left: -6px;">
                            <div style="font-size: 24px; height: 25px; color: #a3a3a3; line-height: 14pt;">
                                <b>Get started with OGDI</b></div>
                            <br />
                            <div class="welcomevideocontainer" style="margin-left: 7px;">
                                <div class="welcomevideo">
                                    <object data="data:application/x-silverlight-2," type="application/x-silverlight-2"
                                        width="360" height="300">
                                        <param name="source" value="http://channel9.msdn.com/App_Themes/default/VideoPlayer10_01_18.xap" />
                                        <param name="initParams" value="deferredLoad=true,duration=0,m=http://ecn.channel9.msdn.com/o9/ch9/8/0/6/2/4/5/OgdiWelcomeVideoR2_ch9.wmv,autostart=false,autohide=true,showembed=true, thumbnail=http://ecn.channel9.msdn.com/o9/ch9/8/0/6/2/4/5/OgdiWelcomeVideoR2_512_ch9.png, postid=542608" />
                                        <param name="background" value="#00FFFFFF" />
                                        <!-- BEGIN original flash video object tag -->
                                        <object classid="<%= UIConstants.HPC_ClassId %>" codebase="<%= UIConstants.HPC_CodeBase %>"
                                            width="360" height="300" id="<%= UIConstants.HPC_Id %>" align="middle">
                                            <param name="allowScriptAccess" value="always" />
                                            <param name="allowFullScreen" value="true" />
                                            <param name="movie" value="<%= Ogdi.Azure.Configuration.OgdiConfiguration.GetValue("WelcomeVideoURL")%>" />

                                            <param name="quality" value="high" />
                                            <param name="bgcolor" value="#ffffff" />
                                            <param name="wmode" value="opaque" />
                                            <embed src="<%= Ogdi.Azure.Configuration.OgdiConfiguration.GetValue("WelcomeVideoURL")%>" quality="high"
                                                bgcolor="#ff0000" width="360" height="300" align="middle" wmode="transparent"
                                                allowscriptaccess="always" allowfullscreen="true" type="application/x-shockwave-flash"
                                                pluginspage="http://www.macromedia.com/go/getflashplayer" />


                                        </object>
                                        <!-- END original video object tag -->
                                    </object>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="boxBottomMedia" style="margin-left: -6px;">
                    </div>
                </td>
            </tr>
        </table>        
        <% Html.RenderPartial("Bookmark"); %>
    </div>
    <div style="clear: both; position: relative">
    </div>
</asp:Content>

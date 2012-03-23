<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Ogdi.InteractiveSdk.Mvc.Models.HomeModel>" %>

<div class="boxTopMedia" style="margin-left:-6px;"></div>  
<div class="boxMidMedia" style="width:440px; height:350px; margin-left:-6px;">
    <div class="boxMidPadMedia" style="margin-left:-6px;">    
  
    <table style="width:95%;">
        <tr style="vertical-align:top;">
            <td>
                <div style="font-size:24px; height:38px; color:#a3a3a3; line-height:14pt;"><b>Announcements and News</b></div>
            </td>
        </tr>
        <% int i = 1;
       int numberOfLinks = Convert.ToInt32(Ogdi.Azure.Configuration.OgdiConfiguration.GetValue("NumberOfBlogLinksOnPage"),
           System.Globalization.CultureInfo.InvariantCulture);
        
       foreach (Ogdi.InteractiveSdk.Mvc.Models.BlogAndAnnouncement item in 
           ViewData.Model.NewsData)
       {
           if (i > numberOfLinks)
           {
               break;
           }
           else
           {%>        
            <tr>
                <td>
                    <b><a href='<%= item.Link %>'><%= item.Title %></a></b>
                </td>
            </tr>            
            <tr>
                <td>
                    <%= item.Description %>
                </td>
            </tr>
            <tr style="height:8pt;">
                <td style="font-size:smaller; color:#a3a3a3;">
                    <%= item.PublishDate %>
                    <hr style="color:#a3a3a3;" />
                </td>
            </tr>            
            <% i++;
               } %>
        <% } %>
    </table>
</div>  
</div> 
<div class="boxBottomMedia" style="margin-left:-6px;"></div>


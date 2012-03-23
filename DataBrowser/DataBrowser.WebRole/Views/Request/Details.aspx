<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc" %>
<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/OGDIMasterPage.Master" Inherits="OgdiViewPage<Ogdi.InteractiveSdk.Mvc.Models.Request.Request>" %>
<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.Models" %>
<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.Models.Request" %>
<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.Models.ViewData" %>
<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.Models.Comments" %>
<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.Models.Rating" %>

<asp:Content runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>

<asp:Content ID="mainContent" ContentPlaceHolderID="MainContent" runat="server">

	<div class="request-details dataset">
		<div class="name"><%= Html.Encode(Model.Subject) %></div>
		<div class="description"><%= Html.Encode(Model.Description) %></div>
		<div class="parameters">
			<div class="left">
				<div class="label">Status</div>
				<div><%= Html.Encode(Model.Status) %></div>
			</div>
			<div class="left">
				<div class="label">Posted Date</div>
				<div><%= Html.Encode(String.Format("{0:" + Ogdi.InteractiveSdk.Mvc.Globals.ShortDateFormat + "}", Model.PostedDate)) %></div>
			</div>
			<div class="left">
				<div class="label">Rating</div>
				<div class="rates"><% Html.RenderPartial("Rates", new RateInfo(Model.ItemKey, Model.PositiveVotes, Model.NegativeVotes) { ReadOnly = false }); %></div>
			</div>
			<div class="left">
				<div class="label">Views</div>
				<div class="rates"><%= this.Model.Views %></div>
			</div>
			<div class="clear"></div>
		</div>
		
		<% if (! String.IsNullOrEmpty(Model.Links)) { %>
			<div class="param">
				<div class="label">Links</div>
				<div class="value"><%= Html.Encode(Model.Links)%></div>
				<div class="clear"></div>
			</div>
		<% } %>
		<% if (! String.IsNullOrEmpty(Model.DatasetLink)) { %>
			<div class="param">
				<div class="label">Dataset Link</div>
				<div class="value"><%= Html.Encode(Model.DatasetLink) %></div>
				<div class="clear"></div>
			</div>
		<% } %>
		
	</div>
	
	
		    
    <div id="dataSetViewComments" class="block">
		<% Html.RenderPartial("Comments", CommentInfo.CreateRequestCommentInfo(Model.RequestID)); %>
     </div>
     

</asp:Content>


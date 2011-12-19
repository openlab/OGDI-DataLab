<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.Models" %>
<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc.Models.Rating" %>
<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc"%>
<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<EntitySetWrapper>" %>

<% var model = this.Model.EntitySet; %>
<div class="dataset">
	<div class="name"><%= Html.Encode(model.Name) %></div>
	<div class="description"><%= Html.Encode(model.Description) %></div>		
	<div class="parameters">
		<div class="left">
			<div class="label">Category</div>
			<div><%= Html.Encode(model.CategoryValue) %></div>
		</div>
		<div class="left">
			<div class="label">Date</div>
			<div><%= model.LastUpdateDate.ToString(Globals.ShortDateFormat) %></div>
		</div>
		<div class="left">
			<div class="label">Rating</div>
			
			<div class="rates"><% Html.RenderPartial("Rates", new RateInfo(Model.EntitySet.ItemKey, Model.PositiveVotes, Model.NegativeVotes) { ReadOnly = false } ); %></div>
		</div>
		<div class="left">
			<div class="label">Views</div>
			<div><%= this.Model.Views %></div>
		</div>
		<div class="clear"></div>
	</div>
</div>

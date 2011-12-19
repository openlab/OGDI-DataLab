<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Ogdi.InteractiveSdk.Mvc.Models.Rating.VoteResults>" %>
<%@ Import Namespace="Ogdi.InteractiveSdk.Mvc"%>

<div class="numbers">
	<% if (this.Model.Positive > 0) { %>
		<span class="positive">+ <%= Model.Positive %></span>
		<% if (this.Model.Negative > 0) { %>
		/	<span class="negative">- <%= Model.Negative %></span>
	<% }} else if (this.Model.Negative > 0) { %>
		<span class="negative"> - <%= Model.Negative %></span>
	<% } else { %>
		<span class="not-rated">not rated</span>
	<% } %>
</div>

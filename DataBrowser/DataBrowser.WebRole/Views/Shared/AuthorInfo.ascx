<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Ogdi.InteractiveSdk.Mvc.Models.AuthorInfo>" %>

<div class="value">
	<% if (!string.IsNullOrEmpty(Model.Name))
    { %>
		    <%= Model.Name%>
		<% if (!string.IsNullOrEmpty(Model.Email))
     { %>
		        ( <a href="mailto:<%= Model.Email %>"><%= Model.Email%></a> )				    		    
        <%	}  %>		    		    
<%	}
    else
    { %>    
    Anonymous    
<% } %>
</div>


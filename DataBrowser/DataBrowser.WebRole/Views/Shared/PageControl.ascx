<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<div class="page-size">
  Items per page
  <select id="perpage">
    <option id="option5" value="5">5</option>
    <option id="option10" value="10">10</option>
    <option id="option15" value="15">15</option>
    <option id="option50" value="50">50</option>
  </select>
</div>
<div id="eidPagingControl" class="paging"></div>

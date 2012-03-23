<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Index</title>
	<link rel="Stylesheet" type="text/css" href="../../Resource/Css" />
	<script type="text/javascript" src="../../Scripts/common.js"></script>
	<script type="text/javascript" src="../../Scripts/querybuilder.js"></script>
	<script type="text/javascript" src="http://ajax.microsoft.com/ajax/jquery/jquery-1.4.2.min.js"></script>

</head>
<body>
	<div id="eidBuilder"></div>
	<textarea cols="80" rows="10" id="eidTarget"></textarea>
	<script type="text/javascript">


		$(document).ready(function() {
			var meta = new Array();
			meta.push({ name: "Field One", type: 3 });
			meta.push({ name: "Field Two", type: 1 });
			var builder = new Ogdi.QueryBuilder($("#eidBuilder"), meta, "eidTarget");
		});

	</script>

</body>
</html>

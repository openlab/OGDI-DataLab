<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Ogdi.InteractiveSdk.Mvc.Models.Comments.CommentInfo>" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Test</title>
	<link href="../../Resource/Css" rel="stylesheet" type="text/css" />
	<link href="../../Content/css/csharp.css" rel="stylesheet" type="text/css" />
	<link href="../../Content/css/tabs.css" rel="stylesheet" type="text/css" />
	<link href="../../Content/css/tab-themes.css" rel="stylesheet" type="text/css" />
	<link href="../../Content/css/round.css" rel="stylesheet" type="text/css" />
	<link href="../../Content/css/round-themes.css" rel="stylesheet" type="text/css" />
	<link href="../../Content/css/redmond/jquery-ui-1.7.1.custom.css" rel="stylesheet"
		type="text/css" />
	<script src="http://ajax.microsoft.com/ajax/jquery/jquery-1.4.2.min.js" type="text/javascript"></script>

	<script type="text/javascript" src="http://api.recaptcha.net/js/recaptcha_ajax.js"></script>

</head>
<body>
	<% Html.RenderPartial("Comments", this.Model); %>
</body>
</html>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Ogdi.InteractiveSdk.Mvc
{
	public static class HtmlExtender
	{
		public static string NiceButton(this HtmlHelper helper, IOgdiUI ui, string name, int width, string id)
		{
			return @"<span " + (String.IsNullOrEmpty(id) ? "" : " id=\"" + id + "\"") + @" class=""button""><img src=""" + ui.SpecialUrls.InvisibleImageUrl + @""" class=""btn-left"" /><img src=""" + ui.SpecialUrls.Button(name) + @""" class=""btn-middle"" " + (width > 0 ? " style=\"width:" + width + "px;\"" : "") +  @" /><img src=""" + ui.SpecialUrls.InvisibleImageUrl + @""" class=""btn-right"" /></span>";
		}

		public static string NiceInputButton(this HtmlHelper helper, IOgdiUI ui, string name, string onclick)
		{
			return @"<span class=""button""><img src=""" + ui.SpecialUrls.InvisibleImageUrl + @""" class=""btn-left"" /><input type=""image"" src=""" + ui.SpecialUrls.Button(name) + @""" value=""UpdateStatus"" onclick=""" + onclick + @""" /><img src=""" + ui.SpecialUrls.InvisibleImageUrl + @""" class=""btn-right"" /></span>";
		}

        public static bool HasFile(this HttpPostedFileBase file)
        {
            return (file != null && file.ContentLength > 0) ? true : false;
        }
	}
}

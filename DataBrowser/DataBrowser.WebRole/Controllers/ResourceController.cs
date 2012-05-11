using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Text.RegularExpressions;
using System.Text;

namespace Ogdi.InteractiveSdk.Mvc.Controllers
{
	public class ResourceController : Controller
	{
		public ActionResult Css()
		{
			string text = System.IO.File.ReadAllText(this.Server.MapPath(ResourceControllerResources.ResourceController.LocalizedCssPath));

			Dictionary<String, String> map = new Dictionary<String, String>();
			Regex declarations = new Regex(@"\/\*\{([^\}]+)\}\*\/");
			{
				var match = declarations.Match(text);
				while (match.Success)
				{
					var found = (match.Groups[1].Value ?? String.Empty).Trim();
					var tokens = found.Split(new char[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);
					if (tokens.Length > 1)
					{
						map.Add(tokens[0], tokens[1]);
					}
					match = match.NextMatch();
				}
			}

			Regex placement = new Regex(@"\/\*\[([^\]]+)\]\*\/");
			var processed = placement.Replace(text, match =>
				{
					var found = (match.Groups[1].Value ?? String.Empty).Trim();
					var tokens = found.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

					StringBuilder builder = new StringBuilder();
					foreach (string token in tokens)
					{
						string value;
						if (map.TryGetValue(token, out value))
						{
							builder.Append(value);
						}
					}

					return builder.ToString();
				});

			return this.Content(processed, "text/css");
		}

	}
}

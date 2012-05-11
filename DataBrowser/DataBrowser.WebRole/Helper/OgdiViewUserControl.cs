
namespace Ogdi.InteractiveSdk.Mvc
{
	public interface IOgdiUI
	{
		SpecialUrls SpecialUrls { get; }
	}

	public interface IUrlResolver
	{
		string ResolveUrl(string relative);
	}

	public class SpecialUrls
	{
		private IUrlResolver control;
		public SpecialUrls(IUrlResolver control)
		{
			this.control = control;
		}
		public string InvisibleImageUrl { get { return this.control.ResolveUrl("~/Images/t.gif"); } }
		public string Button(string name)
		{
			return this.control.ResolveUrl("~/Content/css/buttons/" + name + ".png");
		}
	}

	public class OgdiViewUserControl<TModel> : System.Web.Mvc.ViewUserControl<TModel>, IUrlResolver, IOgdiUI
		where TModel : class
	{
		public OgdiViewUserControl()
		{
			this.SpecialUrls = new SpecialUrls(this);
		}
		public SpecialUrls SpecialUrls { get; private set; }
	}

	public abstract class OgdiViewPage<TModel> : System.Web.Mvc.ViewUserControl<TModel>, IUrlResolver, IOgdiUI
		where TModel : class
	{
		public OgdiViewPage()
		{
			this.SpecialUrls = new SpecialUrls(this);
		}
		public SpecialUrls SpecialUrls { get; private set; }

		public string ResolveUrl(string relative)
		{
			return Url.Content(relative);
		}
	}


	public abstract class OgdiWebViewPage<TModel> : System.Web.Mvc.WebViewPage<TModel>, IUrlResolver, IOgdiUI
		where TModel : class
	{
		public OgdiWebViewPage()
		{
			this.SpecialUrls = new SpecialUrls(this);
			Layout = null;
		}
		public SpecialUrls SpecialUrls { get; private set; }

		public string ResolveUrl(string relative)
		{
			return Url.Content(relative);
		}
	}
}


namespace Ogdi.InteractiveSdk.Mvc.Models.ViewData
{
	public enum AgencyTab
	{
		Comments, Requests, Reports
	}


	public class AgencyTabsViewModel
	{
		public AgencyTab Active {get;private set;}
		public AgencyTabsViewModel(AgencyTab active)
		{
			this.Active = active;
		}
	}
}

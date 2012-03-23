using System.Collections.Generic;
using System.Web.Mvc;

namespace Ogdi.InteractiveSdk.Mvc.Models
{
    public class DataCatalogModel
    {
        #region Properties

    	public string CurrentCategory { get; set; }

    	public SelectList ContainerList { get; set; }

    	public string ErrorLine2 { get; set; }

    	public string ErrorLine1 { get; set; }

    	public List<EntitySet> EntitySet { get; set; }

    	public List<string> CategoryList { get; set; }

    	public string CategoryName { get; set; }

    	#endregion

        #region Constructors

        public DataCatalogModel()
        {
            CategoryList = new List<string>();
            CategoryName = string.Empty;
            ContainerList = new SelectList(new List<Container>());                    
            CurrentCategory = string.Empty;
            EntitySet = new List<EntitySet>();
            ErrorLine1 = string.Empty;
            ErrorLine2 = string.Empty;
        }

        #endregion
    }
}

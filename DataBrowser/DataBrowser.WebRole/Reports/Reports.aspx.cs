using System;
using Ogdi.InteractiveSdk.Mvc.Models.Reports;

namespace Ogdi.InteractiveSdk.Reports
{
    public partial class Reports : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var list = ReportDataSource.Select();

            if(!IsPostBack)
            {
                this.DropDownList1.DataSource = list;
                this.DropDownList1.DataTextField = "DisplayName";
                this.DropDownList1.DataValueField = "Name";
                this.DropDownList1.DataBind();

                this.From.SelectedDate = DateTime.Now.Date;
                this.To.SelectedDate = DateTime.Now.Date.AddDays(+1);
            }

            //this.ReportViewer1.Reset();

            ReportEntry re = list[DropDownList1.SelectedIndex];

            this.ReportViewer1.LocalReport.ReportEmbeddedResource = "Ogdi.InteractiveSdk.Mvc.Reports.data." + re.Name;

            this.ObjectDataSource1.SelectMethod = re.Method;
            this.ObjectDataSource1.TypeName = re.Type;
            this.ObjectDataSource1.DataBind();

            //this.ReportViewer1.LocalReport.DataSources.Add(
            //    new Microsoft.Reporting.WebForms.ReportDataSource("Data", "ObjectDataSource1"));

            this.ReportViewer1.LocalReport.Refresh();
        }
    }
}

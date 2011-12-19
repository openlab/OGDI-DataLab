using System.Collections.Generic;

namespace Ogdi.InteractiveSdk.Mvc.Models.Reports
{
    public class ReportDataSource
    {
        static List<ReportEntry> _reports = new List<ReportEntry>()
        {
            new ReportEntry()
            {
                DisplayName = "Dataset Comments",
                 Name = "DatasetCommentsReport.rdlc",
                  Description = "Dataset comments report",
                   Method = "GetList",
                    Type = "Ogdi.InteractiveSdk.Mvc.Reports.DatasetCommentsDataSource",
            },
            new ReportEntry()
            {
                DisplayName = "Dataset Rates",
                 Name = "DatasetRatesReport.rdlc",
                  Description = "Dataset rates report",
                   Method = "GetRateList",
                    Type = "Ogdi.InteractiveSdk.Mvc.Reports.DatasetsDataSource",
            },
            new ReportEntry()
            {
                DisplayName = "Request Rates",
                 Name = "RequestRatesReport.rdlc",
                  Description = "Request rates reports",
                   Method = "GetRateList",
                    Type = "Ogdi.InteractiveSdk.Mvc.Reports.RequestsDataSource",
            },
             new ReportEntry()
            {
                 DisplayName = "Daily Views",
                 Name = "DailyViews.rdlc",
                  Description = "",
                   Method = "GetViews",
                    Type = "Ogdi.InteractiveSdk.Mvc.Reports.ViewReportDataSource",
            }
        };

        static public IList<ReportEntry> Select()
        {
            return _reports;
        }

       
    }
}

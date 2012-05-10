using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ogdi.Azure.Data;
using Ogdi.Azure.Views;
using System.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Ogdi.Azure.Configuration;

namespace Ogdi.Azure
{
    static public class AnalyticsRepository
    {
        static public void RegisterView(String itemKey, String url, String user)
        {
            DatasetInfoDataSource datasetInfoDataSource = new DatasetInfoDataSource();
            ViewDataSource viewDS = new ViewDataSource();

            datasetInfoDataSource.IncrementView(itemKey);

            // No logging if analytics are disabled
            if (OgdiConfiguration.GetValue("IsAnalytics") == "0") return;

            viewDS.AddView(new ViewEntry()
            {
                Date = DateTime.Now,
                ItemKey = itemKey,
                User = user,
                RequestedUrl = url,
            });
        }
    }
}

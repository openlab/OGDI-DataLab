using System;
using Ogdi.Azure.Data;
using Ogdi.Azure.Views;

namespace Ogdi.Azure
{
    static public class AnalyticsRepository
    {
        static public void RegisterView(String itemKey, String url, String user)
        {
            var datasetInfoDataSource = new DatasetInfoDataSource();
            var viewDS = new ViewDataSource();

            datasetInfoDataSource.IncrementView(itemKey);
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

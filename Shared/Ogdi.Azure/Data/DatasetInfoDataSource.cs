using System;
using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using Ogdi.Azure.Configuration;
using System.Collections.Generic;

namespace Ogdi.Azure.Data
{
	public sealed class DatasetInfoDataSource
	{
		private static readonly CloudStorageAccount StorageAccount;
		private static readonly object Mutex = new object();

		private readonly DatasetInfoDataContext _context;

		static DatasetInfoDataSource()
		{
			StorageAccount = CloudStorageAccount.Parse(OgdiConfiguration.GetValue("DataConnectionString"));

			CloudTableClient.CreateTablesFromModel(
				typeof(DatasetInfoDataContext),
				StorageAccount.TableEndpoint.AbsoluteUri,
				StorageAccount.Credentials);
		}

		public DatasetInfoDataSource()
		{
			_context = new DatasetInfoDataContext(StorageAccount.TableEndpoint.AbsoluteUri, StorageAccount.Credentials) { RetryPolicy = RetryPolicies.Retry(3, TimeSpan.FromSeconds(1)) };
		}

		public void IncrementView(string itemKey)
		{
			lock (Mutex) // NOTE: This synchronization is not enough since there may be several instances of roles.
			{
				var result = GetOrCreateAnalyticInfo(itemKey);

				if (result.last_viewed.Date == DateTime.Today)
				{
					result.last_viewed = DateTime.Now;
					result.views_today += 1;
					result.views_total += 1;
				}
				else
				{
					result.last_viewed = DateTime.Now;
					result.views_average = result.views_total * result.views_average / (result.views_total - result.views_today + result.views_average);
					result.views_total += 1;
					result.views_today = 1;
				}

				_context.UpdateObject(result);
				_context.SaveChanges();
			}
		}

		public void IncrementVote(string itemKey, int vote)
		{
			lock (Mutex) // NOTE: This synchronization is not enough since there may be several instances of roles.
			{
				var result = GetOrCreateAnalyticInfo(itemKey);

                if (vote < 0)
                {
                    result.NegativeVotes += -vote;
                }
                else
                {
                    result.PositiveVotes += vote;
                }

				_context.UpdateObject(result);
				_context.SaveChanges();
			}
		}

		public IEnumerable<AnalyticInfo> SelectAll()
		{
			return from dsi in _context.AnalyticInfo select dsi;
		}

		public AnalyticInfo GetAnalyticSummary(string itemKey)
		{
			return GetOrCreateAnalyticInfo(itemKey);
		}

		private AnalyticInfo GetOrCreateAnalyticInfo(string itemKey)
		{
			AnalyticInfo dataset = (from info in _context.AnalyticInfo
									where info.RowKey == itemKey
									select info).FirstOrDefault();

            if (dataset != null)
            {
                return dataset;
            }

			dataset = new AnalyticInfo(itemKey)
						{
							last_viewed = DateTime.Now,
							views_today = 1,
							views_total = 1,
							views_average = 1,
							NegativeVotes = 0,
							PositiveVotes = 0,
						};

			_context.AddObject(DatasetInfoDataContext.AnalyticInfoTableName, dataset);
			_context.SaveChanges();

			return dataset;
		}
	}
}

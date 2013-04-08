using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Xml;
using InteractiveSdk.WorkerRole.MessageHandlers;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;

namespace InteractiveSdk.WorkerRole
{
	public class WorkerRole : RoleEntryPoint
	{
		private CloudQueue queue;
		private CloudBlobContainer container;
		private IDictionary<string, IMessageHandler> handlers;

		public override void Run()
		{
			InitializeHandlers();

			Trace.TraceInformation("Listening for queue messages...");

			while (true)
			{
				try
				{
					CloudQueueMessage msg = queue.GetMessage();
					if (msg != null)
					{
						queue.DeleteMessage(msg);
						ProcessMessage(msg);
					}
					else
					{
						Thread.Sleep(1000);
					}
				}
				catch (Exception e)
				{
					Trace.TraceError("Exception when processing queue item. Message: '{0}'", e.Message);
					Thread.Sleep(10000);
				}
			}
		}

		private void InitializeHandlers()
		{
			handlers = new Dictionary<string, IMessageHandler>
			{
				{"SendMail", new SendMailHandler()},
				{"ConvertData", new ConvertDataHandler(container)}
			};
		}

		public void ProcessMessage(CloudQueueMessage msg)
		{
			var msgStr = msg.AsString;
            using (var strReader = new StringReader(msgStr ?? string.Empty))
            {
                using (var xmlReader = XmlReader.Create(strReader))
                {
                    xmlReader.ReadToDescendant("command");
                    var commandName = xmlReader.GetAttribute("commandname") ?? string.Empty;
                    IMessageHandler handler;
                    if (handlers.TryGetValue(commandName, out handler))
                    {
                        var body = xmlReader.ReadInnerXml();
                        handler.ProcessMessage(body);
                    }
                }
            }
		}

		public override bool OnStart()
		{
			// Set the maximum number of concurrent connections 
			ServicePointManager.DefaultConnectionLimit = 12;

			DiagnosticMonitor.Start("DiagnosticsConnectionString");

			// For information on handling configuration changes
			// see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.
			RoleEnvironment.Changing += RoleEnvironmentChanging;

			// read storage account configuration settings
			CloudStorageAccount.SetConfigurationSettingPublisher((configName, configSetter)
				=> configSetter(RoleEnvironment.GetConfigurationSettingValue(configName)));

			var storageAccount = CloudStorageAccount.FromConfigurationSetting("DataConnectionString");

			// initialize blob storage
			CloudBlobClient blobStorage = storageAccount.CreateCloudBlobClient();
			container = blobStorage.GetContainerReference("converteddata");

			// initialize queue storage 
			CloudQueueClient queueStorage = storageAccount.CreateCloudQueueClient();
			queue = queueStorage.GetQueueReference("workercommands");

			Trace.TraceInformation("Creating container and queue...");

			bool storageInitialized = false;
			while (!storageInitialized)
			{
				try
				{
					// create the blob container and allow public access
					container.CreateIfNotExist();
					var permissions = container.GetPermissions();
					permissions.PublicAccess = BlobContainerPublicAccessType.Container;
					container.SetPermissions(permissions);


					// create the message queue
					queue.CreateIfNotExist();
					storageInitialized = true;
				}
				catch (StorageClientException e)
				{
					if (e.ErrorCode == StorageErrorCode.TransportError)
					{
						Trace.TraceError("Storage services initialization failure. "
						  + "Check your storage account configuration settings. If running locally, "
						  + "ensure that the Development Storage service is running. Message: '{0}'", e.Message);
						Thread.Sleep(5000);
					}
					else
					{
						throw;
					}
				}
			}

			return base.OnStart();
		}

		private static void RoleEnvironmentChanging(object sender, RoleEnvironmentChangingEventArgs e)
		{
			// If a configuration setting is changing
			if (e.Changes.Any(change => change is RoleEnvironmentConfigurationSettingChange))
			{
				// Set e.Cancel to true to restart this role instance
				e.Cancel = true;
			}
		}
	}
}

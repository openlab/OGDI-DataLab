using System.IO;
using System.Xml;
using System.Xml.Linq;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;
using Ogdi.InteractiveSdk.Mvc;

namespace InteractiveSdk.WorkerRole.MessageHandlers
{
    /// <summary>
    /// Converts dataset to several formats to storage blobs
    /// </summary>
    internal class ConvertDataHandler : IMessageHandler
    {
        private CloudBlobContainer container;

        internal ConvertDataHandler(CloudBlobContainer container)
        {
            this.container = container;
        }

        public void ProcessMessage(string msg)
        {            
            var messageParts = msg.Split(new char[] { ',' });
            var containerName = messageParts[0];
            var tableName = messageParts[1];

            // Get service interface to access azure storage
            string serviceUri = RoleEnvironment.GetConfigurationSettingValue("serviceUri");
            string pathDTD = RoleEnvironment.GetConfigurationSettingValue("pathDTD");
            IsdkStorageProviderInterface service = IsdkStorageProviderInterface.GetServiceObject(serviceUri, pathDTD);

            // Get dbf data and save to blob
            XDocument dataInDbfFormat = service.GetDataAsDaisy(containerName, tableName, null);
            CloudBlockBlob dbfBlob = container.GetBlockBlobReference(tableName + ".xml");
            using (MemoryStream xmlStream = new MemoryStream())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(xmlStream))
                {
                    dataInDbfFormat.Save(xmlWriter);
                    xmlWriter.Close();
                    xmlStream.Position = 0;
                    dbfBlob.UploadFromStream(xmlStream);
                }
            }
            
            // Get csv data and save to blob
            string dataInCsvFormat = service.GetdDataAsCsv(containerName, tableName, null);
            CloudBlockBlob csvBlob = container.GetBlockBlobReference(tableName + ".csv");
            using (MemoryStream csvStream = new MemoryStream())
            {
                using (StreamWriter sw = new StreamWriter(csvStream))
                {
                    sw.Write(dataInCsvFormat);
                    sw.Flush();
                    csvStream.Position = 0;
                    csvBlob.UploadFromStream(csvStream);
                    sw.Close();
                }
            }
        }
    }
}

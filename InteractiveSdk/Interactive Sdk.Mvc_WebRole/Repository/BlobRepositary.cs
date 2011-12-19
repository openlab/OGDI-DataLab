using System.IO;
using System.Xml;
using System.Xml.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using Ogdi.Azure.Configuration;

namespace Ogdi.InteractiveSdk.Mvc.Repository
{

    internal class BlobRepositary
    {
        private static CloudBlobContainer convertedData;

        static BlobRepositary()
        {
			CloudStorageAccount storageAccount = CloudStorageAccount.Parse(OgdiConfiguration.GetValue("OgdiConfigConnectionString"));

            // initialize blob storage
            CloudBlobClient blobStorage = storageAccount.CreateCloudBlobClient();
            convertedData = blobStorage.GetContainerReference("converteddata");
        }

        internal static string GetBlobUrl(string containerName, string tableName, string ext)
        {
            string blobAdressUri = tableName + ext;
            CloudBlockBlob blod = convertedData.GetBlockBlobReference(blobAdressUri);
            // Check if blob exists
            try
            {
                blod.FetchAttributes();
            }
            catch (StorageClientException)
            {
                blod = CreateBlob(containerName, tableName, ext);
            }
            return blod.Uri.ToString();
        }

        internal static CloudBlockBlob CreateBlob(string containerName, string tableName, string ext)
        {

            IsdkStorageProviderInterface service = Helper.ServiceObject;

            switch (ext)
            {
                case ".xml":
                    // Get dbf data and save to blob
                    XDocument dataInDbfFormat = service.GetDataAsDaisy(containerName, tableName, null);
                    CloudBlockBlob dbfBlob = convertedData.GetBlockBlobReference(tableName + ".xml");
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
                    return dbfBlob;

                case ".csv":
                    // Get csv data and save to blob
                    string dataInCsvFormat = service.GetdDataAsCsv(containerName, tableName, null);
                    CloudBlockBlob csvBlob = convertedData.GetBlockBlobReference(tableName + ".csv");
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
                    return csvBlob;

            }

            return null;
        }
    }
}
using System.Xml.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System.IO;
using System.Xml;
using Ogdi.Azure.Configuration;
using System;
using System.Text;

namespace Ogdi.InteractiveSdk.Mvc.Repository
{

	internal class BlobRepositary
	{
		private static CloudBlobContainer convertedData;

		static BlobRepositary()
		{
			CloudStorageAccount storageAccount = CloudStorageAccount.Parse(OgdiConfiguration.GetValue("DataConnectionString"));

			// initialize blob storage
			CloudBlobClient blobStorage = storageAccount.CreateCloudBlobClient();
			convertedData = blobStorage.GetContainerReference("converteddata");
		}

        internal static string GetBlobUrl(string containerName, string tableName, string ext, DateTime DateMAJ)
        {
            CloudBlobDirectory ContainerDirectory = convertedData.GetDirectoryReference(containerName);
            string blobAdressUri = tableName + ext;
            CloudBlockBlob blod = ContainerDirectory.GetBlockBlobReference(blobAdressUri);

            // Check if blob exists
            try
            {
                blod.FetchAttributes();
                if (DateMAJ.Hour == 0 && DateMAJ.Minute == 0 && DateMAJ.Second == 0)
                {
                    if (DateTime.Compare(blod.Properties.LastModifiedUtc, DateMAJ.AddDays(1)) < 0) blod = CreateBlob(containerName, tableName, ext);
                }
                else
                {
                    if (DateTime.Compare(blod.Properties.LastModifiedUtc, DateMAJ) < 0) blod = CreateBlob(containerName, tableName, ext);
                }
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
            CloudBlobDirectory ContainerDirectory = convertedData.GetDirectoryReference(containerName);
            switch (ext)
            {
                case ".xml":
                    // Get dbf data and save to blob
                    XDocument dataInDbfFormat = service.GetDataAsDaisy(containerName, tableName, null);
                    CloudBlockBlob dbfBlob = ContainerDirectory.GetBlockBlobReference(tableName + ext);
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

                    CloudBlockBlob csvBlob = ContainerDirectory.GetBlockBlobReference(tableName + ext);
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
                case ".ANSI.csv":
                    // Get csv data and save to blob for Excel 
                    string dataInANSICsvFormat = service.GetdDataAsCsv(containerName, tableName, null);

                    //Encoding winLatinCodePage = Encoding.GetEncoding(1252);
                    Encoding winLatinCodePage = Encoding.GetEncoding(Convert.ToInt32(OgdiConfiguration.GetValue("ANSICodePage")));

                    CloudBlockBlob ANSIcsvBlob = ContainerDirectory.GetBlockBlobReference(tableName + ext);
                    MemoryStream ANSIcsvStream = new MemoryStream(Encoding.Convert(Encoding.Unicode, winLatinCodePage, Encoding.Unicode.GetBytes(dataInANSICsvFormat)));

                    ANSIcsvStream.Position = 0;
                    ANSIcsvBlob.UploadFromStream(ANSIcsvStream);

                    return ANSIcsvBlob;

            }

            return null;
        }

	}
}
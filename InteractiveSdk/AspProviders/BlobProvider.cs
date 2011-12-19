//
// <copyright file="BlobProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;


namespace Microsoft.Samples.ServiceHosting.AspProviders
{

     public enum EventKind
     {
          Critical,
          Error,
          Warning,
          Information,
          Verbose
     };

    static class Log
    {
        internal static void Write(EventKind eventKind, string message, params object[] args)
        {
            switch (eventKind)
            {
                case EventKind.Error:
                case EventKind.Critical:
                    Trace.TraceError(message, args);
                    break;
                case EventKind.Warning:
                    Trace.TraceWarning(message, args);
                    break;
                case EventKind.Information:
                case EventKind.Verbose:
                    Trace.TraceInformation(message, args);
                    break;
            }
        }
    }

    internal class BlobProvider
    {
        private CloudBlobClient _client;
        private CloudBlobContainer _container;
        private string _containerName;
        private object _lock = new object();

        private static readonly TimeSpan _Timeout = TimeSpan.FromSeconds(30);
        private static readonly RetryPolicy _RetryPolicy = RetryPolicies.Retry(3, TimeSpan.FromSeconds(1));
        private const string _PathSeparator = "/";


        internal BlobProvider(StorageCredentialsAccountAndKey info, Uri baseUri, string containerName)
        {
            this._containerName = containerName;
            this._client = new CloudBlobClient(baseUri.ToString(), info);
        }

        internal string ContainerUrl
        {
            get
            {
                return string.Join(_PathSeparator, new string[] { _client.BaseUri.AbsolutePath, _containerName });
            }
        }

        internal bool GetBlobContentsWithoutInitialization(string blobName, Stream outputStream, out BlobProperties properties)
        {
            Debug.Assert(outputStream != null);

            CloudBlobContainer container = GetContainer();

            try
            {
                var blob = container.GetBlobReference(blobName);

                blob.DownloadToStream(outputStream);

                properties = blob.Properties;
                Log.Write(EventKind.Information, "Getting contents of blob {0}", _client.BaseUri.ToString() + _PathSeparator + _containerName + _PathSeparator + blobName);
                return true;
            }
            catch (InvalidOperationException ex)
            {
                if (ex.InnerException is WebException)
                {
                    var webEx = ex.InnerException as WebException;
                    var resp = webEx.Response as HttpWebResponse;

                    if (resp.StatusCode == HttpStatusCode.NotFound)
                    {
                        properties = null;
                        return false;
                    }
                    else
                        throw;
                }
                else
                    throw;
            }
        }

        internal MemoryStream GetBlobContent(string blobName, out BlobProperties properties)
        {
            MemoryStream blobContent = new MemoryStream();
            properties = GetBlobContent(blobName, blobContent);
            blobContent.Seek(0, SeekOrigin.Begin);
            return blobContent;
        }


        internal BlobProperties GetBlobContent(string blobName, Stream outputStream)
        {
            if (blobName == "")
                return null;

            BlobProperties properties;
            CloudBlobContainer container = GetContainer();
            try
            {
                var blob = container.GetBlobReference(blobName);

                blob.DownloadToStream(outputStream);

                properties = blob.Properties;
                Log.Write(EventKind.Information, "Getting contents of blob {0}", ContainerUrl + _PathSeparator + blobName);
                return properties;
            }
            catch (InvalidOperationException sc)
            {
                Log.Write(EventKind.Error, "Error getting contents of blob {0}: {1}", ContainerUrl + _PathSeparator + blobName, sc.Message);
                throw;
            }
        }

        internal void UploadStream(string blobName, Stream output)
        {
            UploadStream(blobName, output, true);
        }

        internal bool UploadStream(string blobName, Stream output, bool overwrite)
        {
            CloudBlobContainer container = GetContainer();
            try
            {
                output.Position = 0; //Rewind to start
                Log.Write(EventKind.Information, "Uploading contents of blob {0}", ContainerUrl + _PathSeparator + blobName);

                var blob = container.GetBlockBlobReference(blobName);
                
                blob.UploadFromStream(output);

                return true;
            }
            catch (InvalidOperationException se)
            {
                Log.Write(EventKind.Error, "Error uploading blob {0}: {1}", ContainerUrl + _PathSeparator + blobName, se.Message);
                throw;
            }
        }

        internal bool DeleteBlob(string blobName)
        {
            CloudBlobContainer container = GetContainer();
            try
            {
                container.GetBlobReference(blobName).Delete();

                return true;
            }
            catch (InvalidOperationException se)
            {
                Log.Write(EventKind.Error, "Error deleting blob {0}: {1}", ContainerUrl + _PathSeparator + blobName, se.Message);
                throw;
            }
        }

        internal bool DeleteBlobsWithPrefix(string prefix)
        {
            bool ret = true;

            var e = ListBlobs(prefix);
            if (e == null)
            {
                return true;
            }
            var props = e.GetEnumerator();
            if (props == null)
            {
                return true;
            }
            while (props.MoveNext())
            {
                if (props.Current != null)
                {
                    if (!DeleteBlob(props.Current.Uri.ToString()))
                    {
                        // ignore this; it is possible that another thread could try to delete the blob
                        // at the same time
                        ret = false;
                    }
                }
            }
            return ret;
        }

        public IEnumerable<IListBlobItem> ListBlobs(string folder)
        {
            CloudBlobContainer container = GetContainer();
            try
            {
                return container.ListBlobs().Where((blob) => blob.Uri.PathAndQuery.StartsWith(folder));
            }
            catch (InvalidOperationException se)
            {
                Log.Write(EventKind.Error, "Error enumerating contents of folder {0} exists: {1}", ContainerUrl + _PathSeparator + folder, se.Message);
                throw;
            }
        }

        private CloudBlobContainer GetContainer()
        {
            // we have to make sure that only one thread tries to create the container
            lock (_lock)
            {
                if (_container != null)
                {
                    return _container;
                }
                try
                {
                    var container = new CloudBlobContainer(_containerName, _client);
                    var requestModifiers = new BlobRequestOptions()
                    {
                        Timeout = _Timeout,
                        RetryPolicy = _RetryPolicy
                    };

                    container.CreateIfNotExist(requestModifiers);

                    _container = container;

                    return _container;
                }
                catch (InvalidOperationException se)
                {
                    Log.Write(EventKind.Error, "Error creating container {0}: {1}", ContainerUrl, se.Message);
                    throw;
                }
            }
        }

    }
}

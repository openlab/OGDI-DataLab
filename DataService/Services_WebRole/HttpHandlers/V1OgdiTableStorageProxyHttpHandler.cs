using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;
using Ogdi.Azure;

namespace Ogdi.DataServices
{

    public class V1OgdiTableStorageProxyHttpHandler : TableStorageHttpHandlerBase, IHttpHandler
    {

        private HttpContext _context;
        private string _afdPublicServiceReplacementUrl;
        private string _azureTableUrlToReplace;
        private string _entityKind;

        public string AzureTableRequestEntityUrl { get; set; }
        public string OgdiAlias { get; set; }
        public string EntitySet { get; set; }

        public bool IsAvailableEndpointsRequest { get; set; }

        #region IHttpHandler Members

        public bool IsReusable
        {
            get { return true; }
        }


        private string LoadEntityKind(HttpContext context, string entitySet)
        {
            var accountName = AppSettings.EnabledStorageAccounts[this.OgdiAlias].storageaccountname;
            var accountKey = AppSettings.EnabledStorageAccounts[this.OgdiAlias].storageaccountkey;

            var requestUrl = AppSettings.TableStorageBaseUrl + "TableMetadata";
            WebRequest request = this.CreateTableStorageSignedRequest(context, accountName, accountKey, requestUrl, false, true);

            try
            {
                var response = request.GetResponse();

                if (response != null)
                {
                    var responseStream = response.GetResponseStream();

                    if (responseStream != null)
                    {
                        var feed = XElement.Load(XmlReader.Create(responseStream));

                        var propertiesElements = feed.Elements(_nsAtom + "entry").Elements(_nsAtom + "content").Elements(_nsm + "properties");

                        foreach (var e in propertiesElements)
                        {
                            if (e == null) continue;

                            if (entitySet.Equals(e.Element(_nsd + "entityset").Value,
                                                 StringComparison.InvariantCultureIgnoreCase))
                            {
                                return e.Element(_nsd + "entitykind").Value;
                            }
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                var response = ex.Response as HttpWebResponse;
                context.Response.StatusCode = (int)response.StatusCode;
                context.Response.End();
            }
            return null;
        }

        public void ProcessRequest(HttpContext context)
        {
            _entityKind = null;

            if (!this.IsHttpGet(context))
            {
                this.RespondForbidden(context);
            }
            else
            {
                _context = context;
                string accountName;
                string accountKey;

                if (!this.IsAvailableEndpointsRequest)
                {
                    // See AvailableEndpoint.cs for explanation why properties are all lowercase
                    accountName = AppSettings.EnabledStorageAccounts[this.OgdiAlias].storageaccountname;
                    accountKey = AppSettings.EnabledStorageAccounts[this.OgdiAlias].storageaccountkey;

                }
                else
                {
                    accountName = AppSettings.OgdiConfigTableStorageAccountName;
                    accountKey = AppSettings.OgdiConfigTableStorageAccountKey;
                }

                WebRequest request = this.CreateTableStorageSignedRequest(context, accountName, accountKey,
                                                                          this.AzureTableRequestEntityUrl,
                                                                          this.IsAvailableEndpointsRequest);

                Action<string, string, string> incView = AnalyticsRepository.RegisterView;
                incView.BeginInvoke(String.Format("{0}||{1}", this.OgdiAlias, this.EntitySet),
                    context.Request.RawUrl,
                    context.Request.UserHostName,
                    null, null);

                try
                {
                    var response = request.GetResponse();
                    var responseStream = response.GetResponseStream();

                    var feed = XElement.Load(XmlReader.Create(responseStream));

                    _context.Response.Headers["DataServiceVersion"] = "1.0;";
                    _context.Response.CacheControl = "no-cache";
                    _context.Response.AddHeader("x-ms-request-id", response.Headers["x-ms-request-id"]);
                    string continuationNextPartitionKey = response.Headers["x-ms-continuation-NextPartitionKey"];

                    if (continuationNextPartitionKey != null)
                    {
                        _context.Response.AddHeader("x-ms-continuation-NextPartitionKey", continuationNextPartitionKey);
                    }

                    string continuationNextRowKey = response.Headers["x-ms-continuation-NextRowKey"];

                    if (continuationNextRowKey != null)
                    {
                        _context.Response.AddHeader("x-ms-continuation-NextRowKey", continuationNextRowKey);
                    }

                    string format = _context.Request.QueryString["format"];

                    this.SetupReplacementUrls();

                    switch (format)
                    {
                        case "kml":
                            this.RenderKml(feed);
                            break;
                        case "json":
                            this.RenderJson(feed);
                            break;
                        default:
                            // If "format" is not kml or json, then assume AtomPub
                            this.RenderAtomPub(feed);
                            break;
                    }
                }
                catch (WebException ex)
                {
                    var response = ex.Response as HttpWebResponse;
                    _context.Response.StatusCode = (int)response.StatusCode;
                    _context.Response.End();
                }
            }
        }

        #endregion

        private void RenderKml(XElement feed)
        {
            const string STARTING_KML = "<kml xmlns=\"http://www.opengis.net/kml/2.2\"><Document><name></name>";
            const string ENDING_KML = "</Document></kml>";

            _context.Response.ContentType = "application/vnd.google-earth.kml+xml";

            _context.Response.Write(STARTING_KML);

            var propertiesElements = GetPropertiesElements(feed);

            foreach (var propertiesElement in propertiesElements)
            {
                var kmlSnippet = propertiesElement.Element(_nsd + "kmlsnippet");

                if (kmlSnippet != null)
                {
                    // If the kmlsnippet size is <= 64K, then we just store
                    // it in the <kmlsnippet/> element.  However, due to the
                    // 64K string storage limitations in Azure Tables,
                    // we store larger KML snippets in a Azure Blob.
                    // In this case the <kmlsnippet/> element contains:
                    //      <KmlSnippetReference><Container>zipcodes</Container><Blob>33a8d702-c21b-4b09-8cdb-a09cef2e3115</Blob></KmlSnippetReference>
                    // We need to parse this string into an XElement and then
                    // go get the kml snippet out of the blob.
                    // From a perf perspective, this is not ideal.  
                    // However, "it is what it is."

                    var kmlSnippetValue = kmlSnippet.Value;

                    if (kmlSnippetValue.Contains("KmlSnippetReference"))
                    {
                        var blobId = XElement.Parse(kmlSnippetValue).Element("Blob").Value;
                        var request = this.CreateBlobStorageSignedRequest(blobId, this.OgdiAlias, this.EntitySet);
                        var response = request.GetResponse();
                        var strReader = new StreamReader(response.GetResponseStream());
                        var kmlSnippetString = strReader.ReadToEnd();

                        _context.Response.Write(kmlSnippetString);
                    }
                    else
                    {
                        _context.Response.Write(kmlSnippetValue);
                    }
                }
            }

            _context.Response.Write(ENDING_KML);
        }

        private void RenderJson(XElement feed)
        {
            _context.Response.ContentType = "application/json";
            XName kmlSnippetElementString = _nsd + "kmlsnippet";

            var sb = new StringBuilder();
            var sw = new StringWriter(sb);

            using (JsonWriter jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName("d");
                jsonWriter.WriteStartArray();

                IEnumerable<XElement> propertiesElements = GetPropertiesElements(feed);

                foreach (var propertiesElement in propertiesElements)
                {
                    jsonWriter.WriteStartObject();

                    propertiesElement.Elements(kmlSnippetElementString).Remove();

                    foreach (var element in propertiesElement.Elements())
                    {
                        jsonWriter.WritePropertyName(element.Name.LocalName);
                        jsonWriter.WriteValue(element.Value);
                    }

                    jsonWriter.WriteEndObject();
                }

                jsonWriter.WriteEndArray();
                jsonWriter.WriteEndObject();
            }

            var callbackFunctionName = _context.Request["callback"];

            if (callbackFunctionName != null)
            {
                _context.Response.Write(callbackFunctionName);
                _context.Response.Write("(");
                _context.Response.Write(sb.ToString());
                _context.Response.Write(");");
            }
            else
            {
                _context.Response.Write(sb.ToString());
            }
        }

        private void RenderAtomPub(XElement feed)
        {
            // Update Azure Table Storage url for //feed/id
            string idValue = feed.Element(_idXName).Value;
            string baseValue = feed.Attribute(XNamespace.Xml + "base").Value;

            feed.Attribute(XNamespace.Xml + "base").Value = this.ReplaceAzureUrlInString(baseValue);

            feed.Element(_idXName).Value = this.ReplaceAzureUrlInString(idValue);

            // The xml payload coming back has a <kmlsnippet> property.  We want to
            // hide that from the consumer of our service by removing it.
            // NOTE: We only use kmlsnippet when returning KML.

            // Iterate through all the entries to update 
            // Azure Table Storage url for //feed/entry/id
            // and remove kmlsnippet element from all instances of
            // //feed/entry/content/properties

            IEnumerable<XElement> entries = feed.Elements(_entryXName);

            bool isSingleEntry = true;

            foreach (var entry in entries)
            {
                isSingleEntry = false;

                idValue = entry.Element(_idXName).Value;
                entry.Element(_idXName).Value = this.ReplaceAzureUrlInString(idValue);

                ReplaceAzureNamespaceInCategoryTermValue(entry);

                var properties = entry.Elements(_contentXName).Elements(_propertiesXName);

                if (!this.IsAvailableEndpointsRequest)
                {
                    properties.Elements(_kmlSnippetXName).Remove();
                }
                else
                {
                    properties.Elements(_storageAccountNameXName).Remove();
                    properties.Elements(_storageAccountKeyXName).Remove();
                }
            }

            if (isSingleEntry)
            {
                ReplaceAzureNamespaceInCategoryTermValue(feed);
                var properties = feed.Elements(_contentXName).Elements(_propertiesXName);
                properties.Elements(_kmlSnippetXName).Remove();
            }

            _context.Response.ContentType = "application/atom+xml;charset=utf-8";
            _context.Response.Write(feed.ToString());
        }

        private void ReplaceAzureNamespaceInCategoryTermValue(XElement entry)
        {
            // use the simple approach of representing "entitykind" as
            // "entityset" value plus the text "Item."  A decision was made to do
            // this at the service level for now so that we wouldn't have to deal 
            // with changing the data import code and the existing values in the 
            // EntityMetadata table.

            var term = entry.Element(_categoryXName).Attribute("term");

            //TODO: apply real fix. OgdiAlias is null for AvailableEndpoints
            if (this.OgdiAlias != null)
            {
                if (_entityKind == null)
                {
                    var termValue = term.Value;
                    var dotLocation = termValue.ToString().IndexOf(".");
                    var entitySet = termValue.Substring(dotLocation + 1);
                    _entityKind = LoadEntityKind(_context, entitySet);
                }
                term.Value = string.Format(_termNameString, this.OgdiAlias.ToLower(), _entityKind);
            }
        }

        private void SetupReplacementUrls()
        {
            // The xml payload returned from Table Storage data service has urls
            // that point back to Table Storage.  We need to replace the urls with the
            // proper urls for our public service.
            var sb = new StringBuilder(_context.Request.Url.Scheme);
            sb.Append("://");
            sb.Append(_context.Request.Url.Host);
            sb.Append("/v1/");

            if (!this.IsAvailableEndpointsRequest)
            {
                sb.Append(this.OgdiAlias);
                sb.Append("/");

                _azureTableUrlToReplace =
                    string.Format(AppSettings.TableStorageBaseUrl,
                                                AppSettings.EnabledStorageAccounts[this.OgdiAlias].storageaccountname);
            }
            else
            {
                _azureTableUrlToReplace =
                    string.Format(AppSettings.TableStorageBaseUrl,
                                                AppSettings.OgdiConfigTableStorageAccountName);
            }

            _afdPublicServiceReplacementUrl = sb.ToString();
        }

        private string ReplaceAzureUrlInString(string xmlString)
        {
            // The xml payload returned from Table Storage data service has urls
            // that point back to Table Storage.  We need to replace the urls with the
            // proper urls for our public service.
            return xmlString.Replace(_azureTableUrlToReplace, _afdPublicServiceReplacementUrl);
        }

        private static IEnumerable<XElement> GetPropertiesElements(XElement feed)
        {
            return feed.Elements(_entryXName).Elements(_contentXName).Elements(_propertiesXName);
        }
    }
}
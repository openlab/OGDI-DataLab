using Microsoft.WindowsAzure.StorageClient;
using Newtonsoft.Json;
using Ogdi.Azure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml.Linq;

namespace Ogdi.DataServices.v1
{
    public class MainHttpHandler : AbstractHttpHandler
    {
        private static readonly string _TermNamespace = AppSettings.RootServiceNamespace + ".{0}.{1}";
        private string _EntityKind;
        
        public override void ProcessRequest(HttpContext httpContext)
        {
            base.ProcessRequest(httpContext);

            _Account = AppSettings.ParseStorageAccount(
                AppSettings.EnabledStorageAccounts[OgdiAlias].storageaccountname,
                AppSettings.EnabledStorageAccounts[OgdiAlias].storageaccountkey);

            #region <tconte>
            // Modifications pour sécuriser les accès aux flux DPMA
            // On laisse passer les requêtes vers les tables de métadonnées
            // Le reste est intercepté et on implémente une authentification HTTP Basic

            if (this.OgdiAlias == "DPMA"
                && this.EntitySet != "TableMetadata"
                && this.EntitySet != "EntityMetadata"
                && this.EntitySet != "ProcessorParams"
                && this.EntitySet != "TableColumnsMetadata")
            {
                if (_HttpContext.Request.Headers["Authorization"] == null)
                {
                    _HttpContext.Response.StatusCode = 401;
                    _HttpContext.Response.StatusDescription = "Access Denied";
                    _HttpContext.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Secure DPMA feeds\"";
                    _HttpContext.Response.End();
                    return;
                }
                else
                {
                    string credsHeader = _HttpContext.Request.Headers["Authorization"];
                    string creds = null;

                    int credsPosition = credsHeader.IndexOf("Basic", StringComparison.OrdinalIgnoreCase);
                    if (credsPosition != -1)
                    {
                        credsPosition += "Basic".Length + 1;
                        creds = credsHeader.Substring(credsPosition, credsHeader.Length - credsPosition);
                    }

                    string user = ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(creds)).Split(':')[0];
                    if (user != "dpmauser")
                    {
                        _HttpContext.Response.StatusCode = 401;
                        _HttpContext.Response.StatusDescription = "Access Denied";
                        _HttpContext.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Secure DPMA feeds\"";
                        _HttpContext.Response.End();
                        return;
                    }
                }
            }

#endregion

            /*
            Action<string, string, string> incView = AnalyticsRepository.RegisterView;
            incView.BeginInvoke(String.Format("{0}||{1}", OgdiAlias, EntitySet),
                _HttpContext.Request.RawUrl,
                _HttpContext.Request.UserHostName,
                null, null);
            */

            XElement feed = this.GetFeed(EntitySet);

            // Setup URL replacements
            _AzureTableUrlToReplace = string.Format(AppSettings.TableStorageBaseUrl, AppSettings.EnabledStorageAccounts[OgdiAlias].storageaccountname);
            _AzureTableUrlReplacement = string.Format("{0}://{1}:{2}/v1/{3}/",
                _HttpContext.Request.Url.Scheme,
                _HttpContext.Request.Url.Host,
                _HttpContext.Request.Url.Port,
                OgdiAlias);

            this.Render(feed);
        }

        #region Render methods

        protected override void Render(XElement feed)
        {
            string format = null;
            if (!string.IsNullOrEmpty(_HttpContext.Request.QueryString["$format"]))
            {
                format = _HttpContext.Request.QueryString["$format"];
            }
            else if (!string.IsNullOrEmpty(_HttpContext.Request.QueryString["format"]))
            {
                format = _HttpContext.Request.QueryString["format"];
            }

            switch (format)
            {
                case "kml":
                    this.RenderKml(feed);
                    break;
                case "json":
                    this.RenderJson(feed);
                    break;
                case "rdf":
                    this.RenderRdf(feed);
                    break;
                default:
                    this.RenderAtomPub(feed);
                    break;
            }
        }

        protected void RenderKml(XElement feed)
        {
            const string STARTING_KML = "<kml xmlns=\"http://www.opengis.net/kml/2.2\"><Document><name></name>";
            const string ENDING_KML = "</Document></kml>";

            _HttpContext.Response.ContentType = "application/vnd.google-earth.kml+xml";

            _HttpContext.Response.Write(STARTING_KML);

            IEnumerable<XElement> propertiesElements = GetPropertiesElements(feed);
            foreach (XElement propertiesElement in propertiesElements)
            {
                XElement kmlSnippet = propertiesElement.Element(_nsd + "kmlsnippet");
                propertiesElement.Elements(_rdfSnippetXName).Remove();

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

                    string kmlSnippetValue = kmlSnippet.Value;
                    if (kmlSnippetValue.Contains("KmlSnippetReference"))
                    {
                        WebRequest request = CreateAzureBlobStorageRequest(XElement.Parse(kmlSnippetValue).Element("Blob").Value);
                        WebResponse response = request.GetResponse();
                        StreamReader strReader = new StreamReader(response.GetResponseStream());
                        _HttpContext.Response.Write(strReader.ReadToEnd());
                    }
                    else
                    {
                        _HttpContext.Response.Write(kmlSnippetValue);
                    }
                }
            }

            _HttpContext.Response.Write(ENDING_KML);
        }

        protected void RenderJson(XElement feed)
        {
            _HttpContext.Response.ContentType = "application/json";

            XName kmlSnippetElementString = _nsd + "kmlsnippet";

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName("d");
                jsonWriter.WriteStartArray();

                var propertiesElements = this.GetPropertiesElements(feed);
                foreach (var propertiesElement in propertiesElements)
                {
                    jsonWriter.WriteStartObject();

                    propertiesElement.Elements(kmlSnippetElementString).Remove();
                    propertiesElement.Elements(_rdfSnippetXName).Remove();

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

            var callbackFunctionName = _HttpContext.Request["callback"];
            if (callbackFunctionName != null)
            {
                _HttpContext.Response.Write(string.Format("{0}({1})", callbackFunctionName, sb.ToString()));
            }
            else
            {
                _HttpContext.Response.Write(sb.ToString());
            }
        }

        protected void RenderRdf(XElement feed)
        {
            XDocument doc = new XDocument(new XDeclaration("1.0", "utf-8", ""));

            _HttpContext.Response.AddHeader("content-disposition", "attachment; filename=" + EntitySet + ".rdf");
            _HttpContext.Response.ContentType = "application/rss+xml";
            _HttpContext.Response.Charset = "UTF-8";
            _HttpContext.Response.ContentEncoding = System.Text.Encoding.UTF8;

            XElement rdfNamespaces = getRdfNamespaces(EntitySet);
            XElement rdfMetadataValue;

            XNamespace rdfNamespace = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";

            string ogdiUrl = string.Empty;
            string baseUrl = string.Empty;
            string beginUrl = string.Empty;
            string endUrl = string.Empty;

            var propertiesElements = this.GetPropertiesElements(feed);
            foreach (var propertiesElement in propertiesElements)
            {
                string rdfSnip = propertiesElement.Element(_nsd + "rdfsnippet").ToString();

                if (rdfSnip.Contains("ogdiUrl") == true)
                {
                    baseUrl = _HttpContext.Request.Url.AbsoluteUri.Split('?')[0];
                    beginUrl = baseUrl.Substring(0, baseUrl.IndexOf("v1") + 3);
                    endUrl = baseUrl.Substring(baseUrl.IndexOf("v1") + 3);
                    ogdiUrl = beginUrl + "ColumnsMetadata/" + endUrl;
                    rdfSnip = rdfSnip.Replace("ogdiUrl", ogdiUrl);
                }

                XElement rdfSnippet = XElement.Parse(rdfSnip);
                if (rdfSnippet != null)
                {
                    string rdfSnippetValue = rdfSnippet.Value;
                    if (rdfSnippetValue.Contains("RdfSnippetReference"))
                    {
                        WebRequest request = CreateAzureBlobStorageRequest(XElement.Parse(rdfSnippetValue).Element("Blob").Value);
                        WebResponse response = request.GetResponse();
                        StreamReader strReader = new StreamReader(response.GetResponseStream());
                        string rdfSnippetString = strReader.ReadToEnd();

                        rdfNamespaces.Add(XElement.Parse(rdfSnippetValue).Element(rdfNamespace + "Description"));
                    }
                    else
                    {
                        rdfMetadataValue = XElement.Parse(rdfSnippetValue).Element(rdfNamespace + "Description");
                        rdfNamespaces.Add(rdfMetadataValue);
                    }
                }
            }

            doc.Add(rdfNamespaces);
            doc.Save(_HttpContext.Response.Output);

            _HttpContext.Response.End();
        }

        private XElement getRdfNamespaces(string entitySet)
        {
            List<string> ns = new List<string>();

            XNamespace rdfNamespace = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";

            XElement rdfXml = new XElement(rdfNamespace + "RDF", new XAttribute(XNamespace.Xmlns + "rdf", rdfNamespace.ToString()));
            string customNamespaceUrl = string.Empty;
            string baseUrl = string.Empty;
            string beginUrl = string.Empty;
            string endUrl = string.Empty;

            if (entitySet != null)
            {
                ns = this.GetTableColumnsMetadata();
                if (ns.Count > 0)
                {
                    foreach (var item in ns)
                    {
                        if (item == "ogdi=ogdiUrl")
                        {
                            baseUrl = _HttpContext.Request.Url.AbsoluteUri.Split('?')[0];
                            beginUrl = baseUrl.Substring(0, baseUrl.IndexOf("v1") + 3);
                            endUrl = baseUrl.Substring(baseUrl.IndexOf("v1") + 3);
                            customNamespaceUrl = beginUrl + "ColumnsMetadata/" + endUrl;
                        }
                        else
                        {
                            customNamespaceUrl = item.Split('=')[1];
                        }

                        rdfXml.Add(new XAttribute(XNamespace.Xmlns + item.Split('=')[0], customNamespaceUrl));
                    }
                }
            }
            
            return rdfXml;
        }

        private List<string> GetTableColumnsMetadata()
        {
            TableColumnsMetadataDataServiceContext context = new TableColumnsMetadataDataServiceContext(_Account.TableEndpoint.ToString(), _Account.Credentials)
            {
                RetryPolicy = RetryPolicies.RetryExponential(RetryPolicies.DefaultClientRetryCount, RetryPolicies.DefaultClientBackoff)
            };

            List<string> namespaces = new List<string>();

            var query = from entity in context.TableColumnsMetadataTable
                        where entity.PartitionKey == EntitySet
                        select entity;

            string namespaceAdded;
            foreach (var c in query)
            {
                namespaceAdded = string.Empty;
                namespaceAdded = namespaces.Find(item => item.Split('=')[0] == c.columnnamespace.Split('=')[0]);
                if (namespaceAdded == string.Empty || namespaceAdded == null)
                {
                    namespaces.Add(c.columnnamespace);
                }
            }
            
            return namespaces;
        }

        protected void RenderAtomPub(XElement feed)
        {
            _HttpContext.Response.ContentType = _atomXmlContentType;

            string idValue = feed.Element(_idXName).Value;
            feed.Element(_idXName).Value = ReplaceAzureUrlInString(idValue);

            string baseValue = feed.Attribute(XNamespace.Xml + "base").Value;
            feed.Attribute(XNamespace.Xml + "base").Value = ReplaceAzureUrlInString(baseValue);

            // The xml payload coming back has a <kmlsnippet> property.  We want to
            // hide that from the consumer of our service by removing it.
            // NOTE: We only use kmlsnippet when returning KML.

            // Iterate through all the entries to update 
            // Azure Table Storage url for //feed/entry/id
            // and remove kmlsnippet element from all instances of
            // //feed/entry/content/properties

            foreach (var entry in feed.Elements(_entryXName))
            {
                idValue = entry.Element(_idXName).Value;
                entry.Element(_idXName).Value = ReplaceAzureUrlInString(idValue);

                this.ReplaceAzureNamespaceInCategoryTermValue(entry);

                var properties = entry.Elements(_contentXName).Elements(_propertiesXName);
                properties.Elements(_kmlSnippetXName).Remove();
                properties.Elements(_rdfSnippetXName).Remove();
            }

            _HttpContext.Response.Write(feed.ToString());
        }

        private string LoadEntityKind(string entitySet)
        {
            XElement feed = this.GetFeed("TableMetadata", null, false);

            var propertiesElements = feed.Elements(_nsAtom + "entry").Elements(_nsAtom + "content").Elements(_nsm + "properties");
            foreach (var e in propertiesElements)
            {
                if (e != null)
                {
                    if (entitySet.Equals(e.Element(_nsd + "entityset").Value, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return e.Element(_nsd + "entitykind").Value;
                    }
                }
            }

            return null;
        }

        private void ReplaceAzureNamespaceInCategoryTermValue(XElement entry)
        {
            // Use the simple approach of representing "entitykind" as
            // "entityset" value plus the text "Item."  A decision was made to do
            // this at the service level for now so that we wouldn't have to deal 
            // with changing the data import code and the existing values in the 
            // EntityMetadata table.

            XAttribute term = entry.Element(_categoryXName).Attribute("term");

            if (string.IsNullOrEmpty(_EntityKind))
            {
                string termValue = term.Value;
                int dotLocation = termValue.IndexOf(".");
                string entitySet = termValue.Substring(dotLocation + 1);

                _EntityKind = LoadEntityKind(entitySet);
            }

            term.Value = string.Format(_TermNamespace, OgdiAlias, _EntityKind);
        }

        private IEnumerable<XElement> GetPropertiesElements(XElement feed)
        {
            return feed.Elements(_entryXName).Elements(_contentXName).Elements(_propertiesXName);
        }

        private WebRequest CreateAzureBlobStorageRequest(string blobId)
        {
            UriBuilder uri = new UriBuilder(_Account.BlobEndpoint.AbsoluteUri);
            uri.Path = string.Format("{0}/{1}", EntitySet.ToLower(), blobId);

            WebRequest request = WebRequest.Create(uri.ToString());
            request.Headers.Add("x-ms-version", "2011-08-18");

            _Account.Credentials.SignRequest((HttpWebRequest)request);

            return request;
        }

        #endregion
    }
}
using Ogdi.DataServices.Helper;
using System.Web;
using System.Xml.Linq;
using System.Text;

namespace Ogdi.DataServices.v1
{
	public class MetadataHttpHandler : AbstractHttpHandler
	{
        public Cache Cache { get; set; }

        private StringBuilder _CacheData;

        #region Render variables

        private readonly string START_DATASERVICES_TEMPLATE =
@"<?xml version='1.0' encoding='utf-8' standalone='yes'?>
<edmx:Edmx Version='1.0' xmlns:edmx='http://schemas.microsoft.com/ado/2007/06/edmx'>
  <edmx:DataServices>
	<Schema Namespace='" + AppSettings.RootServiceNamespace + @".{0}' xmlns:d='http://schemas.microsoft.com/ado/2007/08/dataservices' xmlns:m='http://schemas.microsoft.com/ado/2007/08/dataservices/metadata' xmlns='http://schemas.microsoft.com/ado/2006/04/edm'>
	  <EntityContainer Name='{0}DataService' m:IsDefaultEntityContainer='true'>
";

		private readonly string ENTITYSET_TEMPLATE =
@"        <EntitySet Name='{0}' EntityType='" + AppSettings.RootServiceNamespace + @".{1}.{2}' />
";

		private const string END_ENTITYCONTAINER_TEMPLATE =
@"      </EntityContainer>
";

		private const string START_ENTITYTYPE_TEMPLATE =
@"      <EntityType Name='{0}'>
		<Key>
		  <PropertyRef Name='PartitionKey' />
		  <PropertyRef Name='RowKey' />
		</Key>
		<Property Name='PartitionKey' Type='Edm.String' Nullable='false' />
		<Property Name='RowKey' Type='Edm.String' Nullable='false' />
		<Property Name='Timestamp' Type='Edm.DateTime' Nullable='false' />
		<Property Name='entityid' Type='Edm.Guid' Nullable='false' />
";

		private const string START_PROPERTY_TEMPLATE =
@"        <Property Name='{0}' Type='{1}' Nullable='true' />
";

		private const string END_ENTITYTYPE_TEMPLATE =
@"      </EntityType>
";

		private const string END_ENTITYTYPESCHEMA_TEMPLATE =
@"    </Schema>
";

		private const string END_DATASERVICES_TEMPLATE =
@"  </edmx:DataServices>
</edmx:Edmx>";

        #endregion

        public override void ProcessRequest(HttpContext httpContext)
        {
            base.ProcessRequest(httpContext);

            // Check if metadata is already cached
            string cachedMetadata = Cache.Get("Metadata") as string;
            if (!string.IsNullOrEmpty(cachedMetadata))
            {
                _HttpContext.Response.ContentType = _xmlContentType;
                _HttpContext.Response.Write(cachedMetadata);
                return;
            }

            _Account = AppSettings.ParseStorageAccount(
                AppSettings.EnabledStorageAccounts[OgdiAlias].storageaccountname,
                AppSettings.EnabledStorageAccounts[OgdiAlias].storageaccountkey);

            XElement feed = this.GetFeed("EntityMetadata");

            this.Render(feed);
        }

        protected override void Render(XElement feed)
        {
            _CacheData = new StringBuilder();

            _HttpContext.Response.ContentType = _xmlContentType;

            WriteAndCache(string.Format(START_DATASERVICES_TEMPLATE, OgdiAlias));

            var propertiesElements = feed.Elements(_nsAtom + "entry").Elements(_nsAtom + "content").Elements(_nsm + "properties");
            foreach (var e in propertiesElements)
            {
                // Changed to use the simple approach of representing "entitykind" as
                // "entityset" value plus the text "Item."  A decision was made to do
                // this at the service level for now so that we wouldn't have to deal 
                // with changing the data import code and the existing values in the 
                // EntityMetadata table.
                // New notice: Import code was changed, so entityKind = entitySet + "Item" in storage for all new data
                // So return the code back:

                string entitySet = e.Element(_nsd + "entityset").Value;
                string entityKind = e.Element(_nsd + "entitykind").Value;

                WriteAndCache(string.Format(ENTITYSET_TEMPLATE,
                                                        entitySet,
                                                        this.OgdiAlias,
                                                        entityKind));
            }

            WriteAndCache(END_ENTITYCONTAINER_TEMPLATE);

            foreach (var e in propertiesElements)
            {
                WriteAndCache(string.Format(START_ENTITYTYPE_TEMPLATE, e.Element(_nsd + "entitykind").Value));

                e.Elements(_nsd + "PartitionKey").Remove();
                e.Elements(_nsd + "RowKey").Remove();
                e.Elements(_nsd + "Timestamp").Remove();
                e.Elements(_nsd + "entityset").Remove();
                e.Elements(_nsd + "entitykind").Remove();
                e.Elements(_nsd + "entityid").Remove();

                foreach (XElement prop in e.Elements())
                {
                    WriteAndCache(string.Format(START_PROPERTY_TEMPLATE,
                                            prop.Name.LocalName,
                                            prop.Value.Replace("System", "Edm")));
                }

                WriteAndCache(END_ENTITYTYPE_TEMPLATE);
            }

            WriteAndCache(END_ENTITYTYPESCHEMA_TEMPLATE);
            WriteAndCache(END_DATASERVICES_TEMPLATE);

            // Put metadata in cache
            Cache.Put("Metadata", _CacheData.ToString());
        }

        private void WriteAndCache(string data)
        {
            _HttpContext.Response.Write(data);
            _CacheData.Append(data);
        }
	}
}

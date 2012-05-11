using System;
using System.Configuration;
using System.Data.Services.Client;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.WindowsAzure;
using Ogdi.Config;

namespace ConfigTool
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                DisplayLatest();
            }            
        }

        private void DisplayLatest()
        {
            string statusMessage = String.Empty;
            try
            {
                var ta = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["DataConnectionString"]);
                var context = new OgdiConfigDataServiceContext(ta.TableEndpoint.AbsoluteUri, ta.Credentials);

                var empty = true;
                foreach (var ep in context.AvailableEndpoints)
                {
                    empty = false;
                    break;
                }
                if (!empty)
                {
                    this.messageList.DataSource = context.AvailableEndpoints.ToList();
                    this.messageList.DataBind();
                }
            }
            catch (DataServiceRequestException ex)
            {
                statusMessage = "Unable to connect to the table storage server. Please check that the service is running.<br />"
                                + ex.Message;
            }
            catch (Exception) { }
            status.Text = statusMessage;
        }

        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            var esa = new AvailableEndpoint
            {
                PartitionKey = AliasBox.Text,
                RowKey = "",
                alias = AliasBox.Text,
                description = DescriptionBox.Text,
                storageaccountname = tableStorageAccountNameBox.Text,
                storageaccountkey = tableStorageAccountKeyBox.Text
            };

            if (DisclaimerBox.Text.Length > 0)
            {
                esa.disclaimer = DisclaimerBox.Text.Replace('\n', ' ');
            }

            var ta = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["DataConnectionString"]);
            var ctx = new OgdiConfigDataServiceContext(ta.TableEndpoint.AbsoluteUri, ta.Credentials);

            ctx.AddObject(OgdiConfigDataServiceContext.EndpointsTableName, esa);
            ctx.SaveChanges();

            DisplayLatest();
        }

        protected void DeleteButton_Click(object sender, EventArgs e)
        {
            var button = sender as LinkButton;
            string[] p = button.CommandArgument.Split('|');
            var partitionKey = p[0];
            var rowKey = p[1];

            var availableEndpoint = new AvailableEndpoint
            {
                PartitionKey = partitionKey,
                RowKey = rowKey
            };

            var ta = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["DataConnectionString"]);
            var context = new OgdiConfigDataServiceContext(ta.TableEndpoint.AbsoluteUri, ta.Credentials);

            context.AttachTo(OgdiConfigDataServiceContext.EndpointsTableName, availableEndpoint, "*");
            context.DeleteObject(availableEndpoint);
            context.SaveChanges();

            DisplayLatest();
        }

        public static string BuildKey(object dataItem)
        {
            var availableEndpoint = dataItem as AvailableEndpoint;
            return string.Format("{0}|{1}",
                availableEndpoint.PartitionKey,
                availableEndpoint.RowKey);
        }
    }
}

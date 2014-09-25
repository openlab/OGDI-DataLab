using System.Linq;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.Web.Administration;
using System.Xml.Linq;
using System;

namespace Ogdi.InteractiveSdk.Mvc
{
    public class WebRole : RoleEntryPoint
    {
        public override bool OnStart()
        {
            DiagnosticMonitor.Start("DiagnosticsConnectionString");

            // Restart the role upon all configuration changes
            // Note: To customize the handling of configuration changes, 
            // remove this line and register custom event handlers instead.
            // See the MSDN topic on “Managing Configuration Changes” for further details 
            // (http://go.microsoft.com/fwlink/?LinkId=166357).
            RoleEnvironment.Changing += RoleEnvironmentChanging;

            Microsoft.WindowsAzure.CloudStorageAccount.SetConfigurationSettingPublisher((configName, configSetter) =>
            {
                configSetter(RoleEnvironment.GetConfigurationSettingValue(configName));
            });

            return base.OnStart();
        }

        private void RoleEnvironmentChanging(object sender, RoleEnvironmentChangingEventArgs e)
        {
            if (e.Changes.Any(change => change is RoleEnvironmentConfigurationSettingChange))
                e.Cancel = true;
        }
    }
}
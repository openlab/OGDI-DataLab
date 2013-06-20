using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace WindowsAzureStorage
{
    public static class Constants
    {
        public static XNamespace nsAtom = XNamespace.Get(AzureResources.nsAtomLink);
        public static XNamespace nsMetadata = XNamespace.Get(AzureResources.nsMetadataLink);
        public static XNamespace nsDataServices = XNamespace.Get(AzureResources.nsDataServicesLink);
    }
}

using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DataConfig.Models
{
    public class AvailableEndpoint : TableEntity
    {
        public string alias { get; set; }
        public string description { get; set; }
        public string disclaimer { get; set; }
        public string storageaccountname { get; set; }
        public string storageaccountkey { get; set; }
    }
}
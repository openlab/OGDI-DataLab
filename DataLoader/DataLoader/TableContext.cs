using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using System.Xml.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using Ogdi.Data.DataLoader.Csv;

namespace Ogdi.Data.DataLoader
{
    internal class TableContext : TableServiceContext
    {
        private readonly TableProcessorParams _params;

        public TableContext(string baseAddress, StorageCredentials credentials, TableProcessorParams _params)
            : base(baseAddress, credentials)
        {
            this._params = _params;
            WritingEntity += OnWritingEntity;
            ReadingEntity += TableContext_ReadingEntity;
        }


        public TableContext(string baseAddress, StorageCredentials credentials, DataLoaderParams _params)
            : base(baseAddress, credentials)
        {
            var pars = _params as CsvToTablesDataLoaderParams;
            if (pars != null)
                this._params = pars.ProcessorParams;
            WritingEntity += OnWritingEntity;
            ReadingEntity += TableContext_ReadingEntity;
        }

        /*
         * Read data from Storage.
         * All properties values will be STRING!
         */

        private void TableContext_ReadingEntity(object sender, ReadingWritingEntityEventArgs e)
        {
            var xnProps = XName.Get("properties", e.Data.GetNamespaceOfPrefix("m").NamespaceName);
            XElement xeProps = e.Data.Descendants().Where(xe => xe.Name == xnProps).First();
            var props = new List<Property>();
            var ID = Guid.Empty;
            foreach (XElement prop in xeProps.Nodes())
            {
                if (prop.Name.LocalName == DataLoaderConstants.PartitionKeyColumnName)
                    ((TableEntity)e.Entity).PartitionKey = prop.Value;
                else if (prop.Name.LocalName == DataLoaderConstants.RowKeyColumnName)
                    ((TableEntity)e.Entity).RowKey = prop.Value;
                else if (prop.Name.LocalName.ToLower() == DataLoaderConstants.PropNameEntityId.ToLower())
                    ID = new Guid(prop.Value);
                else if (prop.Name.LocalName == DataLoaderConstants.TimestampColumnName)
                {
                    ((TableEntity)e.Entity).Timestamp = DateTime.Parse(prop.Value);
                }
                else
                    props.Add(new Property(prop.Name.LocalName, prop.Value));
            }
            var entity = new Entity(ID);
            foreach (Property prop in props)
                entity.AddProperty(prop.Name, prop.Value);
            ((TableEntity)e.Entity).SetEntity(entity);
        }

        private void OnWritingEntity(object sender, ReadingWritingEntityEventArgs e)
        {
            XName xnProps = XName.Get("properties", e.Data.GetNamespaceOfPrefix("m").NamespaceName);
            XElement xeProps = e.Data.Descendants().Where(xe => xe.Name == xnProps).First();
            foreach (TableEntity.NameTypeValueTuple tuple in ((TableEntity)e.Entity).GetProperties())
            {
                if (tuple.Value is DateTime)
                    tuple.Value = ConvertToUtc((DateTime)tuple.Value);
                if (tuple.Type != null)
                {
                    xeProps.Add(new XElement(e.Data.GetNamespaceOfPrefix("d") + tuple.Name,
                                             new XAttribute(e.Data.GetNamespaceOfPrefix("m") + "type", tuple.Type),
                                             tuple.Value));
                }
                else
                {
                    xeProps.Add(new XElement(e.Data.GetNamespaceOfPrefix("d") + tuple.Name, tuple.Value));
                }
            }
        }

        private DateTime ConvertToUtc(DateTime date)
        {
            TimeZoneInfo sourceZone = _params.GetSourceTimeZone() ?? TimeZoneInfo.Local;

            DateTime tstTime = TimeZoneInfo.ConvertTime(date, TimeZoneInfo.Local, sourceZone);

            return TimeZoneInfo.ConvertTimeToUtc(tstTime, sourceZone);
        }
    }
}
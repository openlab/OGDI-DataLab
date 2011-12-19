using System;
using System.Collections.Generic;
using System.Linq;
using Ogdi.Data.DataLoader;

namespace Ogdi.Data.DataLoaderGuiApp.ViewModels
{
    public class ProcessorParamsControlViewModel
    {
        private readonly TableProcessorParams _processorParameters;
        private readonly IEnumerable<string> _timeZones;
        private readonly IEnumerable<string> _datasetColumns;

        public ProcessorParamsControlViewModel(DataLoaderParams parameters)
        {
            #region fill timezones

            var timeZones = TimeZoneInfo.GetSystemTimeZones();

            _timeZones = timeZones.OrderBy(x => x.BaseUtcOffset.Hours).Select(x => x.DisplayName).ToArray();

            #endregion fill timezones

            _processorParameters = parameters.ProcessorParams;

            if (parameters.ProducerParams != null)
                _datasetColumns = (new[] { "New.Guid" }).Union(parameters.ProducerParams.PropertyToTypeMap.Mappings.Select(x => x.Name));
        }

        public string PartitionKeyPropertyName
        {
            get { return _processorParameters.PartitionKeyPropertyName; }
            set { _processorParameters.PartitionKeyPropertyName = value; }
        }

        public string RowKeyPropertyName
        {
            get { return _processorParameters.RowKeyPropertyName; }
            set { _processorParameters.RowKeyPropertyName = value; }
        }

        public string SourceTimeZoneName
        {
            get { return _processorParameters.SourceTimeZoneName; }
            set { _processorParameters.SourceTimeZoneName = value; }
        }

        public IEnumerable<string> TimeZones
        {
            get { return _timeZones; }
        }

        public IEnumerable<string> DatasetColumns
        {
            get { return _datasetColumns; }
        }
    }
}

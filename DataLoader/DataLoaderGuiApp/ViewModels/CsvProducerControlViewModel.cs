using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Ogdi.Data.DataLoader;
using Ogdi.Data.DataLoaderGuiApp.Commands;

namespace Ogdi.Data.DataLoaderGuiApp.ViewModels
{
    public class CsvProducerControlViewModel
    {
        private readonly EntityProducerParams _datasetColumns;
        private readonly PropertyToTypeMap _entityProperties;

        public CsvProducerControlViewModel(DataLoaderParams parameters)
        {
            _datasetColumns = parameters.ProducerParams;
            _entityProperties = new PropertyToTypeMap();

            if (_datasetColumns.PlacemarkParams == null || _datasetColumns.PlacemarkParams.NameProperties == null)
                return;

            foreach (var property in _datasetColumns.PropertyToTypeMap.Mappings)
            {
                for (int i = 0; i < _datasetColumns.PlacemarkParams.NameProperties.Length; i++)
                {
                    string nameProperty = _datasetColumns.PlacemarkParams.NameProperties[i];

                    if (nameProperty != null && nameProperty.ToLower() == property.Name.ToLower())
                        property.IndexInName = "{" + i + "}";
                }
            }
        }

        private string GenerateFieldName()
        {
            var exp = new Regex("^FIELD[0-9]{1,}$");
            int max = 0;

            var l = _datasetColumns.PropertyToTypeMap.Mappings
                .Where(x => exp.IsMatch(x.Name));

            if (l.Count() > 0)
                max = l.Max(x => int.Parse(x.Name.Substring(5)));

            return "FIELD" + (max + 1);
        }

        #region Bindings

        private bool _PlacemarksEnabled = false;
        public bool PlacemarksEnabled
        {
            get
            {
                return _PlacemarksEnabled && _datasetColumns.PlacemarkParams != null;
            }
            set
            {
                _PlacemarksEnabled = value;
                _datasetColumns.PlacemarkParams = _datasetColumns.PlacemarkParams == null ? new PlacemarkParams() : null;
            }
        }

        public IList<string> EntityPopertyTypes
        {
            get { return _entityProperties.Types; }
        }

        public IList<string> IndexInNames
        {
            get { return _entityProperties.IndexInNames; }
        }

        public ObservableCollection<PropertyToType> Latitudes
        {
            get { return _datasetColumns.PropertyToTypeMap.Mappings; }
        }

        public ObservableCollection<PropertyToType> Longitudes
        {
            get { return _datasetColumns.PropertyToTypeMap.Mappings; }
        }

        public ObservableCollection<PropertyToType> PropertyToTypeMaps
        {
            get { return _datasetColumns.PropertyToTypeMap.Mappings; }
        }

        public string NamePropertyFormatString
        {
            get
            {
                if (_datasetColumns.PlacemarkParams == null)
                    return null;

                return _datasetColumns.PlacemarkParams.NamePropertyFormatString;
            }
            set
            {
                if (_datasetColumns.PlacemarkParams == null)
                    return;

                _datasetColumns.PlacemarkParams.NamePropertyFormatString = value;
            }
        }

        public PropertyToType CurrentLatitude
        {
            get
            {
                if (_datasetColumns.PlacemarkParams == null)
                    return null;

                return _datasetColumns.PropertyToTypeMap.GetByName(_datasetColumns.PlacemarkParams.LatitudeProperty);
            }
            set
            {
                if (_datasetColumns.PlacemarkParams == null)
                    return;

                _datasetColumns.PlacemarkParams.LatitudeProperty = value != null ? value.Name : null;
            }
        }

        public PropertyToType CurrentLongitude
        {
            get
            {
                if (_datasetColumns.PlacemarkParams == null)
                    return null;

                return _datasetColumns.PropertyToTypeMap.GetByName(_datasetColumns.PlacemarkParams.LongitudeProperty);
            }
            set
            {
                if (_datasetColumns.PlacemarkParams == null)
                    return;

                _datasetColumns.PlacemarkParams.LongitudeProperty = value != null ? value.Name : null;
            }
        }

        public object SelectedItem { get; set; }

        #endregion

        #region Commands

        private DelegateCommand _addFieldCommand;

        public ICommand OnAddFieldButtonClick
        {
            get { return _addFieldCommand ?? (_addFieldCommand = new DelegateCommand(AddField)); }
        }

        public ICommand OnDeleteFieldButtonClick
        {
            get { return _deleteFieldCommand ?? (_deleteFieldCommand = new DelegateCommand(DeleteField)); }
        }

        private DelegateCommand _deleteFieldCommand;

        private void DeleteField()
        {
            _datasetColumns.PropertyToTypeMap.Mappings.Remove((PropertyToType)SelectedItem);
        }

        private void AddField()
        {
            _datasetColumns.PropertyToTypeMap.Mappings.Add(new PropertyToType(GenerateFieldName(), "string", string.Empty));
        }

        #endregion
    }
}
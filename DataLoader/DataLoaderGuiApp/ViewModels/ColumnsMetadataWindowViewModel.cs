using System;
using Ogdi.Data.DataLoader;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace Ogdi.Data.DataLoaderGuiApp.ViewModels
{
    public class ColumnsMetadataWindowViewModel
    {
        private TableColumnsMetadata _columnsMetadata;
        private PropertyToTypeColumnsMetadataMap _entityColumnsMetadata;

        public ColumnsMetadataWindowViewModel(DataLoaderParams parameters)
        {
            _columnsMetadata = parameters.TableColumnsMetadata;
            _entityColumnsMetadata = new PropertyToTypeColumnsMetadataMap();
        }

        public ObservableCollection<string> ColumnSemantic
        {
            get { return _entityColumnsMetadata.ColumnSemantic; }
        }

        public String Column
        {
            get { return _entityColumnsMetadata.Column; }
        }

        public String ColumnDescription
        {
            get { return _entityColumnsMetadata.ColumnDescription; }
        }

        public String ColumnNamespace
        {
            get { return _entityColumnsMetadata.ColumnNamespace; }
        }

        public ObservableCollection<TableColumnsMetadataItem> PropertyToTypeColumnsMetadataItems
        {
            get { return _columnsMetadata.PropertyToTypeColumnsMetadata.Mappings; }
        }

    }
    public class readOnlyNamespaceConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                string _columnSemantic = value.ToString();
                if (_columnSemantic == "")
                    return false;
            }

            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;

        }
    }
}

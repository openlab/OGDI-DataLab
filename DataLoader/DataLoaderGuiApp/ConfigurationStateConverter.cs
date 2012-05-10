using System;
using System.Globalization;
using System.Windows.Data;
using Ogdi.Data.DataLoader;
using Ogdi.Data.DataLoaderUI;

namespace Ogdi.Data.DataLoaderGuiApp
{
    public class ConfigurationStateConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((ConfigurationState)value)
            {
                case ConfigurationState.Acceptable:
                    return "Configured";
                case ConfigurationState.Incomplete:
                    return "Not configured";
                case ConfigurationState.Ready:
                    return "Ready to upload";
            }
            throw new NotSupportedException(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
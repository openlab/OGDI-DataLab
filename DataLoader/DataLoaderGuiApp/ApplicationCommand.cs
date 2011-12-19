using System.Windows;
using System.Windows.Input;

namespace Ogdi.Data.DataLoaderGuiApp
{
    /// <summary>
    /// This class holds the global commands used by the application
    /// </summary>
    public static class AppCommands
    {
        public static RoutedCommand SelectFilesCommand 
        {
            get { return (RoutedCommand)Application.Current.Resources["SelectFilesCommand"]; }
        }

        public static RoutedCommand UploadCommand 
        {
            get { return (RoutedCommand)Application.Current.Resources["UploadCommand"]; }
        }

        public static RoutedCommand SettingsCommand 
        {
            get { return (RoutedCommand)Application.Current.Resources["ConnectionSettingsCommand"]; }
        }

        public static RoutedCommand RemoveCompletedCommand { get; private set; }
        public static RoutedCommand ClearListCommand { get; private set; }
        public static RoutedCommand RemoveFromListCommand { get; private set; }
        public static RoutedCommand AddPlannedDataSetCommand { get; private set; }
        public static RoutedCommand AddDataSetCommand { get; private set; }

    }
}
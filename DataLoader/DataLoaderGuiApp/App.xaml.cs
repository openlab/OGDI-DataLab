using System.Windows;
using Ogdi.Data.DataLoaderGuiApp.ViewModels;
using Ogdi.Data.DataLoaderGuiApp.Views;

namespace Ogdi.Data.DataLoaderGuiApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void OnStartup(object sender, StartupEventArgs e)
        {
            // Create the ViewModel and expose it using the View's DataContext
            var view = new MainView { DataContext = new MainViewModel() };
            view.Show();
        }
    }
}

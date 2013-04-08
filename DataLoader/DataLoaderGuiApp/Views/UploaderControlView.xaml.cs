using System.Windows.Input;
using Ogdi.Data.DataLoaderGuiApp.ViewModels;
using Ogdi.Data.DataLoader;

namespace Ogdi.Data.DataLoaderGuiApp.Views
{
    public partial class UploaderControlView
    {
        public UploaderControlView()
        {
            InitializeComponent();
        }

        private void DataSetListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ((MainViewModel)DataContext).OnActivateItem();
        }

        private void DataSetListView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ((MainViewModel)DataContext).OnActivateItem();
            }
        }
    }
}
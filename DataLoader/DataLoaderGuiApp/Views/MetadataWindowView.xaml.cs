using System.Windows.Input;
using Ogdi.Data.DataLoaderGuiApp.Commands;

namespace Ogdi.Data.DataLoaderGuiApp.Views
{
    /// <summary>
    /// Interaction logic for Metadataxaml
    /// </summary>
    public partial class MetadataWindowView
    {
        private DelegateCommand _closeCommand;

        public MetadataWindowView()
        {
            InitializeComponent();
        }

        public ICommand CloseCommand
        {
            get { return _closeCommand ?? (_closeCommand = new DelegateCommand(CloseWindow)); }
        }

        private void CloseWindow()
        {
            Close();
        }
    }
}
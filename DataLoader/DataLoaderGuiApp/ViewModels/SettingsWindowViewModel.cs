using System.Windows;
using System.Windows.Input;
using Ogdi.Azure;
using Ogdi.Data.DataLoaderGuiApp.Commands;

namespace Ogdi.Data.DataLoaderGuiApp.ViewModels
{
    public class SettingsWindowViewModel : WorkspaceViewModel
    {
        public string AccountName { get; set; }

        public string AccountKey { get; set; }

        private readonly DataLoaderSettings _settings;

        public SettingsWindowViewModel()
        {
            _settings = new DataLoaderSettings();
            _settings.Load();

            AccountName = _settings.GetAccessName();
            AccountKey = _settings.GetAccessKey();
        }

        #region OKCommand

        private DelegateCommand _okCommand;

        public ICommand OkCommand
        {
            get { return _okCommand ?? (_okCommand = new DelegateCommand(SaveSettings)); }
        }

        private void SaveSettings()
        {
            if (string.IsNullOrEmpty(AccountName))
            {
                MessageBox.Show(Ressources.ViewR.AccountError, "Endpoint Settings", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(AccountKey))
            {
                MessageBox.Show(Ressources.ViewR.KeyError, "Endpoint Settings", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _settings.UpdateDataConnectionString(AccountName, AccountKey);
            _settings.Save();

            Close();
        }

        #endregion

        #region CancelCommand

        private DelegateCommand _cancelCommand;

        public ICommand CancelCommand
        {
            get { return _cancelCommand ?? (_cancelCommand = new DelegateCommand(Close)); }
        }

        #endregion
    }
}

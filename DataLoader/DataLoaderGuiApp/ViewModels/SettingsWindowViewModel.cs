
using System.Windows;
using System.Windows.Input;
using Ogdi.Azure;
using Ogdi.Data.DataLoaderGuiApp.Commands;

namespace Ogdi.Data.DataLoaderGuiApp.ViewModels
{
    public class SettingsWindowViewModel :  WorkspaceViewModel
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

        #region SaveCommand

        private DelegateCommand _saveCommand;

        public ICommand SaveCommand
        {
            get { return _saveCommand ?? (_saveCommand = new DelegateCommand(SaveSettings)); }
        }

        private void SaveSettings()
        {
            if (string.IsNullOrEmpty(AccountName))
            {
                MessageBox.Show("Please enter Account Name", "Endpoint Settings", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(AccountKey))
            {
                MessageBox.Show("Please enter Account Key", "Endpoint Settings", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _settings.UpdateDataConnectionString(AccountName, AccountKey);
            _settings.Save();
            Close();
        }

        #endregion

        #region CloseCommand

        private DelegateCommand _closeCommand;

        public ICommand CloseWindowCommand
        {
            get { return _closeCommand ?? (_closeCommand = new DelegateCommand(Close)); }
        }

        #endregion
    }
}

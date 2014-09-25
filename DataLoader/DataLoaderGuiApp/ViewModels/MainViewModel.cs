using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using Ogdi.Data.DataLoader;
using Ogdi.Data.DataLoaderGuiApp.Commands;
using Ogdi.Data.DataLoaderGuiApp.Views;

namespace Ogdi.Data.DataLoaderGuiApp.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly UserControl _uploaderView;
        private readonly Uploader _uploader;

        public ObservableCollection<UploadParam> DataSetInfos
        {
            get { return _uploader.DataSetInfos; }
        }

        public UserControl UploaderView { get { return _uploaderView; } }

        public MainViewModel()
        {
            _uploader = new Uploader();
            _uploaderView = new UploaderControlView { DataContext = this };
        }

        #region Commands

        private DelegateCommand _exitCommand;

        public ICommand ExitCommand
        {
            get { return _exitCommand ?? (_exitCommand = new DelegateCommand(Exit, CanExit)); }
        }

        private static void Exit()
        {
            Application.Current.Shutdown();
        }

        private bool CanExit()
        {
            return !_uploader.IsWorking;
        }

        #endregion

        #region UploaderView Binding Properties

        private UploadParam _selectedItem;

        public UploadParam SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                OnPropertyChanged("SelectedItem");
            }
        }

        public bool IsIdle
        {
            get
            {
                return !_uploader.IsWorking;
            }
        }

        #endregion

        #region UploaderView Commands

        #region OpenCommand

        private DelegateCommand _openCommand;

        public ICommand OpenCommand
        {
            get { return _openCommand ?? (_openCommand = new DelegateCommand(Open, CanOpen)); }
        }

        private void Open()
        {
            DataSetInfos.Clear();
            ShowSelectFilesDialog();
        }

        private bool CanOpen()
        {
            return !_uploader.IsWorking;
        }

        #endregion

        #region StartCommand

        private DelegateCommand _startCommand;

        public ICommand StartCommand
        {
            get { return _startCommand ?? (_startCommand = new DelegateCommand(OnStart, CanStart)); }
        }

        private bool CanStart()
        {
            return !_uploader.IsWorking && DataSetInfos.Count > 0;
        }

        private void OnStart()
        {
            var notConfigured = DataSetInfos.Where(x => x.ConfigurationState == ConfigurationState.Incomplete).Select(x => x.Name);

            if (notConfigured.Count() > 0)
            {
                MessageBox.Show(Ressources.ViewR.ConfigItemError+":\n - " + notConfigured.Aggregate((x, y) => y += "\n - " + x));
            }

            _uploader.Run();
        }

        #endregion

        #region SettingsCommand

        private DelegateCommand _settingsCommand;

        public ICommand SettingsCommand
        {
            get { return _settingsCommand ?? (_settingsCommand = new DelegateCommand(Settings)); }
        }

        private static void Settings()
        {
            SettingsWindowViewModel settingsViewModel = new SettingsWindowViewModel();
            SettingsWindowView settingsWindow = new SettingsWindowView { DataContext = settingsViewModel };

            settingsViewModel.RequestClose += settingsWindow.Close;

            settingsWindow.ShowDialog();
        }

        #endregion

        #region NewDatasetCommand

        private DelegateCommand _newDatasetCommand;

        public ICommand NewDatasetCommand
        {
            get { return _newDatasetCommand ?? (_newDatasetCommand = new DelegateCommand(NewDataSet, CanAddFile)); }
        }

        private void NewDataSet()
        {
            ShowSelectFilesDialog();
        }

        private bool CanAddFile()
        {
            return IsIdle;
        }

        #endregion

        #region ClearAllCommand

        private DelegateCommand _clearAllCommand;

        public ICommand ClearAllCommand
        {
            get { return _clearAllCommand ?? (_clearAllCommand = new DelegateCommand(ClearAll, CanClearAll)); }
        }

        private void ClearAll()
        {
            DataSetInfos.Clear();
        }

        private bool CanClearAll()
        {
            return IsIdle && DataSetInfos.Count > 0;
        }

        #endregion

        #region NewPlannedDatasetCommand

        private DelegateCommand _newPlannedDatasetCommand;

        public ICommand NewPlannedDatasetCommand
        {
            get { return _newPlannedDatasetCommand ?? (_newPlannedDatasetCommand = new DelegateCommand(AddPlannedDataset, CanAddPlannedDataset)); }
        }

        private void AddPlannedDataset()
        {
            var uploadParam = new UploadParam(SourceDataType.Csv);

            DataSetInfos.Add(uploadParam);
            DataSetInfos.OrderBy(info => info.Name);
            SelectedItem = uploadParam;

            OnActivateItem();
        }

        private bool CanAddPlannedDataset()
        {
            return IsIdle;
        }

        #endregion

        #region DeleteProcessCommand

        private DelegateCommand _deleteProcessCommand;

        public ICommand DeleteProcessCommand
        {
            get { return _deleteProcessCommand ?? (_deleteProcessCommand = new DelegateCommand(DeleteProcess, CanDeleteProcess)); }
        }

        private void DeleteProcess()
        {
            DataSetInfos.Remove(SelectedItem);
        }

        private bool CanDeleteProcess()
        {
            return IsIdle && SelectedItem != null;
        }

        #endregion

        #endregion

        public void OnActivateItem()
        {
            if (SelectedItem == null)
            {
                return;
            }

            try
            {
                MetadataWindowViewModel viewModel = new MetadataWindowViewModel(SelectedItem);
                MetadataWindowView window = new MetadataWindowView { DataContext = viewModel };

                viewModel.RequestClose += window.Close;

                window.ShowDialog();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void ShowSelectFilesDialog()
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                Multiselect = true,
                CheckFileExists = true,
                RestoreDirectory = true,
                DereferenceLinks = true,
                // Uncomment Lines below to enable KMZ Files to be imported (Current no Code Support)
                Filter = string.Concat("CSV (Comma delimited) (*.csv)|*.csv", "|",
                                       "KML (Keyhole Markup Language) (*.kml)|*.kml")
                //, "|",
                //"KMZ (*.kmz)|*.kmz")
            };

            if (!dlg.ShowDialog(Window.GetWindow(Application.Current.MainWindow)).Value)
            {
                return;
            }

            try
            {
                _uploader.AddFiles(dlg.FileNames);

                if (dlg.FileNames.Length == 1)
                {
                    SelectedItem = DataSetInfos[0];
                    OnActivateItem();
                }
            }
            catch (Exception ex)
            {
                Window window = Window.GetWindow(Application.Current.MainWindow);

                if (window == null)
                {
                    throw;
                }

                MessageBox.Show(window, ex.Message, window.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

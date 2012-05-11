using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using Ogdi.Data.DataLoader;
using Ogdi.Data.DataLoaderUI;
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

        #region SelectFilesCommand

        private DelegateCommand _selectFilesCommand;

        public ICommand SelectFilesCommand
        {
            get { return _selectFilesCommand ?? (_selectFilesCommand = new DelegateCommand(SelectFiles, CanSelectFiles)); }
        }

        private void SelectFiles()
        {
            DataSetInfos.Clear();
            ShowSelectFilesDialog();
        }

        private bool CanSelectFiles()
        {
            return !_uploader.IsWorking;
        }

        #endregion

        #region UploadCommand

        private DelegateCommand _uploadCommand;

        public ICommand UploadCommand
        {
            get { return _uploadCommand ?? (_uploadCommand = new DelegateCommand(OnUpload, CanUpload)); }
        }

        private bool CanUpload()
        {
            return !_uploader.IsWorking && DataSetInfos.Count > 0;
        }

        private void OnUpload()
        {
            var notConfigured = DataSetInfos.Where(x => x.ConfigurationState == ConfigurationState.Incomplete).Select(x => x.Name);

            if (notConfigured.Count() > 0)
                MessageBox.Show("One or more items cannot be processed since they haven't been properly configured:\n - " + notConfigured.Aggregate((x, y) => y += "\n - " + x));

            _uploader.Run();
        }

        #endregion

        #region ConnectionSettingsCommand

        private DelegateCommand _connectionSettingsCommand;

        public ICommand ConnectionSettingsCommand
        {
            get { return _connectionSettingsCommand ?? (_connectionSettingsCommand = new DelegateCommand(ConnectionSettings)); }
        }

        private static void ConnectionSettings()
        {
            var settingsViewModel = new SettingsWindowViewModel();
            var settingsWindow = new SettingsWindowView { DataContext = settingsViewModel };
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

        #region CleanListCommand

        private DelegateCommand _cleanListCommand;

        public ICommand CleanListCommand
        {
            get { return _cleanListCommand ?? (_cleanListCommand = new DelegateCommand(CleanList, CanClearList)); }
        }

        private void CleanList()
        {
            DataSetInfos.Clear();
        }

        private bool CanClearList()
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
            //foreach (object item in new ArrayList(DataSetListView.SelectedItems))
            //    DataSetInfos.Remove((UploadParam)item);

            DataSetInfos.Remove(SelectedItem);
        }

        private bool CanDeleteProcess()
        {
            return IsIdle && SelectedItem != null;
        }

        #endregion

        #region RemoveCompletedCommand

        private DelegateCommand _removeCompletedCommand;

        public ICommand RemoveCompletedCommand
        {
            get { return _removeCompletedCommand ?? (_removeCompletedCommand = new DelegateCommand(RemoveCompleted, CanRemoveCompleted)); }
        }

        private void RemoveCompleted()
        {
            foreach (UploadParam info in DataSetInfos.Where(i => i.State == UploaderState.Complete))
                DataSetInfos.Remove(info);
        }

        private bool CanRemoveCompleted()
        {
            return IsIdle && DataSetInfos.Any(x => x.State == UploaderState.Complete);
        }

        #endregion

        #endregion

        public void OnActivateItem()
        {
            if (SelectedItem == null)
                return;

            try
            {
                var viewModel = new MetadataWindowViewModel(SelectedItem);
                var window = new MetadataWindowView { DataContext = viewModel };
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
            var dlg = new OpenFileDialog
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
                return;

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
                    throw;

                MessageBox.Show(window, ex.Message, window.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

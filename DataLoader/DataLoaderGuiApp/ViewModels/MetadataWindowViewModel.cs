using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using Ogdi.Data.DataLoader;
using Ogdi.Data.DataLoaderGuiApp.Commands;
using Ogdi.Data.DataLoaderGuiApp.Views;
using System.ComponentModel.DataAnnotations;
using Tomers.WPF.MVVM;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;
using System.Text;

namespace Ogdi.Data.DataLoaderGuiApp.ViewModels
{
    public class MetadataWindowViewModel : WorkspaceViewModel
    {
        private readonly UploadParam _uploadParam;
        private readonly DataLoaderParams _dataLoaderParams;
        private readonly UserControl _producerParams;
        private readonly UserControl _processorParams;
        private readonly UserControl _metadata;
        private DelegateCommand _saveCommand;
        private DelegateCommand _closeCommand;

        public MetadataWindowViewModel(UploadParam uploadParam)
        {
            if (uploadParam == null)
                return;
            uploadParam.RefreshDataLoaderParams();
            _uploadParam = uploadParam;

            //Assigning ProducerControlView
            switch (uploadParam.Type)
            {
                case SourceDataType.Csv:
                    _dataLoaderParams = uploadParam.DataLoaderParams;

                    if (!_uploadParam.IsPlanned)
                    {
                        _producerParams = new CsvProducerControlView { DataContext = new CsvProducerControlViewModel(_dataLoaderParams) };
                    }
                    break;

                case SourceDataType.DbfAndKml:

                    _dataLoaderParams = uploadParam.DataLoaderParams;
                    _producerParams = new UserControl { IsEnabled = false };

                    break;

                case SourceDataType.Kml:

                    _dataLoaderParams = uploadParam.DataLoaderParams;
                    _producerParams = new CsvProducerControlView { DataContext = new CsvProducerControlViewModel(_dataLoaderParams) };

                    break;
                default:
                    throw new InvalidOperationException("Unknown upload parameter type.");
            }

            _processorParams = new ProcessorParamsControlView { DataContext = new ProcessorParamsControlViewModel(_dataLoaderParams) };
            _metadata = new MetadataControlView { DataContext = new MatadataControlViewModel(_dataLoaderParams) };
            _dataLoaderParams.TableMetadataEntity.IsEmpty = uploadParam.IsPlanned;
        }

        #region Commands


        public ICommand CloseWindowCommand
        {
            get { return _closeCommand ?? (_closeCommand = new DelegateCommand(Close)); }
        }

        public ICommand SaveCommand
        {
            get { return _saveCommand ?? (_saveCommand = new DelegateCommand(SaveData)); }
        }

        private void SaveData()
        {
            if (_dataLoaderParams.TableMetadataEntity.ExpiredDate <= _dataLoaderParams.TableMetadataEntity.ReleasedDate)
            {
                MessageBox.Show("Expiration date cannot be less or equal to Release date.");
                return;
            }

            if (string.IsNullOrEmpty(_uploadParam.Name))
            {
                var dlg = new SaveFileDialog
                              {
                                  AddExtension = true,
                                  DefaultExt = ".cfg",
                                  CheckPathExists = true,
                                  DereferenceLinks = true,
                                  Filter = "*.cfg|*.cfg",
                                  RestoreDirectory = true,
                                  ValidateNames = true,
                              };

                if (!dlg.ShowDialog(null).Value)
                    return;

                _uploadParam.Name = Path.GetFileNameWithoutExtension(dlg.FileName);
                _uploadParam.Directory = Path.GetDirectoryName(dlg.FileName);
            }

            string fileName = Path.Combine(_uploadParam.Directory, _uploadParam.Name + ".cfg");

            try
            {
                _dataLoaderParams.Validate(fileName);
                var viewModel = (MatadataControlViewModel)_metadata.DataContext;
                if (viewModel.Errors.Count > 0)
                {
                    StringBuilder errorBuilder = new StringBuilder();
                    errorBuilder.AppendLine("Error with the following fields\n");
                    foreach (var error in viewModel.Errors)
                    {
                        errorBuilder.AppendLine(error.Key);

                        foreach (var message in error.Value)
                        {
                            errorBuilder.AppendLine(string.Format("\t- {0}", message));
                        }
                    }
                    MessageBox.Show(errorBuilder.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
            }
            catch (WarningException ex)
            {
                if (MessageBox.Show(ex.Message, "OGDI Metadata", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation) != MessageBoxResult.OK)
                    return;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occured. " + ex.Message, "OGDI Metadata", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _dataLoaderParams.Save(fileName);
            _uploadParam.VerifyConfiguration();
            Close();
        }

        #endregion

        #region Bindings

        public UserControl ProcessorParams
        {
            get { return _processorParams; }
        }

        public UserControl Metadata
        {
            get { return _metadata; }
        }

        public Visibility TabsVisibility
        {
            get { return !_uploadParam.IsPlanned ? Visibility.Visible : Visibility.Hidden; }
        }

        public DataLoaderParams DataLoaderParams
        {
            get { return _dataLoaderParams; }
        }

        public UserControl ProducerParams
        {
            get { return _producerParams; }
        }

        #endregion
    }
}
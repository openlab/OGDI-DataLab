using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Ogdi.Data.DataLoader.Csv;
using Ogdi.Data.DataLoaderUI;

namespace Ogdi.Data.DataLoader
{
    public class UploadParam : INotifyPropertyChanged
    {
        private ConfigurationState _configurationState;
        private string _directory;
        private bool _isPlanned;
        private StringBuilder _message = new StringBuilder();
        private string _name;
        private TableOverwriteMode _overwriteMode = TableOverwriteMode.Create;
        private ProcessingStatus _processingStatus;
        private int _progress;
        private bool _refreshLastUpdateDate = true;
        private bool _sourceOrder;
        private UploaderState _state;
        private SourceDataType _type;
        private DataLoaderParams _dataLoaderParams;

        public DataLoaderParams DataLoaderParams
        {
            get { return _dataLoaderParams; }
        }

        public UploadParam(SourceDataType type)
        {
            Type = type;
            IsPlanned = true;

            switch (type)
            {
                case SourceDataType.Csv:
                    _dataLoaderParams = CsvToTablesDataLoaderParams.FromFile(this);
                    break;
                case SourceDataType.DbfAndKml:
                    break;
                case SourceDataType.Kml:
                    _dataLoaderParams = Kml.KmlToTablesDataLoaderParams.FromFile(this);
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        public UploadParam(string fileName)
        {
            var file = new FileInfo(fileName);
            var ext = file.Extension.ToLower();
            Directory = file.DirectoryName;
            Name = Path.GetFileNameWithoutExtension(file.Name);
            Progress = 0;
            IsPlanned = false;

            switch (ext)
            {
                case ".cfg":
                    ExtractSourceDataTypeFromConfigFile(fileName);
                    break;
                case ".csv":
                    goto default;
                case ".kml":
                    Type = SourceDataType.Kml;
                    _dataLoaderParams = Kml.KmlToTablesDataLoaderParams.FromFile(this);
                    break;
                default:
                    Type = SourceDataType.Csv;
                    _dataLoaderParams = CsvToTablesDataLoaderParams.FromFile(this);
                    break;
            }
        }

        private void ExtractSourceDataTypeFromConfigFile(string fileName)
        {
            using (XmlReader xmlReader = XmlReader.Create(fileName))
            {
                xmlReader.MoveToContent();

                switch (xmlReader.Name)
                {
                    case "CsvDataLoaderParams":
                        Type = SourceDataType.Csv;
                        break;
                    case "DbaseKmlDataLoaderParams":
                        Type = SourceDataType.DbfAndKml;
                        break;
                    case "KmlDataLoaderParams":
                        Type = SourceDataType.Kml;
                        break;
                    default:
                        throw new InvalidOperationException("Unknown file extension.");
                }
            }
        }

        public void RefreshDataLoaderParams()
        {
            switch (_type)
            {
                case SourceDataType.Csv:
                    goto default;
                case SourceDataType.DbfAndKml:
                    //_dataLoaderParams = DbaseKml.DbaseKmlToTablesDataLoaderParams.FromFile(this);
                    break;
                case SourceDataType.Kml:
                    _dataLoaderParams = Kml.KmlToTablesDataLoaderParams.FromFile(this);
                    break;
                default:
                    _dataLoaderParams = CsvToTablesDataLoaderParams.FromFile(this);
                    break;
            }


        }

        public string Name
        {
            get { return _name; }
            set { OnPropertyChanged("Name", ref _name, value); }
        }

        public UploaderState State
        {
            get { return _state; }
            set { OnPropertyChanged("State", ref _state, value); }
        }

        public SourceDataType Type
        {
            get { return _type; }
            set { OnPropertyChanged("Type", ref _type, value); }
        }

        public string Image
        {
            get { return "Images/" + ConfigurationState + ".ico"; }
        }

        public string Message
        {
            get { return _message.ToString(); }
        }

        public int Progress
        {
            get { return _progress; }
            set { OnPropertyChanged("Progress", ref _progress, value); }
        }

        public TableOverwriteMode OverwriteMode
        {
            get { return _overwriteMode; }
            set { OnPropertyChanged("OverwriteMode", ref _overwriteMode, value); }
        }

        public bool RefreshLastUpdateDate
        {
            get { return _refreshLastUpdateDate; }
            set { OnPropertyChanged("RefreshLastUpdateDate", ref _refreshLastUpdateDate, value); }
        }

        public bool SourceOrder
        {
            get { return _sourceOrder; }
            set { OnPropertyChanged("SourceOrder", ref _sourceOrder, value); }
        }

        public ProcessingStatus ProcessingStatus
        {
            get { return _processingStatus; }
            set { OnPropertyChanged("ProcessingStatus", ref _processingStatus, value); }
        }

        public string Directory
        {
            get { return _directory; }
            set { OnPropertyChanged("Directory", ref _directory, value); }
        }

        public bool IsPlanned
        {
            get { return _isPlanned; }
            set
            {
                OnPropertyChanged("IsPlanned", ref _isPlanned, value);
                OnPropertyChanged("PlannedAsString");
            }
        }

        public string PlannedAsString
        {
            get { return IsPlanned ? "Yes" : string.Empty; }
        }

        public ConfigurationState ConfigurationState
        {
            get { return _configurationState; }
            set
            {
                OnPropertyChanged("ConfigurationState", ref _configurationState, value);
                OnPropertyChanged("Image");
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public void AppendMessageLine(string message)
        {
            _message.AppendLine(message);
            OnPropertyChanged("Message");
        }

        public void Reset()
        {
            _message = new StringBuilder();
            OnPropertyChanged("Message");
            Progress = 0;
            State = UploaderState.Idle;
            ProcessingStatus = ProcessingStatus.NotProcessed;
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void OnPropertyChanged<TProperty>(string propertyName, ref TProperty oldValue, TProperty newValue)
        {
            if (Equals(oldValue, newValue))
            {
                return;
            }
            oldValue = newValue;

            OnPropertyChanged(propertyName);
        }

        #region Validation

        public void VerifyConfiguration()
        {
            try
            {
                string path = Path.Combine(Directory, Name + ".cfg");
                var serializer = new XmlSerializer(DataLoaderParams.GetType());

                if (File.Exists(path))
                {
                    using (FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read))
                    {
                        var parameters = (DataLoaderParams)serializer.Deserialize(stream);
                        ConfigurationState = parameters.TableMetadataEntity.IsEmpty ? VerifyPlanned(parameters) : VerifyPublished(parameters);
                    }
                }
                else
                {
                    ConfigurationState = IsPlanned ? ConfigurationState.Ready : ConfigurationState.Incomplete;
                }
            }
            catch (Exception)
            {
                ConfigurationState = ConfigurationState.Incomplete;
            }
        }

        private static ConfigurationState VerifyPublished(DataLoaderParams parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");

            if (parameters.TableMetadataEntity == null)
                throw new ArgumentException();

            if (parameters.ProcessorParams == null)
                throw new ArgumentException();

            if (parameters.TableMetadataEntity.IsEmpty)
                throw new ArgumentException();

            return VerifyTableMetadataEntity(parameters);
        }

        private static ConfigurationState VerifyPlanned(DataLoaderParams parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");

            if (parameters.TableMetadataEntity == null)
                throw new ArgumentException();

            if (!parameters.TableMetadataEntity.IsEmpty)
                throw new ArgumentException();

            return VerifyTableMetadataEntity(parameters);
        }

        private static ConfigurationState VerifyTableMetadataEntity(DataLoaderParams parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");

            if (parameters.TableMetadataEntity == null)
                throw new ArgumentException();

            var required = new[]
                               {
                                   parameters.TableMetadataEntity.Name, parameters.TableMetadataEntity.EntitySet,
                                   parameters.TableMetadataEntity.Source, parameters.TableMetadataEntity.Category
                               };

            if (required.Any(s => string.IsNullOrEmpty(s)))
                return ConfigurationState.Incomplete;

            foreach (PropertyInfo propertyInfo in typeof(TableMetadataEntity).GetProperties())
            {
                object val = propertyInfo.GetValue(parameters.TableMetadataEntity, null);
                if (val == null)
                    return ConfigurationState.Acceptable;

                var str = val as string;

                if (str != null && string.IsNullOrEmpty(str))
                    return ConfigurationState.Acceptable;
            }
            return ConfigurationState.Ready;
        }

        #endregion
    }
}
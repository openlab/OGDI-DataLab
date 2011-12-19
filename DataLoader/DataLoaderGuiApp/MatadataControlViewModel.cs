using System;
using Ogdi.Data.DataLoader;

namespace Ogdi.Data.DataLoaderGuiApp.ViewModels
{
    public class MatadataControlViewModel
    {
        private readonly TableMetadataEntity _instance;

        public MatadataControlViewModel(DataLoaderParams parameters)
        {
            _instance = parameters.TableMetadataEntity;
        }

        public string EntitySet
        {
            get { return _instance.EntitySet; }
            set { _instance.EntitySet = value; }
        }

        public string Name
        {
            get { return _instance.Name; }
            set { _instance.Name = value; }
        }

        public string Source
        {
            get { return _instance.Source; }
            set { _instance.Source = value; }
        }

        public string Category
        {
            get { return _instance.Category; }
            set { _instance.Category = value; }
        }

        public string TechnicalInfo
        {
            get { return _instance.TechnicalInfo; }
            set { _instance.TechnicalInfo = value; }
        }

        public string CollectionInstruments
        {
            get { return _instance.CollectionInstruments; }
            set { _instance.CollectionInstruments = value; }
        }

        public string DataDictionaryVariables
        {
            get { return _instance.DataDictionary_Variables; }
            set { _instance.DataDictionary_Variables = value; }
        }

        public string AdditionalInfo
        {
            get { return _instance.AdditionalInfo; }
            set { _instance.AdditionalInfo = value; }
        }

        public string Description
        {
            get { return _instance.Description; }
            set { _instance.Description = value; }
        }

        public string Keywords
        {
            get { return _instance.Keywords; }
            set { _instance.Keywords = value; }
        }

        public string Links
        {
            get { return _instance.Links; }
            set { _instance.Links = value; }
        }

        public string PeriodCovered
        {
            get { return _instance.PeriodCovered; }
            set { _instance.PeriodCovered = value; }
        }

        public string GeographicCoverage
        {
            get { return _instance.GeographicCoverage; }
            set { _instance.GeographicCoverage = value; }
        }

        public string CollectionMode
        {
            get { return _instance.CollectionMode; }
            set { _instance.CollectionMode = value; }
        }

        public string UpdateFrequency
        {
            get { return _instance.UpdateFrequency; }
            set { _instance.UpdateFrequency = value; }
        }

        public DateTime LastUpdateDate
        {
            get { return _instance.LastUpdateDate; }
            set { _instance.LastUpdateDate = value; }
        }

        public DateTime ReleasedDate
        {
            get { return _instance.ReleasedDate; }
            set { _instance.ReleasedDate = value; }
        }

        public DateTime ExpiredDate
        {
            get { return _instance.ExpiredDate; }
            set { _instance.ExpiredDate = value; }
        }

        public string MetadataUrl
        {
            get { return _instance.MetadataUrl; }
            set { _instance.MetadataUrl = value; }
        }

        public string EntityKind
        {
            get { return _instance.EntityKind; }
            set { _instance.EntityKind = value; }
        }
    }
}

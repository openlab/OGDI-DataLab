using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Threading;
using Ogdi.Data.DataLoader;
using Ogdi.Data.DataLoaderUI;

namespace Ogdi.Data.DataLoaderGuiApp
{
    public sealed class Uploader
    {
        public ObservableCollection<UploadParam> DataSetInfos { get; set; }
        public DataLoadingTarget DataLoadingTarget { get; set; }
        public bool IsWorking { get; private set; }
        private int _processingCount;
        private readonly int _maxNumberOfThreads;
        private DispatcherSynchronizationContext _synchronizationContext;
        private delegate void StartWorkDelegate(UploadParam uploadParam);

        public event EventHandler Start;
        public event EventHandler Complete;

        public Uploader()
        {
            DataLoadingTarget = DataLoadingTarget.Tables;
            _maxNumberOfThreads = DataLoaderParams.ConcurrentSetsCount;
            DataSetInfos = new ObservableCollection<UploadParam>();
        }

        public void AddFiles(IEnumerable<string> files)
        {
            foreach (var fileName in files)
            {
                var file = new FileInfo(fileName);
                string name = Path.GetFileNameWithoutExtension(file.Name);

                // Remove duplicates.
                foreach (var item in DataSetInfos.Where(x => x.Name == name).ToArray())
                    DataSetInfos.Remove(item);

                var uploadParam = new UploadParam(fileName);

                uploadParam.VerifyConfiguration();
                DataSetInfos.Add(uploadParam);
            }
            DataSetInfos.OrderBy(x => x.Name);
        }

        public void Run()
        {
            if (DataSetInfos.Count == 0)
                return;

            OnStart();
            foreach (var uploadParam in DataSetInfos)
            {
                uploadParam.Reset();
            }
            _synchronizationContext = new DispatcherSynchronizationContext();
            RunNext();
        }

        private void RunNext()
        {
            foreach (var uploadParam in DataSetInfos)
            {
                if (_processingCount >= _maxNumberOfThreads)
                    break;

                if (uploadParam.ProcessingStatus == ProcessingStatus.Processed ||
                    uploadParam.ProcessingStatus == ProcessingStatus.Processing)
                    continue;

                uploadParam.VerifyConfiguration();

                if (uploadParam.ConfigurationState == ConfigurationState.Incomplete)
                    continue;

                _processingCount++;

                uploadParam.ProcessingStatus = ProcessingStatus.Processing;

                // Start new worker.
                var biDelegate = new StartWorkDelegate(StartWork);
                biDelegate.BeginInvoke(uploadParam, null, null);
            }

            var isComplete = _processingCount == 0;

            if (isComplete)
                OnComplete();
        }

        private void OnStart()
        {
            IsWorking = true;

            if (Start != null)
                Start(this, EventArgs.Empty);
        }

        private void OnComplete()
        {
            IsWorking = false;

            if (Complete != null)
                Complete(this, EventArgs.Empty);
        }

        private void StartWork(UploadParam uploadParam)
        {
            try
            {
                Directory.SetCurrentDirectory(uploadParam.Directory);

                uploadParam.State = UploaderState.Processing;

                bool wasError = false;

                var dataLoader = DataLoaderFactory.CreateDataLoader(
                    uploadParam.Type, DataLoadingTarget, uploadParam.Name, uploadParam.OverwriteMode, uploadParam.SourceOrder);

                dataLoader.Load(
                    (totalCount, pocessedCount) => _synchronizationContext.Post(
                        state => { uploadParam.Progress = (int)(99 * (((float)pocessedCount) / totalCount)); }, null),
                        ex =>
                        {
                            wasError = true;
                            _synchronizationContext.Post(
                                state =>
                                {
                                    uploadParam.AppendMessageLine(ExceptionHelper.GetMessageStack(ex));
                                    uploadParam.State = UploaderState.Errors;
                                }, null);
                        });

                if (!wasError)
                    uploadParam.State = UploaderState.Complete;
            }
            catch (Exception ex)
            {
                _synchronizationContext.Post(
                    state =>
                    {
                        uploadParam.State = UploaderState.Failed;
                        uploadParam.AppendMessageLine(ExceptionHelper.GetMessageStack(ex));
                    }, null);
            }
            finally
            {
                _synchronizationContext.Post(
                    state =>
                    {
                        uploadParam.Progress = 100;
                        uploadParam.ProcessingStatus = ProcessingStatus.Processed;
                        _processingCount--;
                        RunNext();
                    }, null);
            }
        }
    }
}
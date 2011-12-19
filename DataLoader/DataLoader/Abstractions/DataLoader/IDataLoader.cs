using System;

namespace Ogdi.Data.DataLoader
{
    public delegate void ProgressCallback(int totalCount, int pocessedCount);
    public delegate void OnContinueExceptionCallback(Exception ex);

    public interface IDataLoader
    {
        void Load(ProgressCallback progressNotifier, OnContinueExceptionCallback exceptionNotifier);
    }
}

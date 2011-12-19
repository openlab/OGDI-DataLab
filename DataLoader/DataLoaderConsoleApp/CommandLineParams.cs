
using Ogdi.Data.DataLoader;

namespace Ogdi.Data.DataLoaderConsoleApp
{
    class CommandLineParams
    {
        public CommandLineParams()
        {
            RefreshLastUpdateDate = false;
            SourceOrder = false;
        }

        public string FileSetName { get; set; }
        
        public SourceDataType DataType { get; set; }
        
        public DataLoadingTarget LoadingTarget { get; set; }

        public TableOverwriteMode OverwriteMode { get; set; }
        
        public bool RefreshLastUpdateDate { get; set; }

        public bool SourceOrder { get; set; }
    }
}

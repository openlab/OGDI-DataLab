using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ogdi.Data.DataLoader;

namespace Ogdi.Data.DataLoaderGuiApp
{
    public class PropertyToTypeMap
    {
        public ObservableCollection<PropertyToType> PropertyToTypes { get; set; }
        public IList<string> Types { get; set; }
        public IList<string> IndexInNames { get; set; }

        public PropertyToTypeMap()
        {
            Types = new List<string> { "string", "int32", "int64", "double", "bool", "bool-0or1", "datetime", "datetime-yyyymmdd" };
            IndexInNames = new List<string> { "", "{0}", "{1}", "{2}", "{3}", "{4}", "{5}", "{6}", "{7}", "{8}", "{9}" };
        }
    }

    
}

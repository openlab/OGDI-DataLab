using System;

namespace Ogdi.Data.DataLoader
{
    public class PropertyToType
    {
        private string _name;
        private string _type;
        private string _indexInName;

        public String Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public String Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public String IndexInName
        {
            get { return _indexInName; }
            set
            {
                _indexInName = value;
            }
        }

        public PropertyToType(string name, string type, string indexInName)
        {
            Name = name;
            Type = type;
            IndexInName = indexInName;
        }
    }
}

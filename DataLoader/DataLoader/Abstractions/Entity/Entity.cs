using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ogdi.Data.DataLoader
{
    public class Entity : IEnumerable<Property>
    {
        private readonly List<Property> _props = new List<Property>();
        private string _number;

        public Entity()
        {
            AddProperty(DataLoaderConstants.PropNameEntityId, Guid.NewGuid());
        }

        public Entity(Guid id)
        {
            AddProperty(DataLoaderConstants.PropNameEntityId, id);
        }

        public Guid Id
        {
            get
            {
                return (Guid)(_props.Where(p => p.Name.Equals(DataLoaderConstants.PropNameEntityId)).Select(p => p.Value)).First();
            }
        }

        public string Number
        {
            get { return _number; }
        }


        public object this[string name]
        {
            get { return (_props.Where(p => p.Name.ToLower().Equals(name.ToLower())).Select(p => p.Value)).FirstOrDefault(); }

            set
            {
                foreach (Property p in _props)
                {
                    if (p.Name.ToLower() == name.ToLower())
                    {
                        p.Value = value;
                        return;
                    }
                }

                throw new ArgumentException(DataLoaderConstants.MsgPropertyNotFound, name);
            }
        }

        #region IEnumerable<Property> Members

        public IEnumerator<Property> GetEnumerator()
        {
            return _props.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _props.GetEnumerator();
        }

        #endregion

        public void SetNumber(int count, string startTimePrefix)
        {
            _number = string.Format("{0}{1}", startTimePrefix, count.ToString("D10"));
        }

        public void AddProperty(string name, object value)
        {
            if (value != null && value.GetType() != typeof(DBNull))
            {
                if (_props.Count(p => p.Name.Equals(name)) > 0)
                {
                    throw new ArgumentException(string.Format(DataLoaderConstants.MsgDuplicatePropNameException, name,
                                                              value));
                }

                _props.Add(new Property(name, value));
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("\r\nEntity Start:\r\n");
            foreach (Property p in _props)
            {
                sb.AppendFormat("\t{0}\r\n", p);
            }

            sb.Append("Entity End");
            return sb.ToString();
        }

        public void ValidateProperty(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return;
            propertyName = propertyName.ToLower();
            if (
                string.IsNullOrEmpty(propertyName)
                || string.Compare(propertyName, DataLoaderConstants.PropNameEntityId.ToLower(), true) == 0
                || string.Compare(propertyName, DataLoaderConstants.PropNameKmlSnippet.ToLower(), true) == 0
                || string.Compare(propertyName, DataLoaderConstants.PropNameEntitySet.ToLower(), true) == 0
                || string.Compare(propertyName, DataLoaderConstants.PropNameEntityKind.ToLower(), true) == 0
                || string.Compare(propertyName, DataLoaderConstants.PropNameLastUpdateDate.ToLower(), true) == 0
                || string.Compare(propertyName, DataLoaderConstants.ValueUniqueAutoGen.ToLower(), true) == 0
                )
            {
                return;
            }

            if (this.Where(i => string.Compare(propertyName, i.Name.ToLower(), true) == 0).ToList().Count != 0)
            {
                return;
            }

            throw new ParamsValidationException(propertyName);
        }
    }

    public class Property
    {
        public Property(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }

        public object Value { get; set; }

        public override string ToString()
        {
            return string.Format("Name = '{0}' Value = '{1}'", Name, Value);
        }
    }
}
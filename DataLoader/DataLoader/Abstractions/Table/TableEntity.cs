using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.StorageClient;

namespace Ogdi.Data.DataLoader
{
    public class TableEntity : TableServiceEntity
    {
        private Entity _entity;

        public TableEntity()
        {
        }

        public TableEntity(Entity entity)
            : this(entity, entity.Id.ToString(), string.Empty)
        {
        }

        public TableEntity(Entity entity, string partitionKey, string rowKey)
            : base(partitionKey, rowKey)
        {
            _entity = entity;
        }

        public IEnumerable<NameTypeValueTuple> GetProperties()
        {
            foreach (Property e in _entity)
            {
                if (IsValidPropertyValue(e.Value))
                {
                    yield return new NameTypeValueTuple
                                     {
                                         Name = ConvertToValidPropertyName(e.Name),
                                         Type = ConvertClrTypeToEdmType(e.Value.GetType()),
                                         Value = e.Value
                                     };
                }
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("\nTableEntity Start:\n");
            sb.AppendFormat("\t'PartitionKey '{0}'\n", PartitionKey);
            sb.AppendFormat("\t'RowKey '{0}'\n", RowKey);
            foreach (NameTypeValueTuple p in GetProperties())
            {
                sb.AppendFormat("\t'{0}' '{1}' '{2}'\n", p.Name, p.Type, p.Value);
            }

            sb.Append("TableEntity End\n");
            return sb.ToString();
        }

        private static bool IsValidPropertyValue(object p)
        {
            bool isValid = true;

            if (p.GetType() == typeof(DateTime))
            {
                var d = (DateTime)p;
                if (d < new DateTime(1600, 1, 1) || d > new DateTime(9999, 12, 31))
                {
                    isValid = false;
                }
            }

            return isValid;
        }

        private static string ConvertClrTypeToEdmType(Type clrType)
        {
            if (clrType == typeof(string))
                return null;

            if (clrType == typeof(Int32))
                return "Edm.Int32";

            if (clrType == typeof(Int64))
                return "Edm.Int64";

            if (clrType == typeof(double))
                return "Edm.Double";

            if (clrType == typeof(bool))
                return "Edm.Boolean";

            if (clrType == typeof(Guid))
                return "Edm.Guid";

            if (clrType == typeof(DateTime))
                return "Edm.DateTime";

            throw new ArgumentException(string.Format(DataLoaderConstants.MsgUnsupportedClrType, clrType));
        }

        private static string ConvertToValidPropertyName(string name)
        {
            string nameIn = name.ToLower();
            var nameOut = new StringBuilder();

            foreach (char t in nameIn)
            {
                if (char.IsLetterOrDigit(t) || t == '-' || t == '_')
                    nameOut.Append(t);
            }

            if (char.IsLetter(nameOut[0]))
                return nameOut.ToString();

            return nameOut.Insert(0, 'a').ToString();
        }


        public void SetEntity(Entity entity)
        {
            _entity = entity;
        }

        /*
         * Used when we want to load entity from storage and update its columns except EntityID
         */

        public void UpdateEntity(IEnumerable<Property> from)
        {
            foreach (Property prop in from)
            {
                if (_entity[prop.Name] == null)
                    _entity.AddProperty(prop.Name, prop.Value);
                else if (prop.Name.ToLower() != DataLoaderConstants.PropNameEntityId.ToLower())
                    _entity[prop.Name] = prop.Value;
            }
        }

        public void UpdateProperty(string name, object value)
        {
            if (_entity[name] == null)
                _entity.AddProperty(name, value);
            else
                _entity[name] = value;
        }


        /*
         * Check if property values are the same (except ExceptionColumns)
         * Returns null if values are same
         */

        public string FindDifferences(Entity to, string[] ExceptionColumns)
        {
            string result = "";
            var newProps = new List<Property>();
            DateTime oldLastUpdateDate = DateTime.MinValue;
            DateTime newLastUpdateDate = DateTime.MinValue;
            //get only properties with values != null

            foreach (Property prop in to)
            {
                if (IsBelongTo(prop.Name, ExceptionColumns))
                    continue;

                if (prop.Value == null)
                    continue;

                if (prop.Value is DateTime && (DateTime)prop.Value == DateTime.MinValue) //MinDate is like null
                    continue;

                newProps.Add(prop);
            }

            var oldProps = _entity.Where(prop => !IsBelongTo(prop.Name, ExceptionColumns)).ToList();

            //check that columns equal
            if (oldProps.Count() != newProps.Count())
            {
                string oldColumns = oldProps.Aggregate("", (current, prop) => current + (prop.Name + "; "));
                string newColumns = newProps.Aggregate("", (current, prop) => current + (prop.Name + "; "));

                result += "Different columns: " + Environment.NewLine;
                result += "existing columns: " + oldColumns + Environment.NewLine;
                result += "new columns: " + newColumns + Environment.NewLine;
                return result;
            }
            //check values
            foreach (Property prop in oldProps)
            {
                string oldValue = prop.Value == null ? "null" : prop.Value.ToString();
                string newValue = to[prop.Name] == null ? "null" : to[prop.Name].ToString();
                if (oldValue != newValue)
                {
                    result += prop.Name + " : existing = " + oldValue + " : new = " + newValue + Environment.NewLine;
                }
            }
            if (result == "")
                result = null;

            return result;
        }

        //some util function
        private static bool IsBelongTo(string value, IEnumerable<string> set)
        {
            if (String.IsNullOrEmpty(value) || set == null)
                return false;

            return set.Any(st => st != null && value.ToLower() == st.ToLower());
        }

        #region Nested type: NameTypeValueTuple

        public class NameTypeValueTuple
        {
            public string Name { get; set; }

            public string Type { get; set; }

            public object Value { get; set; }
        }

        #endregion
    }
}
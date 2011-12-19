using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ogdi.Data.DataLoader
{
    public enum FieldType
    {
        Required,
        Optinal
    }

    public class ValidateFieldAttribute : Attribute
    {
        public ValidateFieldAttribute(string name)
        {
            Name = name;
            Type = FieldType.Required;
        }

        public ValidateFieldAttribute(string name, FieldType type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; set; }
        public FieldType Type { get; set; }

        public static List<string> GetFieldList(FieldType fieldType, object obj)
        {
            var list = new List<string>();
            Type type = obj.GetType();

            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                var attribute = (ValidateFieldAttribute)property.GetCustomAttributes(typeof(ValidateFieldAttribute), true)
                    .FirstOrDefault(r => ((ValidateFieldAttribute)r).Type == fieldType);

                if (attribute == null)
                    continue;

                object value = property.GetValue(obj, null);

                if (value is string)
                {
                    if (string.IsNullOrEmpty((string)value))
                        list.Add(attribute.Name);
                }
            }

            return list;
        }
    }
}
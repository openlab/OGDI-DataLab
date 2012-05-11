using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Ogdi.Data.DataLoader
{
    public abstract class EntityProducer
    {
        public int EntityCount { get; protected set; }

        public abstract Entity SchemaEntity { get; }

        public abstract IEnumerable<Entity> GetEntitiesEnumerator(OnContinueExceptionCallback exceptionNotifier, DataLoaderParams Params);

        public abstract void ValidateParams();

        public static string GetSecondsFrom2000Prefix()
        {
            var seconds = (int) ((DateTime.Now.Ticks - DataLoaderConstants.InitialDateTime2000)/10000000);
            return string.Format("{0}_", seconds.ToString("D10"));
        }

        protected static object GetPropertyValue(string type, string value)
        {
            type = type.ToLower();
            switch (type)
            {
                case ("string"):
                    return value;
                case ("int32"):
                    return Int32.Parse(value);
                case ("int64"):
                    return Int64.Parse(value);
                case ("double"):
                    var formatInfo = CultureInfo.InvariantCulture.NumberFormat;
                    return double.Parse(value.Replace(',', '.'), formatInfo);
                case ("bool"):
                    return bool.Parse(value);
                case ("bool-0or1"):
                    return int.Parse(value) == 1 ? true : false;
                case ("datetime"):
                    return DateTime.Parse(value);
                case ("datetime-yyyymmdd"):
                    {
                        var s = value.Trim();
                        var y = int.Parse(s.Substring(0, 4));
                        var m = int.Parse(s.Substring(4, 2));
                        var d = int.Parse(s.Substring(6, 2));
                        return new DateTime(y, m, d);
                    }
                default:
                    throw new ArgumentException(DataLoaderConstants.MsgUnsupportedType, type);
            }
        }

        protected static string GetPropertyType(string type)
        {
            type = type.ToLower();
            switch (type)
            {
                case ("string"):
                    return typeof(string).ToString();
                case ("int32"):
                    return typeof(Int32).ToString();
                case ("int64"):
                    return typeof(Int64).ToString();
                case ("double"):
                    return typeof(double).ToString();
                case ("bool"):
                    return typeof(bool).ToString();
                case ("bool-0or1"):
                    return typeof(Int32).ToString();
                case ("datetime"):
                    return typeof(DateTime).ToString();
                case ("datetime-yyyymmdd"):
                    return typeof(DateTime).ToString();
                default:
                    throw new ArgumentException(DataLoaderConstants.MsgUnsupportedType, type);
            }
        }

        protected static string GetRdfType(string type)
        {
            type = type.ToLower();
            switch (type)
            {
                case ("string"):
                    return "xs:string";
                case ("int32"):
                    return "xs:int";
                case ("int64"):
                    return "xs:long";
                case ("double"):
                    return "xs:double";
                case ("bool"):
                    return "xs:boolean";
                case ("bool-0or1"):
                    return "xs:boolean";
                case ("datetime"):
                    return "xs:dateTime";
                case ("datetime-yyyymmdd"):
                    return "xs:dateTime";
                default:
                    throw new ArgumentException(DataLoaderConstants.MsgUnsupportedType, type);
            }
        }

        protected static string CleanStringLower(string messy)
        {
            string final = CleanStringDiacritics(messy);
            final = CleanStringSpecialChars(final);
            return final.ToLower();
        }

        protected static string CleanStringDiacritics(string messy)
        {
            String normalizedString = messy.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < normalizedString.Length; i++)
            {
                Char c = normalizedString[i];
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(c);
            }

            string final = stringBuilder.ToString();

            final = final.Replace("Ø", "O");
            final = final.Replace("ø", "o");
            final = final.Replace("ð", "o");
            final = final.Replace("Ð", "D");

            return final;
        }

        protected static string CleanStringSpecialChars(string messy)
        {
            return Regex.Replace(messy, "[^0-9a-zA-Z]+", "");
        }

        internal const string ValidContainerNameRegex = @"^([a-z]|\d){1}([a-z]|-|\d){1,61}([a-z]|\d){1}$";

        public static bool IsValidContainerName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }
            Regex reg = new Regex(ValidContainerNameRegex);
            if (reg.IsMatch(name))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
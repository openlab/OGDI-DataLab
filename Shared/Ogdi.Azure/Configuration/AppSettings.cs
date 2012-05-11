using System;
using System.Configuration;
using System.Reflection;
using Microsoft.WindowsAzure;
using System.Linq;

namespace Ogdi.Azure
{

    public class DataLoaderSettings : AppSettings
    {
        public string DataConnectionString {get;protected set;}

        public void UpdateDataConnectionString(string accountName, string accountKey)
        {
            DataConnectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", 
                accountName, accountKey);
        }

        public string GetAccessKey()
        {
            return GetValueFromConnectionString("AccountKey");
        }
        public string GetAccessName() 
        {
            return GetValueFromConnectionString("AccountName");
        }

        private string GetValueFromConnectionString(string keyName)
        {
            keyName += "=";
            string[] values = DataConnectionString.Split(';');
            string value = values.Where(r => r.Contains(keyName)).FirstOrDefault();
            if (!string.IsNullOrEmpty(value))
            {
                value = value.Replace(keyName, "").Trim();
            }
            return value;
        }
    }

    //http://geekswithblogs.net/willemf/archive/2006/11/13/96943.aspx
    public abstract class AppSettings
    {
        protected string exePath;

        public AppSettings(string exePath)
        {
            this.exePath = exePath;
        }

        // Load the settings.
        public virtual void Load()
        {
            System.Configuration.Configuration configuration;
            configuration = (string.IsNullOrEmpty(exePath)) ? ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None)
                : ConfigurationManager.OpenExeConfiguration(exePath);

            PropertyInfo[] properties = GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (!property.PropertyType.IsArray)
                {
                    //string appValue = ConfigurationManager.AppSettings[property.Name];
                    string appValue = configuration.AppSettings.Settings[property.Name].Value;
                    if (appValue != null)
                    {
                        try
                        {
                            // Attempt to change the type from System.String to
                            // the property type, and set the property value.
                            property.SetValue(
                              this,
                              Convert.ChangeType(appValue, property.PropertyType),
                              null);
                        }
                        catch {/*ignore*/}
                    }
                }
            }
        }

        // check for empty values and return first empty
        public string CheckEmptyValues()
        {
            System.Configuration.Configuration configuration;
            configuration = (string.IsNullOrEmpty(exePath)) ? ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None)
                : ConfigurationManager.OpenExeConfiguration(exePath);

            PropertyInfo[] properties = GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (!property.PropertyType.IsArray)
                {
                    //string appValue = ConfigurationManager.AppSettings[property.Name];
                    string appValue = configuration.AppSettings.Settings[property.Name].Value;
                    if (string.IsNullOrEmpty(appValue))
                    {
                        return property.Name;
                    }
                }
            }
            return null;
        }

        // Save the settings.
        public virtual void Save()
        {
            // Get the configuration file.
            System.Configuration.Configuration configuration;
            configuration = (string.IsNullOrEmpty(exePath)) ? ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None)
                :ConfigurationManager.OpenExeConfiguration(exePath);
            // Get the public properties of the class.
            PropertyInfo[] properties = GetType().GetProperties();
            // Save each property setting.
            foreach (PropertyInfo property in properties)
            {
                // Save if not an array type.
                if (!property.PropertyType.IsArray)
                {
                    // Remove the setting if it exists.
                    if (configuration.AppSettings.Settings[property.Name] != null)
                    {
                        configuration.AppSettings.Settings.Remove(property.Name);
                    }
                    // Add the setting.
                    configuration.AppSettings.Settings.Add(
                      property.Name,
                      property.GetValue(this, null).ToString());
                }
            }
            // Save the configuration settings.
            configuration.Save(ConfigurationSaveMode.Modified);
            // Force a reload of the whole section.
            ConfigurationManager.RefreshSection("appSettings");
        }
        protected AppSettings()
        {
            // Load the settings when class instance is constructed.
            Load();
        }
        ~AppSettings()
        {
            // Save settings when instance is destroyed.
            Save();
        }
    }
}

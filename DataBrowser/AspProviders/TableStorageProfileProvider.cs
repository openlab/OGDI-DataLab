//
// <copyright file="TableStorageProfileProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Configuration.Provider;
using System.Data.Services.Client;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Profile;
using System.Xml.Serialization;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Microsoft.Samples.ServiceHosting.AspProviders
{

    public class TableStorageProfileProvider : ProfileProvider
    {

        #region Member variables and constants

        private const int NumRetries = 3;

        // member variables shared between most providers
        private string _applicationName;
        private string _tableServiceBaseUri;
        private string _blobServiceBaseUri;
        private string _tableName;
        private string _containerName;
        private BlobProvider _blobProvider;
        private static object _lock = new object();
        private CloudTableClient _tableStorage;
        private CloudStorageAccount _account;
        private RetryPolicy _tableRetry = RetryPolicies.Retry(NumRetries, TimeSpan.FromSeconds(1));
        private ProviderRetryPolicy _providerRetry = ProviderRetryPolicies.RetryN(NumRetries, TimeSpan.FromSeconds(1));

        #endregion

        public override string ApplicationName
        {
            get { return _applicationName; }
            set
            {
                lock (_lock)
                {
                    SecUtility.CheckParameter(ref value, true, true, true, Constants.MaxTableApplicationNameLength, "ApplicationName");
                    _applicationName = value;
                }
            }
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            // Verify that config isn't null
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            // Assign the provider a default name if it doesn't have one
            if (String.IsNullOrEmpty(name))
            {
                name = "TableStorageProfileProvider";
            }

            // Add a default "description" attribute to config if the
            // attribute doesn't exist or is empty
            if (string.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "Table storage-based profile provider");
            }

            // Call the base class's Initialize method
            base.Initialize(name, config);

            bool allowInsecureRemoteEndpoints = Configuration.GetBooleanValue(config, "allowInsecureRemoteEndpoints", false);

            // structure storage-related properties
            ApplicationName = Configuration.GetStringValueWithGlobalDefault(config, "applicationName",
                                                            Configuration.DefaultProviderApplicationNameConfigurationString,
                                                            Configuration.DefaultProviderApplicationName, false);

            _account =
                CloudStorageAccount.Parse(
                    RoleEnvironment.GetConfigurationSettingValue(Configuration.ConfigurationStorageConnectionStringName));

            // profile information are stored in the membership table
            _tableName = Configuration.GetStringValueWithGlobalDefault(config, "membershipTableName",
                                                           Configuration.DefaultMembershipTableNameConfigurationString,
                                                           Configuration.DefaultMembershipTableName, false);
            
            _containerName = Configuration.GetStringValueWithGlobalDefault(config, "containerName", 
                                                           Configuration.DefaultProfileContainerNameConfigurationString,
                                                           Configuration.DefaultProfileContainerName, false);

            if (!SecUtility.IsValidContainerName(_containerName))
            {
                throw new ProviderException("The provider configuration for the TableStorageProfileProvider does not contain a valid container name. " +
                                            "Please refer to the documentation for the concrete rules for valid container names." +
                                            "The current container name is" + _containerName);
            }
            _blobServiceBaseUri = Configuration.GetStringValue(config, "blobServiceBaseUri", null, true);

            // remove required attributes
            config.Remove("allowInsecureRemoteEndpoints");
            config.Remove("applicationName");
            config.Remove("membershipTableName");
            config.Remove("containerName");
            config.Remove("tableServiceBaseUri");
            config.Remove("blobServiceBaseUri");

            // Throw an exception if unrecognized attributes remain
            if (config.Count > 0)
            {
                string attr = config.GetKey(0);
                if (!String.IsNullOrEmpty(attr))
                {
                    throw new ProviderException(string.Format(CultureInfo.InstalledUICulture, "Unrecognized attribute: {0}", attr));
                }
            }

            // profiles are stored within the membership table
            _tableStorage = _account.CreateCloudTableClient();
            var blobClient = _account.CreateCloudBlobClient();
            try
            {
                if (_account == null)
                    throw new ConfigurationErrorsException("Account information incomplete!");

                _tableStorage.RetryPolicy = _tableRetry;

                SecUtility.CheckAllowInsecureEndpoints(allowInsecureRemoteEndpoints, _tableStorage.BaseUri);

                if (_tableStorage.CreateTableIfNotExist(_tableName))
                {
                    var ctx = _tableStorage.GetDataServiceContext();
                    var dummyRow = new MembershipRow("dummy", "none");
                    ctx.AddObject(_tableName, dummyRow);
                    ctx.SaveChangesWithRetries();
                    ctx.DeleteObject(dummyRow);
                    ctx.SaveChangesWithRetries();
                }

                SecUtility.CheckAllowInsecureEndpoints(allowInsecureRemoteEndpoints, blobClient.BaseUri);
                _blobProvider = new BlobProvider(blobClient, _containerName);
            }
            catch (SecurityException)
            {
                throw;
            }
            // catch InvalidOperationException and StorageException
            catch (Exception e)
            {
                string exceptionDescription = Configuration.GetInitExceptionDescription(blobClient, _tableStorage);
                string tableName = _tableName ?? "no profile table name specified";
                string containerName = _containerName ?? "no container name specified";
                Log.Write(EventKind.Error, "Initialization of data service structures (tables and/or blobs) failed!" + Environment.NewLine +
                                            exceptionDescription + Environment.NewLine +
                                            "Configured blob container: " + containerName + Environment.NewLine +
                                            "Configured table name: " + tableName + Environment.NewLine +
                                            e.Message + Environment.NewLine + e.StackTrace);
                throw new ProviderException("Initialization of data service structures (tables and/or blobs) failed! " +
                                            "The most probable reason for this is that " +
                                            "the storage endpoints are not configured correctly. Please look at the configuration settings " +
                                            "in your .cscfg and Web.config files. More information about this error " +
                                            "can be found in the logs when running inside the hosting environment or in the output " +
                                            "window of Visual Studio.", e);
            }
            Debug.Assert(_blobProvider != null);
        }


        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context,
                                                                          SettingsPropertyCollection collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            SettingsPropertyValueCollection settings = new SettingsPropertyValueCollection();

            // Do nothing if there are no properties to retrieve
            if (collection.Count < 1)
            {
                return settings;
            }

            // For properties lacking an explicit SerializeAs setting, set
            // SerializeAs to String for strings and primitives, and XML
            // for everything else
            foreach (SettingsProperty property in collection)
            {
                if (property.SerializeAs == SettingsSerializeAs.ProviderSpecific)
                {
                    if (property.PropertyType.IsPrimitive || property.PropertyType == typeof(String))
                    {
                        property.SerializeAs = SettingsSerializeAs.String;
                    }
                    else
                    {
                        property.SerializeAs = SettingsSerializeAs.Xml;
                    }
                }
                settings.Add(new SettingsPropertyValue(property));
            }

            // Get the user name or anonymous user ID
            string username = (string)context["UserName"];
            
            if (string.IsNullOrEmpty(username))
            {
                return settings;
            }
            
            if (!VerifyUsername(ref username))
            {
                return settings;
            }

            MembershipRow profile = null;
            if (!DoesProfileExistAndUpdateUser(username, out profile))
            {
                // the profile does not exist
                // we update the last activity time of the user only if the profile does exist
                // so we can just return here
                return settings;
            }

            Debug.Assert(profile != null);

            // We are ready to go: load the profile            
            // Note that we do not have to deal with write locks here because we use a 
            // different blob name each time we write a new profile
            StreamReader reader = null;
            MemoryStream stream = null;
            BlobProperties blobProperties;
            string[] names;
            string values;
            byte[] buf = null;
            string line;
            try
            {
                // Open the blob containing the profile data 
                stream = new MemoryStream();
                if (!_blobProvider.GetBlobContentsWithoutInitialization(profile.ProfileBlobName, stream, out blobProperties) || blobProperties.Length == 0)
                {
                    // not an error if the blob does not exist
                    return settings;
                }
                stream.Seek(0, SeekOrigin.Begin);
                reader = new StreamReader(stream);

                // Read names, values, and buf from the blob
                line = reader.ReadLine();
                names = line.Split(':');
                values = reader.ReadLine();
                if (!string.IsNullOrEmpty(values))
                {
                    UnicodeEncoding encoding = new UnicodeEncoding();
                    values = encoding.GetString(Convert.FromBase64String(values));
                }
                string temp = reader.ReadLine();
                if (!String.IsNullOrEmpty(temp))
                {
                    buf = Convert.FromBase64String(temp);
                }
                else
                {
                    buf = new byte[0];
                }
            }
            catch (InvalidOperationException se)
            {
                throw new ProviderException("Error accessing blob storage when getting property values!", se);
            }
            catch (Exception e)
            {
                throw new ProviderException("Error getting property values.", e);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
                if (stream != null)
                {
                    stream.Close();
                }
            }
            // Decode names, values, and buf and initialize the
            // SettingsPropertyValueCollection returned to the caller
            DecodeProfileData(names, values, buf, settings);

            return settings;
        }

        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            // Get information about the user who owns the profile
            string username = (string)context["UserName"];
            bool authenticated = (bool)context["IsAuthenticated"];

            if (String.IsNullOrEmpty(username) || collection.Count == 0)
            {
                return;
            }

            if (!VerifyUsername(ref username))
            {
                return;
            }

            // lets create the blob containing the profile without checking whether the user exits

            // Format the profile data for saving
            string names = String.Empty;
            string values = String.Empty;
            byte[] buf = null;

            EncodeProfileData(ref names, ref values, ref buf, collection, authenticated);

            // Do nothing if no properties need saving
            if (string.IsNullOrEmpty(names))
            {
                return;
            }

            // Save the profile data
            MemoryStream stream = null;
            StreamWriter writer = null;
            string blobName = null;
            try
            {
                stream = new MemoryStream();
                writer = new StreamWriter(stream);

                writer.WriteLine(names);
                if (!String.IsNullOrEmpty(values))
                {
                    UnicodeEncoding encoding = new UnicodeEncoding();
                    writer.WriteLine(Convert.ToBase64String(encoding.GetBytes(values)));
                }
                else
                {
                    writer.WriteLine();
                }
                if (buf != null && buf.Length > 0)
                {
                    writer.WriteLine(Convert.ToBase64String(buf));
                }
                else
                {
                    writer.WriteLine();
                }
                writer.Flush();
                blobName = GetProfileBlobPrefix(username) + Guid.NewGuid().ToString("N");
                Debug.Assert(blobName.Length <= Configuration.MaxStringPropertySizeInChars);
                stream.Seek(0, SeekOrigin.Begin);
                _blobProvider.UploadStream(blobName, stream);

                CreateOrUpdateUserAndProfile(username, authenticated, DateTime.UtcNow, blobName, (int)stream.Length);
            }
            catch(Exception e)
            {
                throw new ProviderException("Error writing property values!", e);
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                }
                if (stream != null)
                {
                    stream.Close();
                }
            }
        }

        public override int DeleteInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
        {
            try
            {
                TableServiceContext context = CreateDataServiceContext();
                List<MembershipRow> inactiveUsers = GetUsersInactive(context, "%", true, authenticationOption, userInactiveSinceDate.ToUniversalTime());
                int ret = 0;
                if (inactiveUsers == null || inactiveUsers.Count == 0)
                {
                    return ret;
                }
                foreach (MembershipRow user in inactiveUsers)
                {
                    if (string.IsNullOrEmpty(user.ProfileBlobName))
                    {
                        continue;
                    }
                    try
                    {
                        if (user.ProfileIsCreatedByProfileProvider)
                        {
                            // we can completely remove this user from the table
                            context.DeleteObject(user);
                            context.SaveChangesWithRetries();
                        }
                        else
                        {
                            user.ProfileBlobName = string.Empty;
                            user.ProfileLastUpdatedUtc = Configuration.MinSupportedDateTime;
                            user.ProfileSize = 0;
                            context.UpdateObject(user);
                            context.SaveChangesWithRetries();
                        }
                        ret++;
                        // if we fail after savechanges, the profile will still be deleted
                        // but it will be a stale profile
                        // deletes all blobs that start with the prefix
                        _blobProvider.DeleteBlobsWithPrefix(GetProfileBlobPrefix(user.UserName));
                    }
                    catch (InvalidOperationException)
                    {
                        // we ignore these errors and try to continue deleting
                        Log.Write(EventKind.Warning, "The deletion of a single profile failed! Problem when writing to table storage.");
                        if (user != null)
                        {
                            context.Detach(user);
                        }
                    }
                    catch (Exception)
                    {
                        // we ignore these errors and try to continue deleting
                        Log.Write(EventKind.Warning, "The deletion of a single profile failed! Problem when deleting blob.");
                    }
                }
                return ret;
            }
            catch (Exception e)
            {
                throw new ProviderException("Error while accessing the data store.", e);
            }
        }

        public override int DeleteProfiles(string[] usernames)
        {
            SecUtility.CheckArrayParameter(ref usernames, true, true, true, Constants.MaxTableUsernameLength, "usernames");

            TableServiceContext context = CreateDataServiceContext();            
            MembershipRow currentProfile = null;
            int ret = 0;

            try
            {             
                foreach (string name in usernames)
                {             
                    DataServiceQuery<MembershipRow> queryObj = context.CreateQuery<MembershipRow>(_tableName);
                    var query = (from profile in queryObj
                                 where profile.PartitionKey == SecUtility.CombineToKey(_applicationName, name)
                                 select profile).AsTableServiceQuery();

                    IEnumerable<MembershipRow> profiles = query.Execute();

                    if (profiles == null)
                    {
                        continue;
                    }
                    // enumerate results
                    List<MembershipRow> l = new List<MembershipRow>(profiles);
                    if (l == null || l.Count == 0)
                    {
                        continue;
                    }
                    else if (l.Count > 1)
                    {
                        // there cannot be more than one items in the list (application name + user name is a primary key)
                        Log.Write(EventKind.Warning, "There should only be one user with the same name in one application!");
                    }
                    try
                    {
                        currentProfile = l.First();
                        if (currentProfile.ProfileIsCreatedByProfileProvider)
                        {
                            // we can completely delete this user from the table
                            context.DeleteObject(currentProfile);
                            context.SaveChangesWithRetries();
                        }
                        else
                        {
                            currentProfile.ProfileBlobName = string.Empty;
                            currentProfile.ProfileLastUpdatedUtc = Configuration.MinSupportedDateTime;
                            currentProfile.ProfileSize = 0;
                            context.UpdateObject(currentProfile);
                            context.SaveChangesWithRetries();
                        }
                        ret++;
                        // if we fail after savechanges, the profile will still be deleted
                        // but it will be a stale profile
                        // deletes all blobs that start with the prefix                     
                        _blobProvider.DeleteBlobsWithPrefix(GetProfileBlobPrefix(l.First().UserName));
                    }
                    catch (InvalidOperationException)
                    {
                        // we ignore these errors and try to continue deleting
                        Log.Write(EventKind.Warning, "The deletion of a single profile failed! Problem when writing to table storage.");
                        if (currentProfile != null)
                        {
                            context.Detach(currentProfile);
                        }
                    }
                    catch (Exception)
                    {
                        // we ignore these errors and try to continue deleting
                        Log.Write(EventKind.Warning, "The deletion of a single profile failed! Problem when deleting blob.");
                    }
                }
                return ret;
            }
            // catch all exceptions, also InvalidOperationExceptions outside the above try block
            catch (Exception e)
            {
                throw new ProviderException("Error while accessing the data store.", e);
            }
        }

        public override int DeleteProfiles(ProfileInfoCollection profiles)
        {
            if (profiles == null)
            {
                throw new ArgumentNullException("profiles");
            }

            if (profiles.Count < 1)
            {
                throw new ArgumentException("Cannot delete empty profile collection.");
            }

            List<string> usernames = new List<string>();

            foreach (ProfileInfo profile in profiles)
            {
                if (!usernames.Contains(profile.UserName))
                {
                    usernames.Add(profile.UserName);
                }
            }
            return DeleteProfiles(usernames.ToArray());
        }

        public override ProfileInfoCollection FindInactiveProfilesByUserName(ProfileAuthenticationOption authenticationOption,
                                                                             string usernameToMatch, DateTime userInactiveSinceDate,
                                                                             int pageIndex, int pageSize, out int totalRecords)
        {
            return GetProfilesForQuery(usernameToMatch, userInactiveSinceDate.ToUniversalTime(), authenticationOption, pageIndex, pageSize, out totalRecords);                        
        }

        public override ProfileInfoCollection FindProfilesByUserName(ProfileAuthenticationOption authenticationOption,
                                                                     string usernameToMatch, int pageIndex, int pageSize,
                                                                     out int totalRecords)
        {
            return GetProfilesForQuery(usernameToMatch, DateTime.Now.ToUniversalTime().AddDays(1), authenticationOption, pageIndex, pageSize, out totalRecords);
        }

        public override ProfileInfoCollection GetAllInactiveProfiles(ProfileAuthenticationOption authenticationOption,
                                                                     DateTime userInactiveSinceDate, int pageIndex, int pageSize,
                                                                     out int totalRecords)
        {
            return GetProfilesForQuery("%", userInactiveSinceDate.ToUniversalTime(), authenticationOption, pageIndex, pageSize, out totalRecords);
        }

        public override ProfileInfoCollection GetAllProfiles(ProfileAuthenticationOption authenticationOption,
                                                             int pageIndex, int pageSize, out int totalRecords)
        {
            return GetProfilesForQuery("%", DateTime.Now.ToUniversalTime().AddDays(1), authenticationOption, pageIndex, pageSize, out totalRecords);            
        }

        public override int GetNumberOfInactiveProfiles(ProfileAuthenticationOption authenticationOption,
                                                        DateTime userInactiveSinceDate)
        {
            // NUM queries are not supported in the PDC offering
            // so we have to retrieve all records and count at the client side
            int totalRecords;
            GetProfilesForQuery("%", userInactiveSinceDate.ToUniversalTime(), authenticationOption, 0, Int32.MaxValue, out totalRecords);
            return totalRecords;
        }

        #region Helper methods

        private TableServiceContext CreateDataServiceContext()
        {
            return _tableStorage.GetDataServiceContext();
        }

        private string GetProfileBlobPrefix(string username)
        {
            return _applicationName + username;
        }

        private ProfileInfoCollection GetProfilesForQuery(string usernameToMatch, DateTime inactiveSinceUtc, 
                                                          ProfileAuthenticationOption auth, int pageIndex, int pageSize, 
                                                          out int totalRecords)
        {
            SecUtility.CheckParameter(ref usernameToMatch, true, true, false, Constants.MaxTableUsernameLength, "usernameToMatch");
            bool startsWith = false;
            if (usernameToMatch.Contains('%'))
            {
                if (usernameToMatch.IndexOf('%') != usernameToMatch.Length - 1)
                {
                    throw new ArgumentException("The TableStorageProfileProvider only supports search strings that contain '%' as the last character!");
                }
                usernameToMatch = usernameToMatch.Substring(0, usernameToMatch.Length - 1);
                startsWith = true;
            }
            if (inactiveSinceUtc < Configuration.MinSupportedDateTime)
            {
                throw new ArgumentException("DateTime not supported by data source.");
            }
            if (pageIndex < 0)
            {
                throw new ArgumentException("pageIndex must not be a negative integer.");
            }
            if (pageSize < 1)
            {
                throw new ArgumentException("pageSize must be a positive integer (strictly larger than zero).");
            }

            long upperBound = (long)pageIndex * pageSize + pageSize - 1;
            if (upperBound > Int32.MaxValue)
            {
                throw new ArgumentException("pageIndex and pageSize too big.");
            }
            try
            {
                ProfileInfoCollection infoColl = new ProfileInfoCollection();
                totalRecords = 0;
                TableServiceContext context = CreateDataServiceContext();
                List<MembershipRow> users = GetUsersInactive(context, usernameToMatch, startsWith, auth, inactiveSinceUtc);
                // default order is by user name (not by escaped user name as it appears in the key)
                users.Sort();
              
                int startIndex = pageIndex * pageSize;
                int endIndex = startIndex + pageSize;
                int i = 0;
                bool userMatches = true;
                MembershipRow user;
                for (i = startIndex; i < endIndex && i < users.Count && userMatches; i++)
                {
                    user = users.ElementAt<MembershipRow>(i);
                    if (startsWith && !string.IsNullOrEmpty(usernameToMatch))
                    {
                        if (!user.UserName.StartsWith(usernameToMatch, StringComparison.Ordinal))
                        {
                            userMatches = false;
                            continue;
                        }
                    }
                    infoColl.Add(new ProfileInfo(user.UserName, user.IsAnonymous, user.LastActivityDateUtc.ToLocalTime(),
                                                 user.ProfileLastUpdatedUtc.ToLocalTime(), user.ProfileSize));
                }
                totalRecords = infoColl.Count;    
                return infoColl;
            }
            catch (Exception e)
            {
                throw new ProviderException("Error accessing the data store.", e);
            }
        }

        // we don't use _providerRetry here because of the out parameter prof
        private bool DoesProfileExistAndUpdateUser(string username, out MembershipRow prof)
        {
            SecUtility.CheckParameter(ref username, true, true, true, Constants.MaxTableUsernameLength, "username");

            int curRetry = 0;
            bool retry = false;
            do
            {
                retry = false;
                try
                {
                    TableServiceContext context = CreateDataServiceContext();
                    DataServiceQuery<MembershipRow> queryObj = context.CreateQuery<MembershipRow>(_tableName);
                    var query = (from profile in queryObj
                                 where profile.PartitionKey == SecUtility.CombineToKey(_applicationName, username)
                                 select profile).AsTableServiceQuery();
                    IEnumerable<MembershipRow> profiles = query.Execute();
                    if (profiles == null)
                    {
                        prof = null;
                        return false;
                    }

                    // instantiate results
                    List<MembershipRow> profileList = new List<MembershipRow>(profiles);

                    if (profileList.Count > 1)
                    {
                        throw new ProviderException("Multiple profile rows for the same user!");
                    }
                    if (profileList.Count == 1)
                    {
                        prof = profileList.First();
                        if (!string.IsNullOrEmpty(prof.ProfileBlobName))
                        {
                            prof.LastActivityDateUtc = DateTime.UtcNow;
                            context.UpdateObject(prof);
                            context.SaveChangesWithRetries();
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        prof = null;
                        return false;
                    }
                }
                catch (InvalidOperationException e)
                {
                    if (e.InnerException is DataServiceClientException && (e.InnerException as DataServiceClientException).StatusCode == (int)HttpStatusCode.PreconditionFailed)
                    {
                        retry = true;
                    }
                    else
                    {
                        throw new ProviderException("Error accessing storage.", e);
                    }
                }
            } while (curRetry++ < NumRetries && retry);
            prof = null;
            return false;
        }

        private void UpdateUser(string username, DateTime now)
        {
            SecUtility.CheckParameter(ref username, true, true, true, Constants.MaxTableUsernameLength, "username");

            _providerRetry(() =>
            {
                TableServiceContext context = CreateDataServiceContext();
                DataServiceQuery<MembershipRow> queryObj = context.CreateQuery<MembershipRow>(_tableName);
                var query = (from user in queryObj
                             where user.PartitionKey == SecUtility.CombineToKey(_applicationName, username)
                             select user).AsTableServiceQuery();
                IEnumerable<MembershipRow> users = query.Execute();

                if (users == null)
                {
                    throw new ProviderException(string.Format(CultureInfo.InstalledUICulture, "The user {0} does not exist!", username));
                } 

                List<MembershipRow> memberList = new List<MembershipRow>(users);

                if (memberList.Count > 1)
                {
                    throw new ProviderException(string.Format(CultureInfo.InstalledUICulture, "Multiple users under the same name {0}!", username));
                }

                memberList.First().LastActivityDateUtc = now;
                context.UpdateObject(memberList.First());
                context.SaveChangesWithRetries();
            });
        }

        private void CreateOrUpdateUserAndProfile(string username, bool authenticated, DateTime now, string blobName, int size)
        {
            Debug.Assert(now.Kind == DateTimeKind.Utc);
            SecUtility.CheckParameter(ref username, true, true, true, Constants.MaxTableUsernameLength, "username");

            _providerRetry(() => 
            {
                TableServiceContext context = CreateDataServiceContext();
                DataServiceQuery<MembershipRow> queryObj = context.CreateQuery<MembershipRow>(_tableName);
                var query = (from user in queryObj
                             where user.PartitionKey == SecUtility.CombineToKey(_applicationName, username)
                             select user).AsTableServiceQuery();
                IEnumerable<MembershipRow> users = query.Execute();

                // instantiate results
                List<MembershipRow> userList = null;
                if (users != null)
                {
                    userList = new List<MembershipRow>(users);
                }
                if (userList != null && userList.Count > 0)
                {
                    MembershipRow current = userList.First();
                    if (current.IsAnonymous != !authenticated)
                    {
                        // this is an error because we would need to create a user with the same name
                        // this should not happen
                        throw new ProviderException("A user with the same name but with a different authentication status already exists!");
                    }
                    current.LastActivityDateUtc = now;
                    current.ProfileBlobName = blobName;
                    current.ProfileSize = size;
                    current.ProfileLastUpdatedUtc = now;
                    context.UpdateObject(current);
                }
                else
                {
                    if (authenticated)
                    {
                        Log.Write(EventKind.Warning, "The authenticated user does not exist in the database.");
                    }
                    MembershipRow member = new MembershipRow(_applicationName, username);
                    member.LastActivityDateUtc = now;
                    member.IsAnonymous = !authenticated;
                    member.ProfileBlobName = blobName;
                    member.ProfileSize = size;
                    member.ProfileLastUpdatedUtc = now;
                    member.ProfileIsCreatedByProfileProvider = true;
                    context.AddObject(_tableName, member);
                }
                context.SaveChanges();
            });            
        }

        private List<MembershipRow> GetUsersInactive(DataServiceContext context, string usernameToMatch, bool startswith, ProfileAuthenticationOption auth, DateTime userInactiveSinceDateUtc)
        {
            DataServiceQuery<MembershipRow> queryObj = context.CreateQuery<MembershipRow>(_tableName);

            CloudTableQuery<MembershipRow> query = null;

            // play a trick to deal with the restrictions of currently supported linq queries
            bool first, second;
            if (auth == ProfileAuthenticationOption.All)
            {
                first = true;
                second = false;
            }
            else if (auth == ProfileAuthenticationOption.Anonymous)
            {
                first = true;
                second = true;
            }
            else
            {
                first = false;
                second = false;
            }


            if (startswith && usernameToMatch == string.Empty)
            {
                query = (from user in queryObj
                         where user.PartitionKey.CompareTo(SecUtility.EscapedFirst(_applicationName)) > 0 &&
                               user.PartitionKey.CompareTo(SecUtility.NextComparisonString(SecUtility.EscapedFirst(_applicationName))) < 0 &&
                               user.LastActivityDateUtc < userInactiveSinceDateUtc &&
                               user.ProfileBlobName != string.Empty &&
                               (user.IsAnonymous == first || user.IsAnonymous == second)
                         select user).AsTableServiceQuery();
            }
            else if (startswith)
            {
                query = (from user in queryObj
                         where user.PartitionKey.CompareTo(SecUtility.EscapedFirst(_applicationName)) > 0 &&
                               user.PartitionKey.CompareTo(SecUtility.NextComparisonString(SecUtility.EscapedFirst(_applicationName))) < 0 &&
                               user.LastActivityDateUtc < userInactiveSinceDateUtc &&
                               user.ProfileBlobName != string.Empty &&
                               user.UserName.CompareTo(usernameToMatch) >= 0 &&
                               (user.IsAnonymous == first || user.IsAnonymous == second)
                         select user).AsTableServiceQuery();
            }
            else
            {
                query = (from user in queryObj
                         where user.PartitionKey.CompareTo(SecUtility.CombineToKey(_applicationName, usernameToMatch)) == 0 &&
                               user.LastActivityDateUtc < userInactiveSinceDateUtc &&
                               user.ProfileBlobName != string.Empty &&
                               (user.IsAnonymous == first || user.IsAnonymous == second)
                         select user).AsTableServiceQuery();
            }

            /*
            if (auth == ProfileAuthenticationOption.All) {
                query = from user in queryObj
                        where user.PartitionKey.CompareTo(SecUtility.EscapedFirst(_applicationName)) > 0 &&
                              user.PartitionKey.CompareTo(SecUtility.NextComparisonString(SecUtility.EscapedFirst(_applicationName))) < 0 &&
                              user.LastActivityDateUtc < userInactiveSinceDateUtc &&
                              user.ProfileBlobName != string.Empty
                        select user;
            }
            else if (auth == ProfileAuthenticationOption.Anonymous)
            {
                query = from user in queryObj
                        where user.PartitionKey.CompareTo(SecUtility.EscapedFirst(_applicationName)) > 0 &&
                              user.PartitionKey.CompareTo(SecUtility.NextComparisonString(SecUtility.EscapedFirst(_applicationName))) < 0 &&
                              user.LastActivityDateUtc < userInactiveSinceDateUtc &&
                              user.ProfileBlobName != string.Empty &&
                              user.IsAnonymous == true
                        select user;
            }
            else
            {
                Debug.Assert(auth == ProfileAuthenticationOption.Authenticated);
                query = from user in queryObj
                        where user.PartitionKey.CompareTo(SecUtility.EscapedFirst(_applicationName)) > 0 &&
                              user.PartitionKey.CompareTo(SecUtility.NextComparisonString(SecUtility.EscapedFirst(_applicationName))) < 0 &&
                              user.LastActivityDateUtc < userInactiveSinceDateUtc &&
                              user.ProfileBlobName != string.Empty &&
                              user.IsAnonymous == false
                        select user;
            } 
            */

            IEnumerable<MembershipRow> users = query.Execute();

            if (users == null)
            {
                return new List<MembershipRow>(); ;
            }
            return new List<MembershipRow>(users);
        }

        private List<MembershipRow> GetAllProfiles()
        {
            TableServiceContext context = CreateDataServiceContext();
            DataServiceQuery<MembershipRow> queryObj = context.CreateQuery<MembershipRow>(_tableName);

            var query = (from profile in queryObj
                         where profile.PartitionKey.CompareTo(SecUtility.EscapedFirst(_applicationName)) > 0 &&
                               profile.PartitionKey.CompareTo(SecUtility.NextComparisonString(SecUtility.EscapedFirst(_applicationName))) < 0
                         select profile).AsTableServiceQuery();
            IEnumerable<MembershipRow> profiles = query.Execute();

            if (profiles == null)
            {
                return new List<MembershipRow>(); ;
            }
            return new List<MembershipRow>(profiles);
        }

        private static bool VerifyUsername(ref string username)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(username.Trim()))
            {
                return false;
            }
            if (username.Length > Constants.MaxTableUsernameLength)
            {
                return false;
            }
            return true;
        }

        private string CompleteContainerName()
        {
            Debug.Assert(!string.IsNullOrEmpty(_containerName));

            string completeContainerName = _containerName;
            if (!string.IsNullOrEmpty(_applicationName))
            {
                completeContainerName = _containerName; 
            }
            return completeContainerName;
        }

        private static object Deserialize(SettingsPropertyValue p, string s) {
            if (p == null)
            {
                throw new ArgumentNullException("p");
            }
            Type type = p.Property.PropertyType;
            SettingsSerializeAs serializeAs = p.Property.SerializeAs;
            object ret = null;

            if (type == typeof(string) && (string.IsNullOrEmpty(s) || (serializeAs == SettingsSerializeAs.String)))
            {
                return s;
            }
            else if (serializeAs == SettingsSerializeAs.String)
            {
                TypeConverter converter = TypeDescriptor.GetConverter(type);
                if (converter == null || !converter.CanConvertTo(typeof(string)) || !converter.CanConvertFrom(typeof(string)))
                {
                    throw new ArgumentException("Cannot convert type!");
                }
                ret = converter.ConvertFromInvariantString(s);
            }
            else if (serializeAs == SettingsSerializeAs.Xml)
            {
                StringReader reader = new StringReader(s);
                XmlSerializer serializer = new XmlSerializer(type);
                ret = serializer.Deserialize(reader);
            }
            else
            {
                throw new ProviderException("The provider does not support binary serialization because of security constraints!");
            }
            return ret;
        }

        private static void DecodeProfileData(string[] names, string values, byte[] buf, SettingsPropertyValueCollection properties)
        {
            if (names == null || values == null || buf == null || properties == null)
            {
                return;
            }

            for (int i = 0; i < names.Length; i += 4)
            {
                // Read the next property name from "names" and retrieve
                // the corresponding SettingsPropertyValue from
                // "properties"
                string name = names[i];
                SettingsPropertyValue pp = properties[name];
                if (pp == null)
                {
                    continue;
                }

                // Get the length and index of the persisted property value
                int pos = Int32.Parse(names[i + 2], CultureInfo.InvariantCulture);
                int len = Int32.Parse(names[i + 3], CultureInfo.InvariantCulture);

                // If the length is -1 and the property is a reference
                // type, then the property value is null
                if (len == -1 && !pp.Property.PropertyType.IsValueType)
                {
                    pp.PropertyValue = null;
                    pp.IsDirty = false;
                    pp.Deserialized = true;
                }
                // If the property value was peristed as a string,
                // restore it from "values"
                else if (names[i + 1] == "S" && pos >= 0 && len > 0 && values.Length >= pos + len)
                {
                    pp.Deserialized = true;
                    pp.PropertyValue = Deserialize(pp, values.Substring(pos, len));
                }
                // If the property value was peristed as a byte array,
                // restore it from "buf"
                else if (names[i + 1] == "B" && pos >= 0 && len > 0 && buf.Length >= pos + len)
                {
                    throw new ProviderException("Not supported because of security-related hosting constraints.");
                }
            }
        }

        private static object Serialize(SettingsPropertyValue p)
        {
            if (p == null)
            {
                throw new ArgumentNullException("p");
            }
            if (p.PropertyValue == null)
            {
                return null;
            }
            if (p.Property.SerializeAs == SettingsSerializeAs.ProviderSpecific)
            {
                if (p.PropertyValue is string || p.Property.PropertyType.IsPrimitive)
                {
                    p.Property.SerializeAs = SettingsSerializeAs.String;
                }
                else
                {
                    p.Property.SerializeAs = SettingsSerializeAs.Xml;
                }
            }
            
            if (p.Property.SerializeAs == SettingsSerializeAs.String)
            {
                return p.PropertyValue.ToString();
            }
            else if (p.Property.SerializeAs == SettingsSerializeAs.Xml)
            {
                string ret;
                XmlSerializer serializer = new XmlSerializer(p.Property.PropertyType);
                using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
                {
                    serializer.Serialize((TextWriter)writer, p.PropertyValue);
                    writer.Flush();
                    ret = writer.ToString();
                }
                return ret;                
            }
            else if (p.Property.SerializeAs == SettingsSerializeAs.Binary)
            {
                throw new ProviderException("Binary serialization is not supported because of security constraints!");
            } 
            throw new ProviderException("Unknown serialization type.");
        }

        private static void EncodeProfileData(ref string allNames, ref string allValues, ref byte[] buf,
                                              SettingsPropertyValueCollection properties, bool userIsAuthenticated)
        {
            StringBuilder names = new StringBuilder();
            StringBuilder values = new StringBuilder();
            // currently not used because of length limitations
            MemoryStream stream = new MemoryStream();

            try
            {
                bool anyItemsToSave = false;

                foreach (SettingsPropertyValue pp in properties)
                {
                    if (pp.IsDirty)
                    {
                        if (!userIsAuthenticated)
                        {
                            bool allowAnonymous = (bool)pp.Property.Attributes["AllowAnonymous"];
                            if (!allowAnonymous)
                            {
                                continue;
                            }
                        }
                        anyItemsToSave = true;
                        break;
                    }
                }

                if (!anyItemsToSave)
                {
                    return;
                }

                foreach (SettingsPropertyValue pp in properties)
                {
                    // Ignore this property if the user is anonymous and
                    // the property's AllowAnonymous property is false
                    if (!userIsAuthenticated && !(bool)pp.Property.Attributes["AllowAnonymous"])
                    {
                        continue;
                    }

                    // Ignore this property if it's not dirty and is
                    // currently assigned its default value
                    if (!pp.IsDirty && pp.UsingDefaultValue)
                    {
                        continue;
                    }

                    int len = 0, pos = 0;
                    string propValue = null;
                    // If Deserialized is true and PropertyValue is null,
                    // then the property's current value is null (which
                    // we'll represent by setting len to -1)
                    if (pp.Deserialized && pp.PropertyValue == null)
                    {
                        len = -1;
                    }
                    // Otherwise get the property value from PropertyValue
                    else if (pp.Deserialized)
                    {
                        // because this implementation cannot make use of the providers internal serialization routine
                        // due to security constraints, deserialized means that we need to do the serialization on our own
                        object sVal = Serialize(pp);
                        // If SerializedValue is null, then the property's
                        // current value is null
                        if (sVal == null)
                            len = -1;
                        // If sVal is a string, then encode it as a string
                        else if (sVal is string)
                        {
                            propValue = (string)sVal;
                            len = propValue.Length;
                            pos = values.Length;
                        }
                        // If sVal is binary, then encode it as a byte
                        // array
                        else
                        {
                            throw new ProviderException("Not supported because of security hosting constraints.");
                        }
                    }
                    // Otherwise get the property value from
                    // SerializedValue
                    // This does not work in this implementation because it runs in medium trust and security permissions for binary serialization 
                    // are not available
                    else
                    {
                        throw new ProviderException("Because this provider currently runs under partial trust, accessing the standard serialization interface is not allowed.");
                    }

                    // Add a string conforming to the following format
                    // to "names:"
                    //
                    // "name:B|S:pos:len"
                    // ^ ^ ^ ^
                    // | | | |
                    // | | | +--- Length of data
                    // | | +------- Offset of data
                    // | +----------- Location (B="buf", S="values")
                    // +--------------- Property name
                    names.Append(pp.Name + ":" + ((propValue != null) ? "S" : "B") + ":" +
                                 pos.ToString(CultureInfo.InvariantCulture) + ":" +
                                 len.ToString(CultureInfo.InvariantCulture) + ":");

                    // If the propery value is encoded as a string, add the
                    // string to "values"
                    if (propValue != null)
                    {
                        values.Append(propValue);
                    }
                }
                // Copy the binary property values written to the
                // stream to "buf"
                buf = stream.ToArray();
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
            allNames = names.ToString();
            allValues = values.ToString();
        }

        #endregion
    }
}


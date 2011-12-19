//
// <copyright file="TableStorageRoleProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Data.Services.Client;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security;
using System.Web.Security;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;


namespace Microsoft.Samples.ServiceHosting.AspProviders
{
    /// <summary>
    /// This class allows DevtableGen to generate the correct table (named 'Roles')
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses",
        Justification="Class is used by the devtablegen tool to generate a database for the development storage tool.")]
    internal class RoleDataServiceContext : TableServiceContext
    {
        public RoleDataServiceContext() : base(null, null) { }
        public IQueryable<RoleRow> Roles
        {
            get
            {
                return this.CreateQuery<RoleRow>("Roles");
            }
        }
    }

    [CLSCompliant(false)]
    public class RoleRow : TableServiceEntity
    {
        private string _applicationName;
        private string _roleName;
        private string _userName;


        // applicationName + userName is partitionKey
        // roleName is rowKey
        public RoleRow(string applicationName, string roleName, string userName)
            : base()
        {
            SecUtility.CheckParameter(ref applicationName, true, true, true, Constants.MaxTableApplicationNameLength, "applicationName");
            SecUtility.CheckParameter(ref roleName, true, true, true, TableStorageRoleProvider.MaxTableRoleNameLength, "roleName");
            SecUtility.CheckParameter(ref userName, true, false, true, Constants.MaxTableUsernameLength, "userName");


            PartitionKey = SecUtility.CombineToKey(applicationName, userName);
            RowKey = SecUtility.Escape(roleName);
            ApplicationName = applicationName;
            RoleName = roleName;
            UserName = userName;
        }

        public RoleRow()
            : base()
        {
        }

        public string ApplicationName
        {
            set
            {
                if (value == null)
                {
                    throw new ArgumentException("To ensure string values are always updated, this implementation does not allow null as a string value.");
                }
                _applicationName = value;
                PartitionKey = SecUtility.CombineToKey(ApplicationName, UserName);
            }
            get
            {
                return _applicationName;
            }
        }

        public string RoleName
        {
            set
            {
                if (value == null)
                {
                    throw new ArgumentException("To ensure string values are always updated, this implementation does not allow null as a string value.");
                }
                _roleName = value;
                RowKey = SecUtility.Escape(RoleName);
            }
            get
            {
                return _roleName;
            }
        }

        public string UserName
        {
            set
            {
                if (value == null)
                {
                    throw new ArgumentException("To ensure string values are always updated, this implementation does not allow null as a string value.");
                }
                _userName = value;
                PartitionKey = SecUtility.CombineToKey(ApplicationName, UserName);
            }
            get
            {
                return _userName;
            }
        }

    }

    public class TableStorageRoleProvider : RoleProvider
    {

        #region Member variables and constants

        internal const int MaxTableRoleNameLength = 512;
        internal const int NumRetries = 3;

        // member variables shared between most providers
        private string _applicationName;
        private string _accountName;
        private string _sharedKey;
        private string _tableName;
        private string _membershipTableName;
        private string _tableServiceBaseUri;
        private CloudTableClient _tableStorage;
        private object _lock = new object();
        private RetryPolicy _tableRetry = RetryPolicies.Retry(NumRetries, TimeSpan.FromSeconds(1));
        // private ProviderRetryPolicy _providerRetry = ProviderRetryPolicies.Retry(NumRetries, TimeSpan.FromSeconds(1));

        #endregion

        #region Properties

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

        #endregion

        #region Public methods

        // RoleProvider methods
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
                name = "TableStorageRoleProvider";
            }

            // Add a default "description" attribute to config if the
            // attribute doesn't exist or is empty
            if (string.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "Table storage-based role provider");
            }

            // Call the base class's Initialize method
            base.Initialize(name, config);

            bool allowInsecureRemoteEndpoints = Configuration.GetBooleanValue(config, "allowInsecureRemoteEndpoints", false);

            // structure storage-related properties
            ApplicationName = Configuration.GetStringValueWithGlobalDefault(config, "applicationName",
                                                Configuration.DefaultProviderApplicationNameConfigurationString,
                                                Configuration.DefaultProviderApplicationName, false);
            _accountName = Configuration.GetStringValue(config, "accountName", null, true);
            _sharedKey = Configuration.GetStringValue(config, "sharedKey", null, true);
            _tableName = Configuration.GetStringValueWithGlobalDefault(config, "roleTableName", 
                                                Configuration.DefaultRoleTableNameConfigurationString,
                                                Configuration.DefaultRoleTableName, false);
            _membershipTableName = Configuration.GetStringValueWithGlobalDefault(config, "membershipTableName", 
                                                Configuration.DefaultMembershipTableNameConfigurationString,
                                                Configuration.DefaultMembershipTableName, false);    
            _tableServiceBaseUri = Configuration.GetStringValue(config, "tableServiceBaseUri", null, true);            

            // remove required attributes
            config.Remove("allowInsecureRemoteEndpoints");
            config.Remove("applicationName");
            config.Remove("accountName");
            config.Remove("sharedKey");
            config.Remove("roleTableName");
            config.Remove("membershipTableName");
            config.Remove("tableServiceBaseUri");


            // Throw an exception if unrecognized attributes remain
            if (config.Count > 0)
            {
                string attr = config.GetKey(0);
                if (!String.IsNullOrEmpty(attr))
                {
                    throw new ProviderException(string.Format(CultureInfo.InstalledUICulture, "Unrecognized attribute: {0}", attr));
                }
            }

            StorageCredentialsAccountAndKey info = null;
            string baseUri = null;
            try
            {
                var sharedKey = Configuration.TryGetAppSetting(Configuration.DefaultAccountSharedKeyConfigurationString);
                var accountName = Configuration.TryGetAppSetting(Configuration.DefaultAccountNameConfigurationString);

                baseUri = Configuration.TryGetAppSetting(Configuration.DefaultTableStorageEndpointConfigurationString);

                if (_tableServiceBaseUri != null)
                {
                    baseUri = _tableServiceBaseUri;
                }
                if (_accountName != null)
                {
                    accountName = _accountName;
                }
                if (_sharedKey != null)
                {
                    sharedKey = _sharedKey;
                }

                if (String.IsNullOrEmpty(sharedKey) || String.IsNullOrEmpty(accountName) || String.IsNullOrEmpty(baseUri))
                    throw new ConfigurationErrorsException("Account information incomplete!");

                info = new StorageCredentialsAccountAndKey(accountName, sharedKey);
                SecUtility.CheckAllowInsecureEndpoints(allowInsecureRemoteEndpoints, info, new Uri(baseUri));
                _tableStorage = new CloudTableClient(baseUri, info);
                _tableStorage.RetryPolicy = _tableRetry;
                if (_tableStorage.CreateTableIfNotExist(_tableName))
                {
                    var ctx = _tableStorage.GetDataServiceContext();
                    var dummyRow = new RoleRow("dummy", "fake", "none");
                    ctx.AddObject(_tableName, dummyRow);
                    ctx.SaveChangesWithRetries();
                    ctx.DeleteObject(dummyRow);
                    ctx.SaveChangesWithRetries();
                }
            }
            catch (SecurityException)
            {
                throw;
            }
            // catch InvalidOperationException as well as StorageException
            catch (Exception e)
            {
                string exceptionDescription = Configuration.GetInitExceptionDescription(info, new Uri(baseUri), "table storage configuration");
                string tableName = (_tableName == null) ? "no role table name specified" : _tableName;
                Log.Write(EventKind.Error, "Could not create or find role table: " + tableName + "!" + Environment.NewLine +
                                            exceptionDescription + Environment.NewLine +
                                            e.Message + Environment.NewLine + e.StackTrace);
                throw new ProviderException("Could not create or find role table. The most probable reason for this is that " +
                            "the storage endpoints are not configured correctly. Please look at the configuration settings " +
                            "in your .cscfg and Web.config files. More information about this error " +
                            "can be found in the logs when running inside the hosting environment or in the output " +
                            "window of Visual Studio.", e);
            }
        }


        public override bool IsUserInRole(string username, string roleName)
        {
            SecUtility.CheckParameter(ref roleName, true, true, true, MaxTableRoleNameLength, "rolename");
            SecUtility.CheckParameter(ref username, true, false, true, Constants.MaxTableUsernameLength, "username");
            if (username.Length < 1)
            {
                return false;
            }

            try
            {
                TableServiceContext svc = CreateDataServiceContext();
                DataServiceQuery<RoleRow> queryObj = svc.CreateQuery<RoleRow>(_tableName);

                var query = (from user in queryObj
                             where (user.PartitionKey == SecUtility.CombineToKey(_applicationName, username) ||                                                     
                                    user.PartitionKey == SecUtility.CombineToKey(_applicationName, string.Empty)) &&
                                    user.RowKey == SecUtility.Escape(roleName)
                             select user).AsTableServiceQuery();

                IEnumerable<RoleRow> userRows = query.Execute();

                if (userRows == null)
                {
                    throw new ProviderException(string.Format(CultureInfo.InstalledUICulture, "The role {0} does not exist.", roleName));
                }
                List<RoleRow> l = new List<RoleRow>(userRows);
                if (l.Count == 0)
                {
                    throw new ProviderException(string.Format(CultureInfo.InstalledUICulture, "The role {0} does not exist.", roleName));
                }
                RoleRow row;
                if (IsStaleRole(l, out row)) {
                    throw new ProviderException(string.Format(CultureInfo.InstalledUICulture, "The role {0} does not exist.", roleName));
                }
                if (l.Count > 2)
                {
                    throw new ProviderException("User name appears twice in the same role!");
                }
                if (l.Count == 1)
                {
                    Debug.Assert(string.IsNullOrEmpty(l.ElementAt(0).UserName));
                    return false;
                }
                return true;
            }
            catch (InvalidOperationException e)
            {              
                throw new ProviderException("Error while accessing the data store.", e);
            }
        }

        public override string[] GetRolesForUser(string username)
        {
            SecUtility.CheckParameter(ref username, true, false, true, Constants.MaxTableUsernameLength, "username");
            if (username.Length < 1)
            {
                return new string[0];
            }

            try
            {
                TableServiceContext svc = CreateDataServiceContext();
                DataServiceQuery<RoleRow> queryObj = svc.CreateQuery<RoleRow>(_tableName);
                
                var query = (from user in queryObj
                             where user.PartitionKey == SecUtility.CombineToKey(_applicationName, username) ||
                                   user.PartitionKey == SecUtility.CombineToKey(_applicationName, string.Empty)
                             select user).AsTableServiceQuery();

                IEnumerable<RoleRow> userRows = query.Execute();

                if (userRows == null)
                {
                    return new string[0];
                }
                List<RoleRow> l = new List<RoleRow>(userRows);
                if (l.Count == 0)
                {
                    return new string[0];
                }
                List<string> ret = new List<string>();
                foreach (RoleRow user in l) {
                    if (!string.IsNullOrEmpty(user.UserName) && !IsStaleRole(l, user.RoleName))
                    {
                        ret.Add(user.RoleName);
                    }
                }
                return ret.ToArray();
            }
            catch (InvalidOperationException e)
            {
                throw new ProviderException("Error while accessing the data store.", e);
            }
        }

        public override string[] GetUsersInRole(string roleName)
        {
            SecUtility.CheckParameter(ref roleName, true, true, true, MaxTableRoleNameLength, "rolename");

            try
            {
                TableServiceContext svc = CreateDataServiceContext();
                DataServiceQuery<RoleRow> queryObj = svc.CreateQuery<RoleRow>(_tableName);

                var query = (from user in queryObj
                             where user.PartitionKey.CompareTo(SecUtility.EscapedFirst(_applicationName)) >= 0 &&
                                   user.PartitionKey.CompareTo(SecUtility.NextComparisonString(SecUtility.EscapedFirst(_applicationName))) < 0 &&
                                   user.RowKey == SecUtility.Escape(roleName)
                             select user).AsTableServiceQuery();
                IEnumerable<RoleRow> userRows = query.Execute();

                if (userRows == null)
                {
                    // role does not exist; we are supposed to throw an exception here
                    throw new ProviderException(string.Format(CultureInfo.InstalledUICulture, "The role {0} does not exist!", roleName));
                }
                List<RoleRow> l = new List<RoleRow>(userRows);
                if (l.Count == 0 || IsStaleRole(l, roleName))
                {
                    throw new ProviderException(string.Format(CultureInfo.InstalledUICulture, "The role {0} does not exist!", roleName));
                }
                List<string> ret = new List<string>();
                foreach (RoleRow user in l)
                {
                    if (!string.IsNullOrEmpty(user.UserName))
                    {
                        ret.Add(user.UserName);
                    }
                }
                return ret.ToArray();
            }
            catch (InvalidOperationException e)
            {
                throw new ProviderException("Error while accessing the data store.", e);
            }
        }

        public override string[] GetAllRoles()
        {
            try
            {
                DataServiceContext svc = CreateDataServiceContext();
                DataServiceQuery<RoleRow> queryObj = svc.CreateQuery<RoleRow>(_tableName);

                var query = (from role in queryObj
                             where role.PartitionKey == SecUtility.CombineToKey(_applicationName, string.Empty)
                             select role).AsTableServiceQuery();
                IEnumerable<RoleRow> userRows = query.Execute();

                if (userRows == null)
                {
                    return new string[0];
                }
                List<RoleRow> l = new List<RoleRow>(userRows);
                if (l.Count == 0)
                {
                    return new string[0];
                }
                List<string> ret = new List<string>();
                foreach (RoleRow role in l)
                {
                    Debug.Assert(role.UserName != null);
                    if (string.IsNullOrEmpty(role.UserName))
                    {
                        ret.Add(role.RoleName);
                    }
                }
                return ret.ToArray();
            }
            catch (InvalidOperationException e)
            {
                throw new ProviderException("Error while accessing the data store.", e);
            }
        }

        public override bool RoleExists(string roleName)
        {
            SecUtility.CheckParameter(ref roleName, true, true, true, MaxTableRoleNameLength, "rolename");
            try
            {
                TableServiceContext svc = CreateDataServiceContext();
                DataServiceQuery<RoleRow> queryObj = svc.CreateQuery<RoleRow>(_tableName);

                var query = (from role in queryObj
                             where role.PartitionKey == SecUtility.CombineToKey(_applicationName, string.Empty) &&
                                   role.RowKey == SecUtility.Escape(roleName)
                             select role).AsTableServiceQuery();

                try
                {
                    // this query addresses exactly one result
                    // we thus should get an exception if there is no element
                    return query.Execute().Any();
                }
                catch (InvalidOperationException e)
                {
                    if (e.InnerException is DataServiceClientException && (e.InnerException as DataServiceClientException).StatusCode == (int)HttpStatusCode.NotFound)
                    {
                        return false;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            catch (InvalidOperationException e)
            {
                throw new ProviderException("Error while accessing the data store.", e);
            }
        }

        public override void CreateRole(string roleName)
        {
            SecUtility.CheckParameter(ref roleName, true, true, true, MaxTableRoleNameLength, "rolename");

            try
            {
                TableServiceContext svc = CreateDataServiceContext();
                RoleRow newRole = new RoleRow(_applicationName, roleName, string.Empty);
                svc.AddObject(_tableName, newRole);
                svc.SaveChangesWithRetries();
            }
            catch (InvalidOperationException e)
            {
                // when retry policies are used we cannot distinguish between a conflict and success
                // so, in the case of a conflict, we just retrun success here
                if (e.InnerException is DataServiceClientException && (e.InnerException as DataServiceClientException).StatusCode == (int)HttpStatusCode.Conflict)
                {
                    return;
                    // the role already exists
                }
                throw new ProviderException("Error accessing role table.", e);
            }

        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            SecUtility.CheckParameter(ref roleName, true, true, true, MaxTableRoleNameLength, "rolename");

            try
            {
                TableServiceContext svc = CreateDataServiceContext();
                DataServiceQuery<RoleRow> queryObj = svc.CreateQuery<RoleRow>(_tableName);

                var query = (from userRole in queryObj
                             where userRole.PartitionKey.CompareTo(SecUtility.EscapedFirst(_applicationName)) >= 0 &&
                                   userRole.PartitionKey.CompareTo(SecUtility.NextComparisonString(SecUtility.EscapedFirst(_applicationName))) < 0 &&
                                   userRole.RowKey == SecUtility.Escape(roleName)
                             select userRole).AsTableServiceQuery();
                IEnumerable<RoleRow> userRows = query.Execute();

                if (userRows == null)
                {
                    return false;
                }
                List<RoleRow> l = new List<RoleRow>(userRows);
                if (l.Count == 0)
                {
                    // the role does not exist
                    return false;
                }
                RoleRow role;
                if (IsStaleRole(l, out role)) {
                    return false;
                }
                if (l.Count > 1 && throwOnPopulatedRole) {
                    throw new ProviderException("Cannot delete populated role.");
                }
                svc.DeleteObject(role);
                svc.SaveChangesWithRetries();
                // lets try to remove all remaining elements in the role
                foreach(RoleRow row in l) {
                    if (row != role) {
                        try
                        {
                            svc.DeleteObject(row);
                            svc.SaveChangesWithRetries();
                        }
                        catch (InvalidOperationException ex)
                        {
                            var dsce = ex.InnerException as DataServiceClientException;

                            if (ex.InnerException is DataServiceClientException && (dsce.StatusCode == (int)HttpStatusCode.NoContent || dsce.StatusCode == (int)HttpStatusCode.NotFound))
                            {
                                // this element already was already deleted by another process or during a failed retry
                                // this is not a fatal error; continue deleting elements
                                Log.Write(EventKind.Warning, string.Format(CultureInfo.InstalledUICulture, "The user {0} does not exist in the role {1}.", row.UserName, row.RoleName));
                            }
                            else
                            {
                                throw new ProviderException(string.Format(CultureInfo.InstalledUICulture, "Error deleting user {0} from role {1}.", row.UserName, row.RoleName));
                            }
                        }
                    }
                }
                return true;
            }
            catch (InvalidOperationException e)
            {
                throw new ProviderException("Error while accessing the data store.", e);
            }
        }

        // Because of limited transactional support in the table storage offering, this function gives limited guarantees 
        // for inserting all users into all roles.
        // We do not recommend using this function because of missing transactional support. 
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            SecUtility.CheckArrayParameter(ref roleNames, true, true, true, MaxTableRoleNameLength, "roleNames");
            SecUtility.CheckArrayParameter(ref usernames, true, true, true, Constants.MaxTableUsernameLength, "usernames");

            RoleRow row;
            try
            {
                TableServiceContext svc = CreateDataServiceContext();
                foreach (string role in roleNames)
                {
                    if (!RoleExists(role))
                    {
                        throw new ProviderException(string.Format(CultureInfo.InstalledUICulture, "The role {0} does not exist!", role));
                    }
                    foreach (string user in usernames)
                    {
                        row = new RoleRow(_applicationName, role, user);
                        try
                        {
                            svc.AddObject(_tableName, row);
                            svc.SaveChangesWithRetries();
                        }
                        catch (InvalidOperationException e)
                        {
                            if (e.InnerException is DataServiceClientException && (e.InnerException as DataServiceClientException).StatusCode == (int)HttpStatusCode.Conflict)
                            {
                                // this element already exists or was created in a failed retry
                                // this is not a fatal error; continue adding elements
                                Log.Write(EventKind.Warning, string.Format(CultureInfo.InstalledUICulture, "The user {0} already exists in the role {1}.", user, role));
                                svc.Detach(row);
                            }
                            else
                            {
                                throw new ProviderException(string.Format(CultureInfo.InstalledUICulture, "Error adding user {0} to role {1}", user, role));
                            }
                        }
                    }
                }
            }
            catch (InvalidOperationException e)
            {
                throw new ProviderException("Error while accessing the data store.", e);
            }            
        }

        // the username to match can be in a format that varies between providers
        // for this implementation, a syntax similar to the one used in the SQL provider is applied
        // "user%" will return all users in a role that start with the string "user"
        // the % sign can only appear at the end of the usernameToMatch parameter
        // because the current version of the table storage service does not support StartsWith in LINQ queries, 
        // calling this function can cause significant network trafic when '%' is used in the usernameToMach
        // parameter
        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            SecUtility.CheckParameter(ref roleName, true, true, true, MaxTableRoleNameLength, "rolename");
            SecUtility.CheckParameter(ref usernameToMatch, true, true, false, Constants.MaxTableUsernameLength, "usernameToMatch");

            bool startswith = false;
            if (usernameToMatch.Contains('%'))
            {
                if (usernameToMatch.IndexOf('%') != usernameToMatch.Length - 1)
                {
                    throw new ArgumentException("The TableStorageRoleProvider only supports search strings that contain '%' as the last character!");
                }
                usernameToMatch = usernameToMatch.Substring(0, usernameToMatch.Length - 1);
                startswith = true;
            }

            try
            {
                TableServiceContext svc = CreateDataServiceContext();
                DataServiceQuery<RoleRow> queryObj = svc.CreateQuery<RoleRow>(_tableName);
                
                CloudTableQuery<RoleRow> query;
                
                if (startswith && string.IsNullOrEmpty(usernameToMatch)) {
                    // get all users in the role
                    query = (from userRole in queryObj
                             where userRole.PartitionKey.CompareTo(SecUtility.EscapedFirst(_applicationName)) >= 0 &&
                                   userRole.PartitionKey.CompareTo(SecUtility.NextComparisonString(SecUtility.EscapedFirst(_applicationName))) < 0 &&
                                   userRole.RowKey == SecUtility.Escape(roleName)
                             select userRole).AsTableServiceQuery();
                } else if (startswith) {
                    // get all users in the role that start with the specified string (we cannot restrict the query more because StartsWith is not supported)
                    // we cannot include the username to search for in the key, because the key might e escaped
                    query = (from userRole in queryObj
                             where userRole.PartitionKey.CompareTo(SecUtility.EscapedFirst(_applicationName)) >= 0 &&
                                   userRole.PartitionKey.CompareTo(SecUtility.NextComparisonString(SecUtility.EscapedFirst(_applicationName))) < 0 &&
                                   userRole.RowKey == SecUtility.Escape(roleName) &&
                                   (userRole.UserName.CompareTo(usernameToMatch) >= 0 || userRole.UserName == string.Empty)
                             select userRole).AsTableServiceQuery();
                } else {
                    // get a specific user
                    query = (from userRole in queryObj
                             where (userRole.PartitionKey == SecUtility.CombineToKey(_applicationName, usernameToMatch) || 
                                    userRole.PartitionKey == SecUtility.CombineToKey(_applicationName, string.Empty)) &&
                                    userRole.RowKey == SecUtility.Escape(roleName)
                             select userRole).AsTableServiceQuery();
                }
                
                IEnumerable<RoleRow> userRows = query.Execute();

                if (userRows == null)
                {
                    throw new ProviderException("The role does not exist!");
                }
                List<RoleRow> l = new List<RoleRow>(userRows);
                if (l.Count == 0)
                {
                    // the role does not exist
                    throw new ProviderException("The role does not exist!");
                }
                RoleRow role;
                if (IsStaleRole(l, out role))
                {
                    throw new ProviderException("The role does not exist!");
                }
                List<string> ret = new List<string>();
                foreach (RoleRow row in l)
                {
                    if (row != role)
                    {
                        if (startswith && !string.IsNullOrEmpty(usernameToMatch) && !row.UserName.StartsWith(usernameToMatch, StringComparison.Ordinal))
                        {
                            continue;
                        }
                        ret.Add(row.UserName);
                    }
                }
                return ret.ToArray();
            }
            catch (InvalidOperationException e)
            {
                throw new ProviderException("Error while accessing the data store.", e);
            }
        }

        // remember that there is no is no rollback functionality for the table storage service right now
        // be cautious when using this function
        // if a role does not exist, we stop deleting roles, if a user in a role does not exist, we continue deleting
        // in case of error conditions, the behavior of this function is different than the SQL role provider
        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            SecUtility.CheckArrayParameter(ref roleNames, true, true, true, MaxTableRoleNameLength, "roleNames");
            SecUtility.CheckArrayParameter(ref usernames, true, true, true, Constants.MaxTableUsernameLength, "usernames");

            RoleRow row;
            try
            {
                TableServiceContext svc = CreateDataServiceContext();
                foreach (string role in roleNames)
                {
                    if (!RoleExists(role))
                    {
                        throw new ProviderException(string.Format(CultureInfo.InstalledUICulture, "The role {0} does not exist!", role));
                    }
                    foreach (string user in usernames)
                    {
                        row = GetUserInRole(svc, role, user);
                        if (row == null)
                        {
                            Log.Write(EventKind.Warning, string.Format(CultureInfo.InstalledUICulture, "The user {0} does not exist in the role {1}.", user, role));
                            continue;
                        }
                        try
                        {
                            svc.DeleteObject(row);
                            svc.SaveChangesWithRetries();
                        }
                        catch (Exception e)
                        {
                            var dsce = e.InnerException as DataServiceClientException;
                            if (dsce != null && (dsce.StatusCode == (int)HttpStatusCode.NoContent || dsce.StatusCode == (int)HttpStatusCode.NotFound))
                            {
                                Log.Write(EventKind.Warning, string.Format(CultureInfo.InstalledUICulture, "The user {0} does not exist in the role {1}.", user, role));
                                svc.Detach(row);
                            }
                            else
                            {
                                throw new ProviderException(string.Format(CultureInfo.InstalledUICulture, "Error deleting user {0} from role {1}.", user, role));
                            }
                        }
                    }
                }
            }
            catch (InvalidOperationException e)
            {
                throw new ProviderException("Error while accessing the data store.", e);
            }
        }

        #endregion


        #region Helper methods

        private TableServiceContext CreateDataServiceContext()
        {
            return _tableStorage.GetDataServiceContext();
        }

        private static bool IsStaleRole(List<RoleRow> l, string rolename) {
            if (l == null || l.Count == 0)
            {
                return false;
            }
            foreach (RoleRow row in l)
            {
                // if (row.RoleName == rolename && row.UserName == string.Empty)
                if (string.Compare(row.RoleName, rolename, StringComparison.Ordinal) == 0 && string.IsNullOrEmpty(row.UserName))
                {
                    return false;
                }
            }
            return true;            
        }


        private static bool IsStaleRole(List<RoleRow> l, out RoleRow role)
        {
            role = null;
            if (l == null || l.Count == 0)
            {
                return false;
            }
            string rolename = l.ElementAt(0).RoleName;
            foreach (RoleRow row in l)
            {
                Debug.Assert(row.RoleName == rolename);
                if (string.IsNullOrEmpty(row.UserName))
                {
                    role = row;
                    return false;
                }
            }
            return true;
        }

        private RoleRow GetUserInRole(DataServiceContext svc, string rolename, string username) 
        {
            SecUtility.CheckParameter(ref username, true, true, true, Constants.MaxTableUsernameLength, "username");
            SecUtility.CheckParameter(ref rolename, true, true, true, MaxTableRoleNameLength, "rolename");

            try
            {
                DataServiceQuery<RoleRow> queryObj = svc.CreateQuery<RoleRow>(_tableName);

                var query = (from user in queryObj
                             where user.PartitionKey == SecUtility.CombineToKey(_applicationName, username) &&                                                    
                                   user.RowKey == SecUtility.Escape(rolename)
                             select user).AsTableServiceQuery();
                try
                {
                    IEnumerable<RoleRow> userRows = query.Execute();
                    return userRows.FirstOrDefault();
                }
                catch (InvalidOperationException e)
                {
                    if (e.InnerException is DataServiceClientException && (e.InnerException as DataServiceClientException).StatusCode == (int)HttpStatusCode.NotFound)
                    {
                        return null;
                    }
                    else
                    {
                        throw;
                    }
                }                
            }
            catch (Exception e)
            {
                throw new ProviderException("Error while accessing the data store.", e);
            }
        }

        #endregion
    }

}

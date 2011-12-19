//
// <copyright file="SecUtil.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
using System;
using System.Collections;
using System.Data.Services.Client;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.WindowsAzure;

namespace Microsoft.Samples.ServiceHosting.AspProviders
{

    internal static class SecUtility
    {
        internal const int Infinite = Int32.MaxValue;

        internal static bool ValidateParameter(ref string param, bool checkForNull, bool checkIfEmpty, bool checkForCommas, int maxSize)
        {
            if (param == null)
            {
                return !checkForNull;
            }

            param = param.Trim();
            if ((checkIfEmpty && param.Length < 1) ||
                 (maxSize > 0 && param.Length > maxSize) ||
                 (checkForCommas && param.Contains(",")))
            {
                return false;
            }

            return true;
        }

        internal static void CheckParameter(ref string param, bool checkForNull, bool checkIfEmpty, bool checkForCommas, int maxSize, string paramName)
        {
            if (param == null)
            {
                if (checkForNull)
                {
                    throw new ArgumentNullException(paramName);
                }

                return;
            }

            param = param.Trim();
            if (checkIfEmpty && param.Length < 1)
            {
                throw new ArgumentException(string.Format(CultureInfo.InstalledUICulture, "The parameter '{0}' must not be empty.", paramName), paramName);
            }

            if (maxSize > 0 && param.Length > maxSize)
            {
                throw new ArgumentException(string.Format(CultureInfo.InstalledUICulture, "The parameter '{0}' is too long: it must not exceed {1} chars in length.", paramName, maxSize.ToString(CultureInfo.InvariantCulture)), paramName);
            }

            if (checkForCommas && param.Contains(","))
            {
                throw new ArgumentException(string.Format(CultureInfo.InstalledUICulture, "The parameter '{0}' must not contain commas.", paramName), paramName);
            }
        }

        internal static void CheckArrayParameter(ref string[] param, bool checkForNull, bool checkIfEmpty, bool checkForCommas, int maxSize, string paramName)
        {
            if (param == null)
            {
                throw new ArgumentNullException(paramName);
            }

            if (param.Length < 1)
            {
                throw new ArgumentException(string.Format(CultureInfo.InstalledUICulture, "The array parameter '{0}' should not be empty.", paramName), paramName);
            }

            Hashtable values = new Hashtable(param.Length);
            for (int i = param.Length - 1; i >= 0; i--)
            {
                SecUtility.CheckParameter(ref param[i], checkForNull, checkIfEmpty, checkForCommas, maxSize,
                    paramName + "[ " + i.ToString(CultureInfo.InvariantCulture) + " ]");
                if (values.Contains(param[i]))
                {
                    throw new ArgumentException(string.Format(CultureInfo.InstalledUICulture, "The array '{0}' should not contain duplicate values.", paramName), paramName);
                }
                else
                {
                    values.Add(param[i], param[i]);
                }
            }
        }

        internal static void SetUtcTime(DateTime value, out DateTime res)
        {
            res = Configuration.MinSupportedDateTime;
            if ((value.Kind == DateTimeKind.Local && value.ToUniversalTime() < Configuration.MinSupportedDateTime) ||
                 value < Configuration.MinSupportedDateTime)
            {
                throw new ArgumentException("Invalid time value!");
            }
            if (value.Kind == DateTimeKind.Local)
            {
                res = value.ToUniversalTime();
            }
            else
            {
                res = value;
            }
        }

        internal const string ValidTableNameRegex = @"^([a-z]|[A-Z]){1}([a-z]|[A-Z]|\d){2,62}$";

        internal static bool IsValidTableName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }
            Regex reg = new Regex(ValidTableNameRegex);
            if (reg.IsMatch(name))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal const string ValidContainerNameRegex = @"^([a-z]|\d){1}([a-z]|-|\d){1,61}([a-z]|\d){1}$";

        internal static bool IsValidContainerName(string name)
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

        // the table storage system currently does not support the StartsWith() operation in 
        // queries. As a result we transform s.StartsWith(substring) into s.CompareTo(substring) > 0 && 
        // s.CompareTo(NextComparisonString(substring)) < 0
        // we assume that comparison on the service side is as ordinal comparison
        internal static string NextComparisonString(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                throw new ArgumentException("The string argument must not be null or empty!");
            }
            string ret;
            char last = s[s.Length - 1];
            if ((int)last + 1 > (int)char.MaxValue)
            {
                throw new ArgumentException("Cannot convert the string.");
            }
            // don't use "as" because we want to have an explicit exception here if something goes wrong
            last = (char)((int)last + 1);
            ret = s.Substring(0, s.Length - 1) + last;
            return ret;
        }

        // we use a normal character as the separator because of string comparison operations
        // these have to be valid characters
        internal const char KeySeparator = 'a';
        internal static readonly string KeySeparatorString = new string(KeySeparator, 1);
        internal const char EscapeCharacter = 'b';
        internal static readonly string EscapeCharacterString = new string(EscapeCharacter, 1);

        // Some characters can cause problems when they are contained in columns 
        // that are included in queries. We are very defensive here and escape a wide range 
        // of characters for key columns (as the key columns are present in most queries)
        internal static bool IsInvalidKeyCharacter(char c)
        {
            return ((c < 32)
                || (c >= 127 && c < 160)
                || (c == '#') 
                || (c == '&') 
                || (c == '+') 
                || (c == '/') 
                || (c == '?') 
                || (c == ':')
                || (c == '%')
                || (c == '\\')
                ); 
        }

        internal static string CharToEscapeSequence(char c) {
            string ret;
            ret = EscapeCharacterString + string.Format(CultureInfo.InvariantCulture, "{0:X2}", (int)c);
            return ret;
        }

        internal static string Escape(string s)
        {
            if (string.IsNullOrEmpty(s)) {
                return s;
            }
            StringBuilder ret = new StringBuilder();
            foreach (char c in s)
            {
                if (c == EscapeCharacter || c == KeySeparator || IsInvalidKeyCharacter(c))
                {
                    ret.Append(CharToEscapeSequence(c));
                }
                else
                {
                    ret.Append(c);
                }
            }
            return ret.ToString();
        }

        internal static string UnEscape(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }
            StringBuilder ret = new StringBuilder();
            char c;
            for (int i = 0; i < s.Length; i++) 
            {
                c = s[i];
                if (c == EscapeCharacter)
                {
                    if (i + 2 >= s.Length)
                    {
                        throw new FormatException("The string " + s + " is not correctly escaped!");
                    }
                    int ascii = Convert.ToInt32(s.Substring(i + 1, 2), 16);
                    ret.Append((char)ascii);
                    i += 2;
                }
                else
                {
                    ret.Append(c);
                }
            }
            return ret.ToString();
        }

        internal static string CombineToKey(string s1, string s2)
        {
            return Escape(s1) + KeySeparator + Escape(s2);
        }

        internal static string EscapedFirst(string s)
        {
            return Escape(s) + KeySeparator;
        }

        internal static string GetFirstFromKey(string key) {
            Debug.Assert(key.IndexOf(KeySeparator) != -1);
            string first = key.Substring(0, key.IndexOf(KeySeparator));
            return UnEscape(first);
        }

        internal static string GetSecondFromKey(string key)
        {
            Debug.Assert(key.IndexOf(KeySeparator) != -1);
            string second = key.Substring(key.IndexOf(KeySeparator) + 1);
            return UnEscape(second);
        }

        internal static void CheckAllowInsecureEndpoints(bool allowInsecureRemoteEndpoints, StorageCredentialsAccountAndKey info, Uri baseUri)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            if (allowInsecureRemoteEndpoints)
            {
                return;
            }
            if (baseUri == null || string.IsNullOrEmpty(baseUri.Scheme))
            {
                throw new SecurityException("allowInsecureRemoteEndpoints is set to false (default setting) but the endpoint URL seems to be empty or there is no URL scheme." +
                                            "Please configure the provider to use an https enpoint for the storage endpoint or " +
                                            "explicitly set the configuration option allowInsecureRemoteEndpoints to true.");
            }
            if (baseUri.Scheme.ToUpper(CultureInfo.InvariantCulture) == Uri.UriSchemeHttps.ToUpper(CultureInfo.InvariantCulture))
            {
                return;
            }
            if (baseUri.IsLoopback)
            {
                return;
            }
            throw new SecurityException("The provider is configured with allowInsecureRemoteEndpoints set to false (default setting) but the endpoint for " +
                                        "the storage system does not seem to be an https or local endpoint. " +
                                        "Please configure the provider to use an https enpoint for the storage endpoint or " +
                                        "explicitly set the configuration option allowInsecureRemoteEndpoints to true.");
        }
    }

    internal static class Constants
    {
        internal const int MaxTableUsernameLength = 256;
        internal const int MaxTableApplicationNameLength = 256;
    }


    /// <summary>
    /// This delegate defines the shape of a provider retry policy. 
    /// Provider retry policies are only used to retry when a row retrieved from a table 
    /// was changed by another entity before it could be saved to the data store.A retry policy will invoke the given
    /// <paramref name="action"/> as many times as it wants to in the face of 
    /// retriable InvalidOperationExceptions.
    /// </summary>
    /// <param name="action">The action to retry</param>
    /// <returns></returns>
    public delegate void ProviderRetryPolicy(Action action);


    /// <summary>
    /// We are using this retry policies for only one purpose: the ASP providers often read data from the server, process it 
    /// locally and then write the result back to the server. The problem is that the row that has been read might have changed 
    /// between the read and write operation. This retry policy is used to retry the whole process in this case.
    /// </summary>
    /// <summary>
    /// Provides definitions for some standard retry policies.
    /// </summary>
    public static class ProviderRetryPolicies
    {

        public static readonly TimeSpan StandardMinBackoff = TimeSpan.FromMilliseconds(100);
        public static readonly TimeSpan StandardMaxBackoff = TimeSpan.FromSeconds(30);
        private static readonly Random Random = new Random();

        /// <summary>
        /// Policy that does no retries i.e., it just invokes <paramref name="action"/> exactly once
        /// </summary>
        /// <param name="action">The action to retry</param>
        /// <returns>The return value of <paramref name="action"/></returns>
        internal static void NoRetry(Action action)
        {
            action();
        }

        /// <summary>
        /// Policy that retries a specified number of times with an exponential backoff scheme
        /// </summary>
        /// <param name="numberOfRetries">The number of times to retry. Should be a non-negative number.</param>
        /// <param name="deltaBackoff">The multiplier in the exponential backoff scheme</param>
        /// <returns></returns>
        /// <remarks>For this retry policy, the minimum amount of milliseconds between retries is given by the 
        /// StandardMinBackoff constant, and the maximum backoff is predefined by the StandardMaxBackoff constant. 
        /// Otherwise, the backoff is calculated as random(2^currentRetry) * deltaBackoff.</remarks>
        public static ProviderRetryPolicy RetryN(int numberOfRetries, TimeSpan deltaBackoff)
        {
            return new ProviderRetryPolicy((Action action) =>
            {
                RetryNImpl(action, numberOfRetries, StandardMinBackoff, StandardMaxBackoff, deltaBackoff);
            }
            );
        }

        /// <summary>
        /// Policy that retries a specified number of times with an exponential backoff scheme
        /// </summary>
        /// <param name="numberOfRetries">The number of times to retry. Should be a non-negative number</param>
        /// <param name="deltaBackoff">The multiplier in the exponential backoff scheme</param>
        /// <param name="minBackoff">The minimum backoff interval</param>
        /// <param name="minBackoff">The minimum backoff interval</param>
        /// <returns></returns>
        /// <remarks>For this retry policy, the minimum amount of milliseconds between retries is given by the 
        /// minBackoff parameter, and the maximum backoff is predefined by the maxBackoff parameter. 
        /// Otherwise, the backoff is calculated as random(2^currentRetry) * deltaBackoff.</remarks>
        public static ProviderRetryPolicy RetryN(int numberOfRetries, TimeSpan minBackoff, TimeSpan maxBackoff, TimeSpan deltaBackoff)
        {
            return new ProviderRetryPolicy((Action action) =>
            {
                RetryNImpl(action, numberOfRetries, minBackoff, maxBackoff, deltaBackoff);
            }
            );
        }


        private static void RetryNImpl(Action action, int numberOfRetries, TimeSpan minBackoff, TimeSpan maxBackoff, TimeSpan deltaBackoff)
        {
            int totalNumberOfRetries = numberOfRetries;
            int backoff;

            if (minBackoff > maxBackoff)
            {
                throw new ArgumentException("The minimum backoff must not be larger than the maximum backoff period.");
            }
            if (minBackoff.TotalMilliseconds < 0)
            {
                throw new ArgumentException("The minimum backoff period must not be negative.");
            }

            do
            {
                try
                {
                    action();
                    break;
                }
                catch (InvalidOperationException e)
                {
                    HttpStatusCode status;

                    var dsce = e.InnerException as DataServiceClientException;

                    // precondition failed is the status code returned by the server to indicate that the etag is wrong
                    if (dsce != null)
                    {
                        status = (HttpStatusCode)dsce.StatusCode;

                        if (status == HttpStatusCode.PreconditionFailed)
                        {
                            if (numberOfRetries == 0)
                            {
                                throw;
                            }
                            try
                            {
                                backoff = CalculateCurrentBackoff(minBackoff, maxBackoff, deltaBackoff, totalNumberOfRetries - numberOfRetries);
                                Debug.Assert(backoff >= minBackoff.TotalMilliseconds);
                                Debug.Assert(backoff <= maxBackoff.TotalMilliseconds);
                                if (backoff > 0)
                                {
                                    Thread.Sleep(backoff);
                                }
                            }
                            catch
                            {
                            }
                        }
                        else
                        {
                            throw;
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            while (numberOfRetries-- > 0);
        }


        private static int CalculateCurrentBackoff(TimeSpan minBackoff, TimeSpan maxBackoff, TimeSpan deltaBackoff, int curRetry)
        {
            int backoff;

            if (curRetry > 31)
            {
                backoff = (int)maxBackoff.TotalMilliseconds;
            }
            try
            {
                backoff = Random.Next((1 << curRetry) + 1);
                // Console.WriteLine("backoff:" + backoff);
                // Console.WriteLine("index:" + ((1 << curRetry) + 1));
                backoff *= (int)deltaBackoff.TotalMilliseconds;
                backoff += (int)minBackoff.TotalMilliseconds;
            }
            catch (OverflowException)
            {
                backoff = (int)maxBackoff.TotalMilliseconds;
            }
            if (backoff > (int)maxBackoff.TotalMilliseconds)
            {
                backoff = (int)maxBackoff.TotalMilliseconds;
            }
            Debug.Assert(backoff >= (int)minBackoff.TotalMilliseconds);
            // Console.WriteLine("real backoff:" + backoff);
            return backoff;
        }
    }
}

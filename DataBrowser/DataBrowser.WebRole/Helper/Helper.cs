/* 
 *   Copyright (c) Microsoft Corporation.  All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of Microsoft Corporation nor the names of its contributors 
 *       may be used to endorse or promote products derived from this software
 *       without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE REGENTS AND CONTRIBUTORS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE REGENTS AND CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Xml.Serialization;
using Ogdi.Azure.Configuration;
using Ogdi.InteractiveSdk.Mvc.App_GlobalResources;
using Ogdi.InteractiveSdk.Mvc.Models;

namespace Ogdi.InteractiveSdk.Mvc
{
    public static class Helper
    {
        #region PrivateMethod

        /// <summary>
        /// This method deserializes the Language XML
        /// </summary>
        /// <returns>returns deserialized form of XML</returns>
        private static Languages DeSerializeLanguageXML()
        {
            Languages languagePathInfo = null;

            // Get path of XML fie
            string pathOfXmlFile = String.Join("\\",
                new string[] {HttpContext.Current.Server.MapPath("~").ToString(),
              UIConstants.DBPC_LanguageFilePath});

            // Get the language xml into StreamReader
            StreamReader streamReader = new StreamReader(pathOfXmlFile);

            // Create an object of XmlSerializer
            XmlSerializer xmlSerializer =
                new System.Xml.Serialization.XmlSerializer(typeof(Languages));

            // Deserialize the StreamReader of languages
            languagePathInfo = (Languages)xmlSerializer.Deserialize(streamReader);

            // returns deserialized form of XML
            return languagePathInfo;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// This property gives list of all DataView languages
        /// </summary>
        public static IList<SampleCodeLanguage> AllDVLanguages
        {
            get
            {
                List<SampleCodeLanguage> codeLanguagesCollection = null;

                // Deserialize the lanuagePathCollection object
                Languages lanuagePathCollection = DeSerializeLanguageXML();

                // if lanuagePathCollection exists then find all DataView languages
                if (lanuagePathCollection != null)
                {
                    // Get list of Data View languages
                    LanguagesDVLanguage[] languageDVLanguage =
                        lanuagePathCollection.DVLanguage;

                    codeLanguagesCollection = new List<SampleCodeLanguage>();
                    // Add each language in codeLanguagesCollection list
                    foreach (LanguagesDVLanguage dvLanguage in languageDVLanguage)
                    {
                        // Create an object of SampleCodeLanguage
                        SampleCodeLanguage sampleCodeLanguage = new SampleCodeLanguage();
                        sampleCodeLanguage.FilePath = dvLanguage.Path;
                        sampleCodeLanguage.LanguageName = dvLanguage.Text;

                        // Add SampleCodeLanguage object in codeLanguagesCollection list
                        codeLanguagesCollection.Add(sampleCodeLanguage);
                    }

                    // Sort codeLanguagesCollection list
                    codeLanguagesCollection.Sort();
                }

                // returns codeLanguagesCollection list as IList<SampleCodeLanguage>
                return codeLanguagesCollection as IList<SampleCodeLanguage>;
            }
        }

        /// <summary>
        /// This property gives list of all MapView languages
        /// </summary>
        public static IList<SampleCodeLanguage> AllMVLanguages
        {
            get
            {
                List<SampleCodeLanguage> codeLanguagesCollection = null;

                // Deserialize the lanuagePathCollection object
                Languages lanuagePathCollection = DeSerializeLanguageXML();

                // if lanuagePathCollection exists then find all MapView languages
                if (lanuagePathCollection != null)
                {
                    // Get list of Map View languages
                    LanguagesMVLanguage[] languageMVLanguage =
                        lanuagePathCollection.MVLanguage;
                    codeLanguagesCollection = new List<SampleCodeLanguage>();

                    // Add each language in codeLanguagesCollection list
                    foreach (LanguagesMVLanguage mvLanguage in languageMVLanguage)
                    {
                        // Create an object of SampleCodeLanguage
                        SampleCodeLanguage sampleCodeLanguage = new SampleCodeLanguage();
                        sampleCodeLanguage.FilePath = mvLanguage.Path;
                        sampleCodeLanguage.LanguageName = mvLanguage.Text;

                        // Add SampleCodeLanguage object in codeLanguagesCollection list
                        codeLanguagesCollection.Add(sampleCodeLanguage);
                    }

                    // Sort codeLanguagesCollection list
                    codeLanguagesCollection.Sort();
                }

                // returns codeLanguagesCollection list as IList<SampleCodeLanguage>
                return codeLanguagesCollection as IList<SampleCodeLanguage>;
            }
        }

        /// <summary>
        /// This property gives list of all BarChart View languages
        /// </summary>
        public static IList<SampleCodeLanguage> AllBarChartLanguages
        {
            get
            {
                List<SampleCodeLanguage> codeLanguagesCollection = null;

                // Deserialize the lanuagePathCollection object
                Languages lanuagePathCollection = DeSerializeLanguageXML();

                // if lanuagePathCollection exists then find all BarChart view languages
                if (lanuagePathCollection != null)
                {
                    // Get list of BarChart View languages
                    LanguagesBarChartLanguage[] languageBarChartLanguage =
                        lanuagePathCollection.BarChartLanguage;
                    codeLanguagesCollection = new List<SampleCodeLanguage>();

                    // Add each language in codeLanguagesCollection list
                    foreach (LanguagesBarChartLanguage barChartLanguage in languageBarChartLanguage)
                    {
                        // Create an object of SampleCodeLanguage
                        SampleCodeLanguage sampleCodeLanguage = new SampleCodeLanguage();
                        sampleCodeLanguage.FilePath = barChartLanguage.Path;
                        sampleCodeLanguage.LanguageName = barChartLanguage.Text;

                        // Add SampleCodeLanguage object in codeLanguagesCollection list
                        codeLanguagesCollection.Add(sampleCodeLanguage);
                    }

                    // Sort codeLanguagesCollection list
                    codeLanguagesCollection.Sort();
                }

                // returns codeLanguagesCollection list as IList<SampleCodeLanguage>
                return codeLanguagesCollection as IList<SampleCodeLanguage>;
            }
        }

        /// <summary>
        /// This property gives list of all PieChart View languages
        /// </summary>
        public static IList<SampleCodeLanguage> AllPieChartLanguages
        {
            get
            {
                List<SampleCodeLanguage> codeLanguagesCollection = null;

                // Deserialize the lanuagePathCollection object
                Languages lanuagePathCollection = DeSerializeLanguageXML();

                // if lanuagePathCollection exists then find all PieChart view languages
                if (lanuagePathCollection != null)
                {
                    // Get list of PieChart View languages
                    LanguagesPieChartLanguage[] languagePieChartLanguage =
                        lanuagePathCollection.PieChartLanguage;
                    codeLanguagesCollection = new List<SampleCodeLanguage>();

                    // Add each language in codeLanguagesCollection list
                    foreach (LanguagesPieChartLanguage pieChartLanguage in languagePieChartLanguage)
                    {
                        // Create an object of SampleCodeLanguage
                        SampleCodeLanguage sampleCodeLanguage = new SampleCodeLanguage();
                        sampleCodeLanguage.FilePath = pieChartLanguage.Path;
                        sampleCodeLanguage.LanguageName = pieChartLanguage.Text;

                        // Add SampleCodeLanguage object in codeLanguagesCollection list
                        codeLanguagesCollection.Add(sampleCodeLanguage);
                    }

                    // Sort codeLanguagesCollection list
                    codeLanguagesCollection.Sort();
                }

                // returns codeLanguagesCollection list as IList<SampleCodeLanguage>
                return codeLanguagesCollection as IList<SampleCodeLanguage>;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// This method finds the path of the file for the Data View
        /// language passed
        /// </summary>
        /// <param name="language">Name of the language</param>
        /// <returns>Returns path of the file</returns>
        public static string GetDVLanguagePath(string language)
        {
            string dvLanguagePath = null;

            // Deserialize the lanuagePathCollection object
            Languages lanuagePathCollection = DeSerializeLanguageXML();

            // if lanuagePathCollection exists then find the path
            if (lanuagePathCollection != null)
            {
                // Get list of DataView languages
                LanguagesDVLanguage[] languageDVLanguage =
                    lanuagePathCollection.DVLanguage;
                foreach (LanguagesDVLanguage dvLanguage in languageDVLanguage)
                {
                    // Check if required language exists
                    if (string.Compare(dvLanguage.Text, language,
                        StringComparison.Ordinal) == 0)
                    {
                        dvLanguagePath = dvLanguage.Path.ToString();
                        break;
                    }
                }
            }
            // Return language path for the Dataview language
            return dvLanguagePath;
        }

        /// <summary>
        /// This method finds the path of the file for the Map View 
        /// language passed
        /// </summary>
        /// <param name="language">Name of the language</param>
        /// <returns>Returns path of the file</returns>
        public static string GetMVLanguagePath(string language)
        {
            string mvLanguagePath = null;

            // Deserialize the lanuagePathCollection object
            Languages lanuagePathCollection = DeSerializeLanguageXML();

            // if lanuagePathCollection exists then find the path
            if (lanuagePathCollection != null)
            {
                // Get list of MapView languages
                LanguagesMVLanguage[] languageMVLanguage =
                    lanuagePathCollection.MVLanguage;
                foreach (LanguagesMVLanguage mvLanguage in languageMVLanguage)
                {
                    // Check if required language exists
                    if (string.Compare(mvLanguage.Text, language,
                        StringComparison.Ordinal) == 0)
                    {
                        mvLanguagePath = mvLanguage.Path.ToString();
                    }
                }
            }

            // Return language path for the Mapview language
            return mvLanguagePath;
        }

        /// <summary>
        /// This method finds the path of the file for the Bar Chart
        /// View language passed
        /// </summary>
        /// <param name="language">Name of the language</param>
        /// <returns>Returns path of the file</returns>
        public static string GetBarChartLanguagePath(string language)
        {
            string barChartLanguagePath = null;

            // Deserialize the lanuagePathCollection object
            Languages lanuagePathCollection = DeSerializeLanguageXML();

            // if lanuagePathCollection exists then find the path
            if (lanuagePathCollection != null)
            {
                // Get list of BarChart View languages
                LanguagesBarChartLanguage[] languageBarChartLanguage =
                    lanuagePathCollection.BarChartLanguage;
                foreach (LanguagesBarChartLanguage barChartLanguage in languageBarChartLanguage)
                {
                    // Check if required language exists
                    if (string.Compare(barChartLanguage.Text, language,
                        StringComparison.Ordinal) == 0)
                    {
                        barChartLanguagePath = barChartLanguage.Path.ToString();
                    }
                }
            }

            // Return language path for the BarChart view language
            return barChartLanguagePath;
        }

        /// <summary>
        /// This method finds the path of the file for the Pie Chart View
        /// language passed
        /// </summary>
        /// <param name="language">Name of the language</param>
        /// <returns>Returns path of the file</returns>
        public static string GetPieChartLanguagePath(string language)
        {
            string pieChartLanguagePath = null;

            // Deserialize the lanuagePathCollection object
            Languages lanuagePathCollection = DeSerializeLanguageXML();

            // if lanuagePathCollection exists then find the path
            if (lanuagePathCollection != null)
            {
                // Get list of PieChart View languages
                LanguagesPieChartLanguage[] languagePieChartLanguage =
                    lanuagePathCollection.PieChartLanguage;
                foreach (LanguagesPieChartLanguage pieChartLanguage in languagePieChartLanguage)
                {
                    // Check if required language exists
                    if (string.Compare(pieChartLanguage.Text, language,
                        StringComparison.Ordinal) == 0)
                    {
                        pieChartLanguagePath = pieChartLanguage.Path.ToString();
                    }
                }
            }

            // Return language path for the PieChart view language
            return pieChartLanguagePath;
        }

        /// <summary>
        /// This property gives service object to access azure storage
        /// </summary>
        public static IsdkStorageProviderInterface ServiceObject
        {
            get
            {
                string serviceUri = OgdiConfiguration.GetValue("serviceUri");
                string pathDTD = OgdiConfiguration.GetValue("pathDTD");

                return IsdkStorageProviderInterface.GetServiceObject(serviceUri, pathDTD);
            }
        }

        /// <summary>
        /// This method generate a key for the dataset that will be used to track analytics
        /// </summary>
        /// <param name="container"></param>
        /// <param name="datasetId"></param>
        /// <returns></returns>
        public static String GenerateDatasetItemKey(String container, String datasetId)
        {
            return String.Format("{0}||{1}", container, datasetId);
        }

        /// <summary>
        /// This helper method generate a key for the request that will be used to track analytics
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns></returns>
        public static String GenerateRequestKey(String requestId)
        {
            return String.Format("{0}", requestId);
        }

        #endregion
    }
}
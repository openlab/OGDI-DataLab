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

namespace Ogdi.InteractiveSdk.Mvc.Models
{
    [Serializable]
    public class SampleCodeLanguage : IComparable
    {
        #region Private Members

        // Private member for SampleCodeLanguage.LanguageName
        private string languageName = string.Empty;

        // Private member for SampleCodeLanguage.FilePath
        private string filePath = string.Empty;

        #endregion

        #region Properties

        /// <summary>
        /// This property represents LanguageName of SampleCodeLanguage
        /// </summary>
        public string LanguageName
        {
            get { return languageName; }
            set { languageName = value; }
        } 

        /// <summary>
        /// This property represents FilePath of SampleCodeLanguage
        /// </summary>
        public string FilePath
        {
            get { return filePath; }
            set { filePath = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default Constructor
        /// </summary>
        public SampleCodeLanguage()
        {
            this.LanguageName = string.Empty;
            this.FilePath = string.Empty;
        }

        /// <summary>
        /// Paramaterized Constructor
        /// </summary>
        /// <param name="languageName">languageName of
        /// SampleCodeLanguage</param>
        /// <param name="filePath">filePath of SampleCodeLanguage</param>
        public SampleCodeLanguage(string languageName, string filePath)
        {
            LanguageName = languageName;
            FilePath = filePath;
        }

        #endregion

        #region IComparable Members

        /// <summary>
        /// This method is overridden for Sort() method
        /// </summary>
        /// <param name="obj">instance of object class</param>
        /// <returns>integer result of comparison</returns>
        public int CompareTo(object obj)
        {
            try
            {
                SampleCodeLanguage c = obj as SampleCodeLanguage;
                return string.Compare(this.LanguageName,
                    c.LanguageName, StringComparison.Ordinal);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// overriden the method Equals to match the object 
        /// of type EntitySet
        /// </summary>
        /// <param name="obj">param of type object</param>
        /// <returns>returns result of match in boolean</returns>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this.GetType() != obj.GetType()) return false;

            SampleCodeLanguage sampleCodeLanguage = 
                obj as SampleCodeLanguage;
            if (!Object.Equals(this.FilePath, sampleCodeLanguage.FilePath)) 
                return false;
            if (!Object.Equals(this.LanguageName, sampleCodeLanguage.LanguageName)) 
                return false;
            return true;
        }

        /// <summary>
        /// overriden the method GetHashCode
        /// </summary>
        /// <returns>return hashcode</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// This method overrides ToString method
        /// </summary>
        /// <returns>returns string form of object</returns>
        public override string ToString()
        {
            return base.ToString();
        }

        /// <summary>
        /// This method overrides == operator
        /// </summary>
        /// <param name="sc1">First SampleCodeLanguage for 
        /// comparison</param>
        /// <param name="sc2">Second SampleCodeLanguage for 
        /// comparison</param>
        /// <returns>returns true if both passed containers 
        /// are same else false</returns>
        public static bool operator ==(SampleCodeLanguage sc1,
            SampleCodeLanguage sc2)
        {
            // returns true if both passed containers are 
            // same else false
            return sc1.Equals(sc2);
        }

        /// <summary>
        /// This method overrides != operator
        /// </summary>
        /// <param name="sc1">First SampleCodeLanguage for 
        /// comparison</param>
        /// <param name="sc2">Second SampleCodeLanguage for 
        /// comparison</param>
        /// <returns>returns true if first container != second
        /// container else false</returns>
        public static bool operator !=(SampleCodeLanguage sc1, 
            SampleCodeLanguage sc2)
        {
            // returns true if first container != second 
            // container else false
            return !(sc1 == sc2);
        }
        
        /// <summary>
        /// This method overrides less than operator
        /// </summary>
        /// <param name="sc1">First SampleCodeLanguage for
        /// comparison</param>
        /// <param name="sc2">Second SampleCodeLanguage for 
        /// comparison</param>
        /// <returns>returns true if first container is less 
        /// than second container else false</returns>
        public static bool operator <(SampleCodeLanguage sc1, 
            SampleCodeLanguage sc2)
        {
            // returns true if first container is less than 
            // second container else false
            return (sc1.CompareTo(sc2) < 0);
        }
        
        /// <summary>
        /// This method overrides greater than operator
        /// </summary>
        /// <param name="sc1">First SampleCodeLanguage for 
        /// comparison</param>
        /// <param name="sc2">Second SampleCodeLanguage for
        /// comparison</param>
        /// <returns>returns true if first container is greter
        /// than second container else false</returns>
        public static bool operator >(SampleCodeLanguage sc1,
            SampleCodeLanguage sc2)
        {
            // returns true if first container is greter than 
            // second container else false
            return (sc1.CompareTo(sc2) > 0);
        }  

        #endregion
    }
}

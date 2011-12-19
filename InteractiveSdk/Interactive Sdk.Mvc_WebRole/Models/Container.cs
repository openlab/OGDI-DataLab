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
    public class Container : IComparable
    {
        #region Private Members

        // Private member for Container.Alias
        private string alias;

        // Private member for Container.Description
        private string description;

        // Private member for Container.Disclaimer
        private string disclaimer;

        #endregion

        #region Properties

        /// <summary>        
        /// This property represents unique name for the Container
        /// </summary>
        public string Alias
        {
            get { return alias; }
            set { alias = value; }
        }  

        /// <summary>
        /// This property represents description of the Container
        /// </summary>
        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        /// <summary>
        /// This property represents Disclaimer of the Container
        /// </summary>
        public string Disclaimer
        {
            get { return disclaimer; }
            set { disclaimer = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default Constructor
        /// </summary>
        public Container()
        {
            this.Alias = string.Empty;
            this.description = string.Empty;
            this.disclaimer = string.Empty;
        }

        /// <summary>
        /// Paramaterized Constructor
        /// </summary>
        /// <param name="alias">alias of the Container</param>
        /// <param name="description">description of the 
        /// Container</param>
        /// <param name="disclaimer">disclaimer of the
        /// Container</param>
        public Container(string alias, string description, 
            string disclaimer)
        {
            this.Alias = alias;
            this.description = description;
            this.disclaimer = disclaimer;
        }

        #endregion

        #region IComparable Members

        /// <summary>
        /// This method is overridden for Sort() operation
        /// </summary>
        /// <param name="obj">instance of object class</param>
        /// <returns>integer result of comparison</returns>
        public int CompareTo(object obj)
        {
            try
            {
                Container c = obj as Container;
                return string.Compare(this.Alias, c.Alias,
                    StringComparison.Ordinal);
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
            if (obj == null)
                return false;
            if (this.GetType() != obj.GetType())
                return false;

            Container container = obj as Container;
            if (!Object.Equals(this.Alias, container.alias)) 
                return false;
            if (!Object.Equals(this.Description, container.Description))
                return false;
            if (!Object.Equals(this.Disclaimer, container.Disclaimer))
                return false;
            return true;
        }

        /// <summary>
        /// overriden the method GetHashCode
        /// </summary>
        /// <returns>return hashcode</returns>
        public override int GetHashCode()
        {
            // return hashcode
            return base.GetHashCode();
        }

        /// <summary>
        /// This method overrides == operator
        /// </summary>
        /// <param name="container1">first container for 
        /// comparison</param>
        /// <param name="container2">second container for
        /// comparison</param>
        /// <returns>returns true if both passed containers 
        /// are same else false</returns>
        public static bool operator ==(Container container1, 
            Container container2)
        {
            // returns true if both passed containers are
            // same else false
            return container1.Equals(container2);
        }

        /// <summary>
        /// This method overrides != operator
        /// </summary>
        /// <param name="container1">first container for 
        /// comparison</param>
        /// <param name="container2">second container for 
        /// comparison</param>
        /// <returns>returns true if first container != second 
        /// container else false</returns>
        public static bool operator !=(Container container1, 
            Container container2)
        {
            // returns true if first container != second 
            // container else false
            return !(container1 == container2);
        }

        /// <summary>
        /// This method overrides less than operator
        /// </summary>
        /// <param name="container1">first container for 
        /// comparison</param>
        /// <param name="container2">second container for 
        /// comparison</param>
        /// <returns>returns true if first container is less
        /// than second container else false</returns>
        public static bool operator <(Container container1, 
            Container container2)
        {
            // returns true if first container is less than 
            // second container else false
            return (container1.CompareTo(container2) < 0);
        }

        /// <summary>
        /// This method overrides greater than operator
        /// </summary>
        /// <param name="container1">first container for
        /// comparison</param>
        /// <param name="container2">second container for 
        /// comparison</param>
        /// <returns>returns true if first container is 
        /// greter than second container else false</returns>
        public static bool operator >(Container container1, 
            Container container2)
        {
            // returns true if first container is greter than 
            // second container else false
            return (container1.CompareTo(container2) > 0);
        }

        #endregion
    }
}

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
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Ogdi.InteractiveSdk.Mvc.Models;

namespace Ogdi.InteractiveSdk.Mvc.Repository
{
    /// <summary>
    /// This Repository will have methods to manage the Container.cs model.
    /// </summary>
    internal class ContainerRepository
    {
        #region Public Methods

        /// <summary>
        /// Fetches all the containers and returns the list of Container objects.
        /// XML Format:
        ///     <Root tableName="AvailableEndpoints">
        ///         <properties>
        ///             <alias>dc</alias>
        ///             <description>District of Columbia</description>
        ///             <disclaimer>...</disclaimer>
        ///         </properties>
        ///     </Root>
        /// </summary>
        /// <returns>List of Container objects</returns>
        internal static IList<Container> GetAllContainers()
        {
            // Declare List<Container> that will have list of Container objects.
            List<Container> containerList = null;
            // 1000 is the max results Azure Table Storage allows per query                
            XElement containerXML =
                Helper.ServiceObject.GetData("", Resources.ContainerTableName, null,
                Convert.ToInt32(Resources.ContainerPageSize, CultureInfo.InvariantCulture),
                null, null);
            // Define containerList
            containerList = new List<Container>();
            if (containerXML != null)
            {
               containerList = (from element in containerXML.Elements("properties")
                                 select new Container
                                     (
                                            element.Element("alias").Value,
                                            element.Element("description") == null ? null : element.Element("description").Value,
                                            element.Element("disclaimer") == null ? null : element.Element("disclaimer").Value
                                    )).Distinct().ToList<Container>();

                // Sort the container list based on Alias
                containerList.Sort();
            }
            // Return containerList
            return containerList;
        }

        internal static IEnumerable<String> GetDisclaimers(String container)
        {
            return from t in GetAllContainers() where t.Alias == container select t.Disclaimer;
        }

        #endregion
    }
}

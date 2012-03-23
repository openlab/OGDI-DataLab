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
using System.Linq;
using Ogdi.InteractiveSdk.Mvc.Models;

namespace Ogdi.InteractiveSdk.Mvc.Repository
{
    /// <summary>
    /// This Repository will have the methods to manage the EntitySet.cs model 
    /// for the list of datasets.
    /// </summary>
    internal class EntitySetRepository
    {
        #region Public Methods

        /// <summary>
        /// This method fetches the EntitySets for the given parameters
        /// </summary>
        /// <param name="containerAlias">alias of the container</param>
        /// <param name="categoryName">name of the category</param>
        /// <returns>returns list of EntitySet</returns>
        internal static IEnumerable<EntitySet> GetEntitySets(string containerAlias, string categoryName)
        {
            if ((String.IsNullOrEmpty(containerAlias)))
                return null;

            if (categoryName == null)
                return Cache.EntitySets(containerAlias);
            else
                return Cache.EntitySets(containerAlias).Where(t => t.CategoryValue == categoryName);
        }

        internal static EntitySet GetEntitySet(string containerAlias, string entName)
        {
            // 1000 is the max results Azure Table Storage allows per query
            if (String.IsNullOrEmpty(containerAlias))
                return null;

            var lstEntitySets = Cache.EntitySets(containerAlias).Where(t => t.EntitySetName == entName);
            return lstEntitySets.FirstOrDefault();
        }

        internal static IQueryable<EntitySet> GetEntitySets()
        {
            IQueryable<EntitySet> _entities = new List<EntitySet>().AsQueryable();

            foreach (Container container in ContainerRepository.GetAllContainers())
            {
                _entities = Queryable.Concat(_entities, Cache.EntitySets(container.Alias));
            }

            return _entities;
        }

        ///// <summary>
        ///// This method fetches the distinct categories from the
        ///// list of EntitySets
        ///// </summary>
        ///// <param name="lstEntitySets">List of EntitySets</param>
        ///// <returns>List of categories</returns>
        //private static List<string> FetchCategories()
        //{
        //    // Get unique categories
        //    return ((from category in Cache.EntitySets(containerAlias)
        //             orderby category.CategoryValue
        //             select category.CategoryValue).Distinct())
        //                         .ToList<string>();
        //}

        #endregion
    }
}

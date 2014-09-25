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

using System.IO;
using Manoli.Utils.CSharpFormat;

namespace Ogdi.InteractiveSdk.Mvc.Repository
{
    internal  class SampleCodeGenerator
    {
        #region Properties

        /// <summary>
        /// Code template.
        /// </summary>
        internal string Template { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Default constructor.
        /// </summary>
        internal SampleCodeGenerator()
        {
        }

        /// <summary>
        /// Initializing constructor.
        /// </summary>
        /// <param name="templateFilespec">Path/filename for template file.</param>
        /// <remarks>
        /// The template file is a text file that contains composite
        /// formatting specifications
        /// compatible with String.Format(). The first parameter {0} 
        /// is the base URI for the
        /// container and authority. The second parameter {1} is the query.
        /// </remarks>
        internal SampleCodeGenerator(string templateFilespec)
        {
            // Create an object of the class StreamReader
            StreamReader codeReader = new StreamReader(templateFilespec,true);

            // Read the stream upto end in the Template property
            Template = codeReader.ReadToEnd();

            // Dispose the object of the class StreamReader
            codeReader.Dispose();

        }

        /// <summary>
        /// Generate code using the current template.
        /// </summary>
        /// <param name="baseUri">Base URI for container and authority.</param>
        /// <param name="tableName">Table name.</param>
        /// <param name="query">Query</param>
        /// <returns>Sample code.</returns>
        internal string GenerateCSharp(string baseUri, string tableName, string query)
        {
            // Create uri
            string uri = baseUri + "/" + tableName;
            Template = Template.Replace("{0}", uri);
            Template = Template.Replace("{1}", query);
            // Get the code in csharpformat
            var csharpFormat = new CSharpFormat();
            csharpFormat.Alternate = true;
            csharpFormat.EmbedStyleSheet = false;
            csharpFormat.LineNumbers = false;

            return csharpFormat.FormatCode(Template);
        }

        /// <summary>
        /// Generates Html
        /// </summary>
        /// <param name="baseUri">base URI</param>
        /// <param name="query">query passed</param>
        /// <returns>HTML format of the data available for the passed 
        /// URI and query</returns>
        internal string GenerateHtml(string baseUri, string tableName, string query)
        {
            // Create uri
            string uri = baseUri + "/" + tableName;
            // create Template
            Template = Template.Replace("{0}", uri);
            Template = Template.Replace("{1}", query);
            Template = Template.Replace("{2}", query.Replace("_KML", string.Empty));
            // Get the code in HTML format
            var htmlFormat = new HtmlFormat();
            htmlFormat.Alternate = true;
            htmlFormat.EmbedStyleSheet = false;
            htmlFormat.LineNumbers = false;

            return htmlFormat.FormatCode(Template);
        }

        #endregion
    }
}

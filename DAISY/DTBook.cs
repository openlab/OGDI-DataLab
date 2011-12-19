using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace DTBookTranslation
{
    public class DTBook
    {
        private static string error;

        /// <summary>
        /// DAISY Plugin - Function for DAISY Xml Translation
        /// </summary>
        /// <param name="inputXmlFile">Input Xml</param>
        /// <param name="pathDTD">Location of the DTD</param>
        /// <returns>Text Reader object having DAISY XML</returns>
        public TextReader TranslationOfAzureXml(XmlReader inputXmlFile, String pathDTD)
        {
            XPathDocument xmlDoc = new XPathDocument(inputXmlFile);
            XslCompiledTransform transform = new XslCompiledTransform();

            //DAISY Plugin - Loading the ConvertToDTBook XSLT which translates the Azure Xml to DTBook Xml
            XmlReader inputXmlReader = XmlReader.Create(ReadXslt("ConvertToDTBook.xslt"));
            transform.Load(inputXmlReader);

            TextWriter oStrWriter = new StringWriter();

            //DAISY Plugin - perform translation
            transform.Transform(xmlDoc, null, oStrWriter);

            String inputContent = ReplaceDTDpath(oStrWriter.ToString(), pathDTD);

            //DAISY Plugin - Calling function to do validation of Output XML file with DTD
            XmlValidation(inputContent);

            //DAISY Plugin - Calling a function to add/remove required data
            TextReader convertedXml = ReplaceData(inputContent, pathDTD);

            return convertedXml;
        }

        /// <summary>
        /// DAISY Plugin - Function to replace the path of the DTD in the XSLT
        /// </summary>
        /// <param name="inputSw">String having DAISY Xml</param>
        /// <param name="pathDTD">Location of the DTD file</param>
        /// <returns>String having updated DAISY Xml</returns>
        private String ReplaceDTDpath(String inputStr, String pathDTD)
        {
            inputStr = inputStr.Replace("<!DOCTYPE dtbook SYSTEM 'dtbook-2005-3.dtd'>", "<!DOCTYPE dtbook SYSTEM '" + pathDTD + "'>");
            return inputStr;
        }


        /// <summary>
        /// DAISY Plugin - function to add/remove required data 
        /// </summary>
        /// <param name="inputContent">String having DAISY Xml</param>
        /// <param name="pathDTD">Location of the DTD file</param>
        /// <returns>Text Reader object having DAISY XML</returns>
        private TextReader ReplaceData(String inputContent, String pathDTD)
        {
            if (inputContent != null)
            {
                inputContent = inputContent.Replace("<!DOCTYPE dtbook SYSTEM '" + pathDTD + "'>", "");
            }

            TextReader replacedXmlData = new StringReader(inputContent);
            return replacedXmlData;
        }

        /// <summary>
        /// DAISY Plugin - Function for reading the xslt that does the DTBook translation
        /// </summary>
        /// <param name="fileName">Fuile name of the XSLT</param>
        /// <returns>Stream of the XSLT file</returns>
        public Stream ReadXslt(string fileName)
        {
            Assembly asmBly = Assembly.GetExecutingAssembly();

            Stream streamIcon = null;
            foreach (string name in asmBly.GetManifestResourceNames())
            {
                if (name.EndsWith(fileName))
                {
                    streamIcon = asmBly.GetManifestResourceStream(name);
                    break;
                }
            }
            return streamIcon;
        }

        /// <summary>
        /// DAISY Plugin - Function to do validation of Output XML file with DTD
        /// </summary>
        /// <param name="inputContent">String having DAISY Xml</param>
        public void XmlValidation(String inputContent)
        {
            error = "";

            System.IO.TextReader stringReader = new System.IO.StringReader(inputContent);
            var xml = new XmlTextReader(stringReader);
            var xmlSettings = new XmlReaderSettings { ValidationType = ValidationType.DTD };

            xmlSettings.ValidationEventHandler += MyValidationEventHandler;

            var xsd = XmlReader.Create(xml, xmlSettings);

            try
            {
                while (xsd.Read())
                {
                }
                xsd.Close();
                Stream stream = null;
                Assembly asm = Assembly.GetExecutingAssembly();
                foreach (string name in asm.GetManifestResourceNames())
                {
                    if (name.EndsWith("Shematron.xsl"))
                    {
                        stream = asm.GetManifestResourceStream(name);
                        break;
                    }
                }

                XmlReader rdr = XmlReader.Create(stream);
                XPathDocument doc = new XPathDocument(xml);

                XslCompiledTransform trans = new XslCompiledTransform(true);
                trans.Load(rdr);

                StringBuilder sbforXML = new StringBuilder();
                TextWriter validatedDTBookXml = new StringWriter(sbforXML);

                trans.Transform(doc, null, validatedDTBookXml);

                rdr.Close();

                StringReader reader = new StringReader(validatedDTBookXml.ToString());

                if (reader.Read() != 0)
                {
                    error += reader.ReadToEnd();

                }
                reader.Close();
            }
            catch (UnauthorizedAccessException a)
            {
                xsd.Close();
                //DAISY Plugin - dont have access permission
                error = a.Message;
            }
            catch (Exception a)
            {
                xsd.Close();
                //DAISY Plugin - and other things that could go wrong
                error = a.Message;
            }
        }

        /// <summary>
        /// DAISY Plugin - Function to Capture all the Validity Errors
        /// </summary>
        public void MyValidationEventHandler(object sender, ValidationEventArgs args)
        {
            error += " Line Number : " + args.Exception.LineNumber + " and " +
             " Line Position : " + args.Exception.LinePosition + Environment.NewLine +
             " Message : " + args.Message + Environment.NewLine;
        }
    }
}

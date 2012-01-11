Open Government Data Initiative (OGDI)
======================================

Governments across the globe are seeking more transparency to improve citizen services and enhance accountability with constituencies. This global drive for governmental transparency has created demand for new technologies that can quickly help agencies comply with open data, open government initiatives. 

OGDI is a solution that makes it possible for agencies to publish government and public data more quickly and efficiently. 

To see OGDI in action please watch this [short video](http://blip.tv/file/get/Ogditeam-OGDIIntroductoryVideo528.wmv).

OGDI is written using **C#** and the **.NET Framework** and uses the [Windows Azure Platform](http://www.microsoft.com/windowsazure)

OGDI Components
---------------

###Data Service
OGDI uses RESTful Web service – a web service implemented using HTTP and the principles of Representational State Transfer (REST) – to expose data for programmatic access. Data service renders data using a number of formats, including **Open Data Protocol ([OData](http://www.odata.org))**, an extension to **Atom Publishing Protocol (AtomPub)**, **Keyhole Markup Language (KML)**, **JSON** and **JSONP**. OData leverages Uniform Resource Identifiers (URIs) for resource identification and an HTTP-based uniform interface for interacting with those resources – just as a user would do with any Web-based application. By building on these principles, OData enables users to achieve high levels of data integration and interoperability across a broad range of clients, servers, services, protocols and tools. 

In fact, OGDI-based web APIs can be accessed through a wide variety of client technologies, including: JavaScript, Flex, PHP, Python, Ruby, ASP.NET and Silverlight. These well understood formats allow developers to start quickly on new applications.

Many of the data sets in OGDI also include geospatial data, which is returned in the KML format, making OGDI compatible with popular desktop and Web-based mapping technologies including Microsoft Bing Maps, Google Maps, Yahoo! Maps, and Google Earth. 


###Data Browser
The Data Browser is an ASP.NET MVC 1.0 web application. It uses jQuery and a variety of other open source components and enables users to browse and query published data. The data can be conveniently visualized in widely used and recognizable formats such as tables, maps, bar graphs or pie charts. Thus, instead of downloading a file and poring over rows upon rows of data, end-users can interact with user-friendly visual tools that present complex data in a more meaningful manner.

In addition to browsing the data, developers can quickly learn how to use published data exposed by OGDI in their own applications from ready-to-run samples available on the site. These samples are available in a variety of languages and frameworks widely used on the Web, including JavaScript, PHP, Python, Flex, Silverlight, C#, among others. 

###Data Loader
The data loader is a tool that helps implementers quickly start enjoying the benefits of OGDI. OGDI includes both GUI-based and console-based data loader tools. The console tool takes CSV formatted data and publishes it into OGDI. In the process of loading the data, the utility can create a new dataset, add data, or update data in an already published dataset. Console tool is controlled through command line parameters and can be automated using shell scripts.


##License

###Microsoft Public License (Ms-PL)

This license governs use of the accompanying software. If you use the software, you accept this license. If you do not accept the license, do not use the software.

1. Definitions

The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the software.

A "contributor" is any person that distributes its contribution under this license.

"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights

(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.

(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations

(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.

(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.

(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.

(D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.

(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.  
 

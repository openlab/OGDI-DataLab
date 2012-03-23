using System;
using System.Collections.Generic;
using System.Globalization;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Xml;
using Ogdi.Azure.Configuration;
using Ogdi.InteractiveSdk.Mvc.App_GlobalResources;
using Ogdi.InteractiveSdk.Mvc.Models;

namespace Ogdi.InteractiveSdk.Mvc.Repository
{
    public static class BlogAndAnnouncementRepository
    {
        #region Public Methods

        /// <summary>
        /// This method will get the list of BlogAndAnnouncement
        /// </summary>
        /// <returns>list of BlogAndAnnouncement</returns>
        public static List<BlogAndAnnouncement> GetBlogsAndAnnouncements
        {
            get
            {
                List<BlogAndAnnouncement> blogList = new List<BlogAndAnnouncement>();

                try
                {
                    // Read the blog from blog URL
                    XmlReader reader = XmlReader.Create(OgdiConfiguration.GetValue("BlogPostsUrl"));

                    // Load the reader to SyndicationFeed
                    SyndicationFeed feed = SyndicationFeed.Load(reader);

                    // Create Regex to remove HTML tags
                    Regex regexHtmlTag = new Regex("<[\\S\\s]+?>");

                    // Loop through Items in the feed
                    foreach (var item in feed.Items)
                    {
                        // Create an object of BlogAndAnnouncement
                        BlogAndAnnouncement blogItem = new BlogAndAnnouncement();

                        // Set Title
                        blogItem.Title = item.Title.Text;
                        blogItem.Title = regexHtmlTag.Replace(blogItem.Title, "");
                        // Trim the Title if too long
                        if (!string.IsNullOrEmpty(blogItem.Title) && blogItem.Title.ToString().Length >
                            Convert.ToInt32(UIConstants.HPC_BlogTitleLength, CultureInfo.InvariantCulture))
                        {
                            blogItem.Title = blogItem.Title.ToString().Substring(0,
                                Convert.ToInt32(UIConstants.HPC_BlogTitleLength,
                                CultureInfo.InvariantCulture)) + "...";
                        }

                        // Set Link
                        blogItem.Link = item.Links[0].Uri.ToString();

                        // Set Description
                        blogItem.Description = item.Summary.Text;
                        blogItem.Description = regexHtmlTag.Replace(blogItem.Description, "");
                        // Trim the Description if too long
                        if (!string.IsNullOrEmpty(blogItem.Description) && blogItem.Description.ToString().Length >
                            Convert.ToInt32(UIConstants.HPC_BlogDescriptionLength, CultureInfo.InvariantCulture))
                        {
                            blogItem.Description = blogItem.Description.ToString().Substring(0,
                                Convert.ToInt32(UIConstants.HPC_BlogDescriptionLength,
                                CultureInfo.InvariantCulture)) + "...";
                        }

                        // Set PublishDate
                        blogItem.PublishDate = item.PublishDate.LocalDateTime.ToLongDateString() + ", " +
                            item.PublishDate.LocalDateTime.ToLongTimeString();

                        // Add the blogItem in the list
                        blogList.Add(blogItem);
                    }

                    // close the reader object
                    reader.Close();
                }
                catch
                { }

                // return list og blogs
                return blogList;
            }
        }

        #endregion
    }
}

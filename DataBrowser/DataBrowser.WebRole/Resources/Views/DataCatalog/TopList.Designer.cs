﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.225
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DataCatalogResources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class TopList {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal TopList() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Ogdi.InteractiveSdk.Mvc.Resources.Views.DataCatalog.TopList", typeof(TopList).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to by.
        /// </summary>
        public static string By {
            get {
                return ResourceManager.GetString("By", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to mm/dd/yyyy.
        /// </summary>
        public static string DateFormat {
            get {
                return ResourceManager.GetString("DateFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MM/dd/yyyy.
        /// </summary>
        public static string LastUpdateDateFormat {
            get {
                return ResourceManager.GetString("LastUpdateDateFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Most Rated.
        /// </summary>
        public static string MostRated {
            get {
                return ResourceManager.GetString("MostRated", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Most Viewed.
        /// </summary>
        public static string MostViewed {
            get {
                return ResourceManager.GetString("MostViewed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Recently Published.
        /// </summary>
        public static string RecentlyPublished {
            get {
                return ResourceManager.GetString("RecentlyPublished", resourceCulture);
            }
        }
    }
}
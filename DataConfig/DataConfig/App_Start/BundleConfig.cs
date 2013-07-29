using BundleTransformer.Core.Orderers;
using BundleTransformer.Core.Transformers;
using System.Web;
using System.Web.Optimization;

namespace DataConfig
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            /*
             * Styles
             */
            bundles.Add(new StyleBundle("~/bundles/foundation/style").Include(
                "~/Content/Styles/Foundation/foundation.css"));

            bundles.Add(new StyleBundle("~/bundles/jqueryui/style").Include(
                "~/Content/Styles/JqueryUI/jquery-ui.css"));

            var cssTransformer = new CssTransformer();
            var nullOrderer = new NullOrderer();

            var css = new StyleBundle("~/bundles/style").Include(
                "~/Content/Styles/Variables.less",
                "~/Content/Styles/Icons.less",
                "~/Content/Styles/Layout.less",
                "~/Content/Styles/Site.less");

            css.Transforms.Add(cssTransformer);
            css.Orderer = nullOrderer;

            bundles.Add(css);


            /*
             * Scripts
             */
            bundles.Add(new ScriptBundle("~/bundles/modernizr/script").Include(
                "~/Content/Scripts/Vendor/modernizr.js"));

            bundles.Add(new ScriptBundle("~/bundles/foundation/script").Include(
                "~/Content/Scripts/Vendor/foundation.js",
                "~/Content/Scripts/Vendor/app.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui/script").Include(
                "~/Content/Scripts/Vendor/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/script").Include(
                "~/Content/Scripts/Shared/_Layout.js",
                "~/Content/Scripts/Main/Data.js"));
        }
    }
}
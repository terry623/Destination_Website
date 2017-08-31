using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;

namespace Destination_Website.App_Start
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection Bundles)
        {
            Bundles.Add(new StyleBundle("~/Content/Bootstrap").Include(
                "~/Content/bootstrap.min.css"
            ));

            Bundles.Add(new ScriptBundle("~/Script/Bootstrap").Include(
                "~/Scripts/bootstrap.min.css"
            ));
        }
    }
}
using System.Web.Optimization;

namespace PaymentRequest
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"
                        ));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr.min.js"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.min.js"
                      ));
            bundles.Add(new ScriptBundle("~/bundles/parsley").Include(
                     "~/Scripts/parsleyjs/dist/parsley.min.js"
                     ));

            bundles.Add(new ScriptBundle("~/bundles/scripts").Include(
                "~/Scripts/x-editable/bootstrap-editable.min.js",
                "~/Scripts/bootstrap-switch/bootstrap-switch.min.js",
                     "~/plugins/jquery-circliful/js/jquery.circliful.min.js",
                     "~/Scripts/jquery.sparkline.min.js",
                     "~/Scripts/skycons.min.js",
                     "~/Scripts/jquery.dashboard.js",
                     "~/Scripts/custombox/custombox.min.js",
                     "~/Scripts/custombox/legacy.min.js"
                     ));


            bundles.Add(new StyleBundle("~/Content/css").Include(
               "~/Content/css/x-editable/bootstrap-editable.css",
                  "~/plugins/switchery/dist/switchery.min.css",
                  "~/Content/css/bootstrap-switch/bootstrap-switch.css",
                  "~/Content/css/jquery.circliful.min.css",
                   "~/Content/css/bootstrap.css",
                       "~/Content/css/core.css",
                       "~/Content/css/icons.css",
                       "~/Content/css/components.css",
                       "~/Content/css/pages.css",
                       "~/Content/css/menu.css",
                       "~/Content/css/responsive.css",
                       "~/Scripts/bootstrap-sweetalert/sweet-alert.css",
                       "~/Content/css/toastr/toastr.css",
                       "~/plugins/notifications/notification.css",
                       "~/plugins/notifications/notify.min.js",
                       "~/plugins/notifications/notify-metro.js",
                       "~/plugins/intl-tel-input/intlTelInput.css"
                       ));
            bundles.Add(new StyleBundle("~/Content/datatablescss").Include(

                "~/Content/css/datatables/jquery.dataTables.min.css",
                "~/Content/css/datatables/buttons.bootstrap.min.css",
                "~/Content/css/datatables/fixedHeader.bootstrap.min.css",
                "~/Content/css/datatables/responsive.bootstrap.min.css",
                "~/Content/css/datatables/scroller.bootstrap.min.css"

                     ));
            bundles.Add(new StyleBundle("~/Content/custombox").Include(
               "~/Content/css/custombox/custombox.min.css"

                    ));
            bundles.Add(new ScriptBundle("~/bundles/datatablesjs").Include(
                "~/Scripts/datatables/jquery.dataTables.min.js",
                "~/Scripts/datatables/dataTables.bootstrap.js",
                "~/Scripts/datatables/dataTables.buttons.min.js",
                "~/Scripts/datatables/buttons.bootstrap.min.js",
                "~/Scripts/datatables/jszip.min.js",
                "~/Scripts/datatables/pdfmake.min.js",
                "~/Scripts/datatables/vfs_fonts.js",
                "~/Scripts/datatables/buttons.html5.min.js",
                "~/Scripts/datatables/buttons.print.min.js",
                "~/Scripts/datatables/dataTables.fixedHeader.min.js",
                "~/Scripts/datatables/dataTables.keyTable.min.js",
                "~/Scripts/datatables/dataTables.responsive.min.js",
                "~/Scripts/datatables/responsive.bootstrap.min.js",
                "~/Scripts/datatables/dataTables.scroller.min.js"
                ));

            bundles.Add(new ScriptBundle("~/bundle/common").Include(
                "~/plugins/pace/pace.min.js",
                "~/Scripts/detect.js",
                "~/Scripts/fastclick.js",
                "~/Scripts/jquery.blockUI.js",
                "~/Scripts/waves.js",
                "~/Scripts/wow.min.js",
                "~/Scripts/jquery.nicescroll.js",
                "~/Scripts/jquery.scrollTo.min.js",
                "~/plugins/switchery/dist/switchery.min.js",
                "~/Scripts/jquery.slimscroll.js",
                "~/Scripts/moment-with-locales.min.js",
                "~/Scripts/toastr.min.js",
                "~/Scripts/bootstrap-sweetalert/sweet-alert.js",
                "~/plugins/intl-tel-input/intlTelInput.min.js"));

            bundles.Add(new ScriptBundle("~/bundle/core-app").Include(
                "~/Scripts/jquery.core.js",
                "~/Scripts/jquery.app.js"));

        }
    }
}

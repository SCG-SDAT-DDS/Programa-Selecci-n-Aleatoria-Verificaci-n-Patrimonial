using System.Web;
using System.Web.Optimization;

namespace Transparencia
{
    public class BundleConfig
    {
        // Para obtener más información sobre las uniones, visite https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-3.1.1.min.js"));


            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/popper.min.js",
                      "~/Scripts/bootstrap.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                      "~/Scripts/plugins/jquery-ui/jquery-ui.min.js"));


            // CSS style (bootstrap/inspinia)
            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.min.css",
                      "~/Content/animate.css",
                      "~/Content/site.css",
                      "~/Content/style.css",
                      "~/Content/Transparencia-custom.css"));

            // jQueryUI CSS
            bundles.Add(new ScriptBundle("~/Scripts/plugins/jquery-ui/jqueryuiStyles").Include(
                        "~/Scripts/plugins/jquery-ui/jquery-ui.min.css"));


            // Font Awesome icons
            bundles.Add(new StyleBundle("~/font-awesome/css").Include(
                      "~/fonts/font-awesome/css/font-awesome.min.css", new CssRewriteUrlTransform()));

            // SlimScroll
            bundles.Add(new ScriptBundle("~/plugins/slimScroll").Include(
                      "~/Scripts/plugins/slimscroll/jquery.slimscroll.min.js"));

            // validate 
            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                     "~/Scripts/jquery.validate.js",
                      "~/Scripts/jquery.validate-vsdoc.js",
                      "~/Scripts/jquery.validate.unobtrusive.js",
                      "~/Scripts/jquery.unobtrusive*",
                       "~/Scripts/jquery.unobtrusive-ajax.js"
                      ));



            // Inspinia script
            bundles.Add(new ScriptBundle("~/bundles/inspinia").Include(
                      "~/Scripts/plugins/metisMenu/jquery.metisMenu.js",
                      "~/Scripts/plugins/pace/pace.min.js",
                      "~/Scripts/app/inspinia.js",
                      "~/Scripts/jQuery.js"));


            // iCheck css styles
            bundles.Add(new StyleBundle("~/Content/plugins/iCheck/iCheckStyles").Include(
                      "~/Content/plugins/iCheck/custom.css"));

            // iCheck
            bundles.Add(new ScriptBundle("~/plugins/iCheck").Include(
                      "~/Scripts/plugins/iCheck/icheck.min.js"));


            // dataPicker styles
            bundles.Add(new StyleBundle("~/plugins/dataPickerStyles").Include(
                      "~/Content/plugins/datapicker/datepicker3.css"));

            // dataPicker 
            bundles.Add(new ScriptBundle("~/plugins/dataPicker").Include(
                      "~/Scripts/plugins/datapicker/bootstrap-datepicker.js",
                      "~/Scripts/plugins/datapicker/bootstrap-datepicker.es.js"));


            // TimePicker styles
            bundles.Add(new StyleBundle("~/plugins/TimePickerStyle").Include(
                      "~/Content/timepicki.css"));

            // TimerPicker 
            bundles.Add(new ScriptBundle("~/plugins/TimePickerJs").Include(
                      "~/Scripts/timepicki.js"));


            // Footable Styless
            bundles.Add(new StyleBundle("~/plugins/footableStyles").Include(
                      "~/Content/plugins/footable/footable.core.css"/*, new CssRewriteUrlTransform()*/));

            // Footable alert
            bundles.Add(new ScriptBundle("~/plugins/footable").Include(
                      "~/Scripts/plugins/footable/footable.all.min.js"));

            //SweetAlert
            bundles.Add(new StyleBundle("~/sweatalert").Include(
                       "~/Scripts/package/dist/sweetalert2.all.js"));
            bundles.Add(new ScriptBundle("~/sweatalert").Include(
                      "~/Scripts/package/dist/sweetalert2.all.js"));


            //SweetAlert
            bundles.Add(new StyleBundle("~/stepCss").Include(
                       "~/Content/jquery.steps.css"));
            bundles.Add(new ScriptBundle("~/stepJs").Include(
                      "~/Scripts/jquery.steps.js"));

            //Select2
            bundles.Add(new StyleBundle("~/select2Css").Include(
                      "~/Public/js/bootstrap/select2/select2.css"));
            bundles.Add(new ScriptBundle("~/select2Js").Include(
                      "~/Public/js/bootstrap/select2/select2.js"));

            //SelectForListBox
            bundles.Add(new StyleBundle("~/selectForListCss").Include(
                      "~/Content/bootstrap-select.css")); ;
            bundles.Add(new ScriptBundle("~/selectForListCssJs").Include(
                      "~/Scripts/bootstrap-select.min.js"));

            //Login
            bundles.Add(new StyleBundle("~/appCss").Include(
                       "~/Public/css/vendor.css",
                       "~/Public/css/app.css",
                       "~/Public/js/bootstrap/sweatAlert/sweatalert.css",
                       "~/Public/js/bootstrap/select2/select2.css",
                       "~/Public/js/bootstrap/toastr/toastr.css",
                       "~/Public/css/custom.css"));

            bundles.Add(new ScriptBundle("~/appJs").Include(
                        "~/Public/js/app.js",
                        "~/Public/js/bootstrap/sweatAlert/sweatalert.js",
                        "~/Public/js/bootstrap/select2/select2.js",
                        "~/Public/js/bootstrap/toastr/toastr.js",
                        "~/Public/js/bootstrap/datepicker/datepicker.js",
                        "~/Public/js/jquery/mask/mask.js",
                        "~/Public/js/bootstrap/autocomplete/mockjax.js",
                        "~/Public/js/bootstrap/autocomplete/typeahead.js",
                        "~/Public/js/init.js"));

            bundles.Add(new ScriptBundle("~/validate").Include(
                        "~/Public/js/jquery/validate/validate*"));
        }
    }
}

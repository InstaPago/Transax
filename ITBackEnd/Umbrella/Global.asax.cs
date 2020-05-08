using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using static InstaTransfer.ITLogic.Helpers.SessionHelper;

namespace Umbrella
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = new System.Globalization.CultureInfo("es-VE");
        }

        //void Application_AcquireRequestState(object sender, EventArgs e)
        //{
        //    HttpContext context = HttpContext.Current;
        //    var url = context.Request.Url;
        //    // CheckSession() inlined
        //    if (!url.AbsolutePath.Contains("requestData"))
        //    {
        //        if (MySession.Current.LoggedIn != true && url.AbsolutePath != "/" && url.AbsolutePath != "/Account/Register" &&  !url.AbsolutePath.ToLower().Contains("propuesta"))
        //        {
        //            context.Response.Redirect("~");
        //        }
        //    }

        //}
    }
}

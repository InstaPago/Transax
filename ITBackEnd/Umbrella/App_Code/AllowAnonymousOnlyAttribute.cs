using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Umbrella.App_Code
{
    public class AllowAnonymousOnlyAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            //Verificamos si esta autenticado el usuario
            if (httpContext.Request.IsAuthenticated)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            //var url = filterContext.HttpContext.Request.Url;
            //if (url.AbsolutePath != "/" && url.AbsolutePath != "/Account/Register" && url.AbsolutePath != "/Nativa/Register")
            //{
            //    // Returns HTTP 401 - see comment in HttpUnauthorizedResult.cs.
            //    filterContext.Result = new RedirectToRouteResult(
            //                               new RouteValueDictionary
            //                               {
            //                           { "action", "Login" },
            //                           { "controller", "Account" }
            //                               });
            //}
        }
    }
}
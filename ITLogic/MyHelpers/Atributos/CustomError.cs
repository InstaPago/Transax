using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web;

namespace MyHelpers.Atributos
{
    public class CustomErrorAtribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            /*****  Original code from MVC source  ******/

            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }
            if (filterContext.IsChildAction)
            {
                return;
            }

            // If custom errors are disabled, we need to let the normal ASP.NET exception handler
            // execute so that the user can see useful debugging information.
            if (filterContext.ExceptionHandled || !filterContext.HttpContext.IsCustomErrorEnabled)
            {
                return;
            }

            Exception exception = filterContext.Exception;

            // If this is not an HTTP 500 (for example, if somebody throws an HTTP 404 from an action method),
            // ignore it.
            if (new HttpException(null, exception).GetHttpCode() != 500)
            {
                return;
            }

            if (!ExceptionType.IsInstanceOfType(exception))
            {
                return;
            }

            //***** This is the new code  *****//
            if (filterContext.HttpContext.Request.IsAjaxRequest()) // If it's a ajax request
            {
                filterContext.Result = new JsonResult // Set the response to JSon
                {
                    Data = new { success = false, message = "Ha ocurrido un error" }
                };

                filterContext.ExceptionHandled = true;
                filterContext.HttpContext.Response.Clear();
                filterContext.HttpContext.Response.StatusCode = 200;  // Maybe it should be 500, but this way you handle the JQuery on the success event
                filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
            }
            else //*** From here, is the original code againg **//
            {
                string controllerName = (string)filterContext.RouteData.Values["controller"];
                string actionName = (string)filterContext.RouteData.Values["action"];
                HandleErrorInfo model = new HandleErrorInfo(filterContext.Exception, controllerName, actionName);
                filterContext.Result = new ViewResult
                {
                    ViewName = View,
                    MasterName = Master,
                    ViewData = new ViewDataDictionary<HandleErrorInfo>(model),
                    TempData = filterContext.Controller.TempData
                };
                filterContext.ExceptionHandled = true;
                filterContext.HttpContext.Response.Clear();
                filterContext.HttpContext.Response.StatusCode = 500;

                // Certain versions of IIS will sometimes use their own error page when
                // they detect a server error. Setting this property indicates that we
                // want it to try to render ASP.NET MVC's error page instead.
                filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
            }
        }
    }
}

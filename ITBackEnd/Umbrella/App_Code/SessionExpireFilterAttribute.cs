using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static InstaTransfer.ITLogic.Helpers.SessionHelper;

namespace Umbrella.App_Code
{
    public class SessionExpireFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // check if session is supported
            var session = MySession.Current;
            if (!session.LoggedIn)
            {
                // check if a new session id was generated
                filterContext.Result = new RedirectResult("~/Account/Login");
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
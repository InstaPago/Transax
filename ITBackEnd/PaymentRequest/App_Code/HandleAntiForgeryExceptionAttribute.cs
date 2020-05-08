using System.Net;
using System.Web.Mvc;

namespace PaymentRequest.App_Code
{
    public class HandleAntiForgeryExceptionAttribute : HandleErrorAttribute
    {
        private string _xforwardedFor = "X-Forwarded-For";
        private IPAddress _ipAddress;

        //[Inject]
        //public ILog _log { get; set; }

        /// <summary>
        /// We want to handle these exceptions to provide the user a better experience.
        /// </summary>
        public HandleAntiForgeryExceptionAttribute()
        {
            this.ExceptionType = typeof(HttpAntiForgeryException);

            //if (_log == null)
            //{
            //    _log = (ILog)DependencyResolver.Current.GetService(typeof(ILog));
            //}
        }

        public override void OnException(ExceptionContext filterContext)
        {
            if (this.ExceptionType.IsAssignableFrom(filterContext.Exception.GetType()))
            {
                // See if we can determine the IP Address
                var headers = filterContext.HttpContext.Request.Headers;
                _ipAddress = string.IsNullOrWhiteSpace(headers[_xforwardedFor]) ? IPAddress.None : IPAddress.Parse(headers[_xforwardedFor]);
                var ipAddressMsg = _ipAddress == IPAddress.None ? "Unknown" : _ipAddress.ToString();

                // Is the user logged in?
                var username = filterContext.HttpContext.User.Identity.IsAuthenticated ? filterContext.HttpContext.User.Identity.Name : "Annonymous";

                // Generate our message and log it
                var message = string.Format("HttpAntiForgeryException occurred for user: {0}, IP Address: {1}", username, ipAddressMsg);
                //_log.Info(message, filterContext.Exception);

                // Mark the exception handled and simply refresh the page.  Alternatively, we could redirect to a specific
                // error view.
                filterContext.ExceptionHandled = true;
                filterContext.Result = new RedirectResult(filterContext.HttpContext.Request.RawUrl);
            }
        }
    }
}
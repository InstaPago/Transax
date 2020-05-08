using System.Web.Mvc;
using PaymentRequest.App_Code;

namespace PaymentRequest
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new HandleAntiForgeryExceptionAttribute());
        }
    }
}

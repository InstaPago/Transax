using System.Web.Mvc;
using Umbrella.App_Code;

namespace Umbrella
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

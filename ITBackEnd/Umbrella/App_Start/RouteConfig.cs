using System.Web.Mvc;
using System.Web.Routing;

namespace Umbrella
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapRoute(
                  name: "Nativa",
                  url: "Nativa",
                  defaults: new { controller = "Nativa", action = "Register" }
                  );
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Account", action = "Login", id = UrlParameter.Optional }
            );


        }
    }
}

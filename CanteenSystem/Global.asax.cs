using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace CanteenSystem
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //// Yêu cầu login cho tất cả action, trừ nơi có [AllowAnonymous]
            //GlobalFilters.Filters.Add(new AuthorizeAttribute());
        }
    }
}

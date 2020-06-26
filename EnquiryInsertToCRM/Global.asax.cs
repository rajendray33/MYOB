using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace EnquiryInsertToCRM
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AutoMapperConfig.Initialize();
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //Services.JobScheduler_Init scheduler = new Services.JobScheduler_Init();
            //scheduler.Start();
        }
        protected void Session_Start()
        {
            
        }
    }
}

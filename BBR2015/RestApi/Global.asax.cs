using Castle.Windsor;
using Database;
using RestApi.Infrastructure;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Database.Infrastructure;
using Repository;

namespace RestApi
{
    public class RestApiApplication : System.Web.HttpApplication
    {
        private readonly IWindsorContainer _container;

        public RestApiApplication()
        {
            _container = CreateContainer();
        }

        public static IWindsorContainer CreateContainer()
        {
            return new WindsorContainer().Install(new DependencyConventions());
        }
        protected void Application_Start()
        {
            ControllerBuilder.Current.SetControllerFactory(new WindsorMvcControllerFactory(_container));

            GlobalConfiguration.Configuration.Services.Replace(typeof(IHttpControllerActivator), new WindsorControllerActivator(_container));
            GlobalConfiguration.Configure(WebApiConfig.Register);

            RouteConfig.RegisterRoutes(RouteTable.Routes);

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        public override void Dispose()
        {
            _container.Dispose();
            base.Dispose();
        }
    }
}

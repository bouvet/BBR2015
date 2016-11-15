using Castle.Windsor;
using RestApi.Infrastructure;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace RestApi
{
    public class RestApiApplication : System.Web.HttpApplication
    {
        private IWindsorContainer _container;

        protected void Application_Start()
        {
            // Container må lages her i Application_Start - ikke i constructor
            _container = CreateContainer();

            ControllerBuilder.Current.SetControllerFactory(new WindsorMvcControllerFactory(_container));
            GlobalConfiguration.Configuration.Services.Replace(typeof(IHttpControllerActivator), new WindsorApiControllerActivator(_container));
            GlobalConfiguration.Configure(WebApiConfig.Register);

            RouteConfig.RegisterRoutes(RouteTable.Routes);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        public static IWindsorContainer CreateContainer()
        {
            return new WindsorContainer().Install(new DependencyConventions());
        }

        public override void Dispose()
        {
            _container?.Dispose();
            base.Dispose();
        }
    }
}

using Castle.Windsor;
using Database;
using RestApi.Infrastructure;
using System.Web.Http;
using System.Web.Http.Dispatcher;
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
            GlobalConfiguration.Configuration.Services.Replace(typeof(IHttpControllerActivator), new WindsorControllerActivator(_container));
            GlobalConfiguration.Configure(WebApiConfig.Register);

            System.Data.Entity.Database.SetInitializer(new Initializer());

            ServiceLocator.Current = _container;           
        }

        public override void Dispose()
        {
            _container.Dispose();
            base.Dispose();
        }
    }
}

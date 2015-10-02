using Castle.Windsor;
using Database;
using RestApi.Infrastructure;
using System.Web.Http;
using System.Web.Http.Dispatcher;

namespace RestApi
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private readonly IWindsorContainer _container;

        public WebApiApplication()
        {
            _container = new WindsorContainer().Install(new DependencyConventions());
        }

        protected void Application_Start()
        {
            GlobalConfiguration.Configuration.Services.Replace(typeof(IHttpControllerActivator), new WindsorControllerActivator(_container));
            GlobalConfiguration.Configure(WebApiConfig.Register);

            System.Data.Entity.Database.SetInitializer(new Initializer());

            ServiceLocator.Current = _container;

            var initialDataCreator = _container.Resolve<InitialDataCreator>();
            initialDataCreator.FyllDatabasen();
        }

        public override void Dispose()
        {
            _container.Dispose();
            base.Dispose();
        }
    }
}

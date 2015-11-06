using Castle.MicroKernel.Registration;
using System.Web.Http;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Repository;
using RestApi.Controllers;
using Database;
using Database.Infrastructure;

namespace RestApi.Infrastructure
{
    public class DependencyConventions : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<CurrentMatchProvider>().LifestyleSingleton());
            container.Register(Component.For<GameStateService>().LifestyleSingleton());
            container.Register(Component.For<PosisjonsService>().LifestyleSingleton());         
            container.Register(Component.For<TilgangsKontroll>().LifestyleSingleton());         

            container.Register(Types.FromAssemblyContaining<TilgangsKontroll>().Pick().WithServiceSelf().LifestyleTransient());
            container.Register(Types.FromAssemblyContaining<BaseController>().BasedOn<ApiController>().WithServiceSelf().LifestylePerWebRequest());
            container.Register(Types.FromAssemblyContaining<DataContextFactory>().Pick().WithServiceSelf().LifestyleTransient());

            ServiceLocator.Current = container; 
        }
    }
}
using System;
using Castle.MicroKernel.Registration;
using System.Web.Http;
using System.Web.Mvc;
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
            container.Register(Types.FromAssemblyContaining<DataContextFactory>().Pick().WithServiceSelf().LifestyleTransient());

            container.Register(Component.For<CurrentMatchProvider>().LifestyleSingleton());
            container.Register(Component.For<GameStateService>().LifestyleSingleton());
            container.Register(Component.For<PosisjonsService>().LifestyleSingleton());
            container.Register(Component.For<TilgangsKontroll>().LifestyleSingleton());           

            container.Register(Types.FromAssemblyContaining<GameService>().Where(x => x != typeof(TilgangsKontroll)).WithServiceSelf().LifestyleTransient());
            container.Register(Types.FromAssemblyContaining<BaseController>().BasedOn<ApiController>().WithServiceSelf().LifestylePerWebRequest());
            container.Register(Types.FromAssemblyContaining<BaseController>().BasedOn<Controller>().WithServiceSelf().LifestylePerWebRequest());
            

            //var tilgangskontroll = container.Resolve<TilgangsKontroll>();
            //container.Register(
            //    Component.For<TilgangsKontroll>()
            //        .Instance(tilgangskontroll)
            //        .Named(Guid.NewGuid().ToString())
            //        .IsDefault());

            ServiceLocator.Current = container; 
        }
    }
}
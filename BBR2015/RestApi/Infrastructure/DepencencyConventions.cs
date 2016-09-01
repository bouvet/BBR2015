using System;
using System.Diagnostics;
using Castle.MicroKernel.Registration;
using System.Web.Http;
using System.Web.Mvc;
using Castle.MicroKernel.ModelBuilder.Descriptors;
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
            container.Register(Types.FromAssemblyContaining<GameService>().Pick().WithServiceSelf().LifestyleTransient());
           
            // Rekkefølgen under er viktig!
            RegisterInstance(container, container.Resolve<CurrentMatchProvider>());
            RegisterInstance(container, container.Resolve<GameStateService>());
            RegisterInstance(container, container.Resolve<TilgangsKontroll>());
            RegisterInstance(container, container.Resolve<PosisjonsService>());

            container.Register(Types.FromAssemblyContaining<BaseController>().BasedOn<ApiController>().WithServiceSelf().LifestyleTransient());
            container.Register(Types.FromAssemblyContaining<BaseController>().BasedOn<Controller>().WithServiceSelf().LifestyleTransient());

            ServiceLocator.Current = container; 
        }

        private static void RegisterInstance<T>(IWindsorContainer container, T instance) where T : class 
        {
            container.Register(Component.For<T>().Instance(instance).Named(Guid.NewGuid().ToString()).IsDefault());
        }
    }
}
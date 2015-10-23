using Castle.MicroKernel.Registration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Repository;
using RestApi.Controllers;
using Database;

namespace RestApi.Infrastructure
{
    public class DependencyConventions : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<CurrentMatchProvider>().LifestyleSingleton());
            container.Register(Component.For<GameStateService>().LifestyleSingleton());
            container.Register(Component.For<PosisjonsRepository>().LifestyleSingleton());
            //container.Register(Component.For<MeldingRepository>().LifestyleSingleton());

            container.Register(Types.FromAssemblyContaining<AdminRepository>().Pick().WithServiceSelf().LifestyleTransient());
            container.Register(Types.FromAssemblyContaining<BaseController>().BasedOn<BaseController>().WithServiceSelf().LifestylePerWebRequest());
            container.Register(Types.FromAssemblyContaining<DataContextFactory>().Pick().WithServiceSelf().LifestyleTransient());

        }
    }
}
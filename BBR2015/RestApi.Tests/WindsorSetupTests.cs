using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Windsor;
using Database.Infrastructure;
using NUnit.Framework;
using Repository;
using RestApi.Infrastructure;

namespace RestApi.Tests
{
    [TestFixture]
    public class WindsorSetupTests
    {
        [Test]
        public void Tilgangskontroll()
        {
            var container = new WindsorContainer();

            container.Install(new DependencyConventions());

            var controllerFactory = new WindsorMvcControllerFactory(container);
            var apiControllerFactory = new WindsorApiControllerActivator(container);

            //var t1 = ServiceLocator.Current.Resolve<TilgangsKontroll>();
            //var t2 = controllerFactory.CreateController()

        }
    }
}

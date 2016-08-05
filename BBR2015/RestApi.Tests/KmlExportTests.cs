using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using NUnit.Framework;
using Repository;
using Repository.Kml;

namespace RestApi.Tests
{
    [TestFixture, Explicit]
    public class KmlExportTests
    {
        private IWindsorContainer _container;

        [Test]
        public void Export()
        {
            _container = RestApiApplication.CreateContainer();

            var hardcodedMatchProvider = new HardcodedMatchProvider(null);
            hardcodedMatchProvider.SetMatchId(new Guid("4A82121D-71A6-45AE-AFD8-41574086B55C"));

            _container.Register(
                Component.For<CurrentMatchProvider>()
                    .Instance(hardcodedMatchProvider)
                    .Named(Guid.NewGuid().ToString())
                    .IsDefault());

            var service = _container.Resolve<KmlExport>();

            var kml = service.GetKml();

            File.WriteAllText(@"c:\temp\bbr2015.kml", kml);
            
            Assert.IsNotNullOrEmpty(kml);
        }


    }
}

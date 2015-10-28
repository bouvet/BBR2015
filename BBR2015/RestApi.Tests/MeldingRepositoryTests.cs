using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Windsor;
using Database;
using NUnit.Framework;
using RestApi.Tests.Infrastructure;

namespace RestApi.Tests
{
    public class MeldingRepositoryTests : BBR2015DatabaseTests
    {
        private IWindsorContainer _container;
        private Gitt _gitt;
        private DataContextFactory _dataContextFactory;

        [SetUp]
        public void Given()
        {
            _container = RestApiApplication.CreateContainer();
            _gitt = new Gitt(_container);
            _dataContextFactory = _container.Resolve<DataContextFactory>();
            TimeService.ResetToRealTime();
        }

        [Test]
        public void NårEnMeldingPostes_SkalDenLagresIDatabasen() { }

        [Test]
        public void NårEnMeldingPostes_SkalDenKunneHentesUtViaPolling()
        {

        }

        [Test]
        public void NårDetErPostetMangeMeldinger_SkalBareSiste10HentesUtHvisEnIkkeSpørMedSekvensNummer()
        {

        }

        [Test]
        public void NårDetErPostetMangeMeldinger_SkalEnKunneHenteAlleNyeMeldingerSidenEtSekvensNummer()
        {

        }
    }
}

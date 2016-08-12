using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Windsor;
using Database;
using Database.Entities;
using NUnit.Framework;
using Repository;
using RestApi.Tests.Infrastructure;

namespace RestApi.Tests
{
    [TestFixture]
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

            // Slett alle meldinger (blir rullet tilbake i transaksjon uansett)
            using (var context = _dataContextFactory.Create())
            {
                context.Meldinger.Clear();
                context.SaveChanges();
            }
        }

        [Test]
        public void NårEnMeldingPostes_SkalDenLagresIDatabasen()
        {
            var gittLag = _gitt.ToLagMedToDeltakere();

            var lag1 = gittLag[0];
            var deltaker1 = lag1.Deltakere[0];

            var meldingsService = _container.Resolve<MeldingService>();

            meldingsService.PostMelding(deltaker1.DeltakerId, lag1.LagId, "Testmelding");

            using (var context = _dataContextFactory.Create())
            {
                var melding = context.Meldinger.Single();

                Assert.AreEqual(deltaker1.DeltakerId, melding.DeltakerId, "DeltakerId");
                Assert.AreEqual(lag1.LagId, melding.LagId, "LagId");
                Assert.AreEqual("Testmelding", melding.Tekst, "Tekst");
            }
        }

        [Test]
        public void NårEnMeldingPostes_SkalDenKunneHentesUtViaPollingForPostetLag()
        {
            var gittLag = _gitt.ToLagMedToDeltakere();

            var lag1 = gittLag[0];
            var deltaker1 = lag1.Deltakere[0];

            var lag2 = gittLag[1];

            var meldingsService = _container.Resolve<MeldingService>();

            meldingsService.PostMelding(deltaker1.DeltakerId, lag1.LagId, "Testmelding");

            var melding = meldingsService.HentMeldinger(lag1.LagId).Single();

            Assert.AreEqual(deltaker1.DeltakerId, melding.DeltakerId, "DeltakerId");
            Assert.AreEqual(lag1.LagId, melding.LagId, "LagId");
            Assert.AreEqual("Testmelding", melding.Tekst, "Tekst");

            Assert.AreEqual(0, meldingsService.HentMeldinger(lag2.LagId).Count(), "Melding skulle ikke komme til lag2");
        }

        [Test]
        public void NårEnMeldingPostesTilAlleLag_SkalAlleLagFåDenUt()
        {
            var gittLag = _gitt.ToLagMedToDeltakere();

            var lag1 = gittLag[0];
            var deltaker1 = lag1.Deltakere[0];

            var lag2 = gittLag[1];

            var meldingsService = _container.Resolve<MeldingService>();

            meldingsService.PostMeldingTilAlle(deltaker1.DeltakerId, lag1.LagId, "Testmelding til alle");

            var melding = meldingsService.HentMeldinger(lag2.LagId).Single();

            Assert.AreEqual(deltaker1.DeltakerId, melding.DeltakerId, "DeltakerId");
            Assert.AreEqual(lag2.LagId, melding.LagId, "LagId");
            Assert.AreEqual("Testmelding til alle", melding.Tekst, "Tekst");
        }

        [Test]
        public void NårDetErPostetMangeMeldinger_SkalBareSiste10HentesUtHvisEnIkkeSpørMedSekvensNummer()
        {
            var gittLag = _gitt.ToLagMedToDeltakere();

            var lag1 = gittLag[0];
            var deltaker1 = lag1.Deltakere[0];

            var meldingsService = _container.Resolve<MeldingService>();

            var antall = 15;
            for (int i = 0; i < antall; i++)
            {
                meldingsService.PostMelding(deltaker1.DeltakerId, lag1.LagId, "Testmelding" + i);
            }

            using (var context = _dataContextFactory.Create())
            {
                Assert.AreEqual(antall, context.Meldinger.Count(), "Alle meldinger skulle være lagret i databasen");
            }

            var antallMeldingerFraPoll = meldingsService.HentMeldinger(lag1.LagId).Count();

            Assert.AreEqual(Constants.Meldinger.MaxAntallUtenSekvensId, antallMeldingerFraPoll, "Skal bare få 10 siste hvis en ikke spesifiserer sekvensnr");
        }

        [Test]
        public void NårDetErPostetMangeMeldinger_SkalEnKunneHenteAlleNyeMeldingerSidenEtSekvensNummer()
        {
            var gittLag = _gitt.ToLagMedToDeltakere();

            var lag1 = gittLag[0];
            var deltaker1 = lag1.Deltakere[0];

            var meldingsService = _container.Resolve<MeldingService>();

            meldingsService.PostMelding(deltaker1.DeltakerId, lag1.LagId, "Testmelding1");

            var sekvensNr = meldingsService.HentMeldinger(lag1.LagId).Select(x => x.SekvensId).Single();

            var antall = 15;
            for (int i = 0; i < antall; i++)
            {
                meldingsService.PostMelding(deltaker1.DeltakerId, lag1.LagId, "Testmelding" + i);
            }

            var antallMeldingerFraPoll = meldingsService.HentMeldinger(lag1.LagId, sekvensNr).Count();

            Assert.AreEqual(antall, antallMeldingerFraPoll, "Skal alle meldinger hvis en spesifiserer sekvensnr");
        }

        [Test]
        public void NårDetErPostetMeldinger_NårEnSpørMedSisteSekvensNummer_SkalEnIkkeFåNoeTilbake()
        {
            var gittLag = _gitt.ToLagMedToDeltakere();

            var lag1 = gittLag[0];
            var deltaker1 = lag1.Deltakere[0];

            var meldingsService = _container.Resolve<MeldingService>();

            meldingsService.PostMelding(deltaker1.DeltakerId, lag1.LagId, "Testmelding1");

            var sekvensNr = meldingsService.HentMeldinger(lag1.LagId).Select(x => x.SekvensId).Single();            

            var antallMeldingerFraPoll = meldingsService.HentMeldinger(lag1.LagId, sekvensNr).Count();

            Assert.AreEqual(0, antallMeldingerFraPoll, "Skal ikke få meldinger");
        }
    }
}

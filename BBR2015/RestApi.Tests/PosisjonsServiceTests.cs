using System;
using System.Linq;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Database;
using NUnit.Framework;
using Repository;
using RestApi.Tests.Infrastructure;

namespace RestApi.Tests
{
    [TestFixture]
    public class PosisjonsServiceTests : BBR2015DatabaseTests
    {
        private IWindsorContainer _container;
        private Given _given;
        private DataContextFactory _dataContextFactory;
        private CascadingAppSettings _appSettings;
       
        [SetUp]
        public void Given()
        {
            _container = RestApiApplication.CreateContainer();

            _appSettings = new CascadingAppSettings();
            _container.Register(Component.For<CascadingAppSettings>().Instance(_appSettings).IsDefault().Named(Guid.NewGuid().ToString()));

            _given = new Given(_container);
            _dataContextFactory = _container.Resolve<DataContextFactory>();
            TimeService.ResetToRealTime();
        }

        [Test]
        public void NårEnDeltakerPosterEnPosisjon_SkalPosisjonenLagresIDatabasen()
        {
            var givenLag = _given.ATwoTeamWithTwoPlayers();

            var posisjonsSevice = _container.Resolve<PosisjonsRepository>();

            var deltaker11 = givenLag[0].Deltakere[0];
            var latitude = 59.6785526164;
            var longitude = 10.6039274298;

            posisjonsSevice.RegistrerPosisjon(deltaker11.Lag.LagId, deltaker11.DeltakerId, latitude, longitude);

            using (var context = _dataContextFactory.Create())
            {
                Assert.AreEqual(1, context.DeltakerPosisjoner.Count(), "Skulle vært 1 posisjon");
            }
        }

        [Test]
        public void NårEnDeltakerPosterSammePosisjonToGanger_SkalPosisjonenLagresIDatabasenBareEnGang()
        {
            var given = _given.ATwoTeamWithTwoPlayers();

            var posisjonsSevice = _container.Resolve<PosisjonsRepository>();

            var deltaker11 = given[0].Deltakere[0];
            var latitude = 59.6785526164;
            var longitude = 10.6039274298;

            posisjonsSevice.RegistrerPosisjon(deltaker11.Lag.LagId, deltaker11.DeltakerId, latitude, longitude);
            TimeService.AddSeconds(30);
            posisjonsSevice.RegistrerPosisjon(deltaker11.Lag.LagId, deltaker11.DeltakerId, latitude, longitude);

            using (var context = _dataContextFactory.Create())
            {
                Assert.AreEqual(1, context.DeltakerPosisjoner.Count(), "Skulle vært 1 posisjon");
            }
        }

        [Test]
        public void NårEnDeltakerPosterNyPosisjonToGangerMenForTettITid_SkalPosisjonenLagresIDatabasenBareEnGang()
        {
            var given = _given.ATwoTeamWithTwoPlayers();

            var posisjonsSevice = _container.Resolve<PosisjonsRepository>();

            var deltaker11 = given[0].Deltakere[0];
            var latitude = 59.6785526164;
            var longitude = 10.6039274298;

            posisjonsSevice.RegistrerPosisjon(deltaker11.Lag.LagId, deltaker11.DeltakerId, latitude, longitude);
            TimeService.AddSeconds(0);
            posisjonsSevice.RegistrerPosisjon(deltaker11.Lag.LagId, deltaker11.DeltakerId, latitude+5, longitude+5);

            using (var context = _dataContextFactory.Create())
            {
                Assert.AreEqual(1, context.DeltakerPosisjoner.Count(), "Skulle vært 1 posisjon");
            }
        }

        [Test]
        public void NårEnDeltakerPosterNyPosisjonToGangerMedGodTidMellom_SkalPosisjonenLagresIDatabasenToGanger()
        {
            var given = _given.ATwoTeamWithTwoPlayers();

            var posisjonsSevice = _container.Resolve<PosisjonsRepository>();

            var deltaker11 = given[0].Deltakere[0];
            var latitude = 59.6785526164;
            var longitude = 10.6039274298;

            posisjonsSevice.RegistrerPosisjon(deltaker11.Lag.LagId, deltaker11.DeltakerId, latitude, longitude);
            TimeService.AddSeconds(200);
            posisjonsSevice.RegistrerPosisjon(deltaker11.Lag.LagId, deltaker11.DeltakerId, latitude + 5, longitude + 5);

            using (var context = _dataContextFactory.Create())
            {
                Assert.AreEqual(2, context.DeltakerPosisjoner.Count(), "Skulle vært 1 posisjon");
            }
        }

        [Test]
        public void
            GittAtAlleDeltakereHarRegistertPosisjoner_NårEnHenterPosisjonerForLag_SkalBareSistePosisjonerForHvertLagReturneres
            ()
        {
            var givenLag = _given.ATwoTeamWithTwoPlayers();

            var posisjonsSevice = _container.Resolve<PosisjonsRepository>();

            var latitude = 59.6785526164;
            var longitude = 10.6039274298;

            foreach (var lag in givenLag)
            {
                foreach (var deltaker in lag.Deltakere)
                {                  
                    posisjonsSevice.RegistrerPosisjon(deltaker.Lag.LagId, deltaker.DeltakerId, latitude, longitude);
                    TimeService.AddSeconds(200);
                    posisjonsSevice.RegistrerPosisjon(deltaker.Lag.LagId, deltaker.DeltakerId, latitude + 5, longitude + 5);
                }
            }

            var posisjonerForLag1 = posisjonsSevice.HentforLag(givenLag[0].LagId).Posisjoner;
            var posisjonerForLag2 = posisjonsSevice.HentforLag(givenLag[1].LagId).Posisjoner;

            Assert.AreEqual(2, posisjonerForLag1.Count, "Feil antall posisjoner for lag 1");
            Assert.AreEqual(2, posisjonerForLag2.Count, "Feil antall posisjoner for lag 2");            
        }

        [Test]
        public void
            GittAtAlleDeltakereHarRegistertPosisjoner_NårScoreboardHenterPosisjonerForAlleLag_SkalSistePosisjonerForDeltakerePåBeggeLagReturneres
            ()
        {
            var givenLag = _given.ATwoTeamWithTwoPlayers();

            var posisjonsSevice = _container.Resolve<PosisjonsRepository>();

            var latitude = 59.6785526164;
            var longitude = 10.6039274298;

            foreach (var lag in givenLag)
            {
                foreach (var deltaker in lag.Deltakere)
                {
                    posisjonsSevice.RegistrerPosisjon(deltaker.Lag.LagId, deltaker.DeltakerId, latitude, longitude);
                    TimeService.AddSeconds(200);
                    posisjonsSevice.RegistrerPosisjon(deltaker.Lag.LagId, deltaker.DeltakerId, latitude + 5, longitude + 5);
                }
            }
            _appSettings.ScoreboardSecret = "HemmeligAdminKode";
            var lagPosisjoner = posisjonsSevice.HentforAlleLag(_appSettings.ScoreboardSecret);

            Assert.AreEqual(2, lagPosisjoner.Count, "Skulle hatt to lag");

            Assert.AreEqual(2, lagPosisjoner[0].Posisjoner.Count, "Feil antall posisjoner for lag 1");
            Assert.AreEqual(2, lagPosisjoner[1].Posisjoner.Count, "Feil antall posisjoner for lag 2");
        }

        [Test]
        public void NårEnDeltakerPosterNyPosisjonToGangerMedGodTidMellom_SkalSistePosisjonReturneresViaService()
        {
            var given = _given.ATwoTeamWithTwoPlayers();

            var posisjonsSevice = _container.Resolve<PosisjonsRepository>();

            var deltaker11 = given[0].Deltakere[0];
            var latitude = 59.6785526164;
            var longitude = 10.6039274298;

            posisjonsSevice.RegistrerPosisjon(deltaker11.Lag.LagId, deltaker11.DeltakerId, latitude, longitude);
            TimeService.AddSeconds(200);
            posisjonsSevice.RegistrerPosisjon(deltaker11.Lag.LagId, deltaker11.DeltakerId, latitude + 5, longitude + 5);

            var posisjonerForLag1 = posisjonsSevice.HentforLag(deltaker11.Lag.LagId);

            var pos = posisjonerForLag1.Posisjoner.Single();

            Assert.AreEqual(latitude + 5, pos.Latitude, "Feil lat");
            Assert.AreEqual(longitude + 5, pos.Longitude, "Feil lon");
            Assert.AreEqual(deltaker11.DeltakerId, pos.DeltakerId, "feil deltaker");
        }
    }
}

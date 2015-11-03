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
        private Gitt _gitt;
        private DataContextFactory _dataContextFactory;
        private OverridableSettings _appSettings;
       
        [SetUp]
        public void Given()
        {
            _container = RestApiApplication.CreateContainer();

            _appSettings = new OverridableSettings();
            _container.Register(Component.For<OverridableSettings>().Instance(_appSettings).IsDefault().Named(Guid.NewGuid().ToString()));

            _gitt = new Gitt(_container);
            _dataContextFactory = _container.Resolve<DataContextFactory>();
            TimeService.ResetToRealTime();

            // Slett alle posisjoner (blir rullet tilbake i transaksjon uansett)
            using (var context = _dataContextFactory.Create())
            {
                context.DeltakerPosisjoner.RemoveRange(context.DeltakerPosisjoner);
                context.SaveChanges();
            }
        }

        [Test]
        public void NårEnDeltakerPosterEnPosisjon_SkalPosisjonenLagresIDatabasen()
        {
            var givenLag = _gitt.ToLagMedToDeltakere();

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
            var given = _gitt.ToLagMedToDeltakere();

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
        public void NårEnDeltakerStårIRoLenge_SkalPosisjonenLagresIDatabasenBareEnGang()
        {
            var given = _gitt.ToLagMedToDeltakere();

            var posisjonsSevice = _container.Resolve<PosisjonsRepository>();

            var deltaker11 = given[0].Deltakere[0];
            var latitude = 59.6785526164;
            var longitude = 10.6039274298;

            posisjonsSevice.RegistrerPosisjon(deltaker11.Lag.LagId, deltaker11.DeltakerId, latitude, longitude);
            TimeService.AddSeconds(30);
            posisjonsSevice.RegistrerPosisjon(deltaker11.Lag.LagId, deltaker11.DeltakerId, latitude, longitude);
            TimeService.AddSeconds(30);
            posisjonsSevice.RegistrerPosisjon(deltaker11.Lag.LagId, deltaker11.DeltakerId, latitude, longitude);
            TimeService.AddSeconds(30);
            posisjonsSevice.RegistrerPosisjon(deltaker11.Lag.LagId, deltaker11.DeltakerId, latitude, longitude);

            using (var context = _dataContextFactory.Create())
            {
                Assert.AreEqual(1, context.DeltakerPosisjoner.Count(), "Skulle vært 1 posisjon");
            }
        }

        [Test]
        public void NårEnDeltakerFlytterSegMenRegistrererOfte_SkalPosisjonenLagresIDatabasenBareHvert10Sekund()
        {
            var given = _gitt.ToLagMedToDeltakere();

            var posisjonsSevice = _container.Resolve<PosisjonsRepository>();

            var deltaker11 = given[0].Deltakere[0];
            var latitude = 59.6785526164;
            var longitude = 10.6039274298;

            for(int i = 1; i < 100; i++)
            {
                posisjonsSevice.RegistrerPosisjon(deltaker11.Lag.LagId, deltaker11.DeltakerId, latitude + 0.01 * i, longitude + 0.01 * i);
                TimeService.AddSeconds(1);               
            }

            using (var context = _dataContextFactory.Create())
            {
                Assert.AreEqual(10, context.DeltakerPosisjoner.Count(), "Skulle vært 1 posisjon");
            }
        }

        [Test]
        public void NårEnDeltakerPosterNyPosisjonToGangerMenForTettITid_SkalPosisjonenLagresIDatabasenBareEnGang()
        {
            var given = _gitt.ToLagMedToDeltakere();

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
            var given = _gitt.ToLagMedToDeltakere();

            var posisjonsSevice = _container.Resolve<PosisjonsRepository>();

            var deltaker11 = given[0].Deltakere[0];
            var latitude = 59.6785526164;
            var longitude = 10.6039274298;

            posisjonsSevice.RegistrerPosisjon(deltaker11.Lag.LagId, deltaker11.DeltakerId, latitude, longitude);
            TimeService.AddSeconds(200);
            posisjonsSevice.RegistrerPosisjon(deltaker11.Lag.LagId, deltaker11.DeltakerId, latitude + 5, longitude + 5);

            using (var context = _dataContextFactory.Create())
            {
                Assert.AreEqual(2, context.DeltakerPosisjoner.Count(), "Feil antall posisjoner");
            }
        }

        [Test]
        public void
            GittAtAlleDeltakereHarRegistertPosisjoner_NårEnHenterPosisjonerForLag_SkalBareSistePosisjonerForHvertLagReturneres
            ()
        {
            var givenLag = _gitt.ToLagMedToDeltakere();

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
            var givenLag = _gitt.ToLagMedToDeltakere();

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
            var given = _gitt.ToLagMedToDeltakere();

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


        [Test]
        public void NårApplikasjonenRestartes_SkalSistePosisjonerHentesFraDatabasen()
        {
            var givenLag = _gitt.ToLagMedToDeltakere();

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

           
            // Lag ny container for å simulere restart
            _container = RestApiApplication.CreateContainer();

            var posisjonService = _container.Resolve<PosisjonsRepository>();

            var posisjonForLag1 = posisjonService.HentforLag(givenLag[0].LagId);
            Assert.AreEqual(2, posisjonForLag1.Posisjoner.Count, "Feil antall posisjoner etter restart - lag1");

            var posisjonForLag2 = posisjonService.HentforLag(givenLag[1].LagId);
            Assert.AreEqual(2, posisjonForLag2.Posisjoner.Count, "Feil antall posisjoner etter restart - lag2");

            var posisjon = posisjonForLag1.Posisjoner[0];

            Assert.AreEqual(latitude + 5, posisjon.Latitude, "Latitude");
            Assert.AreEqual(longitude + 5, posisjon.Longitude, "Longitude");

        }
    }
}

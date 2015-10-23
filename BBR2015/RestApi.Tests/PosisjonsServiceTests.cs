using System.Linq;
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
       
        [SetUp]
        public void Given()
        {
            _container = RestApiApplication.CreateContainer();
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

            var posisjonerForLag1 = posisjonsSevice.HentforLag(givenLag[0].LagId);
            var posisjonerForLag2 = posisjonsSevice.HentforLag(givenLag[1].LagId);

            Assert.AreEqual(2, posisjonerForLag1.Count, "Feil antall posisjoner for lag 1");
            Assert.AreEqual(2, posisjonerForLag2.Count, "Feil antall posisjoner for lag 2");            
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

            var pos = posisjonerForLag1.Single();

            Assert.AreEqual(latitude + 5, pos.Latitude, "Feil lat");
            Assert.AreEqual(longitude + 5, pos.Longitude, "Feil lon");
            Assert.AreEqual(deltaker11.DeltakerId, pos.DeltakerId, "feil deltaker");
        }
    }
}

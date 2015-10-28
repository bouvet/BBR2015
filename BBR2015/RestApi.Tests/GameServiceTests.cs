using System;
using System.Linq;
using Castle.Windsor;
using Database;
using Database.Entities;
using NUnit.Framework;
using Repository;
using RestApi.Tests.Infrastructure;

namespace RestApi.Tests
{
    public class GameServiceTests : BBR2015DatabaseTests
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
        public void GittMatch_NårEtLagIkkeHarRegistrertNoenPoster_SkalDeIkkeHaNoenPoeng()
        {
            var match = _gitt.EnMatchMedTreLagOgToPoster();

            var lag1 = match.DeltakendeLag.First();
            var gamestateservice = _container.Resolve<GameStateService>();

            var lag1State = gamestateservice.Get(lag1.Lag.LagId);

            Assert.AreEqual(0, lag1State.Score, "Skal ikke ha noen poeng");
            Assert.AreEqual(false, lag1State.Poster.Any(x => x.HarRegistert), "Skal ikke ha noen registreringer");
        }

        [Test]
        public void GittMatch_NårEttLagStemplerPåEnPost_SkalDeFåPoengIFeed()
        {
            var match = _gitt.EnMatchMedTreLagOgToPoster();

            var lag1 = match.DeltakendeLag.First();
            var deltaker11 = lag1.Lag.Deltakere.First();

            var gameservice = _container.Resolve<GameServiceRepository>();
            var gamestateservice = _container.Resolve<GameStateService>();

            gameservice.RegistrerNyPost(deltaker11.DeltakerId, lag1.Lag.LagId, "HemmeligKode1", null);
            
            var lag1State = gamestateservice.Get(lag1.Lag.LagId);

            Assert.AreEqual(100, lag1State.Score, "Skal ha poeng for 1 stempling");
            Assert.AreEqual(1, lag1State.Poster.Count(x => x.HarRegistert), "Skal ha 1 registrering");
        }

        [Test]
        public void GittMatch_NårEttLagStemplerPåEnPostSomIkkeErAktiv_SkalDeIkkeFåPoengIFeed()
        {
            var match = _gitt.EnMatchMedTreLagOgToPoster();

            var lag1 = match.DeltakendeLag.First();
            var deltaker11 = lag1.Lag.Deltakere.First();

            var gameservice = _container.Resolve<GameServiceRepository>();
            var gamestateservice = _container.Resolve<GameStateService>();

            var førstePost = match.Poster.First();

            using (var context = _dataContextFactory.Create())
            {
                var postIMatch = context.PosterIMatch.Single(x => x.Id == førstePost.Id);

                postIMatch.SynligFraUTC = new DateTime(2001, 01, 01);
                postIMatch.SynligTilUTC = new DateTime(2001, 01, 02);
                context.SaveChanges();
            }

            gameservice.RegistrerNyPost(deltaker11.DeltakerId, lag1.Lag.LagId, førstePost.Post.HemmeligKode, null);

            var lag1State = gamestateservice.Get(lag1.Lag.LagId);

            Assert.AreEqual(0, lag1State.Score, "Skal ha poeng for 1 stempling");
            Assert.AreEqual(0, lag1State.Poster.Count(x => x.HarRegistert), "Skal ha 1 registrering");
        }

        [Test]
        public void GittMatch_NårEttLagStemplerToGangerPåSammePost_SkalDeBareFåPoengForFørsteStempling()
        {
            var match = _gitt.EnMatchMedTreLagOgToPoster();

            var lag1 = match.DeltakendeLag.First();
            var deltaker11 = lag1.Lag.Deltakere.First();

            var gameservice = _container.Resolve<GameServiceRepository>();
            var gamestateservice = _container.Resolve<GameStateService>();

            gameservice.RegistrerNyPost(deltaker11.DeltakerId, lag1.Lag.LagId, "HemmeligKode1", null);
            gameservice.RegistrerNyPost(deltaker11.DeltakerId, lag1.Lag.LagId, "HemmeligKode1", null);

            var lag1State = gamestateservice.Get(lag1.Lag.LagId);

            Assert.AreEqual(100, lag1State.Score, "Skal ha poeng for 1 stempling");
            Assert.AreEqual(1, lag1State.Poster.Count(x => x.HarRegistert), "Skal ha 1 registrering");
        }

        [Test]
        public void GittMatch_NårEttLagStemplerSomAndreLagPåEnPost_SkalDeFåPoengBeregnetForStempling2()
        {
            var match = _gitt.EnMatchMedTreLagOgToPoster();

            var lag1 = match.DeltakendeLag.First();
            var deltaker11 = lag1.Lag.Deltakere.First();

            var lag2 = match.DeltakendeLag[1];
            var deltaker21 = lag2.Lag.Deltakere.First();

            var gameservice = _container.Resolve<GameServiceRepository>();
            var gamestateservice = _container.Resolve<GameStateService>();

            gameservice.RegistrerNyPost(deltaker11.DeltakerId, lag1.Lag.LagId, "HemmeligKode1", null);
            gameservice.RegistrerNyPost(deltaker21.DeltakerId, lag2.Lag.LagId, "HemmeligKode1", null);

            var lag2State = gamestateservice.Get(lag2.Lag.LagId);

            Assert.AreEqual(80, lag2State.Score, "Skal ha poeng for 2. stempling");
            Assert.AreEqual(1, lag2State.Poster.Count(x => x.HarRegistert), "Skal ha 1 registrering");
        }
    }
}
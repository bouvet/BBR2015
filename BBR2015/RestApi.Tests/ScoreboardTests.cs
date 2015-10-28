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
    public class ScoreboardTests : BBR2015DatabaseTests
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
        public void GittMatch_VedOppstart_SkalScoreboardHaInitiellTilstand()
        {
            _gitt.EnMatchMedTreLagOgTrePoster();

            var gamestateservice = _container.Resolve<GameStateService>();

            var scoreboard = gamestateservice.GetScoreboard();

            Assert.AreEqual(3, scoreboard.Poster.Count, "Poster");
            Assert.IsTrue(scoreboard.Poster.All(x => x.Verdi == 100), "Poster skal ha initiell verdi");
            Assert.AreEqual(3, scoreboard.Lag.Count, "Lag");
            Assert.AreEqual(0, scoreboard.Deltakere.Count, "Skal ikke være noen deltakere med poeng");
        }

        [Test]
        public void GittMatch_NårEnPostErSkjult_SkalPostenHaSynligFlaggSlåttAv()
        {
            var match = _gitt.EnMatchMedTreLagOgTrePoster();

            var gamestateservice = _container.Resolve<GameStateService>();

            var scoreboard = gamestateservice.GetScoreboard();

            Assert.AreEqual(0, scoreboard.Poster.Count(x => x.ErSynlig == false), "Skjulte poster");

            var førstePost = match.Poster.First();
            DisablePostIMatch(førstePost);
            gamestateservice.Calculate();

            scoreboard = gamestateservice.GetScoreboard();

            Assert.AreEqual(false, scoreboard.Poster.Single(x => x.Navn == førstePost.Post.Navn).ErSynlig, "Post skulle blitt usynlig");
        }


        private void DisablePostIMatch(PostIMatch førstePost)
        {
            using (var context = _dataContextFactory.Create())
            {
                var postIMatch = context.PosterIMatch
                                        .Include(x => x.Post)
                                        .Include(x => x.Match)
                                        .Single(x => x.Id == førstePost.Id);

                postIMatch.SynligFraUTC = new DateTime(2001, 01, 01);
                postIMatch.SynligTilUTC = new DateTime(2001, 01, 02);
                context.SaveChanges();
            }
        }

        [Test]
        public void GittMatch_NårEttLagStemplerPåEnPost_SkalDeFåPoengIFeed()
        {
            var match = _gitt.EnMatchMedTreLagOgTrePoster();

            var lag1 = match.DeltakendeLag.First();
            var deltaker11 = lag1.Lag.Deltakere.First();

            var gameservice = _container.Resolve<GameServiceRepository>();
            var gamestateservice = _container.Resolve<GameStateService>();

            gameservice.RegistrerNyPost(deltaker11.DeltakerId, lag1.Lag.LagId, "HemmeligKode1", null);

            var scoreboard = gamestateservice.GetScoreboard();

            var post = scoreboard.Poster.Single(x => x.Navn == "Post 1");

            Assert.AreEqual(80, post.Verdi, "Skulle fått lavere verdi");
            Assert.AreEqual(1, post.AntallRegistreringer, "Antall registreringer");

            Assert.AreEqual(1, scoreboard.Deltakere.Count, "Antall deltakere med score");
            Assert.AreEqual(1, scoreboard.Deltakere.Single().AntallRegistreringer, "Antall deltakere med score");
            Assert.AreEqual(1, scoreboard.Deltakere.Single().MostValueablePlayerRanking, "Ranking");
            Assert.AreEqual(100, scoreboard.Deltakere.Single().Score, "Score");
            Assert.AreEqual("Deltaker1-1", scoreboard.Deltakere.Single().DeltakerId, "DeltakerId");
            Assert.AreEqual("DeltakerNavn1-1", scoreboard.Deltakere.Single().Navn, "DeltakerNavn");

            Assert.AreEqual(1, scoreboard.Lag.Count(x => x.Ranking == 1), "Bare ett lag skal lede");
            Assert.AreEqual(2, scoreboard.Lag.Count(x => x.Ranking == 2), "Lag med samme poengsum skal ha samme rank");
        }
    }
}
using System;
using System.Linq;
using Castle.Core.Internal;
using Castle.Windsor;
using Database;
using Database.Entities;
using NUnit.Framework;
using Repository;
using RestApi.Tests.Infrastructure;
using Constants = Database.Constants;

namespace RestApi.Tests
{
    public class GameStateTests : BBR2015DatabaseTests
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
                context.Achievements.Clear();
                context.Meldinger.Clear();
                context.SaveChanges();
            }
        }

        [Test]
        public void GittMatch_NårEtLagIkkeHarRegistrertNoenPoster_SkalDeIkkeHaNoenPoeng()
        {
            var match = _gitt.EnMatchMedTreLagOgTrePoster();

            var lag1 = match.DeltakendeLag.First();
            var gamestateservice = _container.Resolve<GameStateService>();

            var lag1State = gamestateservice.Get(lag1.Lag.LagId);

            Assert.AreEqual(0, lag1State.Score, "Skal ikke ha noen poeng");
            Assert.AreEqual(false, lag1State.Poster.Any(x => x.HarRegistert), "Skal ikke ha noen registreringer");
        }

        [Test]
        public void GittMatch_IngenPosterErFramITid_SkalIkkeKræsje()
        {
            var match = _gitt.EnMatchMedTreLagOgTrePoster();

            using (var context = _dataContextFactory.Create())
            {
                context.PosterIMatch
                       .Include(x => x.Post)
                       .Include(x => x.Match)
                       .ForEach(x =>
                {
                    x.SynligFraTid = TimeService.Now.Subtract(new TimeSpan(0, 60, 0));
                    x.SynligTilTid = TimeService.Now.Subtract(new TimeSpan(0, 30, 0));
                });

                context.SaveChanges();
            }

            var lag1 = match.DeltakendeLag.First();
            var gamestateservice = _container.Resolve<GameStateService>();

            var lag1State = gamestateservice.Get(lag1.Lag.LagId);

            Assert.AreEqual(0, lag1State.Score, "Skal ikke ha noen poeng");
            Assert.AreEqual(false, lag1State.Poster.Any(x => x.HarRegistert), "Skal ikke ha noen registreringer");
        }

        [Test]
        public void GittMatch_NårEttLagStemplerPåEnPost_SkalDeFåPoengIFeed()
        {
            var match = _gitt.EnMatchMedTreLagOgTrePoster();

            var lag1 = match.DeltakendeLag.First();
            var deltaker11 = lag1.Lag.Deltakere.First();

            var gameservice = _container.Resolve<GameService>();
            var gamestateservice = _container.Resolve<GameStateService>();

            gameservice.RegistrerNyPost(deltaker11.DeltakerId, lag1.Lag.LagId, "HemmeligKode1", null);

            var lag1State = gamestateservice.Get(lag1.Lag.LagId);

            Assert.AreEqual(100, lag1State.Score, "Skal ha poeng for 1 stempling");
            Assert.AreEqual(1, lag1State.Poster.Count(x => x.HarRegistert), "Skal ha 1 registrering");
        }

        [Test]
        public void GittMatch_NårEttLagStemplerPåEnPost_SkalDeIkkeMisteVåpen()
        {
            var match = _gitt.EnMatchMedTreLagOgTrePoster();

            var lag1 = match.DeltakendeLag.First();
            var deltaker11 = lag1.Lag.Deltakere.First();

            var gameservice = _container.Resolve<GameService>();
            var gamestateservice = _container.Resolve<GameStateService>();

            gameservice.RegistrerNyPost(deltaker11.DeltakerId, lag1.Lag.LagId, "HemmeligKode1", null);

            var lag1State = gamestateservice.Get(lag1.Lag.LagId);

            Assert.AreEqual(2, lag1State.Vaapen.Count, "Skal ikke miste våpen");
        }

        [Test]
        public void GittMatch_NårEttLagStemplerPåEnPostSomIkkeErAktiv_SkalDeIkkeFåPoengIFeed()
        {
            var match = _gitt.EnMatchMedTreLagOgTrePoster();

            var lag1 = match.DeltakendeLag.First();
            var deltaker11 = lag1.Lag.Deltakere.First();

            var gameservice = _container.Resolve<GameService>();
            var gamestateservice = _container.Resolve<GameStateService>();

            var førstePost = match.Poster.First();

            DisablePostIMatch(førstePost);

            gameservice.RegistrerNyPost(deltaker11.DeltakerId, lag1.Lag.LagId, førstePost.Post.HemmeligKode, null);

            var lag1State = gamestateservice.Get(lag1.Lag.LagId);

            Assert.AreEqual(0, lag1State.Score, "Skal ha poeng for 1 stempling");
            Assert.AreEqual(0, lag1State.Poster.Count(x => x.HarRegistert), "Skal ha 1 registrering");
        }

        private void DisablePostIMatch(PostIMatch førstePost)
        {
            using (var context = _dataContextFactory.Create())
            {
                var postIMatch = context.PosterIMatch
                                        .Include(x => x.Post)
                                        .Include(x => x.Match)
                                        .Single(x => x.Id == førstePost.Id);

                postIMatch.SynligFraTid = new DateTime(2001, 01, 01);
                postIMatch.SynligTilTid = new DateTime(2001, 01, 02);
                context.SaveChanges();
            }
        }

        [Test]
        public void GittMatch_EnPostSomIkkeErAktiv_SkalIkkeVisesIFeed()
        {
            var match = _gitt.EnMatchMedTreLagOgTrePoster();
            var lag1 = match.DeltakendeLag.First();
            var gamestateservice = _container.Resolve<GameStateService>();

            var førstePost = match.Poster.First();

            DisablePostIMatch(førstePost);

            gamestateservice.Calculate();

            var lag1State = gamestateservice.Get(lag1.Lag.LagId);

            Assert.AreEqual(2, lag1State.Poster.Count, "Skal bare se to poster (aktive)");
        }

        [Test]
        public void GittMatch_NårEttLagStemplerToGangerPåSammePost_SkalDeBareFåPoengForFørsteStempling()
        {
            var match = _gitt.EnMatchMedTreLagOgTrePoster();

            var lag1 = match.DeltakendeLag.First();
            var deltaker11 = lag1.Lag.Deltakere.First();

            var gameservice = _container.Resolve<GameService>();
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
            var match = _gitt.EnMatchMedTreLagOgTrePoster();

            var lag1 = match.DeltakendeLag.First();
            var deltaker11 = lag1.Lag.Deltakere.First();

            var lag2 = match.DeltakendeLag[1];
            var deltaker21 = lag2.Lag.Deltakere.First();

            var gameservice = _container.Resolve<GameService>();
            var gamestateservice = _container.Resolve<GameStateService>();

            gameservice.RegistrerNyPost(deltaker11.DeltakerId, lag1.Lag.LagId, "HemmeligKode1", null);
            gameservice.RegistrerNyPost(deltaker21.DeltakerId, lag2.Lag.LagId, "HemmeligKode1", null);

            var lag2State = gamestateservice.Get(lag2.Lag.LagId);

            Assert.AreEqual(80, lag2State.Score, "Skal ha poeng for 2. stempling");
            Assert.AreEqual(1, lag2State.Poster.Count(x => x.HarRegistert), "Skal ha 1 registrering");
        }

        [Test]
        public void GittMatch_NårToLagStemplerOgHarSammePoengsum_SkalDeFåLikRangeringOgSisteLagSkalBliBakerst()
        {
            var match = _gitt.EnMatchMedTreLagOgTrePoster();

            var lag1 = match.DeltakendeLag.First();
            var deltaker11 = lag1.Lag.Deltakere.First();

            var lag2 = match.DeltakendeLag[1];
            var deltaker21 = lag2.Lag.Deltakere.First();

            var lag3 = match.DeltakendeLag[2];

            var gameservice = _container.Resolve<GameService>();
            var gamestateservice = _container.Resolve<GameStateService>();

            gameservice.RegistrerNyPost(deltaker11.DeltakerId, lag1.Lag.LagId, "HemmeligKode1", null);
            gameservice.RegistrerNyPost(deltaker21.DeltakerId, lag2.Lag.LagId, "HemmeligKode2", null);

            var lag1State = gamestateservice.Get(lag1.Lag.LagId);
            var lag2State = gamestateservice.Get(lag2.Lag.LagId);
            var lag3State = gamestateservice.Get(lag3.Lag.LagId);

            Assert.AreEqual(1, lag1State.Ranking.Rank, "1: Skal lede");
            Assert.AreEqual(0, lag1State.Ranking.PoengBakLagetForan, "1: Poeng bak");
            Assert.AreEqual(100, lag1State.Ranking.PoengForanLagetBak, "1: Poeng foran");

            Assert.AreEqual(1, lag2State.Ranking.Rank, "2: Skal lede");
            Assert.AreEqual(0, lag2State.Ranking.PoengBakLagetForan, "2: Poeng bak");
            Assert.AreEqual(100, lag2State.Ranking.PoengForanLagetBak, "2: Poeng foran");

            Assert.AreEqual(3, lag3State.Ranking.Rank, "3: Skal være sist");
            Assert.AreEqual(100, lag3State.Ranking.PoengBakLagetForan, "3: Poeng bak");
            Assert.AreEqual(0, lag3State.Ranking.PoengForanLagetBak, "3: Poeng foran");


        }

        [Test]
        public void LagSkalHaInitiellVåpenBeholdningIGamestate()
        {
            var match = _gitt.EnMatchMedTreLagOgTrePoster();

            var lag1 = match.DeltakendeLag.First();

            var gamestateservice = _container.Resolve<GameStateService>();

            var lag1State = gamestateservice.Get(lag1.Lag.LagId);
            Assert.AreEqual(2, lag1State.Vaapen.Count(), "Skal ha 2 våpen");
        }

        [Test]
        public void GittAktivPost_NårEtLagBrukerBombe_SkalLagetFåPoengOgPostenBliUsynligIEtAntallMinutterSelvOmStateBlirRekalkulert()
        {
            var match = _gitt.EnMatchMedTreLagOgTrePoster();

            var lag1 = match.DeltakendeLag.First();
            var deltaker11 = lag1.Lag.Deltakere.First();

            var gameservice = _container.Resolve<GameService>();
            var gamestateservice = _container.Resolve<GameStateService>();

            var lag1State = gamestateservice.Get(lag1.Lag.LagId);
            Assert.AreEqual(1, lag1State.Vaapen.Count(x => x.VaapenId == Constants.Våpen.Bombe), "Skal ha 1 bombe");

            gameservice.RegistrerNyPost(deltaker11.DeltakerId, lag1.Lag.LagId, "HemmeligKode1", Constants.Våpen.Bombe);

            lag1State = gamestateservice.Get(lag1.Lag.LagId);
            Assert.AreEqual(100, lag1State.Score, "Skal ha fått poeng");
            Assert.AreEqual(2, lag1State.Poster.Count, "Skal bare se to poster (aktive)");
            Assert.AreEqual(0, lag1State.Vaapen.Count(x => x.VaapenId == Constants.Våpen.Bombe), "Skal ha brukt opp bomben");
            Assert.AreEqual(1, lag1State.Vaapen.Count(x => x.VaapenId == Constants.Våpen.Felle), "Skal fremdeles ha fellen");

            gamestateservice.Calculate();

            TimeService.AddSeconds(Constants.Våpen.BombeSkjulerPostIAntallSekunder + 5);

            lag1State = gamestateservice.Get(lag1.Lag.LagId);

            Assert.AreEqual(3, lag1State.Poster.Count, "Post skal bli synlig igjen");

           
        }

        [Test]
        public void GittAktivPost_NårEtLagBrukerFelle_SkalLagetFåPoengOgPostenForbliSynlig_VedNesteRegistreringSkalRegistrerendeLagFåFratrekkAvPoeng_OgPostenSkalBliUsynligEnPeriode()
        {
            var match = _gitt.EnMatchMedTreLagOgTrePoster();

            var lag1 = match.DeltakendeLag.First();
            var deltaker11 = lag1.Lag.Deltakere.First();

            var gameservice = _container.Resolve<GameService>();
            var gamestateservice = _container.Resolve<GameStateService>();

            var lag1State = gamestateservice.Get(lag1.Lag.LagId);
            Assert.AreEqual(1, lag1State.Vaapen.Count(x => x.VaapenId == Constants.Våpen.Felle), "Skal ha 1 felle");

            gameservice.RegistrerNyPost(deltaker11.DeltakerId, lag1.Lag.LagId, "HemmeligKode1", Constants.Våpen.Felle);

            lag1State = gamestateservice.Get(lag1.Lag.LagId);
            Assert.AreEqual(100, lag1State.Score, "Skal ha fått poeng");
            Assert.AreEqual(3, lag1State.Poster.Count, "Skal bare se alle poster (aktive)");
            Assert.AreEqual(0, lag1State.Vaapen.Count(x => x.VaapenId == Constants.Våpen.Felle), "Skal ha brukt opp fellen");

            var lag2 = match.DeltakendeLag[1];
            var deltaker21 = lag2.Lag.Deltakere.First();

            var lag2State = gamestateservice.Get(lag2.Lag.LagId);
            gameservice.RegistrerNyPost(deltaker21.DeltakerId, lag2.Lag.LagId, "HemmeligKode1", null);
            var lag2StateEtterRegistrering = gamestateservice.Get(lag2.Lag.LagId);

            Assert.AreEqual(lag2State.Score - 80, lag2StateEtterRegistrering.Score, "Skal ha fått fratrekk i score");


            TimeService.AddSeconds(Constants.Våpen.BombeSkjulerPostIAntallSekunder + 5);

            lag1State = gamestateservice.Get(lag1.Lag.LagId);

            Assert.AreEqual(3, lag1State.Poster.Count, "Post skal bli synlig igjen");

            var meldingService = _container.Resolve<MeldingService>();
            Assert.AreEqual(3, meldingService.HentMeldinger(lag1.Lag.LagId).Count(), "Laget som rigget fellen skulle fått melding");
            Assert.AreEqual(2, meldingService.HentMeldinger(lag2.Lag.LagId).Count(), "Laget gikk i fellen skulle fått melding");
        }

        [Test]
        public void GittAktivPostSomErSattOppMedFelle_NårLagetSomStemplerBrukerEtVåpenSkalVåpenetIkkeBliSattoppOgLagetIkkeFåFratrekkIVåpenbeholdningen()
        {
            var match = _gitt.EnMatchMedTreLagOgTrePoster();

            var lag1 = match.DeltakendeLag.First();
            var deltaker11 = lag1.Lag.Deltakere.First();

            var gameservice = _container.Resolve<GameService>();
            var gamestateservice = _container.Resolve<GameStateService>();

            var lag1State = gamestateservice.Get(lag1.Lag.LagId);
            Assert.AreEqual(1, lag1State.Vaapen.Count(x => x.VaapenId == Constants.Våpen.Felle), "Skal ha 1 felle");

            gameservice.RegistrerNyPost(deltaker11.DeltakerId, lag1.Lag.LagId, "HemmeligKode1", Constants.Våpen.Felle);

           
            var lag2 = match.DeltakendeLag[1];
            var deltaker21 = lag2.Lag.Deltakere.First();

            
            gameservice.RegistrerNyPost(deltaker21.DeltakerId, lag2.Lag.LagId, "HemmeligKode1", Constants.Våpen.Felle);
            var lag2StateEtterRegistrering = gamestateservice.Get(lag2.Lag.LagId);

            Assert.AreEqual(1, lag2StateEtterRegistrering.Vaapen.Count(x => x.VaapenId == Constants.Våpen.Felle), "Skal ha 1 felle");
            using (var context = _dataContextFactory.Create())
            {
                var postIMatch = (from p in context.PosterIMatch
                    where p.Post.HemmeligKode == "HemmeligKode1"
                    select p).Single();

                Assert.IsNull(postIMatch.RiggetVåpen, "Våpenet som ble forsøkt brukt da fellen gikk av, skal ikke ha blitt satt opp");
            }
        }

        [Test]
        public void GittAktivPost_NårEtLagBrukerBombeOgNullstillerEtterPå_SkalLagetMistePoengOgFåTilbakeVåpen()
        {
            var match = _gitt.EnMatchMedTreLagOgTrePoster();

            var lag1 = match.DeltakendeLag.First();
            var deltaker11 = lag1.Lag.Deltakere.First();

            var gameservice = _container.Resolve<GameService>();
            var gamestateservice = _container.Resolve<GameStateService>();

            var lag1State = gamestateservice.Get(lag1.Lag.LagId);
            Assert.AreEqual(1, lag1State.Vaapen.Count(x => x.VaapenId == Constants.Våpen.Bombe), "Skal ha 1 bombe");

            gameservice.RegistrerNyPost(deltaker11.DeltakerId, lag1.Lag.LagId, "HemmeligKode1", Constants.Våpen.Bombe);

            gameservice.Nullstill(lag1.Lag.LagId);

            lag1State = gamestateservice.Get(lag1.Lag.LagId);
            Assert.AreEqual(0, lag1State.Score, "Nullstilt lagets poeng");
            Assert.AreEqual(0, lag1State.Poster.Count(x => x.HarRegistert), "Ingen poster registert");
            Assert.AreEqual(1, lag1State.Vaapen.Count(x => x.VaapenId == Constants.Våpen.Bombe), "Skal ha fått våpen tilbake");
        }
    }
}
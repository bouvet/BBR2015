using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Windsor;
using Database;
using Database.Entities;
using NUnit.Framework;
using RestApi.Tests.Infrastructure;

namespace RestApi.Tests.Admin
{
    [TestFixture]
    [Explicit("Kjøres manuelt for å sette opp spillet")]
    public class Oppsett
    {
        private IWindsorContainer _container;
        private DataContextFactory _dataContextFactory;

        [TestFixtureSetUp]
        public void EnsureDatabase()
        {
            using (var context = RestApiApplication.CreateContainer().Resolve<DataContextFactory>().Create())
            {
                var triggerCreateDatabase = context.Lag.Any();
            }
        }

        [SetUp]
        public void Setup()
        {
            _container = RestApiApplication.CreateContainer();
            _dataContextFactory = _container.Resolve<DataContextFactory>();
        }

        [Test]
        [Explicit("Bare hvis du virkelig vet hva du gjør!")]
        [Ignore("Må kjøres HELT separat")]
        public void Tøm_Databasen()
        {
            using (var context = _dataContextFactory.Create())
            {
                context.VåpenBeholdning.Clear();
                context.PostRegisteringer.Clear();
                context.LagIMatch.Clear();
                context.PosterIMatch.Clear();
                context.Matcher.Clear();
                context.DeltakerPosisjoner.Clear();
                context.Meldinger.Clear();
                context.Deltakere.Clear();
                context.Lag.Clear();
                context.Våpen.Clear();
                context.Poster.Clear();

                context.SaveChanges();
            }
        }

        [Test]
        public void Opprett_Våpen()
        {
            var bombe = new Vaapen { VaapenId = Constants.Våpen.Bombe, Beskrivelse = "Sprenger posten for en tid" };
            var felle = new Vaapen { VaapenId = Constants.Våpen.Felle, Beskrivelse = "Sprenger posten ved neste stempling. Laget som stempler får ikke poeng." };

            var alle = new[] { bombe, felle };

            using (var context = _dataContextFactory.Create())
            {
                if (context.Våpen.Count() == alle.Length)
                    return;

                context.Våpen.AddRange(alle);

                context.SaveChanges();
            }
        }

        [Test]
        public void Opprett_testspill_før_BBR()
        {
            Opprett_Arrangørlag();

            using (var context = _dataContextFactory.Create())
            {
                var match = new Match
                {
                    MatchId = Guid.NewGuid(),
                    Navn = "Utvikling",
                    StartTid = new DateTime(2015, 11, 01, 10, 00, 00),
                    SluttTid = new DateTime(2015, 11, 06, 10, 00, 00)
                };

                var lag = context.Lag.Single(x => x.LagId == "SUPPORT_1");
                var deltakelse = match.LeggTil(lag);

                var våpen = context.Våpen.ToList();

                deltakelse.LeggTilVåpen(våpen[0]);
                deltakelse.LeggTilVåpen(våpen[1]);

                foreach (var post in new PostFactory().Les(Constants.Område.Oscarsborg))
                {
                    post.HemmeligKode = post.Navn.Replace(" ", string.Empty);
                    post.Navn = "Test" + post.Navn;
                    post.Omraade = "Testrunde";
                    
                    context.Poster.Add(post);

                    var postIMatch = new PostIMatch
                    {
                        Match = match,
                        Post = post,
                        PoengArray = post.DefaultPoengArray,
                        SynligFraTid = match.StartTid,
                        SynligTilTid = match.SluttTid
                    };

                    match.Poster.Add(postIMatch);
                }
            }

        }

        [Test]
        public void Opprett_Arrangørlag()
        {
            using (var context = _dataContextFactory.Create())
            {
                if(context.Lag.Any(x => x.Navn.StartsWith("SUPPORT_")))
                    return;
                
                var lag = LagFactory.SettOppLagMedDeltakere(1, 5, "SUPPORT_");

                context.Lag.AddRange(lag);
                context.SaveChanges();
            }
        }

        public void Opprett_lagForHelga()
        {
            var lag = LagFactory.SettOppLagMedDeltakere(10, 4, "REELT_LAG");

            using (var context = _dataContextFactory.Create())
            {
                context.Lag.AddRange(lag);
                context.SaveChanges();
            }
        }

        [Test]
        public void Opprett_spill_fredag()
        {
            using (var context = _dataContextFactory.Create())
            {
                var match = new Match()
                {
                    MatchId = Guid.NewGuid(),
                    Navn = "Treningsrunde fredag",
                    StartTid = new DateTime(2015, 11, 06, 10, 00, 00),
                    SluttTid = new DateTime(2015, 11, 07, 10, 00, 00)
                };
            }

        }

        [Test]
        public void Opprett_spill_lørdag()
        {
            using (var context = _dataContextFactory.Create())
            {
                var match = new Match()
                {
                    MatchId = Guid.NewGuid(),
                    Navn = "Bouvet Battle Royale 2015",
                    StartTid = new DateTime(2015, 11, 07, 10, 00, 00),
                    SluttTid = new DateTime(2015, 11, 07, 18, 00, 00)
                };
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Components.DictionaryAdapter;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Database;
using Database.Entities;
using Repository;
using RestApi.Tests.Infrastructure;

namespace RestApi.Tests
{
    public class Gitt
    {
        private IWindsorContainer _container;

        private DataContextFactory _dataContextFactory;

        public Gitt(IWindsorContainer _container)
        {
            this._container = _container;
            _dataContextFactory = _container.Resolve<DataContextFactory>();           
        }

        public List<Lag> ToLagMedToDeltakere()
        {
            using (var context = _dataContextFactory.Create())
            {
                var lag1 = LagFactory.SettOppEtLagMedDeltakere(1, 2);
                var lag2 = LagFactory.SettOppEtLagMedDeltakere(2, 2);
                context.Lag.Add(lag1);
                context.Lag.Add(lag2);

                context.SaveChanges();

                return new List<Lag> {lag1, lag2};
            }
        }

        public Match EnMatchMedTreLagOgTrePoster()
        {
            using (var context = _dataContextFactory.Create())
            {
                var lag1 = LagFactory.SettOppEtLagMedDeltakere(1, 2);
                var lag2 = LagFactory.SettOppEtLagMedDeltakere(2, 2);
                var lag3 = LagFactory.SettOppEtLagMedDeltakere(3, 2);
                context.Lag.Add(lag1);
                context.Lag.Add(lag2);
                context.Lag.Add(lag3);

                var alleLag = new List<Lag> { lag1, lag2, lag3 };

                var match = new Match
                {
                    MatchId = Guid.NewGuid(),
                    Navn = "Unit Test Match",
                    StartUTC = new DateTime(2015, 10, 01),
                    SluttUTC = new DateTime(2015, 11, 01)
                };

                context.Matcher.Add(match);

                if (!context.Våpen.Any())
                {
                    context.Våpen.Add(new Vaapen { VaapenId = Constants.Våpen.Bombe, Beskrivelse = "Sprenger posten for en tid" });
                    context.Våpen.Add(new Vaapen { VaapenId = Constants.Våpen.Felle, Beskrivelse = "Sprenger posten ved neste stempling. Laget som stempler får ikke poeng." });
                    context.SaveChanges();
                }

                var alleVåpen = context.Våpen.ToList();

                var felle = alleVåpen.Single(x => x.VaapenId == Constants.Våpen.Felle);
                var bombe = alleVåpen.Single(x => x.VaapenId == Constants.Våpen.Bombe);

                foreach (var l in alleLag)
                {
                    var lagIMatch = new LagIMatch
                    {
                        Lag = l,
                        Match = match
                    };

                    lagIMatch.VåpenBeholdning.Add(new VaapenBeholdning { LagIMatch = lagIMatch, Våpen = felle });
                    lagIMatch.VåpenBeholdning.Add(new VaapenBeholdning { LagIMatch = lagIMatch, Våpen = bombe });

                    match.LeggTil(lagIMatch);
                }

                foreach (var post in HentTestPoster(3))
                {
                    context.Poster.Add(post);

                    var postIMatch = new PostIMatch
                    {
                        Match = match,
                        Post = post,
                        PoengArray = post.DefaultPoengArray,
                        SynligFraUTC = match.StartUTC,
                        SynligTilUTC = match.SluttUTC
                    };

                    match.Poster.Add(postIMatch);
                }

                context.SaveChanges();


                OverrideMatchId(match);

                return match;
            }
        }

        private void OverrideMatchId(Match match)
        {
            var hardcodedMatchProvider = new HardcodedMatchProvider(null);
            hardcodedMatchProvider.SetMatchId(match.MatchId);
            _container.Register(
                Component.For<CurrentMatchProvider>()
                    .Instance(hardcodedMatchProvider)
                    .Named(Guid.NewGuid().ToString())
                    .IsDefault());
        }

        private List<Post> HentTestPoster(int antall)
        {
            var defaultPoeng = "100,80";

            var latitude = 59.93719;
            var longitude = 10.75797;

            var resultat = new List<Post>();
            for (var i = 1; i <= antall; i++)
            {               
                var post = new Post
                {
                    PostId = Guid.NewGuid(),
                    Navn = string.Format("Post {0}", i),
                    Beskrivelse = "Beskrivelse",
                    Latitude = latitude + (0.02 * i),
                    Longitude = longitude + (0.02 * i),
                    Altitude = 0.0,
                    Image = "image.jpg",
                    DefaultPoengArray = defaultPoeng,
                    HemmeligKode = "HemmeligKode" + i,
                    Omraade = "UNIT TEST"
                };
                resultat.Add(post);
            }

            return resultat;
        }
       
    }

    public class HardcodedMatchProvider : CurrentMatchProvider
    {
        public HardcodedMatchProvider(DataContextFactory dataContextFactory) : base(dataContextFactory)
        {
        }

        private Guid _matchId;

        public void SetMatchId(Guid matchId)
        {
            _matchId = matchId;
        }

        public override Guid GetMatchId()
        {
            return _matchId;
        }
    }
}

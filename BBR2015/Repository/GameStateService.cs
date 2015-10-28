using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using Database;
using Database.Entities;

namespace Repository
{
    public class GameStateService
    {
        private readonly DataContextFactory _dataContextFactory;
        private readonly CurrentMatchProvider _currentMatchProvider;

        private Dictionary<string, GameStateForLag> _gamestates = new Dictionary<string, GameStateForLag>();

        public GameStateService(DataContextFactory dataContextFactory, CurrentMatchProvider currentMatchProvider)
        {
            _dataContextFactory = dataContextFactory;
            _currentMatchProvider = currentMatchProvider;

            Calculate();
        }

        public void Calculate()
        {
            var random = new Random();

            var matchId = _currentMatchProvider.GetMatchId();

            using (var context = _dataContextFactory.Create())
            {
                var sorterteLag =
                    context.LagIMatch.Include(x => x.Lag)
                           .Where(x => x.Match.MatchId == matchId)
                           .OrderByDescending(x => x.PoengSum)
                           .ToArray();

                var poster = (from p in context.PosterIMatch
                              where p.Match.MatchId == matchId && p.SynligFraUTC < TimeService.Now && TimeService.Now < p.SynligTilUTC
                              select new TempPost { PostId = p.Post.PostId, Latitude = p.Post.Latitude, Longitude = p.Post.Longitude, CurrentPoengIndex = p.CurrentPoengIndex, PoengArray = p.PoengArray }).ToList();

                var postRegistreringer = (from l in context.LagIMatch
                                          from p in l.PostRegistreringer
                                          where l.Match.MatchId == matchId
                                          select new
                                          {
                                              PostId = p.RegistertPost.Post.PostId,
                                              LagIMatchId = p.RegistertForLag.Id,
                                              Poeng = p.PoengForRegistrering
                                          }).ToList();


                var nyGameState = new Dictionary<string, GameStateForLag>();

                for (int i = 0; i < sorterteLag.Length; i++)
                {
                    var lag = sorterteLag[i];

                    LagIMatch plassenForan = i == 0 ? null : sorterteLag[i - 1];
                    LagIMatch plassenBak = i == sorterteLag.Length - 1 ? null : sorterteLag[i + 1];

                    var state = new GameStateForLag
                    {
                        LagId = lag.Lag.LagId,
                        LagNavn = lag.Lag.Navn,
                        LagFarge = lag.Lag.Farge,
                        LagIkon = lag.Lag.Ikon,
                        Score = lag.PoengSum,
                        Ranking = new GameStateRanking
                        {
                            Rank = i + 1,
                            PoengBakLagetForan = (plassenForan ?? lag).PoengSum - lag.PoengSum,
                            PoengForanLagetBak = lag.PoengSum - (plassenBak ?? lag).PoengSum,
                        },
                        Poster = (from p in poster
                                  join r in postRegistreringer.Where(x => x.LagIMatchId == lag.Id) on p.PostId equals r.PostId into j
                                  from reg in j.DefaultIfEmpty()
                                  select new GameStatePost
                                  {
                                      Latitude = p.Latitude,
                                      Longitude = p.Longitude,
                                      PoengVerdi = reg != null ? reg.Poeng : PostIMatch.BeregnPoengForNesteRegistrering(p.PoengArray, p.CurrentPoengIndex),
                                      HarRegistert = reg != null,
                                      Rekkefølge = random.Next(0, short.MaxValue)    // order by random                                 
                                  }).OrderBy(x => x.Rekkefølge).ToList(),
                        Vaapen = lag.VåpenBeholdning.Select(x => new GameStateVaapen
                        {
                            VaapenId = x.VaapenId
                        }).ToList()
                    };

                    nyGameState.Add(state.LagId, state);
                }

                // swap current state
                _gamestates = nyGameState;
            }

        }

        public GameStateForLag Get(string lagId)
        {
            return _gamestates[lagId];
        }
    }

    public class TempPost
    {
        public Guid PostId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int CurrentPoengIndex { get; set; }
        public string PoengArray { get; set; }
    }

    public class GameStateForLag
    {
        public string LagNavn { get; set; }
        public GameStateForLag()
        {
            Poster = new List<GameStatePost>();
        }
        public List<GameStatePost> Poster { get; set; }

        public int Score { get; set; }
        public List<GameStateVaapen> Vaapen { get; set; }
        public GameStateRanking Ranking { get; set; }
        public string LagFarge { get; set; }
        public string LagIkon { get; set; }
        public string LagId { get; set; }
    }

    public class GameStateRanking
    {
        public int Rank { get; set; }
        public int PoengForanLagetBak { get; set; }
        public int PoengBakLagetForan { get; set; }
    }
    public class GameStateVaapen
    {
        public string VaapenId { get; set; }
        public string Beskrivelse { get; set; }
    }

    public class GameStatePost
    {
        // NB: IKKE gi ut postnr. Lagene må finne en egen måte å referere postene på. Gjerne miks rekkefølgen på dem i retur.
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool HarRegistert { get; set; }
        public int PoengVerdi { get; set; }

        [ScriptIgnore]        
        public int Rekkefølge { get; set; }
    }
}
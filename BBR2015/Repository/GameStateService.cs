using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using Database;
using Database.Entities;
using Newtonsoft.Json;

namespace Repository
{
    // Registrert som Singleton
    public class GameStateService
    {
        private readonly DataContextFactory _dataContextFactory;
        private readonly CurrentMatchProvider _currentMatchProvider;

        private ConcurrentDictionary<Guid, MatchState> _matchStates = new ConcurrentDictionary<Guid, MatchState>();

        public GameStateService(DataContextFactory dataContextFactory, CurrentMatchProvider currentMatchProvider)
        {
            _dataContextFactory = dataContextFactory;
            _currentMatchProvider = currentMatchProvider;
        }

        public void Calculate()
        {
            var matchId = _currentMatchProvider.GetMatchId();

            using (var context = _dataContextFactory.Create())
            {
                var sorterteLag =
                    context.LagIMatch.Include(x => x.Lag).Include(x => x.VåpenBeholdning).Include("Våpenbeholdning.BruktIPostRegistrering")
                           .Where(x => x.Match.MatchId == matchId)
                           .OrderByDescending(x => x.PoengSum)
                           .ToList();

                var poster = (from p in context.PosterIMatch.Include(x => x.Post)
                              where p.Match.MatchId == matchId
                              select new TempPost
                              {
                                  PostId = p.Post.PostId,
                                  Latitude = p.Post.Latitude,
                                  Longitude = p.Post.Longitude,
                                  CurrentPoengIndex = p.CurrentPoengIndex,
                                  PoengArray = p.PoengArray,
                                  Navn = p.Post.Navn,
                                  ErSynlig = p.SynligFraTid < TimeService.Now && TimeService.Now < p.SynligTilTid,
                                  SynligFra = p.SynligFraTid,
                                  SynligTil = p.SynligTilTid
                              }).ToList();

                var postRegistreringer = (from l in context.LagIMatch
                                          from p in l.PostRegistreringer
                                          where l.Match.MatchId == matchId
                                          select new
                                          {
                                              PostId = p.RegistertPost.Post.PostId,
                                              LagIMatchId = p.RegistertForLag.Id,
                                              Poeng = p.PoengForRegistrering,
                                              Deltaker = p.RegistrertAvDeltaker
                                          }).ToList();


                var rankedeLag = (from l in sorterteLag
                                  select new
                                  {
                                      Id = l.Id,
                                      Lag = l.Lag,
                                      Rank = sorterteLag.Count(x => x.PoengSum > l.PoengSum) + 1,
                                      PoengSum = l.PoengSum,
                                      LagIMatch = l
                                  }).OrderBy(x => x.Rank).ToList();

                var poengsummer = (from l in rankedeLag
                                   select l.PoengSum).Distinct().OrderByDescending(x => x).ToList();

                var nyGameState = new Dictionary<string, GameStateForLag>();
                var random = new Random();

                foreach (var lag in rankedeLag)
                {
                    var egenPoengIndex = poengsummer.IndexOf(lag.PoengSum);

                    var poengForover = egenPoengIndex == 0 ? lag.PoengSum : poengsummer[egenPoengIndex - 1];
                    var poengBakover = egenPoengIndex == poengsummer.Count - 1 ? lag.PoengSum : poengsummer[egenPoengIndex + 1];
                  
                    var state = new GameStateForLag
                    {
                        LagId = lag.Lag.LagId,
                        LagNavn = lag.Lag.Navn,
                        LagFarge = lag.Lag.Farge,
                        LagIkon = lag.Lag.Ikon,
                        Score = lag.PoengSum,
                        Ranking = new GameStateRanking
                        {
                            Rank = lag.Rank,
                            PoengBakLagetForan = poengForover - lag.PoengSum,
                            PoengForanLagetBak = lag.PoengSum - poengBakover,
                        },
                        Poster = (from p in poster.Where(x => x.ErSynlig)
                                  join r in postRegistreringer.Where(x => x.LagIMatchId == lag.Id) on p.PostId equals r.PostId into j
                                  from reg in j.DefaultIfEmpty()
                                  select new GameStatePost
                                  {
                                      Latitude = p.Latitude,
                                      Longitude = p.Longitude,
                                      PoengVerdi = reg != null ? reg.Poeng : PostIMatch.BeregnPoengForNesteRegistrering(p.PoengArray, p.CurrentPoengIndex),
                                      HarRegistert = reg != null,
                                      Rekkefølge = random.Next(0, short.MaxValue) // order by random                                 
                                  }).OrderBy(x => x.Rekkefølge).ToList(),
                        Vaapen = lag.LagIMatch.VåpenBeholdning.Where(x => x.BruktIPostRegistrering == null).Select(x => new GameStateVaapen
                        {
                            VaapenId = x.VaapenId
                        }).ToList()
                    };

                    nyGameState.Add(state.LagId, state);
                }

                var scoreboard = new ScoreboardState();
                scoreboard.Poster = (from p in poster
                                     select new ScoreboardPost
                                     {
                                         Latitude = p.Latitude,
                                         Longitude = p.Longitude,
                                         ErSynlig = p.ErSynlig,
                                         Navn = p.Navn,
                                         Verdi = PostIMatch.BeregnPoengForNesteRegistrering(p.PoengArray, p.CurrentPoengIndex),
                                         AntallRegistreringer = postRegistreringer.Count(x => x.PostId == p.PostId)
                                     }).OrderBy(x => x.Navn).ToList();

                scoreboard.Lag = sorterteLag.Select(l => new ScoreboardLag
                {
                    LagNavn = l.Lag.Navn,
                    LagFarge = l.Lag.Farge,
                    LagIkon = l.Lag.Ikon,
                    Score = l.PoengSum,
                    Ranking = sorterteLag.Count(x => x.PoengSum > l.PoengSum) + 1,
                    AntallRegistreringer = postRegistreringer.Count(x => x.LagIMatchId == l.Id)
                }).ToList();

                var deltakerPoeng = postRegistreringer
                                        .GroupBy(p => p.Deltaker.DeltakerId)
                                        .Select(g => new
                                        {
                                            DeltakerId = g.Key,
                                            Navn = g.First().Deltaker.Navn,
                                            LagIMatchId = g.First().LagIMatchId,
                                            AntallRegistreringer = g.Count(),
                                            Poengsum = g.Sum(x => x.Poeng)
                                        }).ToList();

                scoreboard.Deltakere = (from p in deltakerPoeng
                                        join l in sorterteLag on p.LagIMatchId equals l.Id
                                        select new ScoreboardDeltaker
                                        {
                                            DeltakerId = p.DeltakerId,
                                            Navn = p.Navn,
                                            AntallRegistreringer = p.AntallRegistreringer,
                                            Score = p.Poengsum,
                                            LagId = l.Lag.LagId,
                                            LagFarge = l.Lag.Farge,
                                            LagIkon = l.Lag.Ikon,
                                            LagNavn = l.Lag.Navn,
                                            MostValueablePlayerRanking = deltakerPoeng.Count(x => x.Poengsum > p.Poengsum) + 1
                                        }).ToList();

                var førsteTidspunktEtterNå = (from p in poster
                                              from t in p.Tider
                                              where t > TimeService.Now
                                              select t).Min();

                // swap current state
                _matchStates[matchId] = new MatchState(matchId, nyGameState, scoreboard, førsteTidspunktEtterNå);
            }
        }

        public GameStateForLag Get(string lagId)
        {
            var matchId = _currentMatchProvider.GetMatchId();

            if (!_matchStates.ContainsKey(matchId))
                Calculate();

            if (_matchStates[matchId].ErUtløpt)
                Calculate();

            return _matchStates[matchId].Get(lagId);
        }

        public ScoreboardState GetScoreboard()
        {
            var matchId = _currentMatchProvider.GetMatchId();

            if (!_matchStates.ContainsKey(matchId))
                Calculate();

            if (_matchStates[matchId].ErUtløpt)
                Calculate();

            return _matchStates[matchId].GetScoreboard();
        }
    }

    public class MatchState
    {
        private Dictionary<string, GameStateForLag> _gamestates = new Dictionary<string, GameStateForLag>();
        private ScoreboardState _scoreboard = new ScoreboardState();
        private readonly DateTime _gyldigInntil;

        public Guid MatchId { get; set; }
        public MatchState(Guid matchId, Dictionary<string, GameStateForLag> gamestates, ScoreboardState scoreboard, DateTime? gyldigInntil)
        {
            MatchId = matchId;
            _gamestates = gamestates;
            _scoreboard = scoreboard;
            _gyldigInntil = gyldigInntil ?? DateTime.MaxValue;
        }

        public bool ErUtløpt
        {
            get { return TimeService.Now > _gyldigInntil; }
        }
        public GameStateForLag Get(string lagId)
        {
            return _gamestates[lagId];
        }

        public ScoreboardState GetScoreboard()
        {
            return _scoreboard;
        }
    }

    public class ScoreboardState
    {
        public ScoreboardState()
        {
            Poster = new List<ScoreboardPost>();
            Deltakere = new List<ScoreboardDeltaker>();
            Lag = new List<ScoreboardLag>();
        }
        public List<ScoreboardPost> Poster { get; set; }
        public List<ScoreboardDeltaker> Deltakere { get; set; }
        public List<ScoreboardLag> Lag { get; set; }
    }

    public class ScoreboardPost
    {
        public string Navn { get; set; }
        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public int AntallRegistreringer { get; set; }

        public int Verdi { get; set; }
        public bool ErSynlig { get; set; }
    }

    public class ScoreboardDeltaker
    {
        public string Navn { get; set; }
        public string DeltakerId { get; set; }
        public string LagId { get; set; }
        public string LagNavn { get; set; }
        public string LagFarge { get; set; }
        public string LagIkon { get; set; }
        public int AntallRegistreringer { get; set; }
        public int Score { get; set; }
        public int MostValueablePlayerRanking { get; set; }
    }

    public class ScoreboardLag
    {
        public string LagNavn { get; set; }
        public string LagFarge { get; set; }
        public string LagIkon { get; set; }
        public int AntallRegistreringer { get; set; }
        public int Score { get; set; }
        public int Ranking { get; set; }
    }
    public class TempPost
    {
        public Guid PostId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int CurrentPoengIndex { get; set; }
        public string PoengArray { get; set; }

        [ScriptIgnore]
        public bool ErSynlig { get; set; }

        public string Navn { get; set; }
        public DateTime[] Tider { get { return new[] { SynligFra, SynligTil }; } }
        public DateTime SynligTil { get; set; }
        public DateTime SynligFra { get; set; }
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

        [JsonIgnore]
        public int Rekkefølge { get; set; }
    }
}
using System;
using Database.Entities;
using Database;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

namespace Repository
{
    public class GameService
    {
        private readonly DataContextFactory _dataContextFactory;
        private readonly GameStateService _gameState;
        private readonly CurrentMatchProvider _currentMatchProvider;

        private static object _tillatBareEnStemplingIGangen = new object();
        public GameService(DataContextFactory dataContextFactory, GameStateService gameState, CurrentMatchProvider currentMatchProvider)
        {
            _dataContextFactory = dataContextFactory;
            _gameState = gameState;
            _currentMatchProvider = currentMatchProvider;
        }

        public void RegistrerNyPost(string deltakerId, string lagId, string postkode, string bruktVåpen)
        {
            lock (_tillatBareEnStemplingIGangen)
            {
                RegistrerNyPostSynkront(deltakerId, lagId, postkode, bruktVåpen);
            }
        }

        private void RegistrerNyPostSynkront(string deltakerId, string lagId, string postkode, string bruktVåpen)
        {
            var matchId = _currentMatchProvider.GetMatchId();

            var gyldigInntil = DateTime.MaxValue;

            using (var context = _dataContextFactory.Create())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var lagIMatch = (from lm in context.LagIMatch.Include(x => x.Lag)
                                         where lm.Lag.LagId == lagId && lm.Match.MatchId == matchId
                                         select lm).SingleOrDefault();

                        if (lagIMatch == null)
                            return;                        

                        var post = (from pim in context.PosterIMatch.Include(x => x.Post).Include(x => x.Match)
                                    where pim.Post.HemmeligKode == postkode && pim.Match.MatchId == matchId
                                    select pim).SingleOrDefault();

                        if (post == null)
                            return; // Feil kode eller post - straff?

                        if (post.ErSynlig)
                        {
                            if(context.PostRegisteringer.Any(x => x.RegistertForLag.Id == lagIMatch.Id && x.RegistertPost.Id == post.Id))
                                return; // Allerede registrert post denne matchen

                            var deltaker = context.Deltakere.Single(x => x.DeltakerId == deltakerId);

                            var poeng = post.HentPoengOgInkrementerIndex();
                            lagIMatch.PoengSum += poeng;

                            var registrering = new PostRegistrering
                            {
                                PoengForRegistrering = poeng,
                                RegistertForLag = lagIMatch,
                                RegistrertAvDeltaker = deltaker,
                                RegistertPost = post,
                                BruktVaapenId = bruktVåpen,
                                RegistertTidspunkt = TimeService.Now
                            };

                            context.PostRegisteringer.Add(registrering);

                            if (!string.IsNullOrEmpty(bruktVåpen))
                            {
                                var brukt = (from v in context.VåpenBeholdning
                                             where v.VaapenId == bruktVåpen
                                                   && v.LagIMatchId == lagIMatch.Id
                                             select v).FirstOrDefault();

                                // Har prøvd å bruke noe laget ikke har
                                if (brukt != null)
                                {
                                    brukt.BruktIPostRegistrering = registrering;

                                    if (bruktVåpen == Constants.Våpen.Bombe)
                                    {
                                        gyldigInntil = TimeService.Now.AddSeconds(Constants.Våpen.BombeSkjulerPostIAntallSekunder);
                                        post.SynligFraTid = gyldigInntil;
                                    }                                    
                                }
                            }
                        }

                        context.SaveChanges();
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }

            _gameState.Calculate(gyldigInntil);

        }
    }
}

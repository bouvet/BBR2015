using System;
using Database.Entities;
using Database;
using System.Linq;
using System.Data.Entity;

namespace Repository
{
    public class GameService
    {
        private readonly DataContextFactory _dataContextFactory;
        private readonly GameStateService _gameState;
        private readonly CurrentMatchProvider _currentMatchProvider;
        private readonly GameEventPublisher _eventPublisher;
        private readonly OverridableSettings _settings;

        private static readonly object TillatBareEnStemplingIGangen = new object();

        public GameService(DataContextFactory dataContextFactory, GameStateService gameState, CurrentMatchProvider currentMatchProvider, GameEventPublisher eventPublisher, OverridableSettings settings)
        {
            _dataContextFactory = dataContextFactory;
            _gameState = gameState;
            _currentMatchProvider = currentMatchProvider;
            _eventPublisher = eventPublisher;
            _settings = settings;
        }

        public void RegistrerNyPost(string deltakerId, string lagId, string postkode, string bruktVåpen)
        {
            lock (TillatBareEnStemplingIGangen)
            {
                RegistrerNyPostSynkront(deltakerId, lagId, postkode, bruktVåpen);
            }
        }

        private void RegistrerNyPostSynkront(string deltakerId, string lagId, string postkode, string bruktVåpen)
        {
            var matchId = _currentMatchProvider.GetMatchId();

            using (var context = _dataContextFactory.Create())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        if (string.IsNullOrEmpty(postkode))
                            return;

                        postkode = postkode.Trim();

                        var lagIMatch = (from lm in context.LagIMatch.Include(x => x.Lag)
                                         where lm.Lag.LagId == lagId && lm.Match.MatchId == matchId
                                         select lm).SingleOrDefault();

                        if (lagIMatch == null)
                            return;

                        var post = (from pim in context.PosterIMatch.Include(x => x.Post).Include(x => x.Match)
                                    where pim.Post.HemmeligKode == postkode && pim.Match.MatchId == matchId
                                    select pim).SingleOrDefault();

                        if (post == null)
                        {
                            _eventPublisher.PrøvdeÅRegistrereEnPostMedFeilKode(lagId, deltakerId);
                            return; // Feil kode eller post - straff?
                        }

                        if (!post.ErSynlig)
                        {
                            _eventPublisher.PrøvdeÅRegistrereEnPostSomIkkeErSynlig(lagId, deltakerId);
                        }
                        else
                        {
                            if (context.PostRegisteringer.Any(x => x.RegistertForLag.Id == lagIMatch.Id && x.RegistertPost.Id == post.Id))
                            {
                                _eventPublisher.AlleredeRegistrert(lagId, deltakerId);
                                return; // Allerede registrert post denne matchen
                            }

                            var deltaker = context.Deltakere.Single(x => x.DeltakerId == deltakerId);

                            var poeng = post.HentPoengOgInkrementerIndex();

                            if (!string.IsNullOrEmpty(post.RiggetVåpen))
                            {
                                poeng = KjørRiggedeVåpenOgReturnerPoeng(post, poeng, bruktVåpen, lagId, deltakerId);
                                bruktVåpen = null; // bruker ikke eget våpen når felle går av
                            }
                            else
                            {
                                _eventPublisher.PoengScoret(lagId, deltakerId, poeng);
                            }

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
                                                   && v.BruktIPostRegistrering == null
                                             select v).FirstOrDefault();

                                // Har prøvd å bruke noe laget ikke har
                                if (brukt != null)
                                {
                                    brukt.BruktIPostRegistrering = registrering;

                                    if (bruktVåpen == Constants.Våpen.Bombe)
                                    {
                                        post.SynligFraTid = TimeService.Now.AddSeconds(_settings.PostSkjulesISekunderEtterVåpen);
                                        _eventPublisher.BrukteBombe(lagId, deltakerId);
                                    }
                                    if (bruktVåpen == Constants.Våpen.Felle)
                                    {
                                        post.RiggetVåpen = Constants.Våpen.Felle;
                                        post.RiggetVåpenParam = lagId;

                                        _eventPublisher.RiggetEnFelle(lagId, deltakerId);
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

            _gameState.Calculate();

        }

        private int KjørRiggedeVåpenOgReturnerPoeng(PostIMatch post, int poeng, string bruktVåpen, string lagId, string deltakerId)
        {
            if (post.RiggetVåpen == Constants.Våpen.Felle)
            {
                var riggetAvLag = post.RiggetVåpenParam;
                post.RiggetVåpen = null; // nullstill
                post.RiggetVåpenParam = null;
                post.SynligFraTid = TimeService.Now.AddSeconds(_settings.PostSkjulesISekunderEtterVåpen);

                _eventPublisher.UtløsteFelle(lagId, deltakerId, poeng, riggetAvLag);

                return -poeng;
            }

            return poeng;
        }

        public void Nullstill(string lagId)
        {
            var matchId = _currentMatchProvider.GetMatchId();

            using (var context = _dataContextFactory.Create())
            {
                var lagIMatch = context.LagIMatch
                                       .Include(x => x.VåpenBeholdning)
                                       .Include(x => x.Lag)
                                       .Include(x => x.Match)
                                       .Single(x => x.Lag.LagId == lagId && x.Match.MatchId == matchId);
                lagIMatch.PoengSum = 0;
                
                lagIMatch.VåpenBeholdning.ForEach(x => x.BruktIPostRegistrering = null);

                var lagetsRegistreringerIMatch =
                    context.PostRegisteringer.Where(
                        x => x.RegistertForLag.Match.MatchId == matchId && x.RegistertForLag.Lag.LagId == lagId)
                        .ToList();

                context.PostRegisteringer.RemoveRange(lagetsRegistreringerIMatch);

                //var alleVåpen = context.Våpen.ToList();

                //var felle = alleVåpen.Single(x => x.VaapenId == Constants.Våpen.Felle);
                //var bombe = alleVåpen.Single(x => x.VaapenId == Constants.Våpen.Bombe);

                //lagIMatch.VåpenBeholdning.Add(new VaapenBeholdning { LagIMatch = lagIMatch, Våpen = felle });
                //lagIMatch.VåpenBeholdning.Add(new VaapenBeholdning { LagIMatch = lagIMatch, Våpen = bombe });

                context.SaveChanges();
            }

            _gameState.Calculate();
        }
    }
}

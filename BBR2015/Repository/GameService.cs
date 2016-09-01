using System;
using Database.Entities;
using Database;
using System.Linq;
using System.Data.Entity;
using Database.Infrastructure;

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

    public class GameEventPublisher
    {
        private readonly MeldingService _meldingService;
        private readonly TilgangsKontroll _tilgangsKontroll;

        private string AdminLagId = "SUPPORT_1";

        public GameEventPublisher(MeldingService meldingService, TilgangsKontroll tilgangsKontroll)
        {
            _meldingService = meldingService;
            _tilgangsKontroll = tilgangsKontroll;
        }

        public void PoengScoret(string lagId, string deltakerId, int poeng)
        {
            var lagNavn = _tilgangsKontroll.HentLagNavn(lagId);
            var deltakerNavn = _tilgangsKontroll.HentDeltakerNavn(deltakerId);

            var melding = string.Format("{0} ({1}) fikk {2} poeng", deltakerNavn, lagNavn, poeng);

            _meldingService.PostMeldingTilAlle(deltakerId, lagId, melding);
        }

        public void UtløsteFelle(string lagId, string deltakerId, int poeng, string riggetAvLag)
        {
            var lagNavn = _tilgangsKontroll.HentLagNavn(lagId);
            var deltakerNavn = _tilgangsKontroll.HentDeltakerNavn(deltakerId);
            var riggetAvLagNavn = _tilgangsKontroll.HentLagNavn(riggetAvLag);

            var melding = string.Format("{0} ({1}) utløste en felle rigget av {2} og mistet {3} poeng.", deltakerNavn, lagNavn, riggetAvLagNavn, poeng);

            _meldingService.PostMeldingTilAlle(deltakerId, lagId, melding);
        }

        public void AlleredeRegistrert(string lagId, string deltakerId)
        {
            var lagNavn = _tilgangsKontroll.HentLagNavn(lagId);
            var deltakerNavn = _tilgangsKontroll.HentDeltakerNavn(deltakerId);

            var melding = string.Format("{0} ({1}) prøvde å registrere en post som laget allerede har registrert.", deltakerNavn, lagNavn);

            _meldingService.PostMelding(deltakerId, lagId, melding);
        }

        public void BrukteBombe(string lagId, string deltakerId)
        {
            var lagNavn = _tilgangsKontroll.HentLagNavn(lagId);
            var deltakerNavn = _tilgangsKontroll.HentDeltakerNavn(deltakerId);

            var melding = string.Format("{0} ({1}) sprengte en post i lufta med en bombe.", deltakerNavn, lagNavn);

            _meldingService.PostMeldingTilAlle(deltakerId, lagId, melding);
        }

        public void RiggetEnFelle(string lagId, string deltakerId)
        {
            var lagNavn = _tilgangsKontroll.HentLagNavn(lagId);
            var deltakerNavn = _tilgangsKontroll.HentDeltakerNavn(deltakerId);

            var melding = string.Format("{0} ({1}) rigget en felle.", deltakerNavn, lagNavn);

            _meldingService.PostMelding(deltakerId, lagId, melding);
            _meldingService.PostMelding(deltakerId, AdminLagId, melding);
        }

        public void PrøvdeÅRegistrereEnPostMedFeilKode(string lagId, string deltakerId)
        {
            var lagNavn = _tilgangsKontroll.HentLagNavn(lagId);
            var deltakerNavn = _tilgangsKontroll.HentDeltakerNavn(deltakerId);

            var melding = string.Format("{0} ({1}) prøvde å registrere en post, men brukte feil kode.", deltakerNavn, lagNavn);

            _meldingService.PostMeldingTilAlle(deltakerId, lagId, melding);
        }

        public void PrøvdeÅRegistrereEnPostSomIkkeErSynlig(string lagId, string deltakerId)
        {
            var lagNavn = _tilgangsKontroll.HentLagNavn(lagId);
            var deltakerNavn = _tilgangsKontroll.HentDeltakerNavn(deltakerId);

            var melding = string.Format("{0} ({1}) prøvde å registrere en post som ikke er synlig.", deltakerNavn, lagNavn);

            _meldingService.PostMeldingTilAlle(deltakerId, lagId, melding);
        }
    }
}

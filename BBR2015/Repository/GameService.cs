using System;
using Database.Entities;
using Database;
using System.Linq;

namespace Repository
{
    public class GameService
    {
        private readonly DataContextFactory _dataContextFactory;
        private readonly GameStateService _gameState;
        private readonly CurrentMatchProvider _currentMatchProvider;
        private readonly GameEventPublisher _eventPublisher;

        private static readonly object TillatBareEnStemplingIGangen = new object();

        public GameService(DataContextFactory dataContextFactory, GameStateService gameState, CurrentMatchProvider currentMatchProvider, GameEventPublisher eventPublisher)
        {
            _dataContextFactory = dataContextFactory;
            _gameState = gameState;
            _currentMatchProvider = currentMatchProvider;
            _eventPublisher = eventPublisher;
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
                            if (
                                context.PostRegisteringer.Any(
                                    x => x.RegistertForLag.Id == lagIMatch.Id && x.RegistertPost.Id == post.Id))
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
                                             select v).FirstOrDefault();

                                // Har prøvd å bruke noe laget ikke har
                                if (brukt != null)
                                {
                                    brukt.BruktIPostRegistrering = registrering;

                                    if (bruktVåpen == Constants.Våpen.Bombe)
                                    {
                                        post.SynligFraTid = TimeService.Now.AddSeconds(Constants.Våpen.BombeSkjulerPostIAntallSekunder);
                                    }
                                    if (bruktVåpen == Constants.Våpen.Felle)
                                    {
                                        post.RiggetVåpen = Constants.Våpen.Felle;
                                        post.RiggetVåpenParam = lagId;
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
                post.SynligFraTid = TimeService.Now.AddSeconds(Constants.Våpen.BombeSkjulerPostIAntallSekunder);

                _eventPublisher.UtløsteFelle(lagId, deltakerId, poeng, riggetAvLag);

                return -poeng;
            }

            return poeng;
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

            var melding = string.Format("{0} fikk {1} poeng for {2}", deltakerNavn, poeng, lagNavn);

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

            _meldingService.PostMeldingTilAlle(deltakerId, lagId, melding);
        }
    }
}

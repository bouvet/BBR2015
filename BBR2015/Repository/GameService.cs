﻿using System;
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

        private static readonly object TillatBareEnStemplingIGangen = new object();

        public GameService(DataContextFactory dataContextFactory, GameStateService gameState, CurrentMatchProvider currentMatchProvider)
        {
            _dataContextFactory = dataContextFactory;
            _gameState = gameState;
            _currentMatchProvider = currentMatchProvider;
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
                            if(context.PostRegisteringer.Any(x => x.RegistertForLag.Id == lagIMatch.Id && x.RegistertPost.Id == post.Id))
                                return; // Allerede registrert post denne matchen

                            var deltaker = context.Deltakere.Single(x => x.DeltakerId == deltakerId);

                            var poeng = post.HentPoengOgInkrementerIndex();

                            if (post.RiggetVåpen == Constants.Våpen.Felle)
                            {
                                poeng = -poeng;
                                post.RiggetVåpen = null; // nullstill
                                post.SynligFraTid = TimeService.Now.AddSeconds(Constants.Våpen.BombeSkjulerPostIAntallSekunder);
                                bruktVåpen = null; // Skal ikke bruke eget våpen når fellen går av
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
    }
}

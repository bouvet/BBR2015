using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Database;
using Database.Entities;
using Database.Infrastructure;

namespace Repository
{
    public class LagOppstillingService
    {
        private readonly DataContextFactory _dataContextFactory;
        private readonly TilgangsKontroll _tilgangsKontroll;
        private readonly GameStateService _gameStateService;
        private ConcurrentDictionary<string, Lag> _alleLag;

        public LagOppstillingService(DataContextFactory dataContextFactory, GameStateService gameStateService, TilgangsKontroll tilgangsKontroll)
        {
            _dataContextFactory = dataContextFactory;
            _gameStateService = gameStateService;
            _tilgangsKontroll = tilgangsKontroll;
        }

        public Lag Get(string lagId)
        {
            if (_alleLag == null)
                HentFraDatabasen();

            return _alleLag[lagId];
        }

        private void HentFraDatabasen()
        {
            using (var context = _dataContextFactory.Create())
            {
                _alleLag = new ConcurrentDictionary<string, Lag>(context.Lag.ToDictionary(x => x.LagId, x => x));
            }
        }

        public void OpprettNySpiller(string hemmeligKodeForLag, string kodeForSpiller, string navn)
        {
            using (var context = _dataContextFactory.Create())
            {
                var lag = context.Lag.Single(x => x.HemmeligKode == hemmeligKodeForLag);

                var deltaker = new Deltaker
                {
                    DeltakerId = Guid.NewGuid().ToString(),
                    Kode = kodeForSpiller,
                    Navn = navn
                };

                lag.LeggTilDeltaker(deltaker);

                context.SaveChanges();
            }

            _tilgangsKontroll.Nullstill();
        }

        public void OpprettNyttLag(Guid matchId, string hemmeligKode, string navn)
        {
            using (var context = _dataContextFactory.Create())
            {
                var match = context.Matcher.Single(x => x.MatchId == matchId);

                var lag = new Lag
                {
                    LagId = Guid.NewGuid().ToString(),
                    Navn = navn,
                    HemmeligKode = hemmeligKode
                };

                match.LeggTil(lag);

                context.SaveChanges();
            }

            _tilgangsKontroll.Nullstill();
            _gameStateService.Calculate(); // For å få laget med på resultatlista
        }
    }
}

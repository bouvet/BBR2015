using System;
using Database.Entities;
using Database;
using System.Linq;

namespace Repository
{
    public class GameServiceRepository
    {
        private readonly AdminRepository _adminRepository;
        private readonly DataContextFactory _dataContextFactory;
        private readonly GameStateService _gameState;
        private readonly CurrentMatchProvider _currentMatchProvider;

        public GameServiceRepository(AdminRepository adminRepository, DataContextFactory dataContextFactory, GameStateService gameState, CurrentMatchProvider currentMatchProvider)
        {
            _adminRepository = adminRepository;
            _dataContextFactory = dataContextFactory;
            _gameState = gameState;
            _currentMatchProvider = currentMatchProvider;
        }

        public void RegistrerNyPost(string deltakerId, string lagId, string postkode, string bruktVåpen)
        {
            var lag = _adminRepository.FinnLag(lagId);
            var deltaker = lag.HentDeltaker(deltakerId);
            var matchId = _currentMatchProvider.GetMatchId();

            using (var context = _dataContextFactory.Create())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var lagIMatch = (from lm in context.LagIMatch.Include(x => x.Lag).Include(x => x.Match)
                                         where lm.Lag.LagId == lagId && lm.Match.MatchId == matchId
                                         select lm).SingleOrDefault();
                        
                        if (lagIMatch == null)
                            return;

                        var post = (from pim in context.PosterIMatch.Include(x => x.Post).Include(x => x.Match)
                                    where pim.Post.HemmeligKode == postkode && pim.Match.MatchId == lagIMatch.Match.MatchId
                                    select pim).SingleOrDefault();

                        if (post == null)
                            return; // Feil kode eller post - straff?

                        if (post.ErSynlig)
                        {
                            var poeng = post.HentPoengOgInkrementerIndex();
                            lagIMatch.PoengSum += poeng;

                            var registrering = new PostRegistrering
                            {
                                PoengForRegistrering = poeng,
                                RegistertForLag = lagIMatch,
                                RegistrertAvDeltaker = deltaker,
                                RegistertPost = post,
                                BruktVaapenId = bruktVåpen
                            };

                            context.PostRegisteringer.Add(registrering);
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

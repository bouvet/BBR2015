using System;
using System.Collections.Generic;
using System.Linq;
using Database;

namespace Repository
{
    public class GameStateService
    {
        private readonly DataContextFactory _dataContextFactory;
        private readonly CurrentMatchProvider _currentMatchProvider;

        private List<GameStateForLag> _gamestates = new List<GameStateForLag>();

        public GameStateService(DataContextFactory dataContextFactory, CurrentMatchProvider currentMatchProvider)
        {
            _dataContextFactory = dataContextFactory;
            _currentMatchProvider = currentMatchProvider;
            Calculate();
        }

        public void Calculate()
        {
            var matchId = _currentMatchProvider.GetMatchId();

            using (var context = _dataContextFactory.Create())
            {
                var sorterteLag =
                    context.LagIMatch
                           .Where(x => x.Match.MatchId == matchId)
                           .OrderByDescending(x => x.PoengSum)
                           .ToArray();

                for (int i = 0; i < sorterteLag.Length; i++)
                {
                    // TODO: Fortsett
                }
                

            }

        }

        public object Get(string lagId)
        {
            return null;
        }
    }

    public class GameStateForLag
    {
    }
}
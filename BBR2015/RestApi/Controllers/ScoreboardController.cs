using System;
using System.Web.Http;
using System.Web.Http.Cors;
using Repository;
using RestApi.Filters;

namespace RestApi.Controllers
{
    [RequireScoreboardSecret]
    public class ScoreboardController : ApiController
    {
        private readonly GameStateService _gameStateService;

        public ScoreboardController(GameStateService gameStateService)
        {
            _gameStateService = gameStateService;
        }

        // GET: api/Scoreboard
        /// <summary>
        /// Henter feed for Scoreboard med kampens totale resultater (lag og MVP) + alle poster med status.
        /// Krever ekstra tilgangskontroll.
        /// </summary>
        public ScoreboardState Get()
        {
            return _gameStateService.GetScoreboard();
        }
    }
}
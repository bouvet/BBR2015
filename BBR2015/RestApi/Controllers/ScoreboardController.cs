using System.Web.Http;
using System.Web.Http.Cors;
using Repository;

namespace RestApi.Controllers
{
    [RequireScoreboardSecret]
    [EnableCors("*", "*", "*")]
    public class ScoreboardController : ApiController
    {
        private readonly GameStateService _gameStateService;

        public ScoreboardController(GameStateService gameStateService)
        {
            _gameStateService = gameStateService;
        }

        // GET: api/Scoreboard/
      
        public ScoreboardState Get()
        {
            return _gameStateService.GetScoreboard();
        }
    }
}

using System.Web.Http;
using System.Web.Http.Cors;
using Repository;

namespace RestApi.Controllers
{
    [RequireScoreboardSecret]
    [EnableCors("*", "*", "*")]
    public class AdminController : ApiController
    {
        private readonly GameStateService _gameStateService;

        public AdminController(GameStateService gameStateService)
        {
            _gameStateService = gameStateService;
        }

        // Post: api/Admin/RecalculateState
        [Route("api/Admin/RecalculateState")]
        [HttpPost]
        public IHttpActionResult RecalculateState()
        {
            _gameStateService.Calculate();

            return Ok();
        }
    }
}
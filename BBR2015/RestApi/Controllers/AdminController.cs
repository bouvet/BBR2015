
using System;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Results;
using Database;
using Repository;

namespace RestApi.Controllers
{
    [RequireScoreboardSecret]
    public class AdminController : ApiController
    {
        private readonly GameStateService _gameStateService;
        private readonly CascadingAppSettings _appSettings;

        public AdminController(GameStateService gameStateService, CascadingAppSettings appSettings)
        {
            _gameStateService = gameStateService;
            _appSettings = appSettings;
        }

        // Post: api/Admin/RecalculateState
        [Route("api/Admin/RecalculateState")]
        [HttpPost]
        public IHttpActionResult RecalculateState()
        {
            _gameStateService.Calculate();

            return Ok();
        }

        // Post: api/Admin/RecalculateState
        [Route("api/Admin/ThrowException")]
        [HttpPost]
        public IHttpActionResult ThrowException()
        {
            throw new ApplicationException("Initiert av brukeren");           
        }

        // Post: api/Admin/RecalculateState
        [Route("api/Admin/ConnectionString")]
        [HttpGet]
        public IHttpActionResult ConnectionString()
        {
            return Ok(_appSettings.DatabaseConnectionString);
        }
    }
}
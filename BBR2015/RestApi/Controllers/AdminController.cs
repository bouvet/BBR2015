
using System;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Results;
using Database;
using Repository;
using RestApi.Filters;

namespace RestApi.Controllers
{
    [RequireScoreboardSecret]
    public class AdminController : ApiController
    {
        private readonly GameStateService _gameStateService;
        private readonly OverridableSettings _appSettings;
        private readonly TilgangsKontroll _tilgangsKontroll;

        public AdminController(GameStateService gameStateService, OverridableSettings appSettings, TilgangsKontroll tilgangsKontroll)
        {
            _gameStateService = gameStateService;
            _appSettings = appSettings;
            _tilgangsKontroll = tilgangsKontroll;
        }

        [Route("api/Admin/RecalculateState")]
        [HttpPost]
        public IHttpActionResult RecalculateState()
        {
            _gameStateService.Calculate();

            return Ok();
        }

        
        [Route("api/Admin/ThrowException")]
        [HttpPost]
        public IHttpActionResult ThrowException()
        {
            throw new ApplicationException("Initiert av brukeren");           
        }

        [Route("api/Admin/ClearCaching")]
        [HttpPost]
        public IHttpActionResult ClearCaching()
        {
            _gameStateService.Calculate();
            _tilgangsKontroll.Nullstill();
            ThrottleAttribute.Reload();
            return Ok();
        }

        // Post: api/Admin/RecalculateState
        [Route("api/Admin/ConnectionString")]
        [HttpGet]
        public IHttpActionResult ConnectionString()
        {
            return Ok(_appSettings.DatabaseConnectionString);
        }

        [Route("api/Admin/hemmeligekoder")]
        [HttpGet]
        public IHttpActionResult HemmeligeKoder()
        {
            return Ok(_tilgangsKontroll.HentAlleHemmeligeKoder());
        }

    }
}
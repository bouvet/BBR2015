using System;
using System.Collections.Generic;
using System.Web.Http.Cors;
using Database.Entities;
using Repository;
using RestApi.Filters;

namespace RestApi.Controllers
{
    public class GameStateFeedController : BaseController
    {
        private readonly GameStateService _gameStateService;

        public GameStateFeedController(GameStateService gameStateService)
        {
            _gameStateService = gameStateService;
        }

        /// <summary>
        /// Her henter laget ut sin gamestate med informasjon om poster med deres koordinater og poengverdi (og om laget har registrert den). <br />
        /// Lagets ranking i spillet med egen poengsom og differanse til lagene foran og bak. Også våpenbeholdning.
        /// </summary>
        /// <remarks>GET api/GameStateFeed</remarks>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden - Husk LagKode og DeltakerKode</response>
        /// <response code="500">Internal Server Error</response>
        [RequireApiKey]
        public GameStateForLag Get()
        {
            return _gameStateService.Get(LagId);            
        }
    }
}

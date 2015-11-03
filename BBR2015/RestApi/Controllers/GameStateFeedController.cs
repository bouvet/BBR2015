using System;
using System.Collections.Generic;
using System.Web.Http.Cors;
using Database.Entities;
using Repository;

namespace RestApi.Controllers
{
    public class GameStateFeedController : BaseController
    {
        private readonly GameStateService _gameStateService;

        public GameStateFeedController(GameStateService gameStateService)
        {
            _gameStateService = gameStateService;
        }

        // GET: api/GameStateFeed
        [RequireApiKey]
        public GameStateForLag Get()
        {
            return _gameStateService.Get(LagId);            
        }
    }
}

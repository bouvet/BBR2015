using System;
using System.Collections.Generic;
using System.Web.Http.Cors;
using Database.Entities;
using Repository;

namespace RestApi.Controllers
{

    public class Ranking
    {
        public int Rank { get; set; }
        public int PoengTilNesteNivå { get; set; }
        public int PoengTilForrigeNivå { get; set; }
    }
    public class GameStateFeedController : BaseController
    {
        private readonly GameStateService _gameStateService;

        public GameStateFeedController(GameStateService gameStateService)
        {
            _gameStateService = gameStateService;
        }

        // GET: api/GameStateFeed
        [EnableCors("*", "*", "*")]
        [RequireApiKey]
        public GameStateForLag Get()
        {
            var gameState = _gameStateService.Get(LagId);

            return gameState;
            /* FOR ETT LAG
             *Poster
             *m koorinat
             *m poeng
             *stemplet
             *
             * Sum poeng
             * Våpenbeholdning
             */
            //var tilgjengeligeVåpen = new List<Vaapen>();
            //tilgjengeligeVåpen.Add(new Vaapen { VaapenId = "Bombe", Beskrivelse = "kanskje den ødelegger noe?" });
            //tilgjengeligeVåpen.Add(new Vaapen { VaapenId = "ebmoB", Beskrivelse = "Bombe - stavet baklengs?" });

            //var posterForLag = new List<Object>
            //{
            //    new {Latitude = 59.676035, Longitude = 10.604844, Poengverdi = 100, Fullført = false},
            //    new {Latitude = 59.676135, Longitude = 10.604744, Poengverdi = 150, Fullført = false},
            //    new {Latitude = 59.676235, Longitude = 10.604644, Poengverdi = 100, Fullført = true}
            //};

            //return new
            //{
            //    Lag = LagId , 
            //    Poster = posterForLag,
            //    TilgjengeligeVåpen = tilgjengeligeVåpen,
            //    Score = 1234,
            //    Ranking = new Ranking()
            //};
        }
    }
}

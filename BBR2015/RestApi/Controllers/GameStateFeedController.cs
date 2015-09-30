using System;
using System.Collections.Generic;
using System.Web.Http.Cors;
using Modell;

namespace RestApi.Controllers
{
    public class GameStateFeedController : BaseController
    {
        // GET: api/GameStateFeed
        [EnableCors("*", "*", "*")]
        [RequireApiKey]
        public object Get()
        {
            /* FOR ETT LAG
             *Poster
             *m koorinat
             *m poeng
             *stemplet
             *
             * Sum poeng
             * Våpenbeholdning
             */
            var tilgjengeligeVåpen = new List<Våpen>();
            tilgjengeligeVåpen.Add(new Våpen("Bombe", "kanskje den ødelegger noe?"));
            tilgjengeligeVåpen.Add(new Våpen("ebmoB", "Bombe - stavet baklengs?"));

            var posterForLag = new List<Object>
            {
                new {Post = new Koordinat(59.676035, 10.604844), Poengverdi = 100, Fullført = false},
                new {Post = new Koordinat(59.676135, 10.604744), Poengverdi = 150, Fullført = false},
                new {Post = new Koordinat(59.676235, 10.604644), Poengverdi = 100, Fullført = true}
            };

            return new
            {
                Lag = LagId , 
                Status = "Not done with this response yet!",
                Poster = posterForLag,
                TilgjengeligeVåpen = tilgjengeligeVåpen
            };
        }
    }
}

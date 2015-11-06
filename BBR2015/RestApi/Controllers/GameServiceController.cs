using System;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using System.Web.Http.Results;
using Repository;
using RestApi.Filters;
using RestApi.Models;

namespace RestApi.Controllers
{
    [RequireApiKey]
    public class GameServiceController : BaseController
    {
        private GameService _gameService;

        public GameServiceController(GameService gameService)
        {
            _gameService = gameService;
        }
        /// <summary>
        /// Her registrerer laget poster de finner i terrenget. En trenger bare sende inn den hemmelige koden som står på posten. <br />
        /// Registreringen er designet asynkront, så det er ingen tilbakemelding på selve registrerings-requesten. <br />
        /// Effekten ser en i endret GameState. <br />
        /// Ved registrering kan en også velge å bruke et våpen. Et lag kan bare registrere samme post 1 gang.<br />
        /// Eksempel uten bruk av våpen: POST  { "postKode": "superhemmelig" }<br />
        /// Eksempel med bruk av våpen: POST  { "postKode": "superhemmelig", "våpen": "BOMBE" }
        /// </summary>
        /// <remarks>POST api/GameService/RegistrerNyPost</remarks>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden - Husk LagKode og DeltakerKode</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost]
        [ResponseType(typeof(OkResult))]
        public IHttpActionResult Post([FromBody] RegistrerNyPost registrerNyPost)
        {
            try
            {
                if (registrerNyPost == null)
                {
                    return BadRequest("RegistrerNyPost cannot be null");
                }

                if (registrerNyPost.PostKode == null)
                {
                    return BadRequest("Feil format. Send inn på formen  { postKode: 'hemmelig'}, ev med våpen - ikke noe mer rundt. Også Content-Type: application/json");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _gameService.RegistrerNyPost(DeltakerId, LagId, registrerNyPost.PostKode, registrerNyPost.BruktVåpen);
                
                return Ok();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Sletter alle postregistreringer i runden og tilbakestiller våpenbeholdningen.
        /// </summary>
        /// <returns></returns>
        [Route("api/GameService/RykkTilbakeTilStart")]
        [HttpPost]
        [ResponseType(typeof (OkResult))]
        public IHttpActionResult RykkTilbakeTilStart()
        {
            _gameService.Nullstill(LagId);
            return Ok();
        }
    }
}

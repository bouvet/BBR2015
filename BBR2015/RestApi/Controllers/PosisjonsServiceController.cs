using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using System.Web.Http.Results;
using Repository;
using Database.Entities;
using RestApi.Filters;
using RestApi.Models;

namespace RestApi.Controllers
{

   
    public class PosisjonsServiceController : BaseController
    {
        private PosisjonsService _posisjonsService;

        public PosisjonsServiceController(PosisjonsService posisjonsService)
        {
            _posisjonsService = posisjonsService;
        }

        /// <summary>
        /// Her melder hver av deltakerne inn sin posisjon fortløpende underveis i spillet ved å gjøre en POST på en koordinat av formen { "latitude": 59.676035, "longitude": 10.604844 } <br />
        /// Det er teknisk mulig å gjennomføre spillet uten å melde inn koordinater, men det vil bli gitt ekstrapoeng i form av en Achievement til lagene som melder dette inn. <br />
        /// Under spillet vil deltakernes posisjoner vises på et Scoreboard.
        /// </summary>
        /// <remarks>GET api/PosisjonsService</remarks>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden - Husk LagKode og DeltakerKode</response>
        /// <response code="500">Internal Server Error</response>
        [RequireApiKey]
        [Throttle]
        [ResponseType(typeof (DeltakerPosisjon))]
        [HttpGet]
        public IHttpActionResult Get()
        {
            try
            {
                return Ok(_posisjonsService.HentforLag(LagId).Posisjoner);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Henter ut posisjoner for alle spillere. Krever ekstra tilgangsnivå.
        /// </summary>
        /// <returns></returns>
        [RequireScoreboardSecret]
        [Route("api/PosisjonsService/Alle")]
        [ResponseType(typeof(List<LagPosisjoner>))]
        [HttpGet]
        public IHttpActionResult GetAlle()
        {
            try
            {
                var header = Request.Headers.SingleOrDefault(x => x.Key == "ScoreboardSecret");
                if (header.Value == null)
                    return NotFound();

                return Ok(_posisjonsService.HentforAlleLag(header.Value.FirstOrDefault()));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Her melder hver av deltakerne inn sin posisjon fortløpende underveis i spillet ved å gjøre en POST på en koordinat av formen { "latitude": 59.676035, "longitude": 10.604844 } <br />
        /// Det er teknisk mulig å gjennomføre spillet uten å melde inn koordinater, men det vil bli gitt ekstrapoeng i form av en Achievement til lagene som melder dette inn. <br />
        /// Under spillet vil deltakernes posisjoner vises på et Scoreboard.
        /// </summary>
        /// <remarks>POST api/PosisjonsService</remarks>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden - Husk LagKode og DeltakerKode</response>
        /// <response code="500">Internal Server Error</response>        
        [ResponseType(typeof(OkResult))]
        [Throttle]
        [RequireApiKey]
        [HttpPost]
        public IHttpActionResult Post([FromBody] Koordinat koordinat)
        {
            try
            {
                if (koordinat == null)
                {
                    return BadRequest("koordinat cannot be null");
                }

                if (koordinat.Longitude == 0.0 || koordinat.Latitude == 0.0)
                {
                    return BadRequest("Feil format. Send inn på formen { latitude: 59.676035, longitude: 10.604844 } - ikke noe mer rundt. Også Content-Type: application/json");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                DeltakerPosisjon deltakerPosisjon = _posisjonsService.RegistrerPosisjon(LagId,DeltakerId,koordinat.Latitude, koordinat.Longitude);
                if (deltakerPosisjon == null)
                {
                    return Conflict();
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
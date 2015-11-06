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

        // GET: api/PosisjonsService
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

        [RequireScoreboardSecret]
        [Route("api/PosisjonsService/Alle")]
        [ResponseType(typeof(List<LagPosisjoner>))]
        [HttpGet]
        [Obsolete]
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

        // POST: api/PosisjonsService
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
using System;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using System.Web.Http.Results;
using Modell;
using Repository;
using Database.Entities;

namespace RestApi.Controllers
{

    [EnableCors("*", "*", "*")]
    [RequireApiKey]
    public class PosisjonsServiceController : BaseController
    {
        private PosisjonsRepository _posisjonsRepository;

        public PosisjonsServiceController(PosisjonsRepository posisjonsRepository)
        {
            _posisjonsRepository = posisjonsRepository;
        }

        // GET: api/PosisjonsService
        [ResponseType(typeof (DeltakerPosisjon))]
        public IHttpActionResult Get()
        {
            try
            {
                return Ok(_posisjonsRepository.HentforLag(LagId));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST: api/PosisjonsService
        [ResponseType(typeof(OkResult))]
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

                DeltakerPosisjon deltakerPosisjon = _posisjonsRepository.RegistrerPosisjon(LagId,DeltakerId,koordinat);
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
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Http;
using Repository;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using System.Web.Http.Results;

namespace RestApi.Controllers
{

    public class NyMelding
    {
        [Required(ErrorMessage = "Meldingtekst is required", AllowEmptyStrings = false)]
        [MinLength(1, ErrorMessage = "Meldingtekst min length is 1 characters")]
        [MaxLength(256, ErrorMessage = "Meldingtekst max length is 256 characters")]
        public string Tekst { get; set; }
        public override string ToString()
        {
            return Tekst;
        }
    }

    [RequireApiKey]
    public class MeldingerController : BaseController
    {
        private MeldingService _meldingService;

        public MeldingerController(MeldingService meldingService)
        {
            _meldingService = meldingService;
        }
        // GET: api/Meldinger
        //[ResponseType(typeof (IEnumerable<Object>))]
        //public IHttpActionResult Get()
        //{
        //    long sekvensIfra = 0;
        //    int maksAntall = 10;
        //    return HttpActionResult(sekvensIfra, maksAntall);
        //}

        // GET: api/Meldinger
        [HttpGet]
        [ResponseType(typeof (IEnumerable<Object>))]
       
        public IHttpActionResult Get(long id)
        {
            return HttpActionResult(id);
        }

        private IHttpActionResult HttpActionResult(long sekvensIfra = 0, int maksAntall = int.MaxValue)
        {
            if (sekvensIfra == 0)
                maksAntall = 10;            

            try
            {
                //TODO: Allow message from Admin to all teams!
                var rawData = _meldingService.HentMeldinger(LagId, sekvensIfra, maksAntall).ToList();
                var meldinger = rawData.Select(m => new
                {
                    Sekvens = m.SekvensId,
                    TidspunktUtc = m.Tidspunkt,
                    Deltaker = m.DeltakerId,
                    Melding = m.Tekst
                }).OrderByDescending(m => m.Sekvens);

                var response = new {LagId = LagId, Meldinger = meldinger};
                return Ok(response);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST: api/Meldinger
        [HttpPost]
        [ResponseType(typeof(OkResult))]
        public IHttpActionResult Post([FromBody] NyMelding nyMelding)
        {
            try
            {
                if (nyMelding == null)
                {
                    return BadRequest("Melding cannot be null");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _meldingService.PostMelding(DeltakerId, LagId, nyMelding.Tekst);
               
                return Ok();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ResponseType(typeof(OkResult))]
        [RequireScoreboardSecret]
        public IHttpActionResult PostTilAlle([FromBody] NyMelding nyMelding)
        {
            try
            {
                if (nyMelding == null)
                {
                    return BadRequest("Melding cannot be null");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _meldingService.PostMeldingTilAlle(DeltakerId, LagId, nyMelding.Tekst);
               
                return Ok();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}

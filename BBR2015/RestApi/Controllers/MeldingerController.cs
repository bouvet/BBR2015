using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Modell;
using Repository;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using System.Web.Http.Results;

namespace RestApi.Controllers
{

    public class NyMelding
    {
        public string Tekst { get; set; }
        public override string ToString()
        {
            return Tekst;
        }
    }

    [EnableCorsAttribute("*", "*", "*")]
    [RequireApiKey]
    public class MeldingerController : BaseController
    {

        // GET: api/Meldinger
        [ResponseType(typeof (IEnumerable<Object>))]
        public IHttpActionResult Get(long sekvensIfra = 0)
        {
            try
            {
                var rawData = MeldingRepository.HentMeldinger(LagId, sekvensIfra).ToList();
                var meldinger = rawData.Select(m => new
                {
                    Sekvens = m.SekvensId,
                    TidspunktUtc = m.TidspunktUtc,
                    Deltaker = m.Deltaker.Navn,
                    Melding = m.Meldingtekst
                }).OrderByDescending(m=>m.Sekvens);

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

                Melding melding = MeldingRepository.PostMelding(DeltakerId, LagId, nyMelding.Tekst);
                if (melding == null)
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

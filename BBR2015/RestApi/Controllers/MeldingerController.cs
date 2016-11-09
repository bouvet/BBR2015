using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Http;
using Repository;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using System.Web.Http.Results;
using RestApi.Filters;

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


        /// <summary>
        /// Tjenesten tilbyr to funksjonaliteter: POST for å sende en ny melding på formen { "tekst": "Dette er en melding!" } eller GET for å hente lagets meldinger. <br />
        /// Meldinger som hentes ut har et sekvensnummer som kan brukes ved neste GET api/Meldinger/{sekvensnummer}. Dette kan brukes til å bare hente nye meldinger siden siste uthenting. Hvis en ikke angir sekvensnummer, får en de ti nyeste meldingene. Laget kan kommunisere på valgfri måte (f.eks. går rundt sammen og kommunisere muntlig), men det gis ekstrapoeng i form av en Achievement for å bruke spillets meldingstjeneste. <br />
        /// Spillets administrasjon vil bruke meldingstjenesten til å gi informasjon som kan gi fordeler i spillets gang. <br />
        /// NB: En melding kan være maksimalt 256 tegn lang. Det gjøres ikke noe filtrering av innhold, men siden meldingene bare går til de andre på laget, vil forsøk på Cross-Side-Scripting bare ramme de andre på laget.
        /// </summary>
        /// <remarks>GET api/Meldinger</remarks>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden - Husk LagKode og DeltakerKode</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        [ResponseType(typeof(IEnumerable<Object>))]
        [Throttle]
        [Route("api/Meldinger")]
        public IHttpActionResult Get()
        {
            return HttpActionResult("0");
        }

        /// <summary>
        /// Tjenesten tilbyr to funksjonaliteter: POST for å sende en ny melding på formen { "tekst": "Dette er en melding!" } eller GET for å hente lagets meldinger. <br />
        /// Meldinger som hentes ut har et sekvensnummer som kan brukes ved neste GET api/Meldinger/{sekvensnummer}. Dette kan brukes til å bare hente nye meldinger siden siste uthenting. Hvis en ikke angir sekvensnummer, får en de ti nyeste meldingene. Laget kan kommunisere på valgfri måte (f.eks. går rundt sammen og kommunisere muntlig), men det gis ekstrapoeng i form av en Achievement for å bruke spillets meldingstjeneste. <br />
        /// Spillets administrasjon vil bruke meldingstjenesten til å gi informasjon som kan gi fordeler i spillets gang. <br />
        /// NB: En melding kan være maksimalt 256 tegn lang. Det gjøres ikke noe filtrering av innhold, men siden meldingene bare går til de andre på laget, vil forsøk på Cross-Side-Scripting bare ramme de andre på laget.
        /// </summary>
        /// <remarks>GET api/Meldinger/{sekvensNr}</remarks>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden - Husk LagKode og DeltakerKode</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        [ResponseType(typeof (IEnumerable<Object>))]
        [Throttle]
        [Route("api/Meldinger/{sekvensNr}")]
        public IHttpActionResult Get(string sekvensNr)
        {
            return HttpActionResult(sekvensNr);
        }

        private IHttpActionResult HttpActionResult(string sekvensNr, int maksAntall = int.MaxValue)
        {
            long sekvensIfra;
            if (!long.TryParse(sekvensNr, out sekvensIfra))
                sekvensIfra = 0;

            if (sekvensIfra == 0)
                maksAntall = 10;            

            try
            {
                var rawData = _meldingService.HentMeldinger(LagId, sekvensIfra, maksAntall).ToList();
                var meldinger = rawData.Select(m => new
                {
                    Sekvens = m.SekvensId.ToString(),
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

        /// <summary>
        /// Tjenesten tilbyr to funksjonaliteter: POST for å sende en ny melding på formen { "tekst": "Dette er en melding!" } eller GET for å hente lagets meldinger. <br />
        /// Meldinger som hentes ut har et sekvensnummer som kan brukes ved neste GET api/Meldinger/{sekvensnummer}. Dette kan brukes til å bare hente nye meldinger siden siste uthenting. Hvis en ikke angir sekvensnummer, får en de ti nyeste meldingene. Laget kan kommunisere på valgfri måte (f.eks. går rundt sammen og kommunisere muntlig), men det gis ekstrapoeng i form av en Achievement for å bruke spillets meldingstjeneste. <br />
        /// Spillets administrasjon vil bruke meldingstjenesten til å gi informasjon som kan gi fordeler i spillets gang. <br />
        /// NB: En melding kan være maksimalt 256 tegn lang. Det gjøres ikke noe filtrering av innhold, men siden meldingene bare går til de andre på laget, vil forsøk på Cross-Side-Scripting bare ramme de andre på laget.
        /// </summary>
        /// <remarks>POST api/Meldinger</remarks>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden - Husk LagKode og DeltakerKode</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost]
        [ResponseType(typeof(OkResult))]
        [Throttle]
        [Route("api/Meldinger")]
        public IHttpActionResult Post([FromBody] NyMelding nyMelding)
        {
            try
            {
                if (nyMelding == null)
                {
                    return BadRequest("Melding cannot be null");
                }

                if (nyMelding.Tekst == null)
                {
                    return BadRequest("Feil format. Send inn på formen  { tekst: 'Dette er en melding!'} - ikke noe mer rundt. Også Content-Type: application/json");
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
      
        /// <summary>
        /// Poster en melding til alle lag. Dette er kun tilgjengelig for admin med hemmelig http header.
        /// </summary>
        /// <param name="nyMelding"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(OkResult))]
        [RequireScoreboardSecret]
        [Route("api/Meldinger/PostTilAlle")]
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

                _meldingService.PostMeldingTilAlle(DeltakerId, nyMelding.Tekst);
               
                return Ok();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }    
}

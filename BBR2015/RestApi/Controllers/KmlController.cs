using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Results;
using Repository;
using RestApi.Models;

namespace RestApi.Controllers
{
    public class KmlController : ApiController
    {
        private readonly KmlService _kmlService;

        public KmlController(KmlService kmlService)
        {
            _kmlService = kmlService;
        }

        [HttpGet]
        [ResponseType(typeof(string))]
        [Route("api/Kml/Export/{matchName}")]
        public IHttpActionResult Export(string matchName)
        {
            var response = _kmlService.GetKml();

            return Ok(response);
        }
    }
}
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Results;
using Repository;
using Repository.Kml;
using RestApi.Models;

namespace RestApi.Controllers
{
    public class KmlController : ApiController
    {
        private readonly KmlExport _kmlExport;

        public KmlController(KmlExport kmlExport)
        {
            _kmlExport = kmlExport;
        }

        [HttpGet]
        [ResponseType(typeof(string))]
        [Route("api/Kml/Export/{matchName}")]
        public IHttpActionResult Export(string matchName)
        {
            var response = _kmlExport.GetKml();

            return Ok(response);
        }
    }
}
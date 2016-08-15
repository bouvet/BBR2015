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

        /// <summary>
        /// Eksporterer replay av matchen som KML-format. Kan vises i Google Earth.
        /// </summary>
        /// <param name="matchName"></param>
        /// <returns></returns>
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
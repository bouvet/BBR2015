using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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
        public HttpResponseMessage Export()
        {
            var response = _kmlExport.GetKml();

            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(response)))
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");

            var filename = string.Format("replay_{0:yyyyMMdd}.kml", DateTime.Now);

            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = filename };

            return result;
        }
    }
}
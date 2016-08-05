
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Results;
using Database;
using Repository;
using Repository.Import;
using Repository.Kml;
using RestApi.Filters;

namespace RestApi.Controllers
{
    [RequireScoreboardSecret]
    public class AdminController : ApiController
    {
        private readonly GameStateService _gameStateService;
        private readonly OverridableSettings _appSettings;
        private readonly TilgangsKontroll _tilgangsKontroll;
        private readonly PosisjonsService _posisjonsService;
        private readonly ExcelImport _excelImport;
        private readonly KmlToExcelPoster _kmlToExcelPoster;
        private readonly ExcelWriter _excelWriter;

        public AdminController(GameStateService gameStateService, OverridableSettings appSettings, TilgangsKontroll tilgangsKontroll, PosisjonsService posisjonsService, ExcelImport excelImport, KmlToExcelPoster kmlToExcelPoster, ExcelWriter excelWriter)
        {
            _gameStateService = gameStateService;
            _appSettings = appSettings;
            _tilgangsKontroll = tilgangsKontroll;
            _posisjonsService = posisjonsService;
            _excelImport = excelImport;
            _kmlToExcelPoster = kmlToExcelPoster;
            _excelWriter = excelWriter;
        }

        [Route("api/Admin/RecalculateState")]
        [HttpPost]
        [Obsolete]
        public IHttpActionResult RecalculateState()
        {
            _gameStateService.Calculate();

            return Ok();
        }

        
        [Route("api/Admin/ThrowException")]
        [HttpPost]
        [Obsolete]
        public IHttpActionResult ThrowException()
        {
            throw new ApplicationException("Initiert av brukeren");           
        }

        [Route("api/Admin/ClearCaching")]
        [HttpPost]
        [Obsolete]
        public IHttpActionResult ClearCaching()
        {
            _gameStateService.Calculate();
            _tilgangsKontroll.Nullstill();
            _posisjonsService.Nullstill();
            ThrottleAttribute.Reload();
            return Ok();
        }

        // Post: api/Admin/RecalculateState
        [Route("api/Admin/ConnectionString")]
        [HttpGet]
        [Obsolete]
        public IHttpActionResult ConnectionString()
        {
            return Ok(_appSettings.DatabaseConnectionString);
        }

        [Route("api/Admin/hemmeligekoder")]
        [HttpGet]
        [Obsolete]
        public IHttpActionResult HemmeligeKoder()
        {
            return Ok(_tilgangsKontroll.HentAlleHemmeligeKoder());
        }

        [Route("api/Admin/DateTimeNow")]
        [HttpGet]
        [Obsolete]
        public IHttpActionResult DateTimeNow()
        {
            return Ok(TimeService.Now);
        }

        [Route("api/Admin/ConfigureFromGoogleDrive/{documentId}")]
        [HttpPost]
        [Obsolete]
        public async Task<IHttpActionResult> ConfigureFromGoogleDrive(string documentId)
        {
            var downloader = new GoogleDriveDownloader();

            var content = await downloader.LastNedSpreadsheetFraGoogleDrive(documentId);
            _excelImport.LesInn(Guid.Empty, content);

            return Ok();
        }

        [Route("api/Admin/ConvertMapToExcel/{documentId}")]
        [HttpPost]
        [Obsolete]
        public async Task<HttpResponseMessage> ConvertMapToExcel(string documentId)
        {
            var downloader = new GoogleDriveDownloader();

            var content = await downloader.LastNedMapFraGoogleDrive(documentId);
           
            var poster = _kmlToExcelPoster.LesInn(content);
            _excelWriter.SkrivPoster(poster);
            var bytes = _excelWriter.GetAsByteArray();

            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(new MemoryStream(bytes))
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.ms-excel");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") {FileName = "kml_poster.xlsx"};

            return result;
        }

        [Route("api/Admin/Configure/{matchId}")]
        [HttpPost]
        [Obsolete]
        public async Task<IHttpActionResult> Configure(string matchId)
        {
            Guid matchIdGuid;

            if (!Guid.TryParse(matchId, out matchIdGuid))
                return BadRequest("Parameter 'matchId' må være en gyldig GUID. Hvis det er ny match, må du likevel oppgi en gyldig - og ny - GUID.");

            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Du må poste en fil til denne metoden.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            if (!provider.Contents.Any())
                return BadRequest("Fant ingen fil i posten...");

            var file = provider.Contents.First();
            var excelBytes = await file.ReadAsByteArrayAsync();

            _excelImport.LesInn(matchIdGuid, excelBytes);

            return Ok("Takk for nytt oppsett!");
        }
    }
}
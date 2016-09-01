
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
using Database.Infrastructure;
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
        private readonly ExcelExport _excelExport;
        private readonly DataContextFactory _dataContextFactory;

        public AdminController(GameStateService gameStateService, OverridableSettings appSettings, TilgangsKontroll tilgangsKontroll, PosisjonsService posisjonsService, ExcelImport excelImport, KmlToExcelPoster kmlToExcelPoster, ExcelWriter excelWriter, ExcelExport excelExport, DataContextFactory dataContextFactory)
        {
            _gameStateService = gameStateService;
            _appSettings = appSettings;
            _tilgangsKontroll = tilgangsKontroll;
            _posisjonsService = posisjonsService;
            _excelImport = excelImport;
            _kmlToExcelPoster = kmlToExcelPoster;
            _excelWriter = excelWriter;
            _excelExport = excelExport;
            _dataContextFactory = dataContextFactory;
        }

        /// <summary>
        /// Tvinger en rekalkulering av gamestate. Kan brukes hvis en går rett i databasen for å fikse på ting, og vil ha endringer ut til spillere.
        /// </summary>
        /// <returns></returns>
        [Route("api/Admin/RecalculateState")]
        [HttpPost]
        public IHttpActionResult RecalculateState()
        {
            _gameStateService.Calculate();

            return Ok();
        }

        
        /// <summary>
        /// Støttemetode for å teste logging. Gjør at applikasjonen går i lufta...
        /// </summary>
        [Route("api/Admin/ThrowException")]
        [HttpPost]
        public IHttpActionResult ThrowException()
        {
            throw new ApplicationException("Initiert av brukeren");           
        }

        /// <summary>
        /// Støttemetode i tilfelle en fikk problemer, eller ville fikse ting rett i databasen. Ved å kjøre denne metoden pusher en ut oppdateringer som i utgangspunktet ville blitt cachet til f.eks. neste postregistrering.
        /// </summary>
        [Route("api/Admin/ClearCaching")]
        [HttpPost]
        public IHttpActionResult ClearCaching()
        {
            _gameStateService.Calculate();
            _tilgangsKontroll.Nullstill();
            _posisjonsService.Nullstill();
            ThrottleAttribute.Reload();
            return Ok();
        }

        [Route("api/Admin/ClearCachingTilgang")]
        [HttpPost]
        public IHttpActionResult ClearCachingTilgang()
        {
            _tilgangsKontroll.Nullstill();
            return Ok();
        }

        /// <summary>
        /// Sletter alle data
        /// </summary>
        [Route("api/Admin/DeleteAllData")]
        [HttpPost]
        [Obsolete("Skal være udokumentert")]
        public IHttpActionResult DeleteAllData()
        {
            if(!_appSettings.TillatSlettAlleData)
                return BadRequest();

            _dataContextFactory.DeleteAllData();

            ClearCaching();

            return Ok();
        }

        /// <summary>
        /// Henter ut applikasjonens ConnectionString for å dobbeltsjekke oppsettet uten å måtte gå inn på serveren.
        /// </summary>
        /// <returns>ConnectionString</returns>
        [Route("api/Admin/ConnectionString")]
        [HttpGet]
        public IHttpActionResult ConnectionString()
        {
            return Ok(_appSettings.DatabaseConnectionString);
        }

        /// <summary>
        /// Henter ut alle hemmelige koder for alle spillere og lag. For testing.
        /// </summary>
        /// <returns></returns>
        [Route("api/Admin/hemmeligekoder")]
        [HttpGet]
        public IHttpActionResult HemmeligeKoder()
        {
            return Ok(_tilgangsKontroll.HentAlleHemmeligeKoder());
        }

        /// <summary>
        /// Henter ut alle hemmelige koder for alle spillere og lag. For testing.
        /// </summary>
        /// <returns></returns>
        [Route("api/Admin/kodekombinasjoner")]
        [HttpGet]
        public IHttpActionResult KodeKombinasjoner()
        {
            return Ok(_tilgangsKontroll.HentAlleKodeKombinasjoner());
        }

        [Route("api/Admin/tilgangskontrollhash")]
        [HttpGet]
        public IHttpActionResult TilgangskontrollHashCode()
        {
            return Ok(_tilgangsKontroll.GetHashCode());
        }

        [Route("api/Admin/tilgangskontrollhashcontainer")]
        [HttpGet]
        public IHttpActionResult TilgangskontrollHashCodeContainer()
        {
            return Ok(ServiceLocator.Current.Resolve<TilgangsKontroll>().GetHashCode());
        }

        /// <summary>
        /// Hjelpemetode for å dobbeltsjekke at serveren har samme tidssone som du forventer.
        /// </summary>
        /// <returns>DateTime.Now</returns>
        [Route("api/Admin/DateTimeNow")]
        [HttpGet]
        public IHttpActionResult DateTimeNow()
        {
            return Ok(TimeService.Now);
        }

        /// <summary>
        /// Leser inn et Google Regneark på importformatet og oppretter eller oppdaterer en match ut fra det.
        /// </summary>
        /// <param name="documentId">Id til regneark på Google Drive. Se argument i Googles-url.</param>
        /// <returns></returns>
        [Route("api/Admin/ConfigureFromGoogleDrive/{documentId}")]
        [HttpPost]
        public async Task<IHttpActionResult> ConfigureFromGoogleDrive(string documentId)
        {
            var downloader = new GoogleDriveDownloader();

            var content = await downloader.LastNedSpreadsheetFraGoogleDrive(documentId);
            _excelImport.LesInn(content);

            ClearCaching();

            return Ok();
        }

        /// <summary>
        /// Konverterer et kart på Google Drive til poster. 
        /// Triks: Poengfordeling settes i beskrivelse i klammeparenteser: [100,80,60].
        /// Hemmelig kode settes i beskrivelse i krøllparenteser: {xdUFF43}
        /// Ev. bilde som er koblet opp, blir lagt på posten.
        /// Kartet må være satt opp med linkdeling og lesetilgang for alle med linken.
        /// </summary>
        /// <param name="documentId">Id til kart på Google Drive. Se argument 'mid' i url.</param>
        [Route("api/Admin/ConvertMapToExcel/{documentId}")]
        [HttpPost]
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

        /// <summary>
        /// Eksporterer oppsettet for en match på Excel-format. Bruker samme format som kreves for import. Dvs. at en kan sende inn matchId=hvasomhelst for å få ut en mal-fil.
        /// </summary>
        /// <param name="matchId"></param>
        /// <returns>Byte stream av Excel-fil.</returns>
        [Route("api/Admin/ExportToExcel/{matchId}")]
        [HttpPost]
        public HttpResponseMessage ExportToExcel(string matchId)
        {
            //if (string.IsNullOrEmpty(matchId))
            //    throw new ArgumentException("matchId");

            Guid matchGuid;
            if (!Guid.TryParse(matchId, out matchGuid))
                matchGuid = Guid.Empty;

            var bytes = _excelExport.ToByteArray(matchGuid);

            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(new MemoryStream(bytes))
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.ms-excel");

            var filename = string.Format("oppsett_{0:N}_{1:yyyyMMdd}_{1:HHmmss}.xlsx", matchId, DateTime.Now);

            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = filename };

            return result;
        }

        /// <summary>
        /// Tilbyr å opprette eller oppdatere en match ved å gjøre HTTP POST av en Excel-fil med oppsett av match, lag, deltakere og poster.
        /// </summary>
        [Route("api/Admin/Configure")]
        [HttpPost]
        public async Task<IHttpActionResult> Configure()
        {           
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Du må poste en fil til denne metoden.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            if (!provider.Contents.Any())
                return BadRequest("Fant ingen fil i posten...");

            var file = provider.Contents.First();
            var excelBytes = await file.ReadAsByteArrayAsync();

            _excelImport.LesInn(excelBytes);

            ClearCaching();

            return Ok("Takk for nytt oppsett!");
        }
    }
}
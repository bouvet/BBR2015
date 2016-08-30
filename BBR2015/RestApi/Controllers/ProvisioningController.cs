using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Database;
using Database.Infrastructure;
using Repository;

namespace RestApi.Controllers
{
    public class ProvisioningController : Controller
    {
        private readonly OverridableSettings _settings;
        private readonly TilgangsKontroll _tilgangsKontroll;
        private readonly LagOppstillingService _lagOppstillingService;
        private readonly CurrentMatchProvider _currentMatchProvider;

        public ProvisioningController(OverridableSettings settings, LagOppstillingService lagOppstillingService, CurrentMatchProvider currentMatchProvider)
        {
            _settings = settings;
            _tilgangsKontroll = ServiceLocator.Current.Resolve<TilgangsKontroll>();
            _lagOppstillingService = lagOppstillingService;
            _currentMatchProvider = currentMatchProvider;
        }

        public ActionResult Index(string id)
        {
            if (!_tilgangsKontroll.ErGyldigMatchId(id))
            {
                var currentMatch = _currentMatchProvider.GetMatchId();

                if (currentMatch != Guid.Empty)
                {
                    id = currentMatch.ToString();
                }
                else
                {
                    return RedirectTilForsiden("Ugyldig id for match i url. Sjekk linken.");
                }
            }

            var indexModel = new IndexModel
            {
                TillatNyttLag = _settings.TillatOpprettNyttLag,
                TillatNySpiller = _settings.TillatOpprettNySpiller,
                MatchId = id
            };

            return View(indexModel);
        }

        public ActionResult NySpiller()
        {
            var model = new NySpillerModel();
            return View(model);
        }

        private ActionResult RedirectTilForsiden(string melding, bool erFeilmelding = true)
        {
            var indexModel = new IndexModel
            {
                TillatNyttLag = _settings.TillatOpprettNyttLag,
                TillatNySpiller = _settings.TillatOpprettNySpiller,
                Melding = melding,
                ErFeilmelding = erFeilmelding
            };

            return View("Index", indexModel);
        }

        [HttpPost]
        public ActionResult NySpiller(NySpillerModel model)
        {
            try
            {
                if (!_tilgangsKontroll.ErLagKodeIBruk(model.HemmeligKodeForLag))
                    ModelState.AddModelError("HemmeligKodeForLag", "Ukjent lagkode.");

                if (_tilgangsKontroll.ErDeltakerKodeIBruk(model.KodeForSpiller))
                    ModelState.AddModelError("KodeForSpiller", "Deltakerens kode er opptatt. Bruk en annen verdi.");

                if (!ModelState.IsValid)
                    return View(model);

                _lagOppstillingService.OpprettNySpiller(model.HemmeligKodeForLag, model.KodeForSpiller, model.Navn);

                return RedirectTilForsiden(string.Format($"Ny spiller er opprettet. Bruk LagKode: '{model.HemmeligKodeForLag}' og DeltakerKode: '{model.KodeForSpiller}' i spillet."), false);
            }
            catch
            {
                return View(model);
            }
        }

        public ActionResult NyttLag(string id)
        {
            if (!_tilgangsKontroll.ErGyldigMatchId(id))
            {
                return RedirectTilForsiden("Ugyldig id for match i url. Sjekk linken.");
            }

            var model = new NyttLagModel { MatchId = Guid.Parse(id) };
            return View(model);
        }

        [HttpPost]
        public ActionResult NyttLag(NyttLagModel model)
        {
            try
            {
                if (_tilgangsKontroll.ErLagKodeIBruk(model.HemmeligKode))
                    ModelState.AddModelError("HemmeligKode", "Den hemmelige koden er opptatt. Finn på en annen, litt mer hemmelig, kode.");

                if (!ModelState.IsValid)
                    return View(model);

                _lagOppstillingService.OpprettNyttLag(model.MatchId, model.HemmeligKode, model.Navn);

                return RedirectTilForsiden(string.Format($"Nytt lag er opprettet. Bruk lagkoden: '{model.HemmeligKode}' for opprettelse av ny deltaker. Se link under."), false);
            }
            catch
            {
                return View(model);
            }
        }

        public class IndexModel
        {
            public string Melding { get; set; }
            public bool TillatNyttLag { get; set; }
            public bool TillatNySpiller { get; set; }

            public bool ErFeilmelding { get; set; } = true;

            public bool HarMelding => !string.IsNullOrEmpty(Melding);
            public string MatchId { get; set; }
        }

        public class NyttLagModel
        {
            public Guid MatchId { get; set; }

            [Required(ErrorMessage = "Påkrevd")]
            [MinLength(3, ErrorMessage = "Navn må være minst 3 tegn")]
            [MaxLength(50, ErrorMessage = "Navn kan være maks 50 tegn")]
            public string Navn { get; set; }

            [Required(ErrorMessage = "Påkrevd")]
            [MinLength(5, ErrorMessage = "Koden må være minst 5 tegn")]
            [MaxLength(50, ErrorMessage = "Koden kan være maks 50 tegn")]
            public string HemmeligKode { get; set; }
        }

        public class NySpillerModel
        {         
            [Display(Name = "Lagets kode")]
            [Required(ErrorMessage = "Påkrevd")]
            public string HemmeligKodeForLag { get; set; }

            [Display(Name = "Deltakerens kode (e-post)")]
            [Required(ErrorMessage = "Påkrevd")]
            [EmailAddress(ErrorMessage = "Ugyldig e-postadresse")]
            [MinLength(6, ErrorMessage = "E-post må være minst 6 tegn")]
            [MaxLength(100, ErrorMessage = "E-post kan være maks 100 tegn")]
            public string KodeForSpiller { get; set; }

            [Display(Name = "Deltakerens navn")]
            [Required(ErrorMessage = "Påkrevd")]
            [MinLength(1, ErrorMessage = "Navn må være minst 1 tegn")]
            [MaxLength(50, ErrorMessage = "Navn kan være maks 50 tegn")]
            public string Navn { get; set; }
        }
    }
}

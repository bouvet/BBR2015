using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Database;
using Repository;

namespace RestApi.Controllers
{
    public class ProvisioningController : Controller
    {
        private readonly OverridableSettings _settings;
        private readonly DataContextFactory _dataContextFactory;
        private readonly TilgangsKontroll _tilgangsKontroll;

        public ProvisioningController(OverridableSettings settings, DataContextFactory dataContextFactory, TilgangsKontroll tilgangsKontroll)
        {
            _settings = settings;
            _dataContextFactory = dataContextFactory;
            _tilgangsKontroll = tilgangsKontroll;
        }

        // GET: Provision/NyttLag
        public ActionResult Index(string id)
        {
            var indexModel = new IndexModel
            {
                TillatNyttLag = _settings.TillatOpprettNyttLag,
                TillattNySpiller = _settings.TillatOpprettNySpiller
            };

            return View(indexModel);
        }

        // GET: Provision/Create
        public ActionResult NySpiller(string id)
        {
            if (!_tilgangsKontroll.ErGyldigMatchId(id))
            {
                return RedirectTilForsiden("Ugyldig id for match i url. Sjekk linken.");
                
            }

            var model = new NySpillerModel { MatchId = Guid.Parse(id) };
            return View(model);
        }

        private ActionResult RedirectTilForsiden(string melding)
        {
            var indexModel = new IndexModel
            {
                TillatNyttLag = _settings.TillatOpprettNyttLag,
                TillattNySpiller = _settings.TillatOpprettNySpiller,
                Melding = melding
            };

            return View("Index", indexModel);
        }

        // POST: Provision/NyttLag
        [HttpPost]
        public ActionResult NySpiller(NySpillerModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View(model);
            }
        }

        // GET: Provision/Edit/5
        public ActionResult NyttLag(Guid id)
        {
            var model = new NyttLagModel { MatchId = id };
            return View(model);
        }

        // POST: Provision/Edit/5
        [HttpPost]
        public ActionResult NyttLag(NyttLagModel model)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
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
            public bool TillattNySpiller { get; set; }

            public bool HarMelding => !string.IsNullOrEmpty(Melding);
        }

        public class NyttLagModel
        {
            public Guid MatchId { get; set; }
            public string Navn { get; set; }
            public string HemmeligKode { get; set; }
        }

        public class NySpillerModel
        {
            [Required]
            public Guid MatchId { get; set; }

            [Display(Name="Lagets kode")]
            [Required(ErrorMessage = "Påkrevd")]
            public string HemmeligKodeForLag { get; set; }

            [Display(Name = "Deltakerens kode (e-post)")]
            [Required(ErrorMessage = "Påkrevd")]
            [EmailAddress(ErrorMessage = "Ugyldig e-postadresse")]
            public string KodeForSpiller { get; set; }

            [Display(Name = "Deltakerens navn")]
            [Required(ErrorMessage = "Påkrevd")]
            public string Navn { get; set; }
        }
    }
}

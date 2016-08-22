using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Database;

namespace RestApi.Controllers
{
    public class ProvisioningController : Controller
    {
        private readonly OverridableSettings _settings;
        private readonly DataContextFactory _dataContextFactory;

        public ProvisioningController(OverridableSettings settings, DataContextFactory dataContextFactory)
        {
            _settings = settings;
            _dataContextFactory = dataContextFactory;
        }

        // GET: Provision/NyttLag
        public ActionResult Index(string matchId)
        {
            var indexModel = new IndexModel
            {
                TillatNyttLag = _settings.TillatOpprettNyttLag,
                TillattNySpiller = _settings.TillatOpprettNySpiller
            };

            return View(indexModel);
        }

        // GET: Provision/Create
        public ActionResult NySpiller(Guid matchId)
        {
            var model = new NySpillerModel { MatchId = matchId };
            return View(model);
        }

        // POST: Provision/NyttLag
        [HttpPost]
        public ActionResult NySpiller(NySpillerModel model)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View(model);
            }
        }

        // GET: Provision/Edit/5
        public ActionResult NyttLag(Guid matchId)
        {
            var model = new NyttLagModel { MatchId = matchId };
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
        }

        public class NyttLagModel
        {
            public Guid MatchId { get; set; }
            public string Navn { get; set; }
            public string HemmeligKode { get; set; }
        }

        public class NySpillerModel
        {
            public Guid MatchId { get; set; }
            public string HemmeligKodeForLag { get; set; }
            public string KodeForSpiller { get; set; }
            public string Navn { get; set; }
        }
    }
}

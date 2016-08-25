using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Database;
using Repository;

namespace RestApi.Controllers
{
    public class IndexController : Controller
    {
        private readonly OverridableSettings _settings;
        private readonly CurrentMatchProvider _currentMatchProvider;

        public IndexController(OverridableSettings settings, CurrentMatchProvider currentMatchProvider)
        {
            _settings = settings;
            _currentMatchProvider = currentMatchProvider;
        }

        // GET: Index
        public ActionResult Index()
        {
            var model = new IndexModel
            {
                TillatNyttLag = _settings.TillatOpprettNyttLag,
                TillatNySpiller = _settings.TillatOpprettNySpiller,
                MatchId = _currentMatchProvider.GetMatchId().ToString(),
                EksternInfoUrl = _settings.EksternInfoUrl,
                TestklientUrl = _settings.TestklientUrl
            };

            return View(model);
        }
    }

    public class IndexModel
    {
        
        public bool TillatNyttLag { get; set; }
        public bool TillatNySpiller { get; set; }
        
        public string MatchId { get; set; }
        public string EksternInfoUrl { get; set; }
        public string TestklientUrl { get; set; }
    }
}
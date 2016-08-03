using System;
using System.Linq;
using Database;
using NUnit.Framework;
using OfficeOpenXml;
using Repository.Import;

namespace RestApi.Tests.ExcelImport
{
    public class MatchImportTests : ExcelImportTestsBase
    {
        /// - Match: Ny
        /// - Match: Oppdatering
        /// - Match: Våpenoppretting
        /// 
        /// Lag_Nye_lag_skal_legges_inn
        /// Lag_Fjernede_lag_skal_slettes
        /// Lag_skal_oppdateres
        /// 
        /// Deltakere_Nye_skal_legges_inn
        /// Deltakere_skal_oppdateres
        /// Deltakere_bytte_lag
        /// Deltakere_skal_kunne_slettes_hvis_ikke_registrert_data
        /// Deltakere_med_ugyldig_lagid_skal_ignoreres
        /// 
        /// Poster_nye
        /// Poster_flytte_posisjon
        /// Poster_endre_navn
        /// Poster_endre_poengarray
        public void Tester() { }

        [Test]
        public void Match_ny_skal_legges_inn()
        {
            var match = GetMatch();

            Importer(match);

            using (var context = _dataContextFactory.Create())
            {
                var ny = context.Matcher.Single(x => x.MatchId == match.MatchId);

                Assert.AreEqual(match.Navn, ny.Navn, "Navn");
                Assert.AreEqual(match.StartTid, ny.StartTid, "Starttid");
                Assert.AreEqual(match.SluttTid, ny.SluttTid, "Sluttid");
            }
        }

        [Test]
        public void Import_av_to_filer_med_ulike_matcher_SkalGiToMatcherIDatabasen()
        {
            var match = GetMatch();

            Importer(match);

            var match2 = GetMatch();

            Importer(match2);

            using (var context = _dataContextFactory.Create())
            {
                Assert.AreEqual(2, context.Matcher.Count(), "Begge skal importeres");
            }
        }

        [Test]
        public void Match_NårEnMatchImporteresToGangerMedSammeIdSkalDenOppdateres()
        {
            var match = GetMatch();

            Importer(match);

            match.Navn = "Nytt navn";
            match.StartTid = match.StartTid.AddDays(-1);
            match.SluttTid = match.SluttTid.AddDays(1);

            Importer(match);

            using (var context = _dataContextFactory.Create())
            {
                Assert.AreEqual(1, context.Matcher.Count(), "Count");

                var m = context.Matcher.First();
                Assert.AreEqual(match.Navn, m.Navn, "Navn");
                Assert.AreEqual(match.StartTid, m.StartTid, "Starttid");
                Assert.AreEqual(match.SluttTid, m.SluttTid, "Sluttid");
            }
        }
       
        [Test]
        public void NårEnMatchImporteresFlereGangerITomDatabase_SkalVåpenDefinisjonerOpprettesBareEnGang()
        {
            using (var context = _dataContextFactory.Create())
            {
                Assert.AreEqual(0, context.Våpen.Count(), "Det skal ikke være noen våpen før import");
            }

            var match = GetMatch();

            Importer(match);

            Importer(match);

            using (var context = _dataContextFactory.Create())
            {
                Assert.AreEqual(2, context.Våpen.Count(), "En oppretter bare en rad av hvert våpen");
                Assert.AreEqual(1, context.Våpen.Count(x => x.VaapenId == Constants.Våpen.Felle), "Felle");
                Assert.AreEqual(1, context.Våpen.Count(x => x.VaapenId == Constants.Våpen.Bombe), "Bombe");
            }
        }
        
    }
}
using System.Linq;
using NUnit.Framework;
using RestApi.Tests.Infrastructure;
using System.Data.Entity;
using Database;

namespace RestApi.Tests.ExcelImport
{
    [TestFixture]
    public class LagImportTests : ExcelImportTestsBase
    {
        [Test]
        public void NyeLagSkalImporteresMedAlleProperties()
        {
            var match = GetMatch();

            var lag = LagFactory.SettOppLagMedDeltakere(2, 0, "LAG_");

            Importer(match, lag);

            using (var context = _dataContextFactory.Create())
            {
                Assert.AreEqual(2, context.Lag.Count(), "Antall lag");

                var l = context.Lag.First(x => x.LagId == "LAG_1");

                Assert.AreEqual("LAG_1", l.LagId, "LagId");
                Assert.AreEqual("LAG_Navn1", l.Navn, "Navn");
                Assert.AreEqual("LAG_Farge1", l.Farge, "Farge");
                Assert.AreEqual("LAG_HemmeligKode1", l.HemmeligKode, "HemmeligKode");               
                Assert.AreEqual("LAG_Ikon1.gif", l.Ikon, "Ikon");               
            }
        }

        [Test]
        public void NyeLagSkalKoblesTilMatchen()
        {
            var match = GetMatch();

            var lag = LagFactory.SettOppLagMedDeltakere(2, 0, "LAG_");

            Importer(match, lag);

            using (var context = _dataContextFactory.Create())
            {
                var m = context.Matcher.Include(x => x.DeltakendeLag).Single();

                Assert.AreEqual(2, m.DeltakendeLag.Count(), "Antall lag koblet til match");
            }
        }

        [Test]
        public void NårToLagMedSammeIdImporteresISammeFil_SkalSisteRadGjelde()
        {
            var match = GetMatch();

            var lag = LagFactory.SettOppLagMedDeltakere(2, 0, "LAG_");

            lag.ForEach(x => x.LagId = "LAGID_SAMME");

            Importer(match, lag);

            using (var context = _dataContextFactory.Create())
            {
                Assert.AreEqual(1, context.Lag.Count(), "Antall lag");

                var l = context.Lag.First(x => x.LagId == "LAGID_SAMME");

                Assert.AreEqual("LAG_Navn2", l.Navn, "Siste endring skal gjelde");
            }
        }

        [Test]
        public void OppdateringAvLag_SkalOppdatereProperties()
        {
            var match = GetMatch();

            var lag = LagFactory.SettOppLagMedDeltakere(1, 0, "LAG_");

            Importer(match, lag);

            var laget = lag.Single();

            laget.Navn = "Endret navn";
            laget.HemmeligKode = "Endret kode";
            laget.Farge = "Endret farge";

            Importer(match, lag);

            using (var context = _dataContextFactory.Create())
            {
                Assert.AreEqual(1, context.Lag.Count(), "Antall lag");

                var l = context.Lag.First(x => x.LagId == "LAG_1");

                Assert.AreEqual("Endret navn", l.Navn, "Navn");
                Assert.AreEqual("Endret farge", l.Farge, "Farge");
                Assert.AreEqual("Endret kode", l.HemmeligKode, "HemmeligKode");

                var m = context.Matcher.Include(x => x.DeltakendeLag).Single();

                Assert.AreEqual(1, m.DeltakendeLag.Count(), "Antall lag koblet til match skal fremdeles være 1");
            }
        }

        [Test]
        public void LagSkalFåVåpenFraMatchConfig()
        {
            var match = GetMatch();

            match.PrLagFelle = 99;
            match.PrLagBombe = 88;

            var lag = LagFactory.SettOppLagMedDeltakere(1, 0, "LAG_");

            Importer(match, lag);

            using (var context = _dataContextFactory.Create())
            {
                var m = context.Matcher.Include(x => x.DeltakendeLag.Select(y => y.VåpenBeholdning)).Single();
                var l = m.DeltakendeLag.Single();

                Assert.AreEqual(99, l.VåpenBeholdning.Count(x => x.VaapenId == Constants.Våpen.Felle ), "Feller");
                Assert.AreEqual(88, l.VåpenBeholdning.Count(x => x.VaapenId == Constants.Våpen.Bombe ), "Bomber");
            }
        }

        [Test]
        public void LagSkalIkkeFåFlereVåpenFraMatchConfigVedEtterfølgendeImporter()
        {
            var match = GetMatch();

            match.PrLagFelle = 99;
            match.PrLagBombe = 88;

            var lag = LagFactory.SettOppLagMedDeltakere(1, 0, "LAG_");

            Importer(match, lag);
            Importer(match, lag);

            using (var context = _dataContextFactory.Create())
            {
                var m = context.Matcher.Include(x => x.DeltakendeLag.Select(y => y.VåpenBeholdning)).Single();
                var l = m.DeltakendeLag.Single();

                Assert.AreEqual(99, l.VåpenBeholdning.Count(x => x.VaapenId == Constants.Våpen.Felle), "Feller");
                Assert.AreEqual(88, l.VåpenBeholdning.Count(x => x.VaapenId == Constants.Våpen.Bombe), "Bomber");
            }
        }

        [Test]
        public void LagSkalIkkeFåVåpenHvisMatchConfigErTom()
        {
            var match = GetMatch();

            match.PrLagFelle = null;
            match.PrLagBombe = null;

            var lag = LagFactory.SettOppLagMedDeltakere(1, 0, "LAG_");

            Importer(match, lag);

            using (var context = _dataContextFactory.Create())
            {
                var m = context.Matcher.Include(x => x.DeltakendeLag.Select(y => y.VåpenBeholdning)).Single();
                var l = m.DeltakendeLag.Single();

                Assert.AreEqual(0, l.VåpenBeholdning.Count(x => x.VaapenId == Constants.Våpen.Felle), "Feller");
                Assert.AreEqual(0, l.VåpenBeholdning.Count(x => x.VaapenId == Constants.Våpen.Bombe), "Bomber");
            }
        }

        [Test]
        public void LagSkalIkkeFåVåpenHvisMatchConfigErTalletNull()
        {
            var match = GetMatch();

            match.PrLagFelle = 0;
            match.PrLagBombe = 0;

            var lag = LagFactory.SettOppLagMedDeltakere(1, 0, "LAG_");

            Importer(match, lag);

            using (var context = _dataContextFactory.Create())
            {
                var m = context.Matcher.Include(x => x.DeltakendeLag.Select(y => y.VåpenBeholdning)).Single();
                var l = m.DeltakendeLag.Single();

                Assert.AreEqual(0, l.VåpenBeholdning.Count(x => x.VaapenId == Constants.Våpen.Felle), "Feller");
                Assert.AreEqual(0, l.VåpenBeholdning.Count(x => x.VaapenId == Constants.Våpen.Bombe), "Bomber");
            }
        }

        [Test]
        [Ignore("Skal vi ha dette?")]
        public void FjerningAvLag_SkalFjerneKoblingMotMatch()
        {
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RestApi.Tests.Infrastructure;
using System.Data.Entity;
using Database.Entities;

namespace RestApi.Tests.ExcelImport
{
    [TestFixture]
    public class DeltakerImportTests : ExcelImportTestsBase
    {
        [Test]
        public void Deltakere_SkalImporteres()
        {
            var match = GetMatch();
            var lag = LagFactory.SettOppLagMedDeltakere(1, 2, "LAG_");

            Importer(match, lag);

            using (var context = _dataContextFactory.Create())
            {
                var deltakere = context.Deltakere.Include(x => x.Lag).ToList();

                Assert.AreEqual(2, deltakere.Count, "Riktig antall");

                var d = deltakere.OrderBy(x => x.Navn).First();

                Assert.AreEqual("LAG_1-1Navn", d.Navn, "Navn");
                Assert.AreEqual("Kode1_1", d.Kode, "Kode");
                Assert.AreEqual("LAG_1", d.Lag.LagId, "LagId");
            }
        }

        [Test]
        public void Deltakere_SkalIKoblesTilLag()
        {
            var match = GetMatch();
            var lag = LagFactory.SettOppLagMedDeltakere(1, 2, "LAG_");

            Importer(match, lag);

            using (var context = _dataContextFactory.Create())
            {
                var lagFraDb = context.Lag.Include(x => x.Deltakere).Single();

                Assert.AreEqual(2, lagFraDb.Deltakere.Count, "Riktig antall");
            }
        }

        [Test]
        public void Deltaker_BytteLag()
        {
            var match = GetMatch();
            var lag = LagFactory.SettOppLagMedDeltakere(2, 1, "LAG_");

            Importer(match, lag);

            var deltaker1 = lag[0].Deltakere[0];
            lag[0].Deltakere.Clear();
            lag[1].Deltakere.Add(deltaker1);

            Importer(match, lag);

            using (var context = _dataContextFactory.Create())
            {
                Assert.AreEqual(2, context.Deltakere.Count(), "Deltakere");
                Assert.AreEqual(0, context.Lag.Include(x => x.Deltakere).Single(x => x.LagId =="LAG_1").Deltakere.Count(), "Deltakere LAG_1");
                Assert.AreEqual(2, context.Lag.Include(x => x.Deltakere).Single(x => x.LagId =="LAG_2").Deltakere.Count(), "Deltakere LAG_2");
            }
        }

        [Test]
        public void Deltakere_DuplikaterPåNavn_SisteRadGjelder()
        {
            var match = GetMatch();
            var lag = LagFactory.SettOppLagMedDeltakere(1, 2, "LAG_");

            // Sett samme navn på alle deltakere
            lag.Single().Deltakere.ForEach(x => x.Navn = "Samme navn");

            Importer(match, lag);

            using (var context = _dataContextFactory.Create())
            {
                var deltakere = context.Deltakere.Include(x => x.Lag).ToList();

                Assert.AreEqual(1, deltakere.Count, "Riktig antall");

                var d = deltakere.First();

                //Assert.AreEqual("LAG_1-1", d.DeltakerId, "DeltakerId");
                Assert.AreEqual("Samme navn", d.Navn, "Navn");
                Assert.AreEqual("Kode1_2", d.Kode, "Kode");
                Assert.AreEqual("LAG_1", d.Lag.LagId, "LagId");
            }
        }
    }
}
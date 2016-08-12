using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Repository.Import;
using RestApi.Tests.Infrastructure;

namespace RestApi.Tests.ExcelImport
{
    [TestFixture]
    public class ExcelExportTests : ExcelImportTestsBase
    {
        private Repository.Import.ExcelImport _excelImport;
        private ExcelExport _excelExport;

        [SetUp]
        public void SetupComponents()
        {
            _excelImport = _container.Resolve<Repository.Import.ExcelImport>();
            _excelExport = _container.Resolve<Repository.Import.ExcelExport>();
        }

        [Test]
        public void ImportExport_Clear_Import()
        {
            var match = GetMatch();
            var lag = LagFactory.SettOppLagMedDeltakere(3, 5, "LAG_");
            var poster = GenererPoster(10);

            Importer(match, lag, poster);

            AssertAntallIDatabasen(match.MatchId);

            var exported = _excelExport.ToByteArray(match.MatchId);

            // Clear database
            _dataContextFactory.DeleteAllData();

            // Importer
            _excelImport.LesInn(exported);

            AssertAntallIDatabasen(match.MatchId);
        }

        private void AssertAntallIDatabasen(Guid matchId)
        {
            using (var context = _dataContextFactory.Create())
            {
                Assert.IsNotNull(context.Matcher.Single(x => x.MatchId == matchId), "Match");
                Assert.AreEqual(3, context.Lag.ToList().Count, "Lag");
                Assert.AreEqual(3, context.LagIMatch.Where(x => x.Match.MatchId == matchId).ToList().Count, "LagIMatch");
                Assert.AreEqual(15, context.Deltakere.ToList().Count, "Deltakere");
                Assert.AreEqual(5, context.Deltakere.Where(x => x.Lag.LagId == "LAG_1").ToList().Count, "Deltakere på LAG_1");
                Assert.AreEqual(10, context.Poster.ToList().Count, "Poster");
                Assert.AreEqual(10, context.PosterIMatch.Where(x => x.Match.MatchId == matchId).ToList().Count, "PosterIMatch");
            }
        }
    }
}

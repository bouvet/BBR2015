using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Castle.Windsor;
using Database;
using Database.Entities;
using NUnit.Framework;
using OfficeOpenXml;
using RestApi.Tests.Infrastructure;

namespace RestApi.Tests.Admin
{
    [TestFixture]
    [Explicit("Kjøres manuelt for å sette opp spillet")]
    public class Oppsett
    {
        private IWindsorContainer _container;
        private DataContextFactory _dataContextFactory;

        [TestFixtureSetUp]
        public void EnsureDatabase()
        {
            using (var context = RestApiApplication.CreateContainer().Resolve<DataContextFactory>().Create())
            {
                var triggerCreateDatabase = context.Lag.Any();
            }
        }

        [SetUp]
        public void Setup()
        {
            _container = RestApiApplication.CreateContainer();
            _dataContextFactory = _container.Resolve<DataContextFactory>();
        }

        [Test]
        [Explicit("Bare hvis du virkelig vet hva du gjør!")]
        [Ignore("Må kjøres HELT separat")]
        public void Tøm_Databasen()
        {
            using (var context = _dataContextFactory.Create())
            {
                context.VåpenBeholdning.Clear();
                context.PostRegisteringer.Clear();
                context.LagIMatch.Clear();
                context.PosterIMatch.Clear();
                context.Matcher.Clear();
                context.DeltakerPosisjoner.Clear();
                context.Meldinger.Clear();
                context.Deltakere.Clear();
                context.Lag.Clear();
                context.Våpen.Clear();
                context.Poster.Clear();

                context.SaveChanges();
            }
        }

        [Test]
        public void Opprett_Våpen()
        {
            var bombe = new Vaapen { VaapenId = Constants.Våpen.Bombe, Beskrivelse = "Sprenger posten for en tid" };
            var felle = new Vaapen { VaapenId = Constants.Våpen.Felle, Beskrivelse = "Sprenger posten ved neste stempling. Laget som stempler får ikke poeng." };

            var alle = new[] { bombe, felle };

            using (var context = _dataContextFactory.Create())
            {
                if (context.Våpen.Count() == alle.Length)
                    return;

                context.Våpen.AddRange(alle);

                context.SaveChanges();
            }
        }

        [Test]
        public void Opprett_testspill_før_BBR()
        {
            Opprett_Våpen();
            Opprett_Arrangørlag();
            Opprett_Demolag();

            using (var context = _dataContextFactory.Create())
            {
                var match = new Match
                {
                    MatchId = Guid.NewGuid(),
                    Navn = "Utvikling",
                    StartTid = new DateTime(2015, 11, 01, 10, 00, 00),
                    SluttTid = new DateTime(2015, 11, 06, 10, 00, 00)
                };

                if (context.Matcher.Any(x => x.Navn == match.Navn))
                    return;

                var leggTilLag = context.Lag.Where(x => x.LagId.StartsWith("SUPPORT") || x.LagId.StartsWith("BBR")).ToList();

                foreach (var lag in leggTilLag)
                {
                    var deltakelse = match.LeggTil(lag);

                    var våpen = context.Våpen.ToList();

                    deltakelse.LeggTilVåpen(våpen[0]);
                    deltakelse.LeggTilVåpen(våpen[1]);
                }

                context.Matcher.Add(match);

                foreach (var post in new PostFactory().Les(Constants.Område.Oscarsborg))
                {
                    post.HemmeligKode = post.Navn.Replace(" ", string.Empty);
                    post.Navn = "Test" + post.Navn;
                    post.Omraade = "Testrunde";

                    context.Poster.Add(post);

                    var postIMatch = new PostIMatch
                    {
                        Match = match,
                        Post = post,
                        PoengArray = post.DefaultPoengArray,
                        SynligFraTid = match.StartTid,
                        SynligTilTid = match.SluttTid
                    };

                    match.Poster.Add(postIMatch);
                }
                context.SaveChanges();
            }

        }

        [Test]
        public void Opprett_Arrangørlag()
        {
            using (var context = _dataContextFactory.Create())
            {
                if (context.Lag.Any(x => x.Navn.StartsWith("SUPPORT_")))
                    return;

                var lag = LagFactory.SettOppLagMedDeltakere(1, 5, "SUPPORT_");

                context.Lag.AddRange(lag);
                context.SaveChanges();
            }
        }

        [Test]
        public void Opprett_Demolag()
        {
            using (var context = _dataContextFactory.Create())
            {
                if (context.Lag.Any(x => x.Navn.StartsWith("BBR")))
                    return;

                var lag = LagFactory.SettOppLagMedDeltakere(3, 3, "BBR");

                context.Lag.AddRange(lag);
                context.SaveChanges();
            }
        }

        [Test]
        public void Opprett_lagForHelga()
        {
            var java = LagFactory.SettOppLagMedDeltakere(6, 4, "JAVA_");
            var ms = LagFactory.SettOppLagMedDeltakere(5, 4, "MS_");

            var genererteLag = new List<Lag>();
            genererteLag.AddRange(java);
            genererteLag.AddRange(ms);

            using (var context = _dataContextFactory.Create())
            {
                var alleLag = context.Lag.ToList();

                for (int i = 0; i < genererteLag.Count; i++)
                {
                    var lag = genererteLag[i];
                    if (!alleLag.Any(x => x.LagId == lag.LagId))
                        context.Lag.Add(lag);
                }

                context.SaveChanges();
            }
        }

        [Test]
        public void Opprett_spill_fredag()
        {
            Opprett_Våpen();
            Opprett_Arrangørlag();
            Opprett_lagForHelga();

            using (var context = _dataContextFactory.Create())
            {
                var match = new Match
                {
                    MatchId = Guid.NewGuid(),
                    Navn = "Treningsrunde fredag",
                    StartTid = new DateTime(2015, 11, 06, 10, 00, 00),
                    SluttTid = new DateTime(2015, 11, 07, 10, 00, 00)
                };

                if (context.Matcher.Any(x => x.Navn == match.Navn))
                    return;

                var leggTilLag = context.Lag.Where(x => x.LagId.StartsWith("SUPPORT") || x.LagId.StartsWith("LAG")).ToList();

                foreach (var lag in leggTilLag)
                {
                    var deltakelse = match.LeggTil(lag);

                    var våpen = context.Våpen.ToList();

                    deltakelse.LeggTilVåpen(våpen[0]);
                    deltakelse.LeggTilVåpen(våpen[1]);
                }

                context.Matcher.Add(match);

                foreach (var post in new PostFactory().Les(Constants.Område.OscarsborgFredag))
                {
                    post.HemmeligKode = post.Navn.Replace(" ", string.Empty);
                    post.Navn = "Fredag" + post.Navn;

                    context.Poster.Add(post);

                    var postIMatch = new PostIMatch
                    {
                        Match = match,
                        Post = post,
                        PoengArray = post.DefaultPoengArray,
                        SynligFraTid = match.StartTid,
                        SynligTilTid = match.SluttTid
                    };

                    match.Poster.Add(postIMatch);
                }
                context.SaveChanges();
            }
        }

        [Test]
        public void Opprett_spill_lørdag()
        {
            Opprett_Våpen();
            Opprett_Arrangørlag();
            Opprett_lagForHelga();

            using (var context = _dataContextFactory.Create())
            {
                var match = new Match()
                {
                    MatchId = Guid.NewGuid(),
                    Navn = "Bouvet Battle Royale 2015",
                    StartTid = new DateTime(2015, 11, 07, 10, 00, 00),
                    SluttTid = new DateTime(2015, 11, 07, 18, 00, 00)
                };

                if (context.Matcher.Any(x => x.Navn == match.Navn))
                    return;

                var leggTilLag = context.Lag.Where(x => x.LagId.StartsWith("SUPPORT") || x.LagId.StartsWith("LAG_")).ToList();

                foreach (var lag in leggTilLag)
                {
                    var deltakelse = match.LeggTil(lag);

                    var våpen = context.Våpen.ToList();

                    deltakelse.LeggTilVåpen(våpen[0]);
                    deltakelse.LeggTilVåpen(våpen[1]);
                }

                context.Matcher.Add(match);

                foreach (var post in new PostFactory().Les(Constants.Område.OscarsborgFredag))
                {
                    context.Poster.Add(post);

                    var postIMatch = new PostIMatch
                    {
                        Match = match,
                        Post = post,
                        PoengArray = post.DefaultPoengArray,
                        SynligFraTid = match.StartTid,
                        SynligTilTid = match.SluttTid
                    };

                    match.Poster.Add(postIMatch);
                }
                context.SaveChanges();
            }
        }

        [Test, RequiresSTA]
        public void LesDetaljerFraExcel()
        {
            var dialog = new OpenFileDialog();
            dialog.ShowDialog();
            var file = dialog.FileName;

            using (ExcelPackage excel = new ExcelPackage(new FileInfo(file)))
            {
                LesInnLag(excel.Workbook.Worksheets["Lag"]);
                LesInnDeltakere(excel.Workbook.Worksheets["Deltakere"]);
            }
        }

        private void LesInnLag(ExcelWorksheet worksheet)
        {
            using (var context = _dataContextFactory.Create())
            {
                var alleLag = context.Lag.ToList();

                var start = worksheet.Dimension.Start;
                var end = worksheet.Dimension.End;

                for (int row = start.Row + 1; row <= end.Row; row++)
                {
                    var lagId = GetValue(worksheet, row, "A");
                    var lag = alleLag.SingleOrDefault(x => x.LagId == lagId);

                    if (lag == null)
                        continue;

                    lag.Navn = GetValue(worksheet, row, "B");
                    lag.Farge = GetValue(worksheet, row, "C");
                    lag.Ikon = GetValue(worksheet, row, "D");
                    lag.HemmeligKode = GetValue(worksheet, row, "E");
                }

                context.SaveChanges();
            }
        }

        private static string GetValue(ExcelWorksheet worksheet, int row, string column)
        {
            return (string)worksheet.Cells[column + row].Value;
        }

        private void LesInnDeltakere(ExcelWorksheet excelWorksheet)
        {
        }

        
    }
}

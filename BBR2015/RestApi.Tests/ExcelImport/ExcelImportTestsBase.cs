using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Castle.Windsor;
using Database;
using Database.Entities;
using NUnit.Framework;
using OfficeOpenXml;
using Repository.Import;
using RestApi.Tests.Infrastructure;

namespace RestApi.Tests.ExcelImport
{
    public class ExcelImportTestsBase : BBR2015DatabaseTests
    {
        private IWindsorContainer _container;
        protected DataContextFactory _dataContextFactory;

        private ExcelPackage _excel;

        [SetUp]
        public void Given()
        {
            _container = RestApiApplication.CreateContainer();

            _dataContextFactory = _container.Resolve<DataContextFactory>();

            _dataContextFactory.DeleteAllData();

            TimeService.ResetToRealTime();


            InitialiserExcelFil();
        }

        [TearDown]
        public void Close()
        {
            _excel.Dispose();
        }

        protected void Importer(Match match, List<Lag> lagListe = null, List<PostImport.ExcelPost> poster = null)
        {
            InitialiserExcelFil();
            SkrivTilExcel(match, lagListe, poster);
            Importer();
        }

        protected void Importer()
        {
            var excelImport = _container.Resolve<Repository.Import.ExcelImport>();
            var bytes = _excel.GetAsByteArray();
            excelImport.LesInn(Guid.Empty, bytes);

            // Excel-pakken blir lukket ved skriving til stream, så en må lage ny          
        }


        protected void InitialiserExcelFil()
        {
            _excel = new ExcelPackage();
            _excel.Workbook.Worksheets.Add("Poster");
            _excel.Workbook.Worksheets.Add("Match");
            _excel.Workbook.Worksheets.Add("Lag");
            _excel.Workbook.Worksheets.Add("Deltakere");
        }

        protected ExcelPackage SkrivTilExcel(Match match, List<Lag> lagListe = null, List<PostImport.ExcelPost> poster = null)
        {
            SkrivMatch(match);
            SkrivLag(lagListe);
            SkrivDeltakere(lagListe);
            SkrivPoster(poster);

            return _excel;
        }

        private void SkrivPoster(List<PostImport.ExcelPost> poster)
        {
            if (poster == null)
                return;

            var sheet = _excel.Workbook.Worksheets[ExcelSheet.Poster.SheetName];

            // Headers
            for(var i = 0; i < ExcelSheet.Poster.Kolonner.Length; i++)
            {
                sheet.SetValue(1, i + 1, ExcelSheet.Poster.Kolonner[i]);
            }

            int row = 2;
            foreach (var post in poster)
            {
                //sheet.Set(row, ExcelSheet.Poster.PostId, post.PostId.ToString());
                sheet.Set(row, ExcelSheet.Poster.Navn, post.Navn);
                sheet.Set(row, ExcelSheet.Poster.Område, post.Omraade);
                sheet.Set(row, ExcelSheet.Poster.Latitude, post.Latitude.ToString());
                sheet.Set(row, ExcelSheet.Poster.Longitude, post.Longitude.ToString());
                sheet.Set(row, ExcelSheet.Poster.Altitude, post.Altitude.ToString());
                sheet.Set(row, ExcelSheet.Poster.Beskrivelse, post.Beskrivelse);
                sheet.Set(row, ExcelSheet.Poster.HemmeligKode, post.HemmeligKode);
                sheet.Set(row, ExcelSheet.Poster.PoengFordeling, post.DefaultPoengArray);
                sheet.Set(row, ExcelSheet.Poster.BildeUrl, post.Image);
                sheet.Set(row, ExcelSheet.Poster.SynligFra, post.SynligFra.ToString());
                sheet.Set(row, ExcelSheet.Poster.SynligTil, post.SynligTil.ToString());

                row++;
            }
        }

        private void SkrivDeltakere(List<Lag> lagListe)
        {
            if (lagListe == null)
                return;

            var deltakere = from l in lagListe
                            from d in l.Deltakere
                            select new DeltakerImport.ExcelDeltaker
                            {
                                LagId = l.LagId,
                                Navn = d.Navn,
                                Kode = d.Kode
                            };

            var sheet = _excel.Workbook.Worksheets[ExcelSheet.Deltakere.SheetName];

            // Headers
            for (var i = 0; i < ExcelSheet.Deltakere.Kolonner.Length; i++)
            {
                sheet.SetValue(1, i + 1, ExcelSheet.Deltakere.Kolonner[i]);
            }

            int row = 2;
            foreach (var deltaker in deltakere)
            {
                sheet.Set(row, ExcelSheet.Deltakere.Lag, deltaker.LagId);
                sheet.Set(row, ExcelSheet.Deltakere.Navn, deltaker.Navn);
                sheet.Set(row, ExcelSheet.Deltakere.Kode, deltaker.Kode);

                row++;
            }
        }

        private void SkrivLag(List<Lag> lagListe)
        {
            if (lagListe == null)
                return;

            var sheet = _excel.Workbook.Worksheets["Lag"];

            // Headers
            for (var i = 0; i < ExcelSheet.Lag.Kolonner.Length; i++)
            {
                sheet.SetValue(1, i + 1, ExcelSheet.Lag.Kolonner[i]);
            }
            
            int row = 2;
            foreach (var lag in lagListe)
            {
                sheet.Set(row, ExcelSheet.Lag.LagId, lag.LagId);
                sheet.Set(row, ExcelSheet.Lag.Navn, lag.Navn);
                sheet.Set(row, ExcelSheet.Lag.HemmeligKode, lag.HemmeligKode);
                sheet.Set(row, ExcelSheet.Lag.Farge, lag.Farge);

                row++;
            }
        }

        private void SkrivMatch(Match match)
        {
            var sheet = _excel.Workbook.Worksheets["Match"];
            var row = 2;

            // Headers
            for (var i = 0; i < ExcelSheet.Match.Kolonner.Length; i++)
            {
                sheet.SetValue(1, i + 1, ExcelSheet.Match.Kolonner[i]);
            }

            sheet.Set(2, ExcelSheet.Match.MatchId, match.MatchId.ToString());
            sheet.Set(2, ExcelSheet.Match.Navn, match.Navn);
            sheet.Set(2, ExcelSheet.Match.Starttid, match.StartTid.ToString());
            sheet.Set(2, ExcelSheet.Match.Sluttid, match.SluttTid.ToString());
        }

        protected static Match GetMatch()
        {
            var match = new Match
            {
                MatchId = Guid.NewGuid(),
                Navn = "Testing",
                StartTid = DateTime.Today.AddDays(-1),
                SluttTid = DateTime.Today.AddDays(1)
            };
            return match;
        }
    }
}
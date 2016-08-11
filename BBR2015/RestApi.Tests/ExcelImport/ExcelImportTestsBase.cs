using System;
using System.Collections.Generic;
using Castle.Windsor;
using Database;
using Database.Entities;
using NUnit.Framework;
using Repository.Import;
using RestApi.Tests.Infrastructure;

namespace RestApi.Tests.ExcelImport
{
    public class ExcelImportTestsBase : BBR2015DatabaseTests
    {
        private IWindsorContainer _container;
        protected DataContextFactory _dataContextFactory;

        private ExcelWriter _excelWriter;

        [SetUp]
        public void Given()
        {
            _container = RestApiApplication.CreateContainer();

            _dataContextFactory = _container.Resolve<DataContextFactory>();

            _dataContextFactory.DeleteAllData();

            TimeService.ResetToRealTime();
            _excelWriter = new ExcelWriter();
        }

        [TearDown]
        public void Close()
        {
            _excelWriter.Dispose();
        }

        protected void Importer(MatchImport.ExcelMatch match, List<Lag> lagListe = null, List<PostImport.ExcelPost> poster = null)
        {
            _excelWriter = new ExcelWriter();
            _excelWriter.SkrivTilExcel(match, lagListe, poster);
            Importer();
        }

        protected void Importer()
        {
            var excelImport = _container.Resolve<Repository.Import.ExcelImport>();
            var bytes = _excelWriter.GetAsByteArray();
            excelImport.LesInn(Guid.Empty, bytes);

            // Excel-pakken blir lukket ved skriving til stream, så en må lage ny          
        }

        protected static MatchImport.ExcelMatch GetMatch()
        {
            var match = new MatchImport.ExcelMatch
            {
                MatchId = Guid.NewGuid(),
                Navn = "Testing",
                StartTid = DateTime.Today.AddDays(-1),
                SluttTid = DateTime.Today.AddDays(1),
                GeoboxNWLatitude = 51,
                GeoboxNWLongitude = 11,
                GeoboxSELatitude = 52,
                GeoboxSELongitude = 12,
                DefaultPoengFordeling = "100,90,80",
                PrLagFelle = 1,
                PrLagBombe = 2
            };
            return match;
        }
    }
}
using System;
using System.Collections.Generic;
using Database;
using OfficeOpenXml;
using System.Data.Entity;
using System.Linq;
using Database.Entities;

namespace Repository.Import
{
    public class DeltakerImport
    {
        private readonly DataContextFactory _dataContextFactory;

        public DeltakerImport(DataContextFactory dataContextFactory)
        {
            _dataContextFactory = dataContextFactory;
        }

        public void Les(ExcelWorksheet excelWorksheet, MatchImport.ExcelMatch excelMatch)
        {
            var matchId = excelMatch.MatchId;
            var deltakere = LesFra(excelWorksheet);

            using (var context = _dataContextFactory.Create())
            {
                AddOrUpdate(deltakere,  context);

                context.SaveChanges();
            }
        }

        private void AddOrUpdate(List<ExcelDeltaker> deltakere, DataContext context)
        {
            var alleLag = context.Lag.ToList();

            foreach (var excelDeltaker in deltakere)
            {
                var deltaker = context.Deltakere.SingleOrDefault(x => x.Kode == excelDeltaker.Kode);

                var lag = alleLag.SingleOrDefault(x => x.LagId == excelDeltaker.LagId);

                if (deltaker == null)
                {
                    context.Deltakere.Add(new Deltaker
                    {
                        DeltakerId = Guid.NewGuid().ToString(),
                        Navn = excelDeltaker.Navn,
                        Kode = excelDeltaker.Kode,
                        Lag = lag
                    });
                }
                else
                {
                    deltaker.Navn = excelDeltaker.Navn;
                    deltaker.Lag = lag;
                }
            }            
        }

        private List<ExcelDeltaker> LesFra(ExcelWorksheet excelWorksheet)
        {
            var deltakere = new Dictionary<string, ExcelDeltaker>();

            var sheet = excelWorksheet;

            for (int row = 2; row <= sheet.Dimension.End.Row; row++)
            {
                var deltaker = new ExcelDeltaker
                {
                    LagId = sheet.GetValue(ExcelSheet.Deltakere.Lag, row),
                    Navn = sheet.GetValue(ExcelSheet.Deltakere.Navn, row),
                    Kode = sheet.GetValue(ExcelSheet.Deltakere.Kode, row),
                };

                // Siste endring gjelder
                if (deltakere.ContainsKey(deltaker.Navn))
                    deltakere.Remove(deltaker.Navn);

                deltakere.Add(deltaker.Navn, deltaker);
            }

            return deltakere.Values.ToList();
        }

        public class ExcelDeltaker
        {
            public string LagId { get; set; }
            public string Navn { get; set; }
            public string Kode { get; set; }
        }
    }
}
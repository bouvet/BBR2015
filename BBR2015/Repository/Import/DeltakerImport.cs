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
                var match = (from m in context.Matcher.Include(x => x.DeltakendeLag.Select(y => y.Lag).Select(z => z.Deltakere))
                             where m.MatchId == matchId
                             select m).FirstOrDefault();

                AddOrUpdate(deltakere, match, context);

                context.SaveChanges();
            }
        }

        private void AddOrUpdate(List<ExcelDeltaker> deltakere, Match match, DataContext context)
        {
            foreach (var lagIMatch in match.DeltakendeLag)
            {
                var lag = lagIMatch.Lag;

                var deltakereForLag = deltakere.Where(x => x.LagId == lag.LagId);

                foreach (var excelDeltaker in deltakereForLag)
                {
                    var eksisterende = lag.Deltakere.FirstOrDefault(x => x.Kode == excelDeltaker.Kode);

                    if (eksisterende != null)
                    {
                        eksisterende.Navn = excelDeltaker.Navn;
                    }
                    else
                    {
                        lag.LeggTilDeltaker(new Deltaker
                        {
                            DeltakerId = lag.LagId + "-" + (lag.Deltakere.Count + 1),
                            Navn = excelDeltaker.Navn,
                            Kode = excelDeltaker.Kode // i praksis umulig å endre siden vi slår opp på denne...
                        });
                    }
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
                    Kode = sheet.GetValue(ExcelSheet.Deltakere.Kode,row),
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
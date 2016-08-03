using System;
using System.Collections.Generic;
using System.Linq;
using Database;
using Database.Entities;
using OfficeOpenXml;
using System.Data.Entity;

namespace Repository.Import
{
    public class LagImport
    {
        private readonly DataContextFactory _dataContextFactory;

        public LagImport(DataContextFactory dataContextFactory)
        {
            _dataContextFactory = dataContextFactory;
        }

        public void Les(ExcelWorksheet excelWorksheet, Guid matchId)
        {
            var lagListe = LesFra(excelWorksheet);

            using (var context = _dataContextFactory.Create())
            {
                var match = (from m in context.Matcher.Include(x => x.DeltakendeLag.Select(y => y.Lag))
                             where m.MatchId == matchId
                             select m).FirstOrDefault();

                foreach (var lag in lagListe)
                {
                    AddOrUpdate(lag, match, context);
                }

                context.SaveChanges();
            }
        }

        private List<Lag> LesFra(ExcelWorksheet excelWorksheet)
        {
            var lagListe = new Dictionary<string, Lag>();

            var sheet = excelWorksheet;            

            for (int row = 2; row <= sheet.Dimension.End.Row; row++)
            {
                var lag = new Lag
                {
                    LagId = sheet.GetValue(row, 1).ToString(),
                    Navn = sheet.GetValue(row, 2).ToString(),
                    HemmeligKode = sheet.GetValue(row, 3).ToString(),
                    Farge = sheet.GetValue(row, 4).ToString(),
                };

                // Siste endring gjelder
                if (lagListe.ContainsKey(lag.LagId))
                    lagListe.Remove(lag.LagId);

                lagListe.Add(lag.LagId, lag);
            }

            return lagListe.Values.ToList();
        }

        private void AddOrUpdate(Lag lag, Match match, DataContext context)
        {
            var existing = (from l in context.Lag
                            where l.LagId == lag.LagId
                            select l).FirstOrDefault();

            if (existing == null)
            {
                context.Lag.Add(lag);
            }
            else
            {
                existing.Navn = lag.Navn;
                existing.HemmeligKode = lag.HemmeligKode;
                existing.Farge = lag.Farge;
            }

            if (!match.DeltakendeLag.Any(x => x.Lag.LagId == lag.LagId))
            {
                match.LeggTil(existing ?? lag);
            }

        }
    }

}
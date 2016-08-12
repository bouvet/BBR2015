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

        public void Les(ExcelWorksheet excelWorksheet, MatchImport.ExcelMatch excelMatch)
        {
            var matchId = excelMatch.MatchId;
            var lagListe = LesFra(excelWorksheet);

            using (var context = _dataContextFactory.Create())
            {
                var match = (from m in context.Matcher.Include(x => x.DeltakendeLag.Select(y => y.Lag))
                             where m.MatchId == matchId
                             select m).FirstOrDefault();

                var våpen = context.Våpen.ToList();

                foreach (var lag in lagListe)
                {
                    AddOrUpdate(lag, match, context, excelMatch, våpen);
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

        private void AddOrUpdate(Lag lag, Match match, DataContext context, MatchImport.ExcelMatch excelMatch, List<Vaapen> våpen)
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
                var lagIMatch = match.LeggTil(existing ?? lag);

                // Legg til våpen bare på nye lag i matcher (dvs. ikke få flere våper ved flere importer)
                var felle = våpen.Single(x => x.VaapenId == Constants.Våpen.Felle);
                for (int i = 0; i < excelMatch.PrLagFelle.GetValueOrDefault(); i++)
                {
                    lagIMatch.LeggTilVåpen(felle);
                }

                var bombe = våpen.Single(x => x.VaapenId == Constants.Våpen.Bombe);
                for (int i = 0; i < excelMatch.PrLagBombe.GetValueOrDefault(); i++)
                {
                    lagIMatch.LeggTilVåpen(bombe);
                }
            }
        }
    }

}
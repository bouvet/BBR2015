using System;
using System.Collections.Generic;
using System.Linq;
using Database;
using Database.Entities;
using OfficeOpenXml;

namespace Repository.Import
{
    public class MatchImport
    {
        private readonly DataContextFactory _dataContextFactory;

        public MatchImport(DataContextFactory dataContextFactory)
        {
            _dataContextFactory = dataContextFactory;
        }

        public Guid Les(ExcelWorksheet sheet, Guid matchId)
        {
            var row = 2;
            var match = new ExcelMatch
            {
                MatchId = Guid.Parse(sheet.GetValue(ExcelSheet.Match.MatchId, row)),
                Navn = sheet.GetValue(ExcelSheet.Match.Navn, row),
                StartTid = DateTime.Parse(sheet.GetValue(ExcelSheet.Match.Starttid, row)),
                SluttTid = DateTime.Parse(sheet.GetValue(ExcelSheet.Match.Sluttid, row)),
                DefaultPoengFordeling = sheet.GetValue(ExcelSheet.Match.DefaultPostPoengfordeling, row),

                
                PrLagFelle = int.Parse(sheet.GetValue(ExcelSheet.Match.Pr_lag_FELLE, row)),
                PrLagBombe = int.Parse(sheet.GetValue(ExcelSheet.Match.Pr_lag_BOMBE, row))
            };

            var point = sheet.GetValue(ExcelSheet.Match.GeoBox_NW_latitude, row);

            if (!string.IsNullOrEmpty(point))
                match.GeoboxNWLatitude = double.Parse(point);

            point = sheet.GetValue(ExcelSheet.Match.GeoBox_NW_longitude, row);

            if (!string.IsNullOrEmpty(point))
                match.GeoboxNWLongitude = double.Parse(point);

            point = sheet.GetValue(ExcelSheet.Match.GeoBox_SE_latitude, row);

            if (!string.IsNullOrEmpty(point))
                match.GeoboxSELatitude = double.Parse(point);

            point = sheet.GetValue(ExcelSheet.Match.GeoBox_SE_longitude, row);

            if (!string.IsNullOrEmpty(point))
                match.GeoboxSELongitude = double.Parse(point);           

            AddOrUpdate(match);

            LeggInnV�pen();

            return match.MatchId;
        }

        private void LeggInnV�pen()
        {
            var bombe = new Vaapen { VaapenId = Constants.V�pen.Bombe, Beskrivelse = "Sprenger posten for en tid" };
            var felle = new Vaapen { VaapenId = Constants.V�pen.Felle, Beskrivelse = "Sprenger posten ved neste stempling. Laget som stempler f�r ikke poeng." };

            var alle = new[] { bombe, felle };

            using (var context = _dataContextFactory.Create())
            {
                if (context.V�pen.Count() == alle.Length)
                    return;

                context.V�pen.AddRange(alle);

                context.SaveChanges();
            }
        }

        private void AddOrUpdate(ExcelMatch match)
        {
            using (var context = _dataContextFactory.Create())
            {
                var existing = (from m in context.Matcher
                                where m.MatchId == match.MatchId
                                select m).FirstOrDefault();

                if (existing == null)
                {
                    context.Matcher.Add(match.GetMatch());
                }
                else
                {
                    match.Update(existing);
                }

                context.SaveChanges();
            }
        }

        public class ExcelMatch : Match
        {
            public string DefaultPoengFordeling { get; set; }
            public int? PrLagFelle { get; set; }
            public int? PrLagBombe { get; set; }

            public Match GetMatch()
            {
                var match = new Match { MatchId = MatchId };
                Update(match);
                return match;
            }

            public void Update(Match match)
            {
                match.Navn = Navn;
                match.StartTid = StartTid;
                match.SluttTid = SluttTid;
                match.GeoboxNWLatitude = GeoboxNWLatitude;
                match.GeoboxNWLongitude = GeoboxNWLongitude;
                match.GeoboxSELatitude = GeoboxSELatitude;
                match.GeoboxSELongitude = GeoboxSELongitude;
            }
        }
    }
}
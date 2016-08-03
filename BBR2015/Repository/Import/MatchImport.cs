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
            var match = new Match
            {
                MatchId = Guid.Parse(sheet.GetValue(ExcelSheet.Match.MatchId, row)),
                Navn = sheet.GetValue(ExcelSheet.Match.Navn, row),
                StartTid = DateTime.Parse(sheet.GetValue(ExcelSheet.Match.Starttid, row)),
                SluttTid = DateTime.Parse(sheet.GetValue(ExcelSheet.Match.Sluttid, row))
            };

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

        private void AddOrUpdate(Match match)
        {
            using (var context = _dataContextFactory.Create())
            {
                var existing = (from m in context.Matcher
                    where m.MatchId == match.MatchId
                    select m).FirstOrDefault();

                if (existing == null)
                {
                    context.Matcher.Add(match);
                }
                else
                {
                    existing.Navn = match.Navn;
                    existing.StartTid = match.StartTid;
                    existing.SluttTid = match.SluttTid;
                }

                context.SaveChanges();
            }
        }
    }
}
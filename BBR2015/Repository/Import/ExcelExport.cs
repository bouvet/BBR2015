using System;
using System.Linq;
using Database;
using System.Data.Entity;

namespace Repository.Import
{
    public class ExcelExport
    {
        private readonly DataContextFactory _dataContextFactory;
        private readonly ExcelWriter _excelWriter;

        public ExcelExport(DataContextFactory dataContextFactory, ExcelWriter excelWriter)
        {
            _dataContextFactory = dataContextFactory;
            _excelWriter = excelWriter;
        }

        public byte[] ToByteArray(Guid matchId)
        {
            using (var context = _dataContextFactory.Create())
            {
                var match = context.Matcher.SingleOrDefault(x => x.MatchId == matchId);

                var lag = (from lim in context.LagIMatch.Include(x => x.Lag)asdfadf.Include(x => x.Lag.Deltakere)
                           where lim.Match.MatchId == matchId
                           select lim.Lag).ToList();

                var posterIMatch = context.PosterIMatch.Include(x => x.Post).Where(x => x.Match.MatchId == matchId).ToList();

                var excelMatch = MatchImport.ExcelMatch.FromMatch(match);

                var excelPoster = posterIMatch.Select(PostImport.ExcelPost.Create).ToList();

                _excelWriter.SkrivTilExcel(excelMatch, lag, excelPoster);
            }

            return _excelWriter.GetAsByteArray();
        }
    }
}

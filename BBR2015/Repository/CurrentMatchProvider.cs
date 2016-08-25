using System;
using System.Linq;
using System.Web;
using Database;

namespace Repository
{
    public class CurrentMatchProvider
    {
        private const int CacheSeconds = 5;
        private DateTime _lastWriteTime = DateTime.MinValue;
        private Guid _cachedMatchId;

        private readonly DataContextFactory _dataContextFactory;

        public CurrentMatchProvider(DataContextFactory dataContextFactory)
        {
            _dataContextFactory = dataContextFactory;
        }

        public virtual Guid GetMatchId()
        {
            if (HttpContext.Current != null)
            {
                // Prøv å lese fra HTTP Header for å overstyre i automatiske tester
                var matchIdHeader = HttpContext.Current.Request.Headers["MatchId"];

                if (!string.IsNullOrEmpty(matchIdHeader))
                    return new Guid(matchIdHeader);
            }

            if (_lastWriteTime.AddSeconds(CacheSeconds) < TimeService.Now)
            {
                _cachedMatchId = ReadFromDatabase();
                _lastWriteTime = TimeService.Now;
            }

            return _cachedMatchId;
        }

        private Guid ReadFromDatabase()
        {
            using (var context = _dataContextFactory.Create())
            {
                var currentMatch= (from m in context.Matcher
                    where m.StartTid < TimeService.Now && TimeService.Now < m.SluttTid
                    select m).FirstOrDefault();

                if (currentMatch == null)
                    return Guid.Empty;

                return currentMatch.MatchId;
            }
        }
    }
}
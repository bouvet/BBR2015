using System.Collections.Generic;
using System.Linq;
using Database;
using Database.Entities;

namespace Repository
{
    public class TilgangsKontroll
    {
        private List<Lag> _lagene;

        private List<Lag> Lagene
        {
            get
            {
                if (_lagene != null)
                    return _lagene;

                using (var context = _dataContextFactory.Create())
                {
                    _lagene = context.Lag.Include(x => x.Deltakere).ToList();
                }

                return _lagene;
            }
        }
        private DataContextFactory _dataContextFactory;

        public TilgangsKontroll(DataContextFactory dataContextFactory)
        {
            _dataContextFactory = dataContextFactory;
        }

        public string FinnLagIdFraKode(string hemmeligKode)
        {
            return Lagene.Where(l => l.HemmeligKode == hemmeligKode).Select(x => x.LagId).SingleOrDefault();
        }

        public string SlåOppDeltakerFraKode(string lagId, string deltakerKode)
        {
            var deltakerId = from l in Lagene
                             from d in l.Deltakere
                             where l.LagId == lagId && d.MatcherKode(deltakerKode)
                             select d.DeltakerId;

            return deltakerId.SingleOrDefault();
        }

        public void Nullstill()
        {
            _lagene = null;
        }

        public dynamic HentAlleHemmeligeKoder()
        {
            var koder = from l in Lagene
                from d in l.Deltakere
                select new
                {
                    d.DeltakerId,
                    d.Navn,
                    d.Kode,
                    l.LagId,
                    LagKode = l.HemmeligKode
                };

            return koder;
        }
    }
}

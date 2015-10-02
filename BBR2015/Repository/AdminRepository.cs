using System.Collections.Generic;
using System.Linq;

using Database;
using System;
using Database.Entities;

namespace Repository
{
    public class AdminRepository
    {
        private List<Lag> _lagene;

        private List<Lag> Lagene
        {
            get
            {
                if (_lagene != null)
                    return _lagene;

                using(var context = _dataContextFactory.Create())
                {
                    _lagene = context.Lag.Include(x => x.Deltakere).ToList();
                }

                return _lagene;
            }
        }
        private DataContextFactory _dataContextFactory;

        public AdminRepository(DataContextFactory dataContextFactory)
        {
            _dataContextFactory = dataContextFactory;
        }

        public void LeggTilLag(string lagId, string navn, string farge, string ikon)
        {
            var lag = new Lag(lagId, navn, farge, ikon);
            LeggTilLag(lag);
        }

        public void LeggTilLag(Lag lag)
        {
            Lagene.Add(lag);
        }

        public Lag FinnLag(string lagId)
        {
            return Lagene.FirstOrDefault(l => l.LagId == lagId);
        }

        public bool GyldigLag(string lagId)
        {
            return Lagene.Any(l => l.LagId == lagId);
        }

        public bool GyldigDeltaker(string lagId, string deltakerId)
        {
            return Lagene.Any(l => l.LagId == lagId && l.Deltakere.Exists(d => d.DeltakerId == deltakerId));
        }

        public IEnumerable<Lag> AlleLag()
        {
            return Lagene;
        }
    }
}

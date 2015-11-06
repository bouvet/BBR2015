using System.Collections.Generic;
using System.Linq;
using Database;
using Database.Entities;

namespace Repository
{
    public class TilgangsKontroll
    {
        private readonly DataContextFactory _dataContextFactory;
        private List<Lag> _lagene;
        private Dictionary<string, KontrollResultat> _kodeKombinasjoner;

        public TilgangsKontroll(DataContextFactory dataContextFactory)
        {
            _dataContextFactory = dataContextFactory;
        }

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

        private Dictionary<string, KontrollResultat> KodeKombinasjoner
        {
            get
            {
                if (_kodeKombinasjoner == null)
                {
                    _kodeKombinasjoner = (from l in Lagene
                                          from d in l.Deltakere
                                          select new KontrollResultat
                                          {
                                              KodeKombinasjon = LagKodeKombinasjon(l.HemmeligKode, d.Kode),
                                              LagId = l.LagId,
                                              DeltakerId = d.DeltakerId
                                          }
                        ).ToDictionary(x => x.KodeKombinasjon, x => x);
                }

                return _kodeKombinasjoner;
            }
        }
              
        public void Nullstill()
        {
            _lagene = null;
            _kodeKombinasjoner = null;
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

        public KontrollResultat SjekkTilgang(string lagKode, string deltakerKode)
        {
            var kodeKombinasjon = LagKodeKombinasjon(lagKode, deltakerKode);

            if (!KodeKombinasjoner.ContainsKey(kodeKombinasjon))
                return null;

            return KodeKombinasjoner[kodeKombinasjon];
        }

        private string LagKodeKombinasjon(string lagKode, string deltakerKode)
        {
            return string.Format("{0}¤¤¤{1}", lagKode, deltakerKode);
        }

        public List<string> HentAlleLagIder()
        {
            return Lagene.Select(x => x.LagId).ToList();
        }
    }

    public class KontrollResultat
    {
        public string KodeKombinasjon { get; set; }
        public string LagId { get; set; }
        public string DeltakerId { get; set; }
    }
}

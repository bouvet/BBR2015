using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Database;
using Database.Entities;
using System.Data.Entity;

namespace Repository
{
    public class TilgangsKontroll
    {
        private readonly DataContextFactory _dataContextFactory;
        private List<Lag> _lagene;
        private ConcurrentDictionary<string, KontrollResultat> _kodeKombinasjoner;
        private ConcurrentDictionary<string, string> _deltakerNavn = new ConcurrentDictionary<string, string>();
        private ConcurrentDictionary<string, string> _lagNavn = new ConcurrentDictionary<string, string>();

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

        private ConcurrentDictionary<string, KontrollResultat> KodeKombinasjoner
        {
            get
            {
                if (_kodeKombinasjoner == null)
                {
                    var kodeKombinasjoner = (from l in Lagene
                                             from d in l.Deltakere
                                             select new KontrollResultat
                                             {
                                                 KodeKombinasjon = LagKodeKombinasjon(l.HemmeligKode, d.Kode),
                                                 LagId = l.LagId,
                                                 DeltakerId = d.DeltakerId
                                             }
                        ).ToDictionary(x => x.KodeKombinasjon, x => x);
                    _kodeKombinasjoner = new ConcurrentDictionary<string, KontrollResultat>(kodeKombinasjoner);
                }

                return _kodeKombinasjoner;
            }
        }

        public void Nullstill()
        {
            _lagene = null;
            _kodeKombinasjoner = null;
            _deltakerNavn = new ConcurrentDictionary<string, string>();
            _lagNavn = new ConcurrentDictionary<string, string>();
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

        public string HentAlleKodeKombinasjoner()
        {
            var koder = from l in KodeKombinasjoner
                        select new
                        {
                            key = l.Key                           
                        };

            return string.Join(Environment.NewLine, koder);
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
            return string.Format("{0}¤¤¤{1}", lagKode, deltakerKode).ToLower();
        }

        public List<string> HentAlleLagIder()
        {
            return Lagene.Select(x => x.LagId).ToList();
        }

        public string HentDeltakerNavn(string deltakerId)
        {
            if (!_deltakerNavn.ContainsKey(deltakerId))
            {
                var navn = (from l in Lagene
                            from d in l.Deltakere
                            where d.DeltakerId == deltakerId
                            select d.Navn).SingleOrDefault();

                _deltakerNavn[deltakerId] = navn;
            }
            return _deltakerNavn[deltakerId];
        }

        public string HentLagNavn(string lagId)
        {
            if (!_lagNavn.ContainsKey(lagId))
            {
                var navn = Lagene.Where(x => x.LagId == lagId).Select(x => x.Navn).SingleOrDefault();
                _lagNavn[lagId] = navn;
            }
            return _lagNavn[lagId];
        }

        public bool ErGyldigMatchId(string id)
        {
            Guid matchId;
            if (!Guid.TryParse(id, out matchId))
                return false;

            using (var context = _dataContextFactory.Create())
            {
                return context.Matcher.Any(x => x.MatchId == matchId);
            }
        }

        public bool ErLagKodeIBruk(string hemmeligKodeForLag)
        {
            return Lagene.Any(x => x.HemmeligKode.Equals(hemmeligKodeForLag, StringComparison.InvariantCultureIgnoreCase));
        }

        public bool ErDeltakerKodeIBruk(string kodeForSpiller)
        {
            return (from l in Lagene
                    from d in l.Deltakere
                    select d).Any(x => x.Kode.Equals(kodeForSpiller, StringComparison.InvariantCultureIgnoreCase));
        }
    }

    public class KontrollResultat
    {
        public string KodeKombinasjon { get; set; }
        public string LagId { get; set; }
        public string DeltakerId { get; set; }
    }
}

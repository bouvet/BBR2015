using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Database.Entities;
using Database;

namespace Repository
{
    public class PosisjonsService
    {
        // Registreres som singleton, så dictionary trenger ikke være static (sjekk ut threadsafety, dog)
        private ConcurrentDictionary<string, EksternDeltakerPosisjon> _gjeldendePosisjon;
        private ConcurrentDictionary<string, EksternDeltakerPosisjon> _lagretPosisjon;

        private readonly DataContextFactory _dataContextFactory;
        private readonly OverridableSettings _appSettings;
        private readonly TilgangsKontroll _tilgangsKontroll;

        private readonly object _lockGjeldende = new object();
        private readonly object _lockLagret = new object();

        public PosisjonsService(DataContextFactory dataContextFactory, OverridableSettings appSettings, TilgangsKontroll tilgangsKontroll)
        {
            _dataContextFactory = dataContextFactory;
            _appSettings = appSettings;
            _tilgangsKontroll = tilgangsKontroll;
        }

        private ConcurrentDictionary<string, EksternDeltakerPosisjon> GjeldendePosisjon
        {
            get
            {
                if (_gjeldendePosisjon == null)
                {
                    lock (_lockGjeldende)
                    {
                        if (_gjeldendePosisjon == null)
                            _gjeldendePosisjon = HentFraDatabasen();
                    }
                }

                return _gjeldendePosisjon;

            }
        }

        private ConcurrentDictionary<string, EksternDeltakerPosisjon> LagretPosisjon
        {
            get
            {
                if (_lagretPosisjon == null)
                {
                    lock (_lockLagret)
                    {
                        if (_lagretPosisjon == null)
                            _lagretPosisjon = HentFraDatabasen();
                    }
                }

                return _lagretPosisjon;

            }
        }

        protected virtual ConcurrentDictionary<string, EksternDeltakerPosisjon> HentFraDatabasen()
        {
            //using (With.ReadUncommitted())
            using (var context = _dataContextFactory.Create())
            {
                var sistePosisjoner = from p in context.DeltakerPosisjoner
                                      group p by p.DeltakerId
                                          into g
                                          select g.OrderByDescending(x => x.Tidspunkt).FirstOrDefault();

                var alle = sistePosisjoner.ToList().Select(siste => new DeltakerPosisjon
                {
                    DeltakerId = siste.DeltakerId,
                    LagId = siste.LagId,
                    Latitude = siste.Latitude,
                    Longitude = siste.Longitude,
                    Tidspunkt = siste.Tidspunkt
                }).Select(TilEksternDeltakerPosisjon);



                var dictionary = alle.ToDictionary(x => x.DeltakerId, x => x);

                return new ConcurrentDictionary<string, EksternDeltakerPosisjon>(dictionary);
            }
        }



        public DeltakerPosisjon RegistrerPosisjon(string lagId, string deltakerId, double latitude, double longitude)
        {
            var deltakerPosisjon = new DeltakerPosisjon
            {
                DeltakerId = deltakerId,
                LagId = lagId,
                Latitude = latitude,
                Longitude = longitude,
                Tidspunkt = TimeService.Now
            };

            LagrePosisjonTilDatabasen(deltakerPosisjon);

            GjeldendePosisjon[deltakerId] = TilEksternDeltakerPosisjon(deltakerPosisjon);
            return deltakerPosisjon;
        }

        private void LagrePosisjonTilDatabasen(DeltakerPosisjon posisjon)
        {
            if (LagretPosisjon.ContainsKey(posisjon.DeltakerId) &&
                ErForKortEllerHyppig(LagretPosisjon[posisjon.DeltakerId], posisjon))
                return;

            // Oppdater før skriving til databasen - eventual consistent, men strammer inn mulighenten for å smette forbi under lagring
            LagretPosisjon[posisjon.DeltakerId] = TilEksternDeltakerPosisjon(posisjon);

            Lagre(posisjon);
        }

        protected virtual void Lagre(DeltakerPosisjon posisjon)
        {
            using (var context = _dataContextFactory.Create())
            {
                context.DeltakerPosisjoner.Add(posisjon);
                context.SaveChanges();
            }
        }

        private bool ErForKortEllerHyppig(EksternDeltakerPosisjon forrige, DeltakerPosisjon posisjon)
        {
            var avstandIMeter = DistanseKalkulator.MeterMellom(forrige.Latitude, forrige.Longitude, posisjon.Latitude, posisjon.Longitude);
            var avstandISekunder = posisjon.Tidspunkt.Subtract(forrige.Tidspunkt).TotalSeconds;

            if (avstandISekunder < _appSettings.MinstTidMellomPosisjoner)
                return true;

            return avstandIMeter < _appSettings.MinstAvstandMellomPosisjoner;
        }

        public LagPosisjoner HentforLag(string lagId)
        {
            return new LagPosisjoner
            {
                LagId = lagId,
                Posisjoner = GjeldendePosisjon.Values.Where(x => x.LagId == lagId).ToList()
            };
        }

        public List<LagPosisjoner> HentforAlleLag(string scoreboardSecret)
        {
            if (scoreboardSecret != _appSettings.ScoreboardSecret)
                throw new InvalidOperationException("Denne operasjonen har ekstra sikkerhet. Feil kode.");

            var perLag = from p in GjeldendePosisjon.Values
                         group p by p.LagId
                             into g
                             select new LagPosisjoner
                             {
                                 LagId = g.Key,
                                 Posisjoner = g.ToList()
                             };

            return perLag.ToList();
        }

        private EksternDeltakerPosisjon TilEksternDeltakerPosisjon(DeltakerPosisjon posisjon)
        {
            var deltakerNavn = _tilgangsKontroll.HentDeltakerNavn(posisjon.DeltakerId);

            return new EksternDeltakerPosisjon
            {
                Navn = deltakerNavn,
                DeltakerId = posisjon.DeltakerId,
                LagId = posisjon.LagId,
                Latitude = posisjon.Latitude,
                Longitude = posisjon.Longitude,
                Tidspunkt = posisjon.Tidspunkt
            };
        }

        public void Nullstill()
        {
            _gjeldendePosisjon = HentFraDatabasen();
        }
    }

    public class LagPosisjoner
    {
        public string LagId { get; set; }
        public List<EksternDeltakerPosisjon> Posisjoner { get; set; }
    }

    public class EksternDeltakerPosisjon
    {
        public string Navn { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime Tidspunkt { get; set; }
        public string DeltakerId { get; set; }
        public string LagId { get; set; }
    }
}

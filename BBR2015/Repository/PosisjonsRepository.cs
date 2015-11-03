using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Database.Entities;
using Database;

namespace Repository
{
    public class PosisjonsRepository
    {
        // Registreres som singleton, så dictionary trenger ikke være static (sjekk ut threadsafety, dog)
        private ConcurrentDictionary<string, DeltakerPosisjon> _gjeldendePosisjon;
        private ConcurrentDictionary<string, DeltakerPosisjon> _lagretPosisjon;

        private readonly DataContextFactory _dataContextFactory;
        private readonly OverridableSettings _appSettings;

        private readonly object _lockGjeldende = new object();
        private readonly object _lockLagret = new object();

        public PosisjonsRepository(DataContextFactory dataContextFactory, OverridableSettings appSettings)
        {
            _dataContextFactory = dataContextFactory;
            _appSettings = appSettings;
        }

        private ConcurrentDictionary<string, DeltakerPosisjon> GjeldendePosisjon
        {
            get
            {
                if(_gjeldendePosisjon == null)
                {
                    lock (_lockGjeldende)
                    {
                        if(_gjeldendePosisjon == null)
                            _gjeldendePosisjon = HentFraDatabasen();
                    }
                }

                return _gjeldendePosisjon;

            }
        }

        private ConcurrentDictionary<string, DeltakerPosisjon> LagretPosisjon
        {
            get
            {
                if(_lagretPosisjon == null)
                {
                    lock (_lockLagret)
                    {
                        if(_lagretPosisjon == null)
                            _lagretPosisjon = HentFraDatabasen();
                    }
                }

                return _lagretPosisjon;

            }
        }

        private ConcurrentDictionary<string, DeltakerPosisjon> HentFraDatabasen()
        {
            using (var context = _dataContextFactory.Create())
            {
                var sistePosisjoner = from p in context.DeltakerPosisjoner
                    group p by p.DeltakerId
                    into g
                    select g.OrderByDescending(x => x.Tidspunkt).FirstOrDefault();                      

                var dictionary = sistePosisjoner.ToDictionary(x => x.DeltakerId, siste => new DeltakerPosisjon
                {
                    DeltakerId = siste.DeltakerId,
                    LagId = siste.LagId,
                    Latitude = siste.Latitude,
                    Longitude = siste.Longitude,
                    Tidspunkt = siste.Tidspunkt
                });

                return new ConcurrentDictionary<string, DeltakerPosisjon>(dictionary);
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

            GjeldendePosisjon[deltakerId] = deltakerPosisjon;
            return deltakerPosisjon;
        }

        private void LagrePosisjonTilDatabasen(DeltakerPosisjon posisjon)
        {
            if (LagretPosisjon.ContainsKey(posisjon.DeltakerId) &&
                ErForKortEllerHyppig(LagretPosisjon[posisjon.DeltakerId], posisjon))
                return;

            // Oppdater før skriving til databasen - eventual consistent, men strammer inn mulighenten for å smette forbi under lagring
            LagretPosisjon[posisjon.DeltakerId] = posisjon;

            using (var context = _dataContextFactory.Create())
            {
                context.DeltakerPosisjoner.Add(posisjon);
                context.SaveChanges();
            }            
        }

        private bool ErForKortEllerHyppig(DeltakerPosisjon forrige, DeltakerPosisjon posisjon)
        {
            var avstandIMeter = DistanseKalkulator.MeterMellom(forrige.Latitude, forrige.Longitude, posisjon.Latitude, posisjon.Longitude);
            var avstandISekunder = posisjon.Tidspunkt.Subtract(forrige.Tidspunkt).TotalSeconds;

            if (avstandISekunder < 10)
                return true;
            
            return avstandIMeter < 5;
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
    }

    public class LagPosisjoner
    {
        public string LagId { get; set; }
        public List<DeltakerPosisjon> Posisjoner { get; set; }
    }
}

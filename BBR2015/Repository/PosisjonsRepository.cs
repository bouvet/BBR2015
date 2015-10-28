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

        private AdminRepository _adminRepository;
        private DataContextFactory _dataContextFactory;
        private readonly CascadingAppSettings _appSettings;

        private object _lock = new object();

        public PosisjonsRepository(AdminRepository adminRepository, DataContextFactory dataContextFactory, CascadingAppSettings appSettings)
        {
            _adminRepository = adminRepository;
            _dataContextFactory = dataContextFactory;
            _appSettings = appSettings;
        }

        private ConcurrentDictionary<string, DeltakerPosisjon> GjeldendePosisjon
        {
            get
            {
                if (_gjeldendePosisjon == null)
                {
                    lock (_lock)
                    {
                        if(_gjeldendePosisjon == null)
                            _gjeldendePosisjon = HentFraDatabasen();
                    }
                }

                return _gjeldendePosisjon;
                
            }
        }

        private ConcurrentDictionary<string, DeltakerPosisjon> HentFraDatabasen()
        {
            using (var context = _dataContextFactory.Create())
            {
                var sistePosisjoner = from p in context.DeltakerPosisjoner
                    group p by p.DeltakerId
                    into g
                    select g.OrderByDescending(x => x.TidspunktUTC).FirstOrDefault();                      

                var dictionary = sistePosisjoner.ToDictionary(x => x.DeltakerId, siste => new DeltakerPosisjon
                {
                    DeltakerId = siste.DeltakerId,
                    LagId = siste.LagId,
                    Latitude = siste.Latitude,
                    Longitude = siste.Longitude,
                    TidspunktUTC = siste.TidspunktUTC
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
                TidspunktUTC = TimeService.Now
            };

            LagrePosisjonTilDatabasen(deltakerPosisjon);

            GjeldendePosisjon[deltakerId] = deltakerPosisjon;
            return deltakerPosisjon;
        }

        private void LagrePosisjonTilDatabasen(DeltakerPosisjon posisjon)
        {
            if (GjeldendePosisjon.ContainsKey(posisjon.DeltakerId) &&
                ErForKortEllerHyppig(GjeldendePosisjon[posisjon.DeltakerId], posisjon))
                return;

            using (var context = _dataContextFactory.Create())
            {
                context.DeltakerPosisjoner.Add(posisjon);
                context.SaveChanges();
            }
        }

        private bool ErForKortEllerHyppig(DeltakerPosisjon forrige, DeltakerPosisjon posisjon)
        {
            var avstandIMeter = DistanseKalkulator.MeterMellom(forrige.Latitude, forrige.Longitude, posisjon.Latitude, posisjon.Longitude);
            var avstandISekunder = posisjon.TidspunktUTC.Subtract(forrige.TidspunktUTC).TotalSeconds;

            // Forkast for små forflytninger eller for tette rapporteringer
            return avstandIMeter < 5 || avstandISekunder < 10;
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

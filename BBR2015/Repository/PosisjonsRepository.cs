using System;
using System.Collections.Generic;
using System.Linq;
using Database.Entities;
using Database;

namespace Repository
{
    

    public class PosisjonsRepository
    {
        // Registreres som singleton, så dictionary trenger ikke være static (sjekk ut threadsafety, dog)
        private readonly Dictionary<string, DeltakerPosisjon> GjeldendePosisjon = new Dictionary<string, DeltakerPosisjon>();

        private AdminRepository _adminRepository;
        private DataContextFactory _dataContextFactory;

        public PosisjonsRepository(AdminRepository adminRepository, DataContextFactory dataContextFactory)
        {
            _adminRepository = adminRepository;
            _dataContextFactory = dataContextFactory;
        }

        public DeltakerPosisjon RegistrerPosisjon(string lagId, string deltakerId, double latitude, double longitude)
        {           
            var deltakerPosisjon = new DeltakerPosisjon
            {
                DeltakerId = deltakerId,
                LagId = lagId,
                Latitude = latitude,
                Longitude = longitude,
                TidspunktUTC = TimeService.UtcNow
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

        public List<DeltakerPosisjon> HentforLag(string lagId)
        {
            return GjeldendePosisjon.Values.Where(x => x.LagId == lagId).ToList();            
        }
    }
}

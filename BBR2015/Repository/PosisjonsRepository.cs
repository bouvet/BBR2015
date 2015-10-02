using System;
using System.Collections.Generic;
using System.Linq;
using Modell;
using Database.Entities;
using Database;

namespace Repository
{
    public class PosisjonsRepository
    {
        private static readonly Dictionary<string, DeltakerPosisjon> GjeldendePosisjon = new Dictionary<string, DeltakerPosisjon>();
        private AdminRepository _adminRepository;
        private DataContextFactory _dataContextFactory;

        public PosisjonsRepository(AdminRepository adminRepository, DataContextFactory dataContextFactory)
        {
            _adminRepository = adminRepository;
            _dataContextFactory = dataContextFactory;
        }

        public DeltakerPosisjon RegistrerPosisjon(string lagId, string deltakerId, Koordinat koordinat)
        {           
            var deltakerPosisjon = new DeltakerPosisjon
            {
                DeltakerId = deltakerId,
                LagId = lagId,
                Latitude = koordinat.Latitude,
                Longitude = koordinat.Longitude,
                TidspunktUTC = DateTime.UtcNow
            };

            LagrePosisjonTilDatabasen(deltakerPosisjon);

            GjeldendePosisjon[deltakerId] = deltakerPosisjon;
            return deltakerPosisjon;
        }

        private void LagrePosisjonTilDatabasen(DeltakerPosisjon deltakerPosisjon)
        {
            using (var context = _dataContextFactory.Create())
            {
                context.DeltakerPosisjoner.Add(deltakerPosisjon);
                context.SaveChanges();
            }
        }

        public List<DeltakerPosisjon> HentforLag(string lagId)
        {
            return GjeldendePosisjon.Values.Where(x => x.LagId == lagId).ToList();            
        }
    }
}

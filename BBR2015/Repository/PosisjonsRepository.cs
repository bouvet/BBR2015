using System;
using System.Collections.Generic;
using System.Linq;
using Modell;

namespace Repository
{
    public static class PosisjonsRepository
    {

        private static readonly List<DeltakerPosisjon> Posisjoner = new List<DeltakerPosisjon>();
        private static readonly Dictionary<Deltaker,DeltakerPosisjon> GjeldendePosisjon = new Dictionary<Deltaker, DeltakerPosisjon>();

        public static DeltakerPosisjon RegistrerPosisjon(string lagId, string deltakerId, Koordinat koordinat)
        {
            var lag = AdminRepository.FinnLag(lagId);
            var deltaker = lag.HentDeltaker(deltakerId);
            var deltakerPosisjon = new DeltakerPosisjon(deltaker, koordinat, DateTime.UtcNow);
            Posisjoner.Add(deltakerPosisjon);
            GjeldendePosisjon[deltaker] = deltakerPosisjon;
            return deltakerPosisjon;
        }

        public static List<DeltakerPosisjon> HentforLag(string lagId)
        {
            var lag = AdminRepository.FinnLag(lagId);
            return (from deltaker in lag.Deltakere
                where GjeldendePosisjon.ContainsKey(deltaker)
                select GjeldendePosisjon[deltaker])
                .ToList();
        }
    }
}

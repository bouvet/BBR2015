using System;
using System.Collections.Generic;
using System.Linq;
using Modell;

namespace Repository
{
    public static class MeldingRepository
    {
        private static readonly List<Melding> Meldinger = new List<Melding>();

        public static  Melding PostMelding(string deltakerId, string lagId, string meldingstekst)
        {
            var lag = AdminRepository.FinnLag(lagId);
            if (lag == null)
            {
                throw new ArgumentException("Ugyldig lagId:" + lagId);
            }
            var deltaker = lag.HentDeltaker(deltakerId);
            if (deltaker == null)
            {
                throw new ArgumentException("Ugyldig deltakerId:" + deltakerId);
            }
            return PostMelding(deltaker, lag, meldingstekst);
        }

        public static Melding PostMelding(Deltaker deltaker, Lag lag, string meldingstekst)
        {
            var melding = new Melding(deltaker, lag, meldingstekst);
            return PostMelding(melding);
        }

        public static Melding PostMelding(Melding melding)
        {
            Meldinger.Add(melding);
            return melding;
        }

        public static IEnumerable<Melding> HentMeldinger(string lagId, long sekvensIfra, int maksAntall=Int32.MaxValue)
        {
            return Meldinger
                .Where(m => m.Lag.LagId.Equals(lagId) && m.SekvensId >= sekvensIfra)
                .OrderByDescending(m=>m.SekvensId)
                .Take(maksAntall);
        }
    }
}

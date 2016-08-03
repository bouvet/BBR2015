using System.Collections.Generic;
using System.Linq;
using Database.Entities;

namespace RestApi.Tests.Infrastructure
{
    public class LagFactory
    {
        public static List<Lag> SettOppLagMedDeltakere(int antallLag, int medAntallDeltakere, string lagPrefix)
        {
            return Enumerable.Range(1, antallLag)
                             .Select(x => SettOppEtLagMedDeltakere(x, medAntallDeltakere, lagPrefix))
                             .ToList();
        }

        public static Lag SettOppEtLagMedDeltakere(int lagIndex, int antallDeltakere, string prefix = "Lag")
        {
            var lag = new Lag
            {
                LagId = prefix + string.Format("{0}", lagIndex),
                Navn = prefix + string.Format("Navn{0}", lagIndex),
                Farge = prefix + string.Format("Farge{0}", lagIndex),
                HemmeligKode = prefix + string.Format("HemmeligKode{0}", lagIndex),
                Ikon = prefix + string.Format("Ikon{0}.gif", lagIndex),                
            };

            //lag.HemmeligKode = lag.LagId;

            for (int i = 1; i <= antallDeltakere; i++)
            {
                lag.LeggTilDeltaker(new Deltaker(string.Format("{0}-{1}", lag.LagId, i), string.Format("{0}-{1}Navn", lag.LagId, i)) { Kode = "Kode" + i });
            }

            return lag;
        }
    }
}

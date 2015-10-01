using System.Collections.Generic;
using System.Linq;
using Modell;

namespace Repository
{
    public static class AdminRepository
    {
        private static readonly List<Lag> Lagene = new List<Lag>();

        static AdminRepository()
        {
            var bbr1 = new Lag("BBR1", "BBR #1", "00FF00", "abc1.gif");
            bbr1.LeggTilDeltaker(new Deltaker("BBR1-A", "BBR1-A"));
            bbr1.LeggTilDeltaker(new Deltaker("BBR1-B", "BBR1-B"));
            bbr1.LeggTilDeltaker(new Deltaker("BBR1-C", "BBR1-C"));
            Lagene.Add(bbr1);

            var bbr2 = new Lag("BBR2", "BBR #2", "FFFF00", "abc2.gif");
            bbr2.LeggTilDeltaker(new Deltaker("BBR2-A", "BBR2-A"));
            bbr2.LeggTilDeltaker(new Deltaker("BBR2-B", "BBR2-B"));
            bbr2.LeggTilDeltaker(new Deltaker("BBR2-C", "BBR2-C"));
            Lagene.Add(bbr2);

            var bbr3 = new Lag("BBR3", "BBR #3", "00FFFF", "abc1.gif");
            bbr1.LeggTilDeltaker(new Deltaker("BBR3-A", "BBR3-A"));
            bbr1.LeggTilDeltaker(new Deltaker("BBR3-B", "BBR3-B"));
            bbr1.LeggTilDeltaker(new Deltaker("BBR3-C", "BBR3-C"));
            Lagene.Add(bbr3);
        }

        public static void LeggTilLag(string lagId, string navn, string farge, string ikon)
        {
            var lag = new Lag(lagId, navn, farge, ikon);
            LeggTilLag(lag);
        }

        public static void LeggTilLag(Lag lag)
        {
            Lagene.Add(lag);
        }

        public static Lag FinnLag(string lagId)
        {
            return Lagene.FirstOrDefault(l => l.LagId == lagId);
        }

        public static bool GyldigLag(string lagId)
        {
            return Lagene.Any(l => l.LagId == lagId);
        }

        public static bool GyldigDeltaker(string lagId, string deltakerId)
        {
            return Lagene.Any(l => l.LagId == lagId && l.Deltakere.Exists(d=>d.DeltakerId==deltakerId));
        }

        public static IEnumerable<Lag> AlleLag()
        {
            return Lagene;
        }
    }
}

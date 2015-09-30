using System;

namespace Modell
{
    public class DeltakerPosisjon
    {
        public Deltaker Deltaker { get; set; }
        public Koordinat Koordinat { get; set; }
        public DateTime TidspunktUtc { get; set; }

        public DeltakerPosisjon(Deltaker deltaker, Koordinat koordinat, DateTime tidspunktUtc)
        {
            Deltaker = deltaker;
            Koordinat = koordinat;
            TidspunktUtc = tidspunktUtc;
        }
    }
}
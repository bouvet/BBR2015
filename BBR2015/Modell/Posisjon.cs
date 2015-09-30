using System;

namespace Modell
{
    public class Posisjon
    {
        public Koordinat Koordinat { get; set; }
        public DateTime TidspunktUTC { get; set; }
        public Deltaker Deltaker { get; set; }
    }
}
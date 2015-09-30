using System;

namespace Modell
{
    public class Koordinat
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public Koordinat(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public override string ToString()
        {
            return string.Format("{0},{1}", Math.Round(Latitude, 5), Math.Round(Longitude, 5));
        }
    }
}
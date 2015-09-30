
using System;
using Modell;

namespace RestApi.Models
{
    public class Melding
    {
        public string LagId { get; set; }

    }

    public class PosisjonPost
    {
        public Koordinat Koordinat { get; set; }
        public DateTime TidspunktUTC { get; set; }
        public string DeltakerId { get; set; }
    }

    public class Post
    {
        public Koordinat Koordinat { get; set; }
        public int DefaultPoeng { get; set; }
        public bool Done { get; set; }


    }
}
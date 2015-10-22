using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Entities
{
    public class Post
    {
        public Guid PostId { get; set; }

        public string Navn { get; set; }

        public string HemmeligKode { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public double Altitude { get; set; }

        public string DefaultPoengArray { get; set; }

        public string Beskrivelse { get; set; }

        public string Image { get; set; }

        public string Omraade { get; set; }
    }
}

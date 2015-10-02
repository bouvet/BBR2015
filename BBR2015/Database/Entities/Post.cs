using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Entities
{
    public class Post
    {
        public Guid PostId { get; set; }

        public string HemmeligKode { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string DefaultPoengArray { get; set; }


        public string Beskrivelse { get; set; }
    }
}

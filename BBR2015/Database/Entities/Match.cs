using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Entities
{
    public class Match
    {
        public Guid MatchId { get; set; }

        public string Navn { get; set; }

        public DateTime StartUTC { get; set; }

        public DateTime SluttUTC { get; set; }

        public virtual List<LagIMatch> DeltakendeLag { get; set; }

        public virtual List<PostIMatch> Poster { get; set; }
    }
}

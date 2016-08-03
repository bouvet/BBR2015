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

        public DateTime StartTid { get; set; }

        public DateTime SluttTid { get; set; }

        public virtual List<LagIMatch> DeltakendeLag { get; set; }

        public virtual List<PostIMatch> Poster { get; set; }

        public Match()
        {
            DeltakendeLag = new List<LagIMatch>();
            Poster = new List<PostIMatch>();
        }

        public LagIMatch LeggTil(LagIMatch lag)
        {
            DeltakendeLag.Add(lag);

            return lag;
        }

        public LagIMatch LeggTil(Lag lag)
        {
            var lagIMatch = new LagIMatch
            {
                Lag = lag,
                Match = this
            };
            LeggTil(lagIMatch);

            return lagIMatch;
        }

        //public double Area_NW_Latitude { get; set; }
        //public double Area_NW_Longitude { get; set; }

        //public double Area_SE_Latitude { get; set; }
        //public double Area_SE_Longitude { get; set; }


    }
}

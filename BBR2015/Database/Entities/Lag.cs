using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Entities
{
    public class Lag
    {
        public string LagId { get; set; }

        public string Navn { get; set; }

        public string Farge { get; set; }

        public string Ikon { get; set; }

        public virtual List<Deltaker> Deltakere { get; set; }

        public string HemmeligKode { get; set; }

        public Lag()
        {
            Deltakere = new List<Deltaker>();
        }

        public Lag(string lagId, string navn, string farge, string ikon)
        {
            LagId = lagId;
            Navn = navn;
            Farge = farge;
            Ikon = ikon;
            Deltakere = new List<Deltaker>();
        }

        public void LeggTilDeltaker(Deltaker deltaker)
        {
            if (Deltakere.All(d => d.DeltakerId != deltaker.DeltakerId))
            {
                Deltakere.Add(deltaker);
            }
        }

        public void FjernDeltaker(Deltaker deltaker)
        {
            if (Deltakere.Any(d => d.DeltakerId == deltaker.DeltakerId))
            {
                Deltakere.Remove(deltaker);
            }
        }

        public Deltaker HentDeltaker(string deltakerId)
        {
            return Deltakere.FirstOrDefault(d => d.DeltakerId == deltakerId);
        }

        public bool GyldigDeltaker(string deltakerId)
        {
            return Deltakere.Any(d => d.DeltakerId == deltakerId);
        }
    }
}

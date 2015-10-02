using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Entities
{
    public class Deltaker
    {
        public string DeltakerId { get; set; }
        public string Navn { get; set; }
        public virtual Lag Lag { get; set; }

        public static string NormaliserDeltakerId(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return input.Trim()
                        .Replace(" ", string.Empty)
                        .Replace("+47", string.Empty)
                        .Replace("+46", string.Empty);
        }

        public Deltaker(string deltakerId, string navn)
        {
            DeltakerId = deltakerId;
            Navn = navn;
        }

        public Deltaker()
        {

        }
        public override string ToString()
        {
            return Navn;
        }
    }
}

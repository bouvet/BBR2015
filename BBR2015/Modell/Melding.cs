using System;
using System.ComponentModel.DataAnnotations;

namespace Modell
{
    public class Melding
    {
        private static long NextSekvensId { get; set; }
        public Deltaker Deltaker { get; private set; }
        public Lag Lag { get; private set; }
        public DateTime TidspunktUtc { get; private set; }
        public long SekvensId { get; private set; }
        public string Meldingtekst { get; private set; }

        public Melding(Deltaker deltaker, Lag lag, string meldingtekst)
        {
            Deltaker = deltaker;
            Lag = lag;
            TidspunktUtc = DateTime.UtcNow;
            SekvensId = ++NextSekvensId;
            Meldingtekst = meldingtekst;
        }
    }
}

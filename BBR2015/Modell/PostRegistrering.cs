using System;

namespace Modell
{
    public class PostRegistrering
    {
        public Post Post { get; set; }
        public Våpen BruktVåpen { get; set; }
        public Deltaker Deltaker { get; set; }
        public Lag Lag { get; set; }
        public DateTime RegistrertTidUTC { get; set; }
        public int Poeng { get; set; }

        public PostRegistrering(Post post, Våpen bruktVåpen, Deltaker deltaker, Lag lag, DateTime registrertTidUTC, int poeng)
        {
            Post = post;
            BruktVåpen = bruktVåpen;
            Deltaker = deltaker;
            Lag = lag;
            RegistrertTidUTC = registrertTidUTC;
            Poeng = poeng;
        }
    }
}

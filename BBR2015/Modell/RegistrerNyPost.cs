using Database.Entities;

namespace Modell
{
    public class RegistrerNyPost
    {
        public string PostKode { get; set; }
        public string BruktVåpen { get; set; }

        public RegistrerNyPost(string postKode, string bruktVåpen)
        {
            PostKode = postKode;
            BruktVåpen = bruktVåpen;
        }
    }
}

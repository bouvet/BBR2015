namespace Modell
{
    public class RegistrerNyPost
    {
        public string PostKode { get; set; }
        public Våpen BruktVåpen { get; set; }

        public RegistrerNyPost(string postKode, Våpen bruktVåpen)
        {
            PostKode = postKode;
            BruktVåpen = bruktVåpen;
        }
    }
}

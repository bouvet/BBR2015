namespace Modell
{
    public class Post
    {
        public string Kode { get; set; }
        public Koordinat Koordinat { get; set; }
        public int[] DefaultPoeng { get; set; }

        public Post(string kode, Koordinat koordinat, int[] defaultPoeng)
        {
            Kode = kode;
            Koordinat = koordinat;
            DefaultPoeng = defaultPoeng;
        }
    }
}

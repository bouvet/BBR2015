namespace Modell
{
    public class Våpen
    {
        public string Navn { get; set; }
        public string Beskrivelse { get; set; }

        public Våpen(string navn, string beskrivelse)
        {
            Navn = navn;
            Beskrivelse = beskrivelse;
        }
    }
}
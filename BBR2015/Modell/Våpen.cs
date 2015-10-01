namespace Modell
{
    public class Våpen
    {
        public string Id { get; set; }
        public string Beskrivelse { get; set; }

        public Våpen(string id, string beskrivelse)
        {
            Id = id;
            Beskrivelse = beskrivelse;
        }
    }
}
namespace Modell
{
    public class Deltaker
    {
        public string DeltakerId { get; set; }
        public string Navn { get; set; }

        public Deltaker(string deltakerId, string navn)
        {
            DeltakerId = deltakerId;
            Navn = navn;
        }

        public override string ToString()
        {
            return Navn;
        }
    }
}
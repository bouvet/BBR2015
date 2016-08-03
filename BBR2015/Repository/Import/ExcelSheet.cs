using System.Security.Cryptography;

namespace Repository.Import
{
    public class ExcelSheet
    {
        public class Match
        {
            public const string SheetName = "Match";

            public const string MatchId = "MatchId";
            public const string Navn = "Navn";
            public const string Starttid = "Starttid";
            public const string Sluttid = "Sluttid";

            public const string DefaultPostPoengfordeling = "DefaultPostPoengfordeling";

            public static string[] Kolonner = { MatchId, Navn, Starttid, Sluttid };
        }

        public class Lag
        {
            public const string SheetName = "Lag";

            public const string LagId = "LagId";
            public const string Navn = "Navn";
            public const string HemmeligKode = "HemmeligKode";
            public const string Farge = "Farge";

            public static string[] Kolonner = { LagId, Navn, HemmeligKode, Farge };
        }

        public class Deltakere
        {
            public const string SheetName = "Deltakere";

            public const string Tidsmerke = "Tidsmerke";

            public const string Navn = "Navn";
            public const string Kode = "Hemmelig lagkode (som utdelt)";
            public const string Lag = "Lag";

            public static string[] Kolonner = { Tidsmerke, Navn, Kode, Lag };
        }

        public class Poster
        {
            public const string SheetName = "Poster";

            public const string PostId = "PostId";
            public const string Navn = "Navn";

            public const string Latitude = "Latitude";
            public const string Longitude = "Longitude";
            public const string Altitude = "Altitude";
            public const string PoengFordeling = "PoengFordeling";
            public const string SynligFra = "SynligFra";
            public const string SynligTil = "SynligTil";
            public const string BildeUrl = "BildeUrl";
            public const string Beskrivelse = "Beskrivelse";
            public const string Område = "Område";
            public const string HemmeligKode = "HemmeligKode";

            public static string[] Kolonner = { Navn, Latitude, Longitude, Beskrivelse, HemmeligKode, SynligFra, SynligTil, PoengFordeling, BildeUrl, Område, Altitude };
        }

    }
}

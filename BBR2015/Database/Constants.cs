using Database;

namespace Database
{
    public class Constants
    {
        public class Område
        {
            public const string Oscarsborg = "Oscarsborg";
            public const string OscarsborgFredag= "OscarsborgFredag";
        }

        public class Våpen
        {
            public const string Felle = "FELLE";
            public const string Bombe = "BOMBE";

            public const int BombeSkjulerPostIAntallSekunder = 60;
        }

        public class Meldinger
        {
            public const int MaxAntallUtenSekvensId = 10;
        }

        public class Headers
        {
            public const string HTTPHEADER_LAGKODE = "LagKode";
            public const string REQUESTPROPERTY_LAGID = "LagId";

            public const string HTTPHEADER_DELTAKERKODE = "DeltakerKode";
            public const string REQUESTPROPERTY_DELTAKERID = "DeltakerId";
        }

        public const string AdminLagId = "SUPPORT_1";
    }

}
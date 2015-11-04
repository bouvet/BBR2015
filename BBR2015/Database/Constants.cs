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

            public const int BombeSkjulerPostIAntallSekunder = 60*5;
        }

        public class Meldinger
        {
            public const int MaxAntallUtenSekvensId = 10;
        }

        public class Headers
        {
            public const string HTTPHEADER_LAGKODE = "LagId";
            public const string REQUESTPROPERTY_LAGID = "LagId";

            public const string HTTPHEADER_DELTAKERKODE = "DeltakerId";
            public const string REQUESTPROPERTY_DELTAKERID = "DeltakerId";
        }
    }

}
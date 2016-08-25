using System;
using System.Collections.Generic;
using System.Configuration;

namespace Database
{
    public class OverridableSettings
    {
        public OverridableSettings()
        {
            
        }
        private readonly Dictionary<string, string> _overrides = new Dictionary<string, string>();

        private const string ConnectionStringKey = "BBR_DatabaseConnectionString";

        public string DatabaseConnectionString
        {
            get
            {
                if (_overrides.ContainsKey(ConnectionStringKey))
                    return _overrides[ConnectionStringKey];

                return ConfigurationManager.ConnectionStrings[ConnectionStringKey].ConnectionString;
            }
            set { _overrides[ConnectionStringKey] = value; }
        }

        public string ScoreboardSecret
        {
            get { return Get("BBR_ScoreboardSecret"); }
            set { _overrides["BBR_ScoreboardSecret"] = value; }
        }

        public int MinstAvstandMellomPosisjoner
        {
            get
            {
                int meter;
                if (int.TryParse(Get("BBR_MinstAvstandMellomPosisjoner"), out meter))
                    return meter;
                return 5;
            }
        }

        public int MinstTidMellomPosisjoner
        {
            get
            {
                int sekunder;
                if (int.TryParse(Get("BBR_MinstSekunderMellomPosisjoner"), out sekunder))
                    return sekunder;
                return 10;
            }
        }

        public int PostSkjulesISekunderEtterVåpen
        {
            get
            {
                int sekunder;
                if (int.TryParse(Get("BBR_PostSkjulesISekunderEtterVåpen"), out sekunder))
                    return sekunder;
                return Constants.Våpen.BombeSkjulerPostIAntallSekunder;
            }
        }

        public int MinsteTidMellomRequestsIMs
        {
            get
            {
                int ms;
                if (int.TryParse(Get("BBR_MinsteTidMellomRequestsIMs"), out ms))
                    return ms;
                return 1;
            }
        }

        public bool KjørReadUncommitted
        {
            get
            {
                bool readUncommitted;
                if (Boolean.TryParse(Get("BBR_ReadUncommitted"), out readUncommitted))
                    return readUncommitted;
                return false;
            }
            set { _overrides["BBR_ReadUncommitted"] = value.ToString(); }
        }

        public bool TillatOpprettNyttLag
        {
            get
            {
                bool value;
                if (Boolean.TryParse(Get("BBR_TillatOpprettNyttLag"), out value))
                    return value;
                return false;
            }
            set { _overrides["BBR_TillatOpprettNyttLag"] = value.ToString(); }
        }

        public bool TillatOpprettNySpiller
        {
            get
            {
                bool value;
                if (Boolean.TryParse(Get("BBR_TillatOpprettNySpiller"), out value))
                    return value;
                return false;
            }
            set { _overrides["BBR_TillatOpprettNySpiller"] = value.ToString(); }
        }

        public bool TillatSlettAlleData {
            get
            {
                bool value;
                if (Boolean.TryParse(Get("BBR_TillatSlettAlleData"), out value))
                    return value;
                return false;
            }
            set { _overrides["BBR_TillatSlettAlleData"] = value.ToString(); }
        }


        public string EksternInfoUrl
        {
            get { return Get("BBR_EksternInfoUrl"); }
            set { _overrides["BBR_EksternInfoUrl"] = value; }
        }

        public string TestklientUrl
        {
            get { return Get("BBR_TestklientUrl"); }
            set { _overrides["BBR_TestklientUrl"] = value; }
        }
        
        private string Get(string settingsKey)
        {
            if (_overrides.ContainsKey(settingsKey))
                return _overrides[settingsKey];

            return ConfigurationManager.AppSettings[settingsKey];
        }
    }
}

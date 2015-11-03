using System;
using System.Collections.Generic;
using System.Configuration;

namespace Database
{
    public class OverridableSettings
    {
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

        private string Get(string settingsKey)
        {
           if (_overrides.ContainsKey(settingsKey))
                return _overrides[settingsKey];

            return ConfigurationManager.AppSettings[settingsKey];
        }
    }

}

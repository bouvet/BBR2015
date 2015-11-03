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

        private string Get(string settingsKey)
        {
           if (_overrides.ContainsKey(settingsKey))
                return _overrides[settingsKey];

            return ConfigurationManager.AppSettings[settingsKey];
        }
    }

}

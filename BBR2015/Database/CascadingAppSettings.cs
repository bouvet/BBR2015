using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Database
{
    public class CascadingAppSettings
    {
        private Dictionary<string, string> _overrides = new Dictionary<string, string>();

        public string DatabaseConnectionString
        {
            get { return Get("BBR_DatabaseConnectionString"); }
        }

        public string ScoreboardSecret
        {
            get { return Get("BBR_ScoreboardSecret"); }
            set { _overrides["BBR_ScoreboardSecret"] = value; }
        }

        private string Get(string settingsKey)
        {
            var settingValue = string.Empty;

            if (_overrides.ContainsKey(settingsKey))
                settingValue = _overrides[settingsKey];

            if (string.IsNullOrWhiteSpace(settingValue))
                settingValue = Environment.GetEnvironmentVariable(settingsKey, EnvironmentVariableTarget.Machine);

            if (string.IsNullOrWhiteSpace(settingValue))
                settingValue = Environment.GetEnvironmentVariable(settingsKey, EnvironmentVariableTarget.Process);

            if (string.IsNullOrWhiteSpace(settingValue))
                settingValue = Environment.GetEnvironmentVariable(settingsKey, EnvironmentVariableTarget.User);

            if (string.IsNullOrWhiteSpace(settingValue))
                settingValue = System.Configuration.ConfigurationManager.AppSettings[settingsKey];

            return settingValue;
        }
    }

}

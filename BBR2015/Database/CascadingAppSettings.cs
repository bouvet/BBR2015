using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    public class CascadingAppSettings
    {
        public string GetConnectionString()
        {
            return Get("BBR2015_ConnectionString");
        }

        public string Get(string settingsKey)
        {
            var connectionString = System.Configuration.ConfigurationManager.AppSettings[settingsKey];

            if (string.IsNullOrEmpty(connectionString))
                connectionString = Environment.GetEnvironmentVariable(settingsKey, EnvironmentVariableTarget.User);

            if (string.IsNullOrEmpty(connectionString))
                connectionString = Environment.GetEnvironmentVariable(settingsKey, EnvironmentVariableTarget.Process);

            if (string.IsNullOrEmpty(connectionString))
                connectionString = Environment.GetEnvironmentVariable(settingsKey, EnvironmentVariableTarget.Machine);
            
            return connectionString;
        }
    }
}

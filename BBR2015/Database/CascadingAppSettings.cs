using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    public class CascadingAppSettings
    {
        public string DatabaseConnectionString
        {
            get { return Get("BBR_DatabaseConnectionString"); }
        }

        public string Get(string settingsKey)
        {
            var connectionString = Environment.GetEnvironmentVariable(settingsKey, EnvironmentVariableTarget.Machine);

            if (string.IsNullOrEmpty(connectionString))
                connectionString = Environment.GetEnvironmentVariable(settingsKey, EnvironmentVariableTarget.Process);

            if (string.IsNullOrEmpty(connectionString))
                connectionString = Environment.GetEnvironmentVariable(settingsKey, EnvironmentVariableTarget.User);

            if (string.IsNullOrEmpty(connectionString))
                connectionString = System.Configuration.ConfigurationManager.AppSettings[settingsKey];
          
            return connectionString;
        }

        public string BaseAddress { get { return Get("BBR_WebApiBaseAddress"); }}
    }
}

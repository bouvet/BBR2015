using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    public class DataContextFactory
    {
        private CascadingAppSettings _appSettings;

        public DataContextFactory(CascadingAppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public DataContext Create()
        {            
            return new DataContext(_appSettings.DatabaseConnectionString);
            
        }
    }
}

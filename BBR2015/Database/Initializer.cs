using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Database.Migrations;

namespace Database
{
    public class Initializer : System.Data.Entity.MigrateDatabaseToLatestVersion<DataContext, Configuration>
    {
        public Initializer()
        {

        }

       
    }
}

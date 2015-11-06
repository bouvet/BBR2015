using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Database.Infrastructure;

namespace Database
{
    public class ConnectionFactory : IDbConnectionFactory
    {
        public DbConnection CreateConnection(string nameOrConnectionString)
        {
            var settings = ServiceLocator.Current.Resolve<OverridableSettings>();
            var connectionString = settings.DatabaseConnectionString;

            DbConnection connection = SqlClientFactory.Instance.CreateConnection();
            connection.ConnectionString = connectionString;

            return connection;
        }
    }
}

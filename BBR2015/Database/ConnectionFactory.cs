using System.Configuration;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using Database.Infrastructure;

namespace Database
{
    public class ConnectionFactory : IDbConnectionFactory
    {
        public DbConnection CreateConnection(string nameOrConnectionString)
        {
            string connectionString;

            if (ServiceLocator.Current != null)
            {
                var settings = ServiceLocator.Current.Resolve<OverridableSettings>();
                connectionString = settings.DatabaseConnectionString;
            }
            else
            {
                connectionString = ConfigurationManager.ConnectionStrings[0].ConnectionString;
            }

            DbConnection connection = SqlClientFactory.Instance.CreateConnection();
            connection.ConnectionString = connectionString;

            return connection;
        }
    }
}

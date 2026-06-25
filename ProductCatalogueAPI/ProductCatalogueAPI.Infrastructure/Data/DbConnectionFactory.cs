using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace ProductCatalogueAPI.Infrastructure.Data
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;
        public DbConnectionFactory(IConfiguration configuration)
        {
            // WHY we validate at startup:
            // If the connection string is missing, we want the app to
            // fail immediately on startup with a clear error —
            // not fail silently on the first database call at runtime.
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' is not configured. " +
                    "Check appsettings.json or environment variables.");
        }
        public IDbConnection CreateConnection()
        {
            // WHY we return a closed connection:
            // Dapper opens and closes the connection automatically
            // when you call QueryAsync, ExecuteAsync, etc.
            // We just need to provide a configured connection object.
            return new SqlConnection(_connectionString);
        }
    }
}

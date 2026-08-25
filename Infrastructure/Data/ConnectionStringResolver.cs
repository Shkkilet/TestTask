using Microsoft.Extensions.Configuration;

namespace Infrastructure.Data
{
    public static class ConnectionStringResolver
    {
        public static string Resolve(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' is missing. " +
                    "Please configure it using: dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"<your_connection_string>\"");
            }

            return connectionString;
        }
    }
}

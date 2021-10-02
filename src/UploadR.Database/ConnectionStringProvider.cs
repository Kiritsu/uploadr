using Npgsql;

namespace UploadR.Database
{
    public sealed class ConnectionStringProvider
    {
        public string ConnectionString { get; }

        public ConnectionStringProvider(IDatabaseConfigurationProvider databaseConfiguration)
        {
            var config = databaseConfiguration.GetConfiguration();

            ConnectionString = "Server=uploadr-postgres; Port=5432; Database=uploadr; User Id=uploadr; Password=1234";
        }
    }
}

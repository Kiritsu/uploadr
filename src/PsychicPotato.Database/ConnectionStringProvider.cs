using Npgsql;

namespace PsychicPotato.Database
{
    public sealed class ConnectionStringProvider
    {
        public string ConnectionString { get; }

        public ConnectionStringProvider(IDatabaseConfigurationProvider databaseConfiguration)
        {
            var config = databaseConfiguration.GetConfiguration();

            ConnectionString = new NpgsqlConnectionStringBuilder
            {
                Host = config.Hostname,
                Port = config.Port,
                Database = config.Database,
                Username = config.Username,
                Password = config.Password,
                UseSslStream = config.UseSsl
            }.ConnectionString;
        }
    }
}

using Microsoft.Extensions.Options;
using ShareY.Configurations;
using ShareY.Database;

namespace ShareY.Services
{
    public sealed class DatabaseConfigurationProvider : IDatabaseConfigurationProvider
    {
        private readonly DatabaseConfiguration _databaseConfiguration;

        public DatabaseConfigurationProvider(IOptions<DatabaseConfiguration> config)
        {
            _databaseConfiguration = config.Value;
        }

        public IDatabaseConfiguration GetConfiguration()
        {
            return _databaseConfiguration;
        }
    }
}

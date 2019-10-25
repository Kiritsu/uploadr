using Microsoft.Extensions.Options;
using PsychicPotato.Configurations;
using PsychicPotato.Database;

namespace PsychicPotato.Services
{
    public sealed class DatabaseConfigurationProvider : IDatabaseConfigurationProvider
    {
        private readonly IDatabaseConfiguration _databaseConfiguration;

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

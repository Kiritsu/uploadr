using Microsoft.Extensions.Options;
using UploadR.Configurations;
using UploadR.Database;

namespace UploadR.Providers
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

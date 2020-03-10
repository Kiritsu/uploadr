using System;
using Microsoft.Extensions.Options;
using UploadR.Configurations;
using UploadR.Database;

namespace UploadR.Services
{
    public class DatabaseConfigurationProvider : IDatabaseConfigurationProvider
    {
        private readonly IDatabaseConfiguration _configuration;

        public DatabaseConfigurationProvider(IOptions<DatabaseConfiguration> options)
        {
            _configuration = options.Value;
        }

        public IDatabaseConfiguration GetConfiguration()
        {
            return _configuration;
        }
    }
}

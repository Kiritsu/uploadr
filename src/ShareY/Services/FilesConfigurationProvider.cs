using Microsoft.Extensions.Options;
using ShareY.Configurations;
using ShareY.Interfaces;

namespace ShareY.Services
{
    public class FilesConfigurationProvider : IFilesConfigurationProvider
    {
        private readonly FilesConfiguration _filesConfiguration;

        public FilesConfigurationProvider(IOptions<FilesConfiguration> config)
        {
            _filesConfiguration = config.Value;
        }

        public FilesConfiguration GetConfiguration()
        {
            return _filesConfiguration;
        }
    }
}

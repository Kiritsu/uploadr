using Microsoft.Extensions.Options;
using UploadR.Configurations;
using UploadR.Interfaces;

namespace UploadR.Providers
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

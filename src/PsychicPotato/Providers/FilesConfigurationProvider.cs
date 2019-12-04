using Microsoft.Extensions.Options;
using PsychicPotato.Configurations;
using PsychicPotato.Interfaces;

namespace PsychicPotato.Providers
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

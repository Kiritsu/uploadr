using Microsoft.Extensions.Options;
using UploadR.Configurations;

namespace UploadR.Services
{
    public class UploadConfigurationProvider
    {
        private readonly UploadConfiguration _configuration;

        public UploadConfigurationProvider(IOptions<UploadConfiguration> options)
        {
            _configuration = options.Value;
        }

        public UploadConfiguration GetConfiguration()
        {
            return _configuration;
        }
    }
}
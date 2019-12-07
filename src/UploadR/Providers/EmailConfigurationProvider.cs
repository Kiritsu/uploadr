using Microsoft.Extensions.Options;
using UploadR.Configurations;
using UploadR.Interfaces;

namespace UploadR.Providers
{
    public class EmailConfigurationProvider : IEmailConfigurationProvider
    {
        private readonly EmailConfiguration _emailConfiguration;

        public EmailConfigurationProvider(IOptions<EmailConfiguration> config)
        {
            _emailConfiguration = config.Value;
        }

        public EmailConfiguration GetConfiguration()
        {
            return _emailConfiguration;
        }
    }
}

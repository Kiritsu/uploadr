using Microsoft.Extensions.Options;
using ShareY.Configurations;
using ShareY.Interfaces;

namespace ShareY.Services
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

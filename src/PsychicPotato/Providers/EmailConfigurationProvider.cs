using Microsoft.Extensions.Options;
using PsychicPotato.Configurations;
using PsychicPotato.Interfaces;

namespace PsychicPotato.Providers
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

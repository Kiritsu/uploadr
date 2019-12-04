using Microsoft.Extensions.Options;
using PsychicPotato.Configurations;
using PsychicPotato.Interfaces;

namespace PsychicPotato.Services
{
    public sealed class OneTimeTokenConfigurationProvider : IOneTimeTokenConfigurationProvider
    {
        private readonly OneTimeTokenConfiguration _ottConfiguration;

        public OneTimeTokenConfigurationProvider(IOptions<OneTimeTokenConfiguration> config)
        {
            _ottConfiguration = config.Value;
        }

        public OneTimeTokenConfiguration GetConfiguration()
        {
            return _ottConfiguration;
        }
    }
}

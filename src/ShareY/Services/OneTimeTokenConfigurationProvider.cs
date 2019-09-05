using Microsoft.Extensions.Options;
using ShareY.Configurations;
using ShareY.Interfaces;

namespace ShareY.Services
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

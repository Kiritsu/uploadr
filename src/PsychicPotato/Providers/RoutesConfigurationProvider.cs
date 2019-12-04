using Microsoft.Extensions.Options;
using PsychicPotato.Configurations;
using PsychicPotato.Interfaces;

namespace PsychicPotato.Providers
{
    public class RoutesConfigurationProvider : IRoutesConfigurationProvider
    {
        private readonly RoutesConfiguration _routesConfiguration;

        public RoutesConfigurationProvider(IOptions<RoutesConfiguration> config)
        {
            _routesConfiguration = config.Value;
        }

        public RoutesConfiguration GetConfiguration()
        {
            return _routesConfiguration;
        }
    }
}

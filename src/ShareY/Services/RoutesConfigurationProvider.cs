using Microsoft.Extensions.Options;
using ShareY.Configurations;
using ShareY.Interfaces;

namespace ShareY.Services
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

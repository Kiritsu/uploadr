using Microsoft.Extensions.Options;
using UploadR.Configurations;
using UploadR.Interfaces;

namespace UploadR.Providers
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

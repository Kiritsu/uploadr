using Microsoft.Extensions.Options;
using ShareY.Configurations;
using ShareY.Interfaces;

namespace ShareY.Services
{
    public sealed class ReCaptchaConfigurationProvider : IReCaptchaConfigurationProvider
    {
        private readonly ReCaptchaConfiguration _reCaptchaConfiguration;

        public ReCaptchaConfigurationProvider(IOptions<ReCaptchaConfiguration> config)
        {
            _reCaptchaConfiguration = config.Value;
        }

        public ReCaptchaConfiguration GetConfiguration()
        {
            return _reCaptchaConfiguration;
        }
    }
}

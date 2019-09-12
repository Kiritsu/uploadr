using ShareY.Configurations;

namespace ShareY.Interfaces
{
    public interface IReCaptchaConfigurationProvider
    {
        ReCaptchaConfiguration GetConfiguration();
    }
}

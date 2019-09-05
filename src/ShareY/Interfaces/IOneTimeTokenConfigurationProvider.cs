using ShareY.Configurations;

namespace ShareY.Interfaces
{
    public interface IOneTimeTokenConfigurationProvider
    {
        OneTimeTokenConfiguration GetConfiguration();
    }
}

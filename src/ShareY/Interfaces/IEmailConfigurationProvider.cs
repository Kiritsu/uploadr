using ShareY.Configurations;

namespace ShareY.Interfaces
{
    public interface IEmailConfigurationProvider
    {
        EmailConfiguration GetConfiguration();
    }
}

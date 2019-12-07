using UploadR.Configurations;

namespace UploadR.Interfaces
{
    public interface IOneTimeTokenConfigurationProvider
    {
        OneTimeTokenConfiguration GetConfiguration();
    }
}

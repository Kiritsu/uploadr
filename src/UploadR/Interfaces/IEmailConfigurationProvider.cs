using UploadR.Configurations;

namespace UploadR.Interfaces
{
    public interface IEmailConfigurationProvider
    {
        EmailConfiguration GetConfiguration();
    }
}

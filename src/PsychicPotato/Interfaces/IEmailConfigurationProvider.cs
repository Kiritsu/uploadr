using PsychicPotato.Configurations;

namespace PsychicPotato.Interfaces
{
    public interface IEmailConfigurationProvider
    {
        EmailConfiguration GetConfiguration();
    }
}

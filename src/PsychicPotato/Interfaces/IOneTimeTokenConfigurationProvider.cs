using PsychicPotato.Configurations;

namespace PsychicPotato.Interfaces
{
    public interface IOneTimeTokenConfigurationProvider
    {
        OneTimeTokenConfiguration GetConfiguration();
    }
}

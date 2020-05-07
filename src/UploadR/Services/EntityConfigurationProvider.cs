using Microsoft.Extensions.Options;
using UploadR.Configurations;

namespace UploadR.Services
{
    public class EntityConfigurationProvider<T> where T : EntityConfiguration, new()
    {
        private readonly T _configuration;

        public EntityConfigurationProvider(IOptions<T> options)
        {
            _configuration = options.Value;
        }

        public T GetConfiguration()
        {
            return _configuration;
        }
    }
}
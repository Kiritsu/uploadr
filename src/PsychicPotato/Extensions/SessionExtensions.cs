using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace PsychicPotato.Extensions
{
    public static class SessionExtensions
    {
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T Get<T>(this ISession session, string key)
        {
            var element = session.GetString(key);

            return element is null
                ? default
                : JsonConvert.DeserializeObject<T>(element);
        }
    }
}

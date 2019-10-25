using System.IO;
using Microsoft.EntityFrameworkCore.Design;
using Newtonsoft.Json.Linq;
using PsychicPotato.Configurations;
using PsychicPotato.Database;

namespace PsychicPotato
{
    public sealed class DesignTimePsychicPotatoContextFactory : IDesignTimeDbContextFactory<PsychicPotatoContext>
    {
        public PsychicPotatoContext CreateDbContext(string[] args)
        {
            return new PsychicPotatoContext(new ConnectionStringProvider(new DesignTimeDatabaseConfigurationProvider()));
        }
    }

    public sealed class DesignTimeDatabaseConfigurationProvider : IDatabaseConfigurationProvider
    {
        public IDatabaseConfiguration GetConfiguration()
        {
            var config = JObject.Parse(File.ReadAllText("potato.json"))["Database"];

            return new DatabaseConfiguration
            {
                Database = config["Database"].Value<string>(),
                Hostname = config["Hostname"].Value<string>(),
                Password = config["Password"].Value<string>(),
                Username = config["Username"].Value<string>(),
                Port = config["Port"].Value<int>(),
                UseSsl = config["UseSsl"].Value<bool>()
            };
        }
    }
}

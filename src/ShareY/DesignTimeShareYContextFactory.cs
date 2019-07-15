using System.IO;
using Microsoft.EntityFrameworkCore.Design;
using Newtonsoft.Json.Linq;
using ShareY.Configurations;
using ShareY.Database;

namespace ShareY
{
    public sealed class DesignTimeShareYContextFactory : IDesignTimeDbContextFactory<ShareYContext>
    {
        public ShareYContext CreateDbContext(string[] args)
        {
            return new ShareYContext(new ConnectionStringProvider(new DesignTimeDatabaseConfigurationProvider()));
        }
    }

    public sealed class DesignTimeDatabaseConfigurationProvider : IDatabaseConfigurationProvider
    {
        public IDatabaseConfiguration GetConfiguration()
        {
            var config = JObject.Parse(File.ReadAllText("sharey.json"))["Database"];

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

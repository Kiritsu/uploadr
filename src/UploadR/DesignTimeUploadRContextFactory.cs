using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.EntityFrameworkCore.Design;
using Newtonsoft.Json.Linq;
using UploadR.Configurations;
using UploadR.Database;

namespace UploadR
{
    public sealed class DesignTimeUploadRContextFactory : IDesignTimeDbContextFactory<UploadRContext>
    {
        public UploadRContext CreateDbContext(string[] args)
        {
            var connectionStringProvider = new ConnectionStringProvider(new DesignTimeDatabaseConfigurationProvider());
            var builder = new DbContextOptionsBuilder()
                .UseNpgsql(connectionStringProvider.ConnectionString);
            return new UploadRContext(builder.Options);
        }
    }

    public sealed class DesignTimeDatabaseConfigurationProvider : IDatabaseConfigurationProvider
    {
        public IDatabaseConfiguration GetConfiguration()
        {
            var config = JObject.Parse(File.ReadAllText("uploadr.json"))["Database"];

            return new DatabaseConfiguration
            {
                Database = config["Database"].Value<string>(),
                Hostname = config["Hostname"].Value<string>(),
                Password = config["Password"].Value<string>(),
                Username = config["Username"].Value<string>(),
                Port = config["Port"].Value<int>()
            };
        }
    }
}
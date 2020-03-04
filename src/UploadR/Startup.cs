using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UploadR.Configurations;
using UploadR.Database;
using UploadR.Services;

namespace UploadR
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            var path = Environment.GetEnvironmentVariable("UPLOADR_PATH")
                ?? "uploadr.json";

            var cfg = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .AddJsonFile(path, false)
                .Build();

            Configuration = cfg;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            services.AddMemoryCache();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.Configure<DatabaseConfiguration>(Configuration.GetSection("Database"));
            services.AddSingleton<IDatabaseConfigurationProvider, DatabaseConfigurationProvider>();

            services.AddSingleton<AccountService>();

            services.AddSingleton<ConnectionStringProvider>();
            services.AddDbContext<UploadRContext>(ServiceLifetime.Scoped);

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseIpRateLimiting();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapControllers());

            using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<UploadRContext>();
            db.Database.Migrate();
        }
    }
}

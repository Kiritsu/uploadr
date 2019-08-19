using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShareY.Authentications;
using ShareY.Configurations;
using ShareY.Database;
using ShareY.Interfaces;
using ShareY.Services;

namespace ShareY
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            var envVariableConf = Environment.GetEnvironmentVariable("SHAREY_CONFIGURATION");

            var configPath = !string.IsNullOrWhiteSpace(envVariableConf) && File.Exists(envVariableConf)
                ? envVariableConf
                : "sharey.json";

            var cfg = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .AddJsonFile(configPath, false)
                .Build();

            Configuration = cfg;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<DatabaseConfiguration>(x => Configuration.GetSection("Database").Bind(x));
            services.Configure<RoutesConfiguration>(x => Configuration.GetSection("Routes").Bind(x));
            services.Configure<FilesConfiguration>(x => Configuration.GetSection("Files").Bind(x));

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddSingleton<IDatabaseConfigurationProvider, DatabaseConfigurationProvider>();
            services.AddSingleton<IRoutesConfigurationProvider, RoutesConfigurationProvider>();
            services.AddSingleton<IFilesConfigurationProvider, FilesConfigurationProvider>();

            services.AddSingleton<ConnectionStringProvider>();
            services.AddDbContext<ShareYContext>(ServiceLifetime.Transient);

            services.AddAuthentication(TokenAuthenticationHandler.AuthenticationSchemeName)
                .AddScheme<TokenAuthenticationOptions, TokenAuthenticationHandler>(TokenAuthenticationHandler.AuthenticationSchemeName, null);

            services.AddSession(options =>
            {
                options.Cookie.IsEssential = true;
                options.Cookie.HttpOnly = true;
            });

            services.AddMvc()
                .AddControllersAsServices()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection()
                .UseStaticFiles()
                .UseAuthentication()
                .UseCookiePolicy()
                .UseSession()
                .UseMvc(routes => { });

            using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            using (var db = scope.ServiceProvider.GetRequiredService<ShareYContext>())
            {
                db.Database.Migrate();
            }
        }
    }
}

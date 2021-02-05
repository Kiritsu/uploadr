using System;
using System.Security.Cryptography;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UploadR.Authentications;
using UploadR.Configurations;
using UploadR.Database;
using UploadR.Database.Models;
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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            
            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            services.AddMemoryCache();
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.Configure<DatabaseConfiguration>(Configuration.GetSection("Database"));
            services.AddSingleton<IDatabaseConfigurationProvider, DatabaseConfigurationProvider>();
            
            services.Configure<UploadConfiguration>(Configuration.GetSection("Upload"));
            services.AddSingleton<EntityConfigurationProvider<UploadConfiguration>>();
            
            services.Configure<ShortenConfiguration>(Configuration.GetSection("Shorten"));
            services.AddSingleton<EntityConfigurationProvider<ShortenConfiguration>>();

            services.AddSingleton<SHA512Managed>();
            
            services.AddSingleton<AccountService>();
            services.AddSingleton<UploadService>();
            services.AddSingleton<ShortenService>();

            services.AddSingleton<ConnectionStringProvider>();
            services.AddDbContext<UploadRContext>(
                (provider, builder) =>
                {
                    var connectionString = provider.GetRequiredService<ConnectionStringProvider>().ConnectionString;
                    builder.UseNpgsql(connectionString);
                },
                optionsLifetime: ServiceLifetime.Singleton);
            
            services.AddAuthentication(TokenAuthenticationHandler.AuthenticationSchemeName)
                .AddScheme<TokenAuthenticationOptions, TokenAuthenticationHandler>(
                    TokenAuthenticationHandler.AuthenticationSchemeName, null);

            services.AddSingleton<IAuthorizationHandler, AdminRequirementHandler>();
            services.AddSingleton<IAuthorizationHandler, UserRequirementHandler>();
            services.AddSingleton<IAuthorizationHandler, UnverifiedRequirementHandler>();
            
            services.AddAuthorization(auth =>
            {
                auth.AddPolicy(AdminRequirement.PolicyName, 
                    policy => policy.Requirements.Add(new AdminRequirement()));
                auth.AddPolicy(UserRequirement.PolicyName, 
                    policy => policy.Requirements.Add(new UserRequirement()));
                auth.AddPolicy(UnverifiedRequirement.PolicyName,
                    policy => policy.Requirements.Add(new UnverifiedRequirement()));

                auth.DefaultPolicy = auth.GetPolicy(UserRequirement.PolicyName)!;
            });

            services.AddControllers();

            services.AddSingleton<DatabaseMigrationCheckService>();
            services.AddSingleton<IHostedService>(x => x.GetService<DatabaseMigrationCheckService>());
            
            services.AddSingleton<ExpiryCheckService<Upload>>();
            services.AddSingleton<IHostedService>(x => x.GetService<ExpiryCheckService<Upload>>());
            
            services.AddSingleton<ExpiryCheckService<ShortenedUrl>>();
            services.AddSingleton<IHostedService>(x => x.GetService<ExpiryCheckService<ShortenedUrl>>());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseIpRateLimiting();
            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
            
            //app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
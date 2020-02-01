using System;
using System.IO;
using MailKit.Net.Smtp;
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
using UploadR.Interfaces;
using UploadR.Providers;
using UploadR.Services;

namespace UploadR
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            var envVariableConf = Environment.GetEnvironmentVariable("UPLOADR_CONFIGURATION");

            var configPath = !string.IsNullOrWhiteSpace(envVariableConf) && File.Exists(envVariableConf)
                ? envVariableConf
                : "uploadr.json";

            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException("Unable to find config path. Setup UPLOADR_CONFIGURATION env variable first.");
            }

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
            services.Configure<OneTimeTokenConfiguration>(x => Configuration.GetSection("OneTimeToken").Bind(x));
            services.Configure<EmailConfiguration>(x => Configuration.GetSection("Email").Bind(x));

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddSingleton<QuickAuthService>();
            services.AddSingleton<EmailService>();
            services.AddSingleton<SmtpClient>();
            services.AddSingleton<Random>();
            services.AddSingleton<RateLimiterService<QuickAuthService>>();
            services.AddSingleton<UploadsService>();
            services.AddSingleton<UserService>();

            services.AddSingleton<IDatabaseConfigurationProvider, DatabaseConfigurationProvider>();
            services.AddSingleton<IRoutesConfigurationProvider, RoutesConfigurationProvider>();
            services.AddSingleton<IFilesConfigurationProvider, FilesConfigurationProvider>();
            services.AddSingleton<IOneTimeTokenConfigurationProvider, OneTimeTokenConfigurationProvider>();
            services.AddSingleton<IEmailConfigurationProvider, EmailConfigurationProvider>();

            services.AddSingleton<ConnectionStringProvider>();
            services.AddDbContext<UploadRContext>(ServiceLifetime.Transient);

            services.AddAuthentication(TokenAuthenticationHandler.AuthenticationSchemeName)
                .AddScheme<TokenAuthenticationOptions, TokenAuthenticationHandler>(TokenAuthenticationHandler.AuthenticationSchemeName, null);

            services.AddSingleton<IAuthorizationHandler, AdminRequirementHandler>();
            services.AddSingleton<IAuthorizationHandler, UserRequirementHandler>();
            services.AddAuthorization(auth =>
            {
                auth.AddPolicy(AdminRequirement.PolicyName, policy => policy.Requirements.Add(new AdminRequirement()));
                auth.AddPolicy(UserRequirement.PolicyName, policy => policy.Requirements.Add(new UserRequirement()));

                auth.DefaultPolicy = auth.GetPolicy(UserRequirement.PolicyName);
            });

            services.AddHttpContextAccessor();

            services.AddSession(options =>
            {
                options.Cookie.Name = "PotatoSession";
                options.Cookie.IsEssential = true;
                options.Cookie.HttpOnly = true;
            });

            services.AddControllersWithViews()
                .AddControllersAsServices();
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
                app.UseExceptionHandler("/");
                app.UseHsts();
            }

            app.UseHttpsRedirection()
                .UseStaticFiles()
                .UseCookiePolicy()
                .UseSession()
                .UseAuthentication()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                    endpoints.MapControllerRoute(
                        name: "default",
                        pattern: "{controller=Index}/{action=Index}/{id?}"));

            using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<UploadRContext>();
            db.Database.Migrate();
        }
    }
}

using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace UploadR
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var host = Environment.GetEnvironmentVariable("UPLOADR_HOST") ?? "localhost";
                    var port = Environment.GetEnvironmentVariable("UPLOADR_PORT") ?? "8888";

                    webBuilder.UseStartup<Startup>()
                        .UseUrls($"http://localhost:{port}/");
                });
    }
}

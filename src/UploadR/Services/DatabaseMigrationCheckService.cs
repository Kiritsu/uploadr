using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UploadR.Database;

namespace UploadR.Services
{
    public sealed class DatabaseMigrationCheckService : IHostedService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<DatabaseMigrationCheckService> _logger;
        private bool _isReady;
        
        public DatabaseMigrationCheckService(
            IServiceProvider services,
            ILogger<DatabaseMigrationCheckService> logger)
        {
            _services = services;
            _logger = logger;
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Applying database migrations...");

            using var scope = _services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<UploadRContext>();
            await db.Database.MigrateAsync(cancellationToken);

            _isReady = true;
            _logger.LogInformation("Database migrations applied");
        }

        public async Task WaitForReadyAsync()
        {
            while (!_isReady)
            {
                await Task.Delay(100);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
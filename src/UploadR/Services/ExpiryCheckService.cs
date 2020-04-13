using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UploadR.Database;
using UploadR.Database.Models;

namespace UploadR.Services
{
    public class ExpiryCheckService<T> : BackgroundService where T : EntityBase
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<ExpiryCheckService<T>> _logger;

        private CancellationTokenSource _tokenSource;
        private Task _currentTask;
        
        public T WatchedItem { get; internal set; }

        public ExpiryCheckService(
            IServiceProvider services,
            ILogger<ExpiryCheckService<T>> logger)
        {
            _services = services;
            _logger = logger;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting the ExpiryCheck service.");
            
            return RestartAsync();
        }

        public Task RestartAsync()
        {
            try
            {
                _tokenSource?.Cancel();
                _tokenSource?.Dispose();
            }
            finally
            {
                _tokenSource = new CancellationTokenSource();
            }
            
            _currentTask = ExecuteAsync(_tokenSource.Token);
            return _currentTask.IsCompleted ? _currentTask : Task.CompletedTask;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _services.GetRequiredService<IServiceScopeFactory>().CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<UploadRContext>();
                
                WatchedItem = await db.Set<T>()
                    .OrderBy(x => x.ExpiryTime)
                    .FirstOrDefaultAsync(x => x.ExpiryTime > TimeSpan.Zero && !x.Removed, stoppingToken) as T;

                if (WatchedItem is null)
                {
                    _logger.LogDebug("No item is expirying. Putting the service on hold. Waiting for restart.");
                    break;
                }

                var now = DateTimeOffset.UtcNow;
                var expiryLeft = WatchedItem.CreatedAt + WatchedItem.ExpiryTime - now;
                if (expiryLeft <= TimeSpan.Zero)
                {
                    _logger.LogError(
                        $"Item {WatchedItem.Guid} must have expired at {(now + expiryLeft):g} ({expiryLeft:g} ago).");
                    
                    WatchedItem.Removed = true;
                    db.Update(WatchedItem);
                    await db.SaveChangesAsync(stoppingToken);
                    
                    continue;
                }
                
                _logger.LogDebug(
                    $"Item {WatchedItem.Guid} must expire at {now + expiryLeft} (in {expiryLeft:g}).");
                
                await Task.Delay(expiryLeft, stoppingToken);
                
                db.Update(WatchedItem);
                await db.SaveChangesAsync(stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping the ExpiryCheck service completely.");
            
            if (_currentTask == null)
            {
                return;
            }

            try
            {
                _tokenSource?.Cancel();
            }
            finally
            {
                await Task.WhenAny(_currentTask, Task.Delay(Timeout.Infinite, cancellationToken));
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            _tokenSource?.Dispose();
        }
    }
}
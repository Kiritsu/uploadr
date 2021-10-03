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
    public class ExpiryCheckService<T> : IHostedService, IDisposable where T : BaseEntity
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<ExpiryCheckService<T>> _logger;
        private readonly DatabaseMigrationCheckService _dmcs;

        private CancellationTokenSource _tokenSource;
        private Task _currentTask;
        
        public T WatchedItem { get; internal set; }

        public ExpiryCheckService(
            IServiceProvider services,
            ILogger<ExpiryCheckService<T>> logger,
            DatabaseMigrationCheckService dmcs)
        {
            _services = services;
            _logger = logger;
            _dmcs = dmcs;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("[{Name}] Starting the ExpiryCheck service", typeof(T).Name);
            
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

        protected async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _dmcs.WaitForReadyAsync();
            
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _services.GetRequiredService<IServiceScopeFactory>().CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<UploadRContext>();
                
                WatchedItem = await db.Set<T>()
                    .OrderBy(x => x.ExpiryTime)
                    .FirstOrDefaultAsync(x => x.ExpiryTime > TimeSpan.Zero && !x.Removed, stoppingToken);

                if (WatchedItem is null)
                {
                    _logger.LogInformation(
                        "[{Name}] No item is expirying. Putting the service on hold. Waiting for restart",
                        typeof(T).Name);
                    break;
                }

                var now = DateTimeOffset.UtcNow;
                var expiryLeft = WatchedItem.CreatedAt + WatchedItem.ExpiryTime - now;
                if (expiryLeft <= TimeSpan.Zero)
                {
                    _logger.LogError(
                        "[{Name}] Item {ItemGuid} must have expired {ExpiryLeft:g} ago",
                        typeof(T).Name,
                        WatchedItem.Guid,
                        now + expiryLeft);
                    
                    WatchedItem.Removed = true;
                    db.Update(WatchedItem);
                    await db.SaveChangesAsync(stoppingToken);
                    
                    continue;
                }

                _logger.LogInformation(
                    "[{Name}] Item {ItemGuid} must expire at {ExpiresAt} (in {ExpiryLeft:g})",
                    typeof(T).Name,
                    WatchedItem.Guid,
                    now + expiryLeft,
                    expiryLeft);
                
                await Task.Delay(expiryLeft, stoppingToken);
                
                db.Update(WatchedItem);
                await db.SaveChangesAsync(stoppingToken);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("[{Name}] Stopping the ExpiryCheck service completely", typeof(T).Name);
            
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

        public void Dispose()
        {
            _tokenSource?.Dispose();
        }
    }
}
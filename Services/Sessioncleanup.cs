using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MultiTenantAPI.Data;

namespace MultiTenantAPI.Services
{
    public class SessionCleanupService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<SessionCleanupService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(2);

        public SessionCleanupService(IServiceProvider services, ILogger<SessionCleanupService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🧹 Session cleanup service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _services.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();

                    var expired = context.SessionStores
                        .Where(s => s.IsActive && s.ExpiresAt < DateTime.Now)
                        .ToList();

                    foreach (var s in expired)
                        s.IsActive = false;

                    if (expired.Count > 0)
                    {
                        await context.SaveChangesAsync();
                        _logger.LogInformation($"🧹 Deactivated {expired.Count} expired session(s).");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"❌ Cleanup error: {ex.Message}");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}
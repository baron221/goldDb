using System;
using System.Threading;
using System.Threading.Tasks;
using GoldbApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GoldbApi.Services;

public class MaterializedViewRefreshService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MaterializedViewRefreshService> _logger;

    private readonly TimeSpan _refreshInterval = TimeSpan.FromMinutes(5);

    public MaterializedViewRefreshService(IServiceProvider serviceProvider, ILogger<MaterializedViewRefreshService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Materialized View Refresh Service is starting.");

        try
        {
            _logger.LogInformation("Initial refresh of materialized views...");
            await RefreshViewsAsync();
            _logger.LogInformation("Initial refresh completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during initial materialized views refresh.");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_refreshInterval, stoppingToken);

            try
            {
                _logger.LogInformation("Refreshing materialized views...");
                await RefreshViewsAsync();
                _logger.LogInformation("Materialized views refreshed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while refreshing materialized views.");
            }
        }

        _logger.LogInformation("Materialized View Refresh Service is stopping.");
    }

    private async Task RefreshViewsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var views = new[]
        {
            "goldb.mv_admin_dashboard_summary",
            "goldb.mv_daily_order_trends",
            "goldb.mv_monthly_order_trends",
            "goldb.mv_product_performance",
            "goldb.mv_partner_retailer_stats"
        };

        foreach (var view in views)
        {
            try
            {

#pragma warning disable EF1002
                await dbContext.Database.ExecuteSqlRawAsync($"REFRESH MATERIALIZED VIEW CONCURRENTLY {view}");
#pragma warning restore EF1002
                _logger.LogDebug("Successfully refreshed {View}", view);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh {View}", view);
            }
        }
    }
}

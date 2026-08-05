using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GoldbApi.Data;
using GoldbApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GoldbApi.Services;

// Mirrors the 16:00 KST cutoff already applied at order-creation time
// (OrderService.CreateOrderAsync marks orders placed after 16:00 as
// already-approved). This catches orders placed before the cutoff that
// are still sitting unapproved once the cutoff passes.
public class LogisticsAutoApprovalService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LogisticsAutoApprovalService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);
    private static readonly TimeZoneInfo KstZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Seoul");

    private DateTime? _lastRunDateKst;

    public LogisticsAutoApprovalService(IServiceProvider serviceProvider, ILogger<LogisticsAutoApprovalService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Logistics Auto-Approval Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var kstNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, KstZone);
                var today = kstNow.Date;

                if (kstNow.Hour >= 16 && _lastRunDateKst != today)
                {
                    await AutoApproveOrdersAsync();
                    _lastRunDateKst = today;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while running logistics auto-approval check.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Logistics Auto-Approval Service is stopping.");
    }

    private async Task AutoApproveOrdersAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var pendingOrders = await dbContext.Orders
            .Where(o => o.Status == "ORDERED")
            .ToListAsync();

        if (!pendingOrders.Any())
        {
            _logger.LogInformation("Logistics auto-approval: no orders waiting for approval.");
            return;
        }

        foreach (var order in pendingOrders)
        {
            order.Status = "LogisticsApproved";
            dbContext.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId = order.Id,
                Status = "LogisticsApproved",
                Remarks = "상태 변경: ORDERED -> LogisticsApproved\n[오후 4시 자동 물류승인]"
            });
        }

        await dbContext.SaveChangesAsync();
        _logger.LogInformation("Logistics auto-approval: {Count} orders auto-approved.", pendingOrders.Count);
    }
}

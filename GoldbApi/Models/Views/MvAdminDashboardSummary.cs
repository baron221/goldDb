using Microsoft.EntityFrameworkCore;

namespace GoldbApi.Models.Views;

[Keyless]
public class MvAdminDashboardSummary
{

    public int Id { get; set; }

    public int TotalUsers { get; set; }

    public int TotalCompanies { get; set; }

    public int TotalProducts { get; set; }

    public int TotalOrders { get; set; }

    public decimal TotalRevenue { get; set; }

    public int PendingApprovalCount { get; set; }

    public int UnassignedCompanyUserCount { get; set; }

    public int UnassignedLogisticsRetailerCount { get; set; }
}

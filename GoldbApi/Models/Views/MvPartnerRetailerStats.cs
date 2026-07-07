using Microsoft.EntityFrameworkCore;

namespace GoldbApi.Models.Views;

[Keyless]
public class MvPartnerRetailerStats
{

    public int CompanyId { get; set; }

    public string CompanyName { get; set; } = string.Empty;

    public bool IsDirectManagement { get; set; }

    public string CEO { get; set; } = string.Empty;

    public string Region { get; set; } = string.Empty;

    public int? LogisticsCompanyId { get; set; }

    public int TotalStockCount { get; set; }

    public decimal TotalStockWeight { get; set; }

    public int TotalOrderCount { get; set; }

    public decimal TotalOrderAmount { get; set; }

    public int MonthlyOrderCount { get; set; }

    public decimal MonthlyOrderAmount { get; set; }

    public int PendingOrderCount { get; set; }
}

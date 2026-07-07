namespace GoldbApi.DTOs;

public class DashboardStatsResponse
{

    public int UserCount { get; set; }

    public int RoleCount { get; set; }

    public int MenuCount { get; set; }

    public int ArticleCount { get; set; }

    public List<RecentUserDto> RecentUsers { get; set; } = new();
}

public class RecentUserDto
{

    public string Username { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}

public class RetailDashboardStatsDto
{

    public int TotalOrderCount { get; set; }

    public int InProductionCount { get; set; }

    public int SettlingCount { get; set; }

    public int InDeliveryCount { get; set; }

    public int ReceivedCount { get; set; }

    public int DailySalesCount { get; set; }

    public int DailyPurchaseCount { get; set; }

    public int MonthlySalesCount { get; set; }

    public int MonthlyPurchaseCount { get; set; }

    public List<CategoryCountDto> InventoryByCategory { get; set; } = new();

    public decimal TotalUnpaid { get; set; }

    public decimal CurrentMonthCharge { get; set; }

    public decimal CurrentMonthDeposit { get; set; }

    public decimal CurrentMonthUnpaid { get; set; }
}

public class CategoryCountDto
{

    public string CategoryName { get; set; } = string.Empty;

    public int Count { get; set; }
}

public class CompanyStatsDto
{

    public int EmployeeCount { get; set; }

    public int OrderCount { get; set; }
}

public class DailyCompanyStatsResponse
{

    public List<string> Dates { get; set; } = new();

    public List<EmployeeOrderSeriesDto> Series { get; set; } = new();
}

public class EmployeeOrderSeriesDto
{

    public string EmployeeName { get; set; } = string.Empty;

    public List<int> OrderCounts { get; set; } = new();
}

public class FactoryDashboardStatsDto
{

    public int NewRequestCount { get; set; }

    public int InProductionCount { get; set; }

    public int CompletedCount { get; set; }

    public int ShippedCount { get; set; }

    public List<string> RecentDates { get; set; } = new();

    public List<int> DailyRequestCounts { get; set; } = new();

    public List<int> DailyCompletedCounts { get; set; } = new();

    public int RegularProductCount { get; set; }

    public int SetProductCount { get; set; }

    public List<CategoryCountDto> ProductByCategory { get; set; } = new();

    public List<CategoryTrendDto> ProductTrendByCategory { get; set; } = new();

    public List<CategoryTrendDto> OrderTrendByCategory { get; set; } = new();

    public List<FactoryRecentItemDto> RecentItems { get; set; } = new();
}

public class CategoryTrendDto
{

    public string CategoryName { get; set; } = string.Empty;

    public List<int> DailyCounts { get; set; } = new();
}

public class FactoryRecentItemDto
{

    public int OrderItemId { get; set; }

    public string OrderNo { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal RequestedWeight { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime RequestedAt { get; set; }
}

public class AdminDashboardStatsDto
{

    public int TotalUsers { get; set; }

    public int TotalCompanies { get; set; }

    public int TotalProducts { get; set; }

    public int TotalOrders { get; set; }

    public decimal TotalRevenue { get; set; }

    public int PendingApprovalCount { get; set; }

    public int UnassignedCompanyUserCount { get; set; }

    public int UnassignedLogisticsRetailerCount { get; set; }

    public List<TrendItemDto> DailyTrends { get; set; } = new();

    public List<TrendItemDto> MonthlyTrends { get; set; } = new();

    public List<ProductPerformanceDto> TopSellingProducts { get; set; } = new();

    public List<CategoryPerformanceDto> CategoryPerformance { get; set; } = new();

    public List<CompanyTypePerformanceDto> PerformanceByCompanyType { get; set; } = new();

    public List<CompanyPerformanceDto> TopRetailers { get; set; } = new();

    public List<CompanyPerformanceDto> TopManufacturers { get; set; } = new();

    public List<StatusCountDto> OrderStatusDistribution { get; set; } = new();

    public List<StatusCountDto> StockStatusDistribution { get; set; } = new();

    public List<RecentOrderDto> RecentOrders { get; set; } = new();

    public List<RecentLoginDto> RecentLogins { get; set; } = new();
}

public class TrendItemDto
{

    public string Label { get; set; } = string.Empty;

    public int OrderCount { get; set; }

    public decimal TotalAmount { get; set; }
}

public class ProductPerformanceDto
{

    public string ProductName { get; set; } = string.Empty;

    public string ProductNo { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal TotalAmount { get; set; }
}

public class CategoryPerformanceDto
{

    public string CategoryName { get; set; } = string.Empty;

    public int OrderCount { get; set; }

    public decimal TotalAmount { get; set; }
}

public class CompanyTypePerformanceDto
{

    public string CompanyType { get; set; } = string.Empty;

    public int CompanyCount { get; set; }

    public int ProcessedCount { get; set; }
}

public class CompanyPerformanceDto
{

    public string CompanyName { get; set; } = string.Empty;

    public int TransactionCount { get; set; }

    public decimal TotalAmount { get; set; }
}

public class StatusCountDto
{

    public string Status { get; set; } = string.Empty;

    public int Count { get; set; }
}

public class RecentOrderDto
{

    public int OrderId { get; set; }

    public string OrderNo { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}

public class RecentLoginDto
{

    public string Username { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? LastLoginIp { get; set; }

    public DateTime LastLoginAt { get; set; }
}

public class PartnerRetailerStatsDto
{

    public int CompanyId { get; set; }

    public string CompanyName { get; set; } = string.Empty;

    public bool IsDirectManagement { get; set; }

    public string CEO { get; set; } = string.Empty;

    public string Region { get; set; } = string.Empty;

    public int? LogisticsCompanyId { get; set; }

    public int TotalStockCount { get; set; }

    public decimal TotalStockWeight { get; set; }

    public int MonthlyOrderCount { get; set; }

    public decimal MonthlyOrderAmount { get; set; }

    public int TotalOrderCount { get; set; }

    public decimal TotalOrderAmount { get; set; }

    public int PendingOrderCount { get; set; }
}

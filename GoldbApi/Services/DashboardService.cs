using GoldbApi.Data;
using GoldbApi.DTOs;
using GoldbApi.Models;
using GoldbApi.Models.Views;
using GoldbApi.Repositories;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace GoldbApi.Services;

public interface IDashboardService
{

    Task<ApiResponse<DashboardStatsResponse>> GetStatsAsync();

    Task<ApiResponse<RetailDashboardStatsDto>> GetRetailStatsAsync();

    Task<ApiResponse<CompanyStatsDto>> GetCompanyStatsAsync();

    Task<ApiResponse<DailyCompanyStatsResponse>> GetDailyCompanyStatsAsync();

    Task<ApiResponse<FactoryDashboardStatsDto>> GetFactoryStatsAsync();

    Task<ApiResponse<AdminDashboardStatsDto>> GetAdminDashboardStatsAsync();

    Task<ApiResponse<List<PartnerRetailerStatsDto>>> GetPartnerRetailerStatsAsync();
}

public class DashboardService : IDashboardService
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Role> _roleRepository;
    private readonly IRepository<Menu> _menuRepository;
    private readonly IRepository<Article> _articleRepository;
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<OrderItem> _orderItemRepository;
    private readonly IRepository<Company> _companyRepository;
    private readonly IRepository<Stock> _stockRepository;
    private readonly IRepository<Receivable> _receivableRepository;
    private readonly IRepository<UserCompany> _userCompanyRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<ProductSet> _productSetRepository;
    private readonly IRepository<CommonCode> _commonCodeRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly AppDbContext _dbContext;

    public DashboardService(
        IRepository<User> userRepository,
        IRepository<Role> roleRepository,
        IRepository<Menu> menuRepository,
        IRepository<Article> articleRepository,
        IRepository<Order> orderRepository,
        IRepository<OrderItem> orderItemRepository,
        IRepository<Company> companyRepository,
        IRepository<Stock> stockRepository,
        IRepository<Receivable> receivableRepository,
        IRepository<UserCompany> userCompanyRepository,
        IRepository<Product> productRepository,
        IRepository<ProductSet> productSetRepository,
        IRepository<CommonCode> commonCodeRepository,
        ICurrentUserService currentUserService,
        AppDbContext dbContext)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _menuRepository = menuRepository;
        _articleRepository = articleRepository;
        _orderRepository = orderRepository;
        _orderItemRepository = orderItemRepository;
        _companyRepository = companyRepository;
        _stockRepository = stockRepository;
        _receivableRepository = receivableRepository;
        _userCompanyRepository = userCompanyRepository;
        _productRepository = productRepository;
        _productSetRepository = productSetRepository;
        _commonCodeRepository = commonCodeRepository;
        _currentUserService = currentUserService;
        _dbContext = dbContext;
    }

    private int GetCurrentUserId()
    {
        return _currentUserService.UserId ?? throw new UnauthorizedAccessException("User is not authenticated");
    }

    private async Task<int?> GetUserCompanyIdAsync()
    {
        var userId = GetCurrentUserId();
        var userCompany = await _userCompanyRepository.GetQueryable().FirstOrDefaultAsync(uc => uc.UserId == userId);
        return userCompany?.CompanyId;
    }

    public async Task<ApiResponse<DashboardStatsResponse>> GetStatsAsync()
    {
        var stats = new DashboardStatsResponse
        {
            UserCount = await _userRepository.GetQueryable().CountAsync(),
            RoleCount = await _roleRepository.GetQueryable().CountAsync(),
            MenuCount = await _menuRepository.GetQueryable().CountAsync(),
            ArticleCount = await _articleRepository.GetQueryable().CountAsync(),
            RecentUsers = await _userRepository.GetQueryable()
                .OrderByDescending(u => u.Id)
                .Take(5)
                .ProjectToType<RecentUserDto>()
                .ToListAsync()
        };

        return ApiResponse<DashboardStatsResponse>.Success(stats);
    }

    private IQueryable<Order> GetBaseOrdersQuery(bool excludeCancelled = true)
    {
        var query = _orderRepository.GetQueryable();
        if (excludeCancelled)
        {
            query = query.Where(o => o.Status != "Cancelled" && o.Status != "SETTLED_CANCELLED");
        }
        return query;
    }

    private async Task<int> GetOrderCountByStatusesAsync(IQueryable<Order> query, params string[] statuses)
    {
        return await query.CountAsync(o => statuses.Contains(o.Status));
    }

    public async Task<ApiResponse<AdminDashboardStatsDto>> GetAdminDashboardStatsAsync()
    {
        var stats = new AdminDashboardStatsDto();
        var baseOrders = GetBaseOrdersQuery(false);

        var summary = await _dbContext.MvAdminDashboardSummaries.FirstOrDefaultAsync() ?? new MvAdminDashboardSummary();
        stats.TotalUsers = summary.TotalUsers;
        stats.TotalCompanies = summary.TotalCompanies;
        stats.TotalProducts = summary.TotalProducts;
        stats.TotalOrders = summary.TotalOrders;
        stats.TotalRevenue = summary.TotalRevenue;
        stats.PendingApprovalCount = summary.PendingApprovalCount;
        stats.UnassignedCompanyUserCount = summary.UnassignedCompanyUserCount;
        stats.UnassignedLogisticsRetailerCount = summary.UnassignedLogisticsRetailerCount;

        var endDate = DateTime.UtcNow.Date;
        var startDate = endDate.AddDays(-14);
        var dailyData = await _dbContext.MvDailyOrderTrends
            .Where(t => t.OrderDate >= startDate)
            .ToListAsync();

        for (int i = 0; i < 15; i++)
        {
            var date = startDate.AddDays(i);
            var match = dailyData.FirstOrDefault(d => d.OrderDate == date);
            stats.DailyTrends.Add(new TrendItemDto
            {
                Label = date.ToString("MM/dd"),
                OrderCount = match?.OrderCount ?? 0,
                TotalAmount = match?.TotalAmount ?? 0
            });
        }

        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-5);
        var monthlyData = await _dbContext.MvMonthlyOrderTrends
            .Where(t => t.OrderMonth >= startOfMonth)
            .ToListAsync();

        for (int i = 0; i < 6; i++)
        {
            var month = startOfMonth.AddMonths(i);
            var match = monthlyData.FirstOrDefault(d => d.OrderMonth == month);
            stats.MonthlyTrends.Add(new TrendItemDto
            {
                Label = month.ToString("yyyy-MM"),
                OrderCount = match?.OrderCount ?? 0,
                TotalAmount = match?.TotalAmount ?? 0
            });
        }

        stats.TopSellingProducts = await _dbContext.MvProductPerformances
            .OrderByDescending(p => p.Quantity)
            .Take(10)
            .Select(p => new ProductPerformanceDto
            {
                ProductName = p.ProductName,
                ProductNo = p.ProductNo,
                Quantity = p.Quantity,
                TotalAmount = p.TotalAmount
            })
            .ToListAsync();

        stats.CategoryPerformance = await _orderItemRepository.GetQueryable()
            .Include(oi => oi.Product)
            .Where(oi => oi.Product != null && !string.IsNullOrEmpty(oi.Product.CategoryLarge))
            .GroupBy(oi => oi.Product!.CategoryLarge)
            .Select(g => new CategoryPerformanceDto
            {
                CategoryName = g.Key!,
                OrderCount = g.Select(oi => oi.OrderId).Distinct().Count(),
                TotalAmount = g.Sum(oi => oi.Price * oi.Quantity)
            })
            .OrderByDescending(c => c.TotalAmount)
            .ToListAsync();

        stats.PerformanceByCompanyType = await _companyRepository.GetQueryable()
            .GroupBy(c => c.Category)
            .Select(g => new CompanyTypePerformanceDto
            {
                CompanyType = g.Key,
                CompanyCount = g.Count()
            })
            .ToListAsync();

        var activeOrders = GetBaseOrdersQuery(true);
        stats.TopRetailers = await activeOrders
            .Include(o => o.User).ThenInclude(u => u!.UserCompanies).ThenInclude(uc => uc.Company)
            .GroupBy(o => o.User!.UserCompanies.FirstOrDefault()!.Company!.Name)
            .Select(g => new CompanyPerformanceDto
            {
                CompanyName = g.Key ?? "Unknown",
                TransactionCount = g.Count(),
                TotalAmount = g.Sum(o => o.TotalAmount)
            })
            .OrderByDescending(c => c.TotalAmount)
            .Take(10)
            .ToListAsync();

        stats.TopManufacturers = await _orderItemRepository.GetQueryable()
            .Include(oi => oi.Product).ThenInclude(p => p!.Company)
            .Where(oi => oi.Product != null && oi.Product.Company != null)
            .GroupBy(oi => oi.Product!.Company!.Name)
            .Select(g => new CompanyPerformanceDto
            {
                CompanyName = g.Key,
                TransactionCount = g.Select(oi => oi.OrderId).Distinct().Count(),
                TotalAmount = g.Sum(oi => oi.Price * oi.Quantity)
            })
            .OrderByDescending(c => c.TotalAmount)
            .Take(10)
            .ToListAsync();

        stats.OrderStatusDistribution = await baseOrders
            .GroupBy(o => o.Status)
            .Select(g => new StatusCountDto { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        stats.StockStatusDistribution = await _stockRepository.GetQueryable()
            .GroupBy(s => s.Status)
            .Select(g => new StatusCountDto { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        stats.RecentOrders = await baseOrders
            .Include(o => o.User).ThenInclude(u => u!.UserCompanies).ThenInclude(uc => uc.Company)
            .OrderByDescending(o => o.CreatedAt)
            .Take(10)
            .Select(o => new RecentOrderDto
            {
                OrderId = o.Id,
                OrderNo = o.OrderNo,
                CompanyName = o.User != null && o.User.UserCompanies.Any() ? o.User.UserCompanies.First().Company!.Name : "Unknown",
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                CreatedAt = o.CreatedAt
            })
            .ToListAsync();

        stats.RecentLogins = await _userRepository.GetQueryable()
            .Where(u => u.LastLoginAt.HasValue)
            .OrderByDescending(u => u.LastLoginAt)
            .Take(10)
            .Select(u => new RecentLoginDto
            {
                Username = u.Username,
                Name = u.Name,
                LastLoginIp = u.LastLoginIp,
                LastLoginAt = u.LastLoginAt!.Value
            })
            .ToListAsync();

        return ApiResponse<AdminDashboardStatsDto>.Success(stats);
    }

    public async Task<ApiResponse<List<PartnerRetailerStatsDto>>> GetPartnerRetailerStatsAsync()
    {
        var currentUserId = GetCurrentUserId();
        var user = await _userRepository.GetQueryable().Include(u => u.UserRoles).ThenInclude(ur => ur.Role).FirstOrDefaultAsync(u => u.Id == currentUserId);
        var isAdmin = user?.UserRoles.Any(ur => ur.Role != null && ur.Role.Name == "admin") ?? false;

        var logisticsCompanyId = await GetUserCompanyIdAsync();

        IQueryable<MvPartnerRetailerStats> query = _dbContext.MvPartnerRetailerStats;

        if (!isAdmin)
        {
            if (!logisticsCompanyId.HasValue)
            {
                return ApiResponse<List<PartnerRetailerStatsDto>>.Failure("User has no associated company.");
            }
            query = query.Where(t => t.LogisticsCompanyId == logisticsCompanyId.Value);
        }

        var stats = await query
            .Select(t => new PartnerRetailerStatsDto
            {
                CompanyId = t.CompanyId,
                CompanyName = t.CompanyName,
                IsDirectManagement = t.IsDirectManagement,
                CEO = t.CEO,
                Region = t.Region,
                LogisticsCompanyId = t.LogisticsCompanyId,
                TotalStockCount = t.TotalStockCount,
                TotalStockWeight = t.TotalStockWeight,
                TotalOrderCount = t.TotalOrderCount,
                TotalOrderAmount = t.TotalOrderAmount,
                MonthlyOrderCount = t.MonthlyOrderCount,
                MonthlyOrderAmount = t.MonthlyOrderAmount,
                PendingOrderCount = t.PendingOrderCount
            })
            .ToListAsync();

        return ApiResponse<List<PartnerRetailerStatsDto>>.Success(stats);
    }

    public async Task<ApiResponse<RetailDashboardStatsDto>> GetRetailStatsAsync()
    {
        var userId = GetCurrentUserId();
        var myOrders = _orderRepository.GetQueryable().Where(o => o.UserId == userId);

        var userCompany = await _userCompanyRepository.GetQueryable().FirstOrDefaultAsync(uc => uc.UserId == userId);
        var companyId = userCompany?.CompanyId;

        var todayUtc = DateTime.UtcNow.Date;
        var startOfMonthUtc = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var dailyPurchases = await myOrders.CountAsync(o => o.CreatedAt >= todayUtc);
        var monthlyPurchases = await myOrders.CountAsync(o => o.CreatedAt >= startOfMonthUtc);

        int dailySales = 0;
        int monthlySales = 0;
        var categoryInventory = new List<CategoryCountDto>();

        if (companyId.HasValue)
        {
            var myStocks = _stockRepository.GetQueryable().Where(s => s.CompanyId == companyId.Value);

            dailySales = await myStocks.CountAsync(s => s.Status == "SOLD" && s.ExhaustionDate >= todayUtc);
            monthlySales = await myStocks.CountAsync(s => s.Status == "SOLD" && s.ExhaustionDate >= startOfMonthUtc);

            var activeStocks = await myStocks
                .Include(s => s.Product)
                .Where(s => s.Status == "보유중" || s.Status == "대여중")
                .ToListAsync();

            categoryInventory = activeStocks
                .Where(s => s.Product != null && !string.IsNullOrEmpty(s.Product.CategoryLarge))
                .GroupBy(s => s.Product!.CategoryLarge)
                .Select(g => new CategoryCountDto
                {
                    CategoryName = g.Key!,
                    Count = g.Count()
                })
                .OrderByDescending(c => c.Count)
                .ToList();
        }

        decimal totalUnpaid = 0;
        decimal currentMonthCharge = 0;
        decimal currentMonthDeposit = 0;
        decimal currentMonthUnpaid = 0;

        if (companyId.HasValue)
        {
            var companyUserIds = await _userCompanyRepository.GetQueryable()
                .Where(uc => uc.CompanyId == companyId.Value)
                .Select(uc => uc.UserId)
                .ToListAsync();

            var companyReceivables = await _receivableRepository.GetQueryable()
                .Where(r => companyUserIds.Contains(r.UserId))
                .ToListAsync();

            totalUnpaid = companyReceivables.Where(r => r.Type == "CHARGE").Sum(r => r.RemainingAmount);
            currentMonthCharge = companyReceivables.Where(r => r.Type == "CHARGE" && r.CreatedAt >= startOfMonthUtc).Sum(r => r.Amount);
            currentMonthDeposit = companyReceivables.Where(r => r.Type == "DEPOSIT" && r.CreatedAt >= startOfMonthUtc).Sum(r => r.Amount);
            currentMonthUnpaid = companyReceivables.Where(r => r.Type == "CHARGE" && r.CreatedAt >= startOfMonthUtc).Sum(r => r.RemainingAmount);
        }

        var stats = new RetailDashboardStatsDto
        {
            TotalOrderCount = await myOrders.CountAsync(),

            InProductionCount = await myOrders.CountAsync(o => 
                o.Status == "LogisticsApproved" || 
                o.Status == "FactoryRequested" || 
                o.Status == "InspectedRequested" || 
                o.Status == "Inspected"),

            SettlingCount = await myOrders.CountAsync(o => 
                o.Status == "PENDING" || 
                o.Status == "PROCESSING" || 
                o.Status == "SETTLED"),

            InDeliveryCount = await myOrders.CountAsync(o => 
                o.Status == "DELIVERY_READY" || 
                o.Status == "DELIVERY_IN_TRANSIT" || 
                o.Status == "DELIVERED"),

            ReceivedCount = await myOrders.CountAsync(o => o.Status == "Completed"),
            DailyPurchaseCount = dailyPurchases,
            MonthlyPurchaseCount = monthlyPurchases,
            DailySalesCount = dailySales,
            MonthlySalesCount = monthlySales,
            InventoryByCategory = categoryInventory,
            TotalUnpaid = totalUnpaid,
            CurrentMonthCharge = currentMonthCharge,
            CurrentMonthDeposit = currentMonthDeposit,
            CurrentMonthUnpaid = currentMonthUnpaid
        };

        return ApiResponse<RetailDashboardStatsDto>.Success(stats);
    }

    public async Task<ApiResponse<CompanyStatsDto>> GetCompanyStatsAsync()
    {
        var userId = GetCurrentUserId();
        var userCompany = await _userCompanyRepository.GetQueryable()
            .Include(uc => uc.Company)
            .FirstOrDefaultAsync(uc => uc.UserId == userId);

        if (userCompany == null || userCompany.Company == null)
        {
            return ApiResponse<CompanyStatsDto>.Failure("Company information not found.");
        }

        var companyId = userCompany.CompanyId;
        var companyType = userCompany.Company.Category;

        var employeeCount = await _userCompanyRepository.GetQueryable().CountAsync(uc => uc.CompanyId == companyId);
        int orderCount = 0;

        if (companyType == "RTL") 
        {
            orderCount = await _orderRepository.GetQueryable().CountAsync(o => o.UserId == userId);
        }
        else if (companyType == "DCC") 
        {
            orderCount = await _orderRepository.GetQueryable().CountAsync(o => o.LogisticsCompanyId == companyId);
        }
        else if (companyType == "MFG") 
        {
            orderCount = await _orderItemRepository.GetQueryable()
                .Where(oi => oi.Product != null && oi.Product.CompanyId == companyId)
                .Select(oi => oi.OrderId)
                .Distinct()
                .CountAsync();
        }

        return ApiResponse<CompanyStatsDto>.Success(new CompanyStatsDto
        {
            EmployeeCount = employeeCount,
            OrderCount = orderCount
        });
    }

    public async Task<ApiResponse<DailyCompanyStatsResponse>> GetDailyCompanyStatsAsync()
    {
        var userId = GetCurrentUserId();
        var userCompany = await _userCompanyRepository.GetQueryable()
            .Include(uc => uc.Company)
            .FirstOrDefaultAsync(uc => uc.UserId == userId);

        if (userCompany == null || userCompany.Company == null)
        {
            return ApiResponse<DailyCompanyStatsResponse>.Failure("Company information not found.");
        }

        var companyId = userCompany.CompanyId;
        var companyType = userCompany.Company.Category;

        var endDate = DateTime.UtcNow.Date;
        var startDate = endDate.AddDays(-6);
        var dates = Enumerable.Range(0, 7)
            .Select(i => startDate.AddDays(i).ToString("yyyy-MM-dd"))
            .ToList();

        var employees = await _userCompanyRepository.GetQueryable()
            .Where(uc => uc.CompanyId == companyId)
            .Include(uc => uc.User)
            .Select(uc => new { uc.UserId, Name = uc.User != null ? uc.User.Name : "Unknown" })
            .ToListAsync();

        var response = new DailyCompanyStatsResponse { Dates = dates };

        foreach (var emp in employees)
        {
            var series = new EmployeeOrderSeriesDto { EmployeeName = emp.Name };
            var orderCounts = new List<int>();

            foreach (var dateStr in dates)
            {
                var date = DateTime.SpecifyKind(DateTime.Parse(dateStr), DateTimeKind.Utc);
                var nextDate = date.AddDays(1);
                int count = 0;

                if (companyType == "RTL")
                {
                    count = await _orderRepository.GetQueryable().CountAsync(o => 
                        o.UserId == emp.UserId && 
                        o.CreatedAt >= date && o.CreatedAt < nextDate);
                }
                else if (companyType == "DCC")
                {
                    count = await _orderRepository.GetQueryable().CountAsync(o => 
                        o.LogisticsCompanyId == companyId && 
                        o.CreatedAt >= date && o.CreatedAt < nextDate);
                }
                else if (companyType == "MFG")
                {
                    count = await _orderItemRepository.GetQueryable()
                        .Where(oi => oi.Product != null && oi.Product.CompanyId == companyId &&
                                     oi.CreatedAt >= date && oi.CreatedAt < nextDate)
                        .Select(oi => oi.OrderId)
                        .Distinct()
                        .CountAsync();
                }

                orderCounts.Add(count);
            }
            series.OrderCounts = orderCounts;
            response.Series.Add(series);

            if (companyType != "RTL") break; 
        }

        if (companyType != "RTL" && response.Series.Count > 0)
        {
            response.Series[0].EmployeeName = "업체 전체";
        }

        return ApiResponse<DailyCompanyStatsResponse>.Success(response);
    }

    public async Task<ApiResponse<FactoryDashboardStatsDto>> GetFactoryStatsAsync()
    {
        var userId = GetCurrentUserId();
        var userCompany = await _userCompanyRepository.GetQueryable()
            .FirstOrDefaultAsync(uc => uc.UserId == userId);

        if (userCompany == null)
        {
            return ApiResponse<FactoryDashboardStatsDto>.Failure("소속 업체 정보가 없습니다.");
        }

        var companyId = userCompany.CompanyId;

        var factoryOrderItems = _orderItemRepository.GetQueryable()
            .Include(oi => oi.Order)
            .Include(oi => oi.Product)
            .Where(oi => oi.Product != null && oi.Product.CompanyId == companyId && oi.Order != null);

        var stats = new FactoryDashboardStatsDto();

        stats.NewRequestCount = await factoryOrderItems.CountAsync(oi => oi.Order!.Status == "FactoryRequested");
        stats.InProductionCount = await factoryOrderItems.CountAsync(oi => oi.Order!.Status == "InspectedRequested");
        stats.CompletedCount = await factoryOrderItems.CountAsync(oi => oi.Order!.Status == "Inspected");

        var shippedStatuses = new[] { "DELIVERY_READY", "DELIVERY_IN_TRANSIT", "DELIVERED", "Completed", "PROCESSING", "SETTLED" };
        stats.ShippedCount = await factoryOrderItems.CountAsync(oi => shippedStatuses.Contains(oi.Order!.Status));

        var endDate = DateTime.UtcNow.Date;
        var startDate = endDate.AddDays(-6);

        for (int i = 0; i < 7; i++)
        {
            var date = startDate.AddDays(i);
            var nextDate = date.AddDays(1);

            stats.RecentDates.Add(date.ToString("MM/dd"));

            var reqCount = await factoryOrderItems.CountAsync(oi => oi.Order!.CreatedAt >= date && oi.Order!.CreatedAt < nextDate);
            stats.DailyRequestCounts.Add(reqCount);

            var compCount = await factoryOrderItems.CountAsync(oi => 
                (oi.Order!.Status == "Inspected" || shippedStatuses.Contains(oi.Order!.Status)) && 
                oi.UpdatedAt >= date && oi.UpdatedAt < nextDate);
            stats.DailyCompletedCounts.Add(compCount);
        }

        stats.RecentItems = await factoryOrderItems
            .Where(oi => oi.Order!.Status == "FactoryRequested" || oi.Order!.Status == "InspectedRequested")
            .OrderByDescending(oi => oi.CreatedAt)
            .Take(5)
            .ProjectToType<FactoryRecentItemDto>()
            .ToListAsync();

        stats.RegularProductCount = await _productRepository.GetQueryable()
            .CountAsync(p => p.CompanyId == companyId);

        stats.SetProductCount = await _productSetRepository.GetQueryable()
            .CountAsync(ps => ps.CompanyId == companyId);

        var productByCategoryCodes = await _productRepository.GetQueryable()
            .Where(p => p.CompanyId == companyId && !string.IsNullOrEmpty(p.CategoryLarge))
            .GroupBy(p => p.CategoryLarge)
            .Select(g => new CategoryCountDto
            {
                CategoryName = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(c => c.Count)
            .ToListAsync();

        var topCategoryCodes = productByCategoryCodes.Take(5).Select(c => c.CategoryName).ToList();
        var productTrendData = await _productRepository.GetQueryable()
            .Where(p => p.CompanyId == companyId && p.CreatedAt >= startDate && topCategoryCodes.Contains(p.CategoryLarge))
            .GroupBy(p => new { p.CategoryLarge, Date = p.CreatedAt.Date })
            .Select(g => new { g.Key.CategoryLarge, g.Key.Date, Count = g.Count() })
            .ToListAsync();

        var allCategoryCodes = productByCategoryCodes.Select(c => c.CategoryName).Distinct().ToList();
        var categoryCodes = await _commonCodeRepository.GetQueryable()
            .Where(c => allCategoryCodes.Contains(c.Code))
            .ToListAsync();
        var categoryNameMap = categoryCodes
            .GroupBy(c => c.Code)
            .ToDictionary(g => g.Key, g => g.First().Name);

        foreach (var item in productByCategoryCodes)
        {
            item.CategoryName = categoryNameMap.GetValueOrDefault(item.CategoryName, item.CategoryName);
        }
        stats.ProductByCategory = productByCategoryCodes;

        foreach (var catCode in topCategoryCodes)
        {
            var catName = categoryNameMap.GetValueOrDefault(catCode, catCode);
            var trend = new CategoryTrendDto { CategoryName = catName };
            for (int i = 0; i < 7; i++)
            {
                var date = startDate.AddDays(i).Date;
                var dailyCount = productTrendData
                    .FirstOrDefault(d => d.CategoryLarge == catCode && d.Date == date)?.Count ?? 0;
                trend.DailyCounts.Add(dailyCount);
            }
            stats.ProductTrendByCategory.Add(trend);
        }

        var orderTrendData = await factoryOrderItems
            .Where(oi => oi.Order!.CreatedAt >= startDate)
            .GroupBy(oi => new { oi.Product!.CategoryLarge, Date = oi.Order!.CreatedAt.Date })
            .Select(g => new { g.Key.CategoryLarge, g.Key.Date, Count = g.Count() })
            .ToListAsync();

        foreach (var catCode in topCategoryCodes)
        {
            var catName = categoryNameMap.GetValueOrDefault(catCode, catCode);
            var trend = new CategoryTrendDto { CategoryName = catName };
            for (int i = 0; i < 7; i++)
            {
                var date = startDate.AddDays(i).Date;
                var dailyCount = orderTrendData
                    .FirstOrDefault(d => d.CategoryLarge == catCode && d.Date == date)?.Count ?? 0;
                trend.DailyCounts.Add(dailyCount);
            }
            stats.OrderTrendByCategory.Add(trend);
        }

        return ApiResponse<FactoryDashboardStatsDto>.Success(stats);
    }
}

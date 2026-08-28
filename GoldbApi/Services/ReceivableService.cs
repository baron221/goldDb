using GoldbApi.Data;
using GoldbApi.DTOs;
using GoldbApi.Models;
using GoldbApi.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace GoldbApi.Services;

public interface IReceivableService
{

    Task<ApiResponse<PagedResult<UserReceivableSummaryDto>>> GetUserSummariesAsync(int page, int pageSize, string? search);

    Task<ApiResponse<UserReceivableSummaryDto?>> GetUserSummaryByIdAsync(int userId);

    Task<ApiResponse<UserReceivableSummaryDto?>> GetReceivableChargeSummaryAsync(List<int> orderIds);

    Task<ApiResponse<PagedResult<ReceivableDto>>> GetReceivablesAsync(ReceivableQueryDto query);

    Task<ApiResponse<bool>> ProcessDepositAsync(CreateDepositDto request);

    Task<ApiResponse<bool>> UpdateDepositAsync(int id, UpdateDepositDto request);

    Task<ApiResponse<bool>> CancelDepositAsync(int id);

    Task<ApiResponse<LogisticsReceivableSummaryDto>> GetLogisticsSummaryAsync();

    Task<ApiResponse<PuritySummaryDto>> GetPuritySummaryAsync(int userId);

    Task<ApiResponse<LedgerBalanceDto>> GetLedgerBeforeAsync(int receivableId);

    Task<ApiResponse<PagedResult<ReceivableOrderRowDto>>> GetReceivableOrderHistoryAsync(ReceivableOrderHistoryQueryDto query);

    Task<ApiResponse<PagedResult<ReceivableOrderRowDto>>> GetCompletedReceivableOrdersAsync(ReceivableOrderHistoryQueryDto query);

    Task<ApiResponse<List<ReceivableChargeApplicationRowDto>>> GetReceivableChargeApplicationsAsync(int chargeId);

    Task<ApiResponse<ReceivableOrderHistorySummaryDto>> GetReceivableOrderHistorySummaryAsync(ReceivableOrderHistoryQueryDto query);

    Task<ApiResponse<List<ReceivableOverdueRowDto>>> GetReceivableOverdueSummaryAsync(ReceivableOverdueQueryDto query);

    // Bills the retailer for a settled order and, split per manufacturer with no
    // logistics margin, records what the logistics company now owes each of them.
    // Shared by the normal production-flow PENDING transition (OrderService) and the
    // "fulfil from existing stock" shortcut (StockService), which otherwise bypasses
    // OrderService entirely and would never bill/pay anyone for the order.
    Task CreateOrderSettlementChargesAsync(int orderId);
}

public class ReceivableService : IReceivableService
{
    private readonly IReceivableRepository _receivableRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ReceivableService> _logger;
    private readonly AppDbContext _dbContext;

    public ReceivableService(IReceivableRepository receivableRepository, ICurrentUserService currentUserService, ILogger<ReceivableService> logger, AppDbContext dbContext)
    {
        _receivableRepository = receivableRepository;
        _currentUserService = currentUserService;
        _logger = logger;
        _dbContext = dbContext;
    }

    private static decimal GetPureGoldRatio(string? purity)
    {
        if (string.IsNullOrEmpty(purity)) return 0m;
        var p = purity.ToUpperInvariant();
        if (p.Contains("24K") || p.Contains("PURE")) return 1.0m;
        if (p.Contains("18K")) return 0.825m;
        if (p.Contains("14K")) return 0.6435m;
        if (p.Contains("PT") || p.Contains("PLATINUM")) return 0.95m;
        return 0m;
    }

    // Mirrors PayableService.GetEffectiveWeight - a stored 0 doesn't count as "measured"
    // (no physical item actually weighs 0g), so it falls back to the requested weight
    // instead of ?? stopping at that 0.
    private static decimal GetEffectiveWeight(decimal? confirmedWeight, decimal? actualWeight, decimal? orderWeight)
    {
        if (confirmedWeight.HasValue && confirmedWeight.Value > 0) return confirmedWeight.Value;
        if (actualWeight.HasValue && actualWeight.Value > 0) return actualWeight.Value;
        return orderWeight ?? 0m;
    }

    public async Task CreateOrderSettlementChargesAsync(int orderId)
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null) return;

        var topLevelItems = await _dbContext.OrderItems
            .Include(oi => oi.Product)
            .Include(oi => oi.ProductSet)
            .Where(oi => oi.OrderId == orderId && oi.ParentId == null)
            .ToListAsync();

        // A DCC-confirmed per-item SettlementAmount (set via the 정산처리 screen) overrides
        // the retailerConfirm/factory-input estimate that was used when the charge was
        // first drafted at 물류도착 - this is what lets 정산처리 correct the retailer's
        // actual charge after the fact instead of it being locked in at goods-arrival time.
        var calculatedSettlementAmount = topLevelItems.Sum(oi =>
            oi.SettlementAmount ?? (((oi.RetailerConfirmMaterialCost ?? oi.FactoryInputMaterialCost ?? 0) +
             (oi.RetailerConfirmLaborCost ?? oi.FactoryInputLaborCost ?? 0)) * oi.Quantity));

        order.SettlementAmount = calculatedSettlementAmount > 0 ? calculatedSettlementAmount : order.TotalAmount;

        var calculatedPureGoldWeight = topLevelItems.Sum(oi =>
            GetEffectiveWeight(oi.ConfirmedWeight, oi.ActualWeight, oi.OrderWeight) * GetPureGoldRatio(oi.Purity) * oi.Quantity);

        var existingReceivable = await _dbContext.Receivables.FirstOrDefaultAsync(r => r.OrderId == orderId && r.Type == "CHARGE");
        if (existingReceivable != null)
        {
            var diffAmount = (order.SettlementAmount ?? 0m) - existingReceivable.Amount;
            var diffWeight = calculatedPureGoldWeight - existingReceivable.Weight;

            existingReceivable.Amount = order.SettlementAmount ?? 0m;
            existingReceivable.RemainingAmount = Math.Max(0m, existingReceivable.RemainingAmount + diffAmount);
            existingReceivable.Weight = calculatedPureGoldWeight;
            existingReceivable.RemainingWeight = Math.Max(0m, existingReceivable.RemainingWeight + diffWeight);
        }
        else
        {
            var charge = new Receivable
            {
                UserId = order.UserId,
                OrderId = order.Id,
                Type = "CHARGE",
                Amount = order.SettlementAmount ?? 0m,
                RemainingAmount = order.SettlementAmount ?? 0m,
                Weight = calculatedPureGoldWeight,
                RemainingWeight = calculatedPureGoldWeight,
                Memo = $"주문 정산 청구 (정산시작): {order.OrderNo}"
            };
            await _receivableRepository.AddAsync(charge);
        }

        if (order.LogisticsCompanyId.HasValue)
        {
            var byManufacturer = topLevelItems
                .Select(oi => new
                {
                    Item = oi,
                    ManufacturerId = oi.Product?.CompanyId ?? oi.ProductSet?.CompanyId
                })
                .Where(x => x.ManufacturerId.HasValue)
                .GroupBy(x => x.ManufacturerId!.Value);

            foreach (var group in byManufacturer)
            {
                var manufacturerAmount = group.Sum(x =>
                    ((x.Item.FactoryInputMaterialCost ?? 0) +
                     (x.Item.FactoryInputLaborCost ?? 0)) * x.Item.Quantity);

                var manufacturerWeight = group.Sum(x =>
                    GetEffectiveWeight(x.Item.ConfirmedWeight, x.Item.ActualWeight, x.Item.OrderWeight) * GetPureGoldRatio(x.Item.Purity) * x.Item.Quantity);

                var existingPayable = await _dbContext.Payables.FirstOrDefaultAsync(p => p.OrderId == orderId && p.ManufacturerCompanyId == group.Key && p.Type == "CHARGE");
                if (existingPayable != null)
                {
                    var diffAmount = manufacturerAmount - existingPayable.Amount;
                    var diffWeight = manufacturerWeight - existingPayable.Weight;

                    existingPayable.Amount = manufacturerAmount;
                    existingPayable.RemainingAmount = Math.Max(0m, existingPayable.RemainingAmount + diffAmount);
                    existingPayable.Weight = manufacturerWeight;
                    existingPayable.RemainingWeight = Math.Max(0m, existingPayable.RemainingWeight + diffWeight);
                }
                else
                {
                    _dbContext.Payables.Add(new Payable
                    {
                        LogisticsCompanyId = order.LogisticsCompanyId.Value,
                        ManufacturerCompanyId = group.Key,
                        OrderId = order.Id,
                        Type = "CHARGE",
                        Amount = manufacturerAmount,
                        RemainingAmount = manufacturerAmount,
                        Weight = manufacturerWeight,
                        RemainingWeight = manufacturerWeight,
                        Memo = $"주문 정산 청구 (정산시작): {order.OrderNo}"
                    });
                }
            }
        }

        await _receivableRepository.SaveChangesAsync();
    }

    private int GetCurrentUserId()
    {
        return _currentUserService.UserId ?? throw new UnauthorizedAccessException("User is not authenticated");
    }

    private async Task<int> GetCurrentCompanyIdAsync()
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("GetCurrentCompanyIdAsync: Searching for UserId: {UserId}", userId);

        if (userId == 0) return 0;

        var user = await _receivableRepository.GetUserWithCompaniesAsync(userId);

        if (user == null) {
            _logger.LogWarning("GetCurrentCompanyIdAsync: User not found for ID: {UserId}", userId);
            return 0;
        }

        _logger.LogInformation("GetCurrentCompanyIdAsync: Found user {Username}, associated companies: {Companies}", user.Username, string.Join(", ", user.UserCompanies.Select(uc => $"{uc.Company?.Name} (Cat: {uc.Company?.Category})")));

        var company = user.UserCompanies
            .Select(uc => uc.Company)
            .FirstOrDefault(c => c != null && c.Category == "DCC");

        if (company == null) {
            _logger.LogWarning("GetCurrentCompanyIdAsync: No DCC company found for user {Username}", user.Username);
        } else {
            _logger.LogInformation("GetCurrentCompanyIdAsync: Found DCC company: {CompanyName} (ID: {Id})", company.Name, company.Id);
        }

        return company?.Id ?? 0;
    }

    public async Task<ApiResponse<LogisticsReceivableSummaryDto>> GetLogisticsSummaryAsync()
    {
        var currentLogisticsCompanyId = await GetCurrentCompanyIdAsync();
        if (currentLogisticsCompanyId == 0)
        {
            return ApiResponse<LogisticsReceivableSummaryDto>.Failure("물류센터 소속 정보를 찾을 수 없습니다.", 400);
        }

        var managedRetailers = await _receivableRepository.GetManagedRetailersAsync(currentLogisticsCompanyId);
        var managedRetailerIds = managedRetailers.Select(r => r.Id).ToList();

        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var receivables = await _receivableRepository.GetReceivablesForLogisticsAsync(managedRetailerIds);

        _logger.LogInformation("GetLogisticsSummaryAsync: Found {Count} receivables for {RetailerCount} retailers", receivables.Count, managedRetailers.Count);
        foreach(var r in receivables.Take(5)) {
            _logger.LogInformation("Receivable ID: {Id}, Type: {Type}, Amount: {Amount}, UserId: {UserId}", r.Id, r.Type, r.Amount, r.UserId);
        }

        var totalUnpaid = receivables.Where(r => r.Type == "CHARGE").Sum(r => r.RemainingAmount);

        var currentMonthCharge = receivables
            .Where(r => r.Type == "CHARGE" && r.CreatedAt >= startOfMonth)
            .Sum(r => r.Amount);

        var currentMonthDeposit = receivables
            .Where(r => r.Type == "DEPOSIT" && r.CreatedAt >= startOfMonth)
            .Sum(r => r.Amount);

        var currentMonthUnpaid = receivables
            .Where(r => r.Type == "CHARGE" && r.CreatedAt >= startOfMonth)
            .Sum(r => r.RemainingAmount);

        var retailerSummaries = new List<RetailerReceivableSummaryDto>();

        foreach (var retailer in managedRetailers)
        {
            var retailerReceivables = receivables.Where(r => r.User != null && r.User.UserCompanies.Any(uc => uc.CompanyId == retailer.Id)).ToList();

            retailerSummaries.Add(new RetailerReceivableSummaryDto
            {
                CompanyId = retailer.Id,
                CompanyName = retailer.Name,
                TotalUnpaid = retailerReceivables.Where(r => r.Type == "CHARGE").Sum(r => r.RemainingAmount),
                CurrentMonthCharge = retailerReceivables.Where(r => r.Type == "CHARGE" && r.CreatedAt >= startOfMonth).Sum(r => r.Amount),
                CurrentMonthDeposit = retailerReceivables.Where(r => r.Type == "DEPOSIT" && r.CreatedAt >= startOfMonth).Sum(r => r.Amount)
            });
        }

        var summary = new LogisticsReceivableSummaryDto
        {
            TotalUnpaid = totalUnpaid,
            CurrentMonthCharge = currentMonthCharge,
            CurrentMonthDeposit = currentMonthDeposit,
            CurrentMonthUnpaid = currentMonthUnpaid,
            RetailerSummaries = retailerSummaries
        };

        return ApiResponse<LogisticsReceivableSummaryDto>.Success(summary);
    }

    public async Task<ApiResponse<PagedResult<UserReceivableSummaryDto>>> GetUserSummariesAsync(int page, int pageSize, string? search)
    {
        var userId = GetCurrentUserId();
        string? companyCategoryFilter = null;
        List<int>? allowedCompanyIds = null;
        if (userId != 0)
        {
            var user = await _receivableRepository.GetUserWithCompaniesAsync(userId);

            if (user != null)
            {
                var isRetailer = user.UserCompanies
                    .Any(uc => uc.Company != null && uc.Company.Category == "RTL");

                if (isRetailer)
                {
                    search = user.Username;
                }

                var isManufacturer = user.UserCompanies
                    .Any(uc => uc.Company != null && uc.Company.Category == "MFG");

                if (isManufacturer)
                {
                    companyCategoryFilter = "DCC";
                }

                // A DCC viewer's debtor list was previously unscoped (every retailer in the
                // whole app, since GetUserIdsWithReceivablesAsync is system-wide) - restrict it
                // to the same "managed retailers" set GetLogisticsSummaryAsync already uses
                // (Company.LogisticsCompanyId), so a DCC only ever sees its own customers.
                var isLogisticsCompany = user.UserCompanies
                    .Any(uc => uc.Company != null && uc.Company.Category == "DCC");

                if (isLogisticsCompany)
                {
                    companyCategoryFilter = "RTL";
                    var logisticsCompanyId = await GetCurrentCompanyIdAsync();
                    var managedRetailers = await _receivableRepository.GetManagedRetailersAsync(logisticsCompanyId);
                    allowedCompanyIds = managedRetailers.Select(r => r.Id).ToList();
                }
            }
        }

        var userIdsWithReceivables = await _receivableRepository.GetUserIdsWithReceivablesAsync();
        var totalCount = await _receivableRepository.GetUsersWithReceivablesCountAsync(search, userIdsWithReceivables, companyCategoryFilter, allowedCompanyIds);
        var users = await _receivableRepository.GetUsersWithReceivablesPagedAsync(page, pageSize, search, userIdsWithReceivables, companyCategoryFilter, allowedCompanyIds);

        var summaries = new List<UserReceivableSummaryDto>();
        foreach (var user in users)
        {
            var userReceivables = await _receivableRepository.GetReceivablesForUserAsync(user.Id);

            var totalCharge = userReceivables.Where(r => r.Type == "CHARGE").Sum(r => r.Amount);
            // "Deposited" = what actually landed on a charge: raw Amount plus any Discount
            // granted, minus whatever part is still unapplied (RemainingAmount, set in
            // ProcessDepositAsync/UpdateDepositAsync). Keeps TotalCharge - TotalDeposit ==
            // TotalReceivable consistent even with a discount or an overpayment.
            var totalDeposit = userReceivables.Where(r => r.Type == "DEPOSIT" && !r.IsCancelled).Sum(r => r.Amount + r.Discount - r.RemainingAmount);
            var totalReceivable = userReceivables.Where(r => r.Type == "CHARGE").Sum(r => r.RemainingAmount);

            var totalChargeWeight = userReceivables.Where(r => r.Type == "CHARGE").Sum(r => r.Weight);
            var totalDepositWeight = userReceivables.Where(r => r.Type == "DEPOSIT" && !r.IsCancelled).Sum(r => r.Weight - r.RemainingWeight);
            var totalReceivableWeight = userReceivables.Where(r => r.Type == "CHARGE").Sum(r => r.RemainingWeight);

            var lastPaymentDate = userReceivables.Where(r => r.Type == "DEPOSIT" && !r.IsCancelled)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => (DateTime?)r.CreatedAt)
                .FirstOrDefault();

            summaries.Add(new UserReceivableSummaryDto
            {
                UserId = user.Id,
                UserName = user.Username,
                UserDisplayName = user.Name,
                CompanyName = user.UserCompanies.FirstOrDefault()?.Company?.Name,
                TotalCharge = totalCharge,
                TotalDeposit = totalDeposit,
                TotalReceivable = totalReceivable,
                TotalChargeWeight = totalChargeWeight,
                TotalDepositWeight = totalDepositWeight,
                TotalReceivableWeight = totalReceivableWeight,
                LastPaymentDate = lastPaymentDate
            });
        }

        return ApiResponse<PagedResult<UserReceivableSummaryDto>>.Success(new PagedResult<UserReceivableSummaryDto>
        {
            Items = summaries,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<UserReceivableSummaryDto?>> GetUserSummaryByIdAsync(int userId)
    {
        var user = await _receivableRepository.GetUserWithCompaniesAsync(userId);
        if (user == null) return ApiResponse<UserReceivableSummaryDto?>.Success(null);

        var userReceivables = await _receivableRepository.GetReceivablesForUserAsync(userId);

        var totalCharge = userReceivables.Where(r => r.Type == "CHARGE").Sum(r => r.Amount);
        var totalDeposit = userReceivables.Where(r => r.Type == "DEPOSIT" && !r.IsCancelled).Sum(r => r.Amount + r.Discount - r.RemainingAmount);
        var totalReceivable = userReceivables.Where(r => r.Type == "CHARGE").Sum(r => r.RemainingAmount);

        var totalChargeWeight = userReceivables.Where(r => r.Type == "CHARGE").Sum(r => r.Weight);
        var totalDepositWeight = userReceivables.Where(r => r.Type == "DEPOSIT" && !r.IsCancelled).Sum(r => r.Weight - r.RemainingWeight);
        var totalReceivableWeight = userReceivables.Where(r => r.Type == "CHARGE").Sum(r => r.RemainingWeight);

        var lastPaymentDate = userReceivables.Where(r => r.Type == "DEPOSIT" && !r.IsCancelled)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => (DateTime?)r.CreatedAt)
            .FirstOrDefault();

        return ApiResponse<UserReceivableSummaryDto?>.Success(new UserReceivableSummaryDto
        {
            UserId = user.Id,
            UserName = user.Username,
            UserDisplayName = user.Name,
            CompanyName = user.UserCompanies.FirstOrDefault()?.Company?.Name,
            TotalCharge = totalCharge,
            TotalDeposit = totalDeposit,
            TotalReceivable = totalReceivable,
            TotalChargeWeight = totalChargeWeight,
            TotalDepositWeight = totalDepositWeight,
            TotalReceivableWeight = totalReceivableWeight,
            LastPaymentDate = lastPaymentDate
        });
    }

    // Scoped to only the selected orders' own CHARGE records instead of the retailer's
    // whole account, so 정산처리 (confirming one or several specific orders) doesn't show
    // the retailer's unrelated balance from every other order they've ever been charged
    // for. All selected orders must belong to the same retailer - mixing retailers into
    // one confirmation isn't meaningful, so that's rejected outright rather than guessed at.
    public async Task<ApiResponse<UserReceivableSummaryDto?>> GetReceivableChargeSummaryAsync(List<int> orderIds)
    {
        if (orderIds == null || orderIds.Count == 0) return ApiResponse<UserReceivableSummaryDto?>.Success(null);

        var charges = await _dbContext.Receivables
            .Include(r => r.User).ThenInclude(u => u!.UserCompanies).ThenInclude(uc => uc.Company)
            .Include(r => r.Order)
            .Where(r => r.Type == "CHARGE" && r.OrderId.HasValue && orderIds.Contains(r.OrderId.Value))
            .ToListAsync();

        if (charges.Count == 0) return ApiResponse<UserReceivableSummaryDto?>.Success(null);

        var distinctUsers = charges.Select(r => r.UserId).Distinct().ToList();
        if (distinctUsers.Count > 1)
        {
            return ApiResponse<UserReceivableSummaryDto?>.Failure("선택한 주문들이 서로 다른 거래처에 속해 있습니다. 같은 거래처의 주문만 함께 정산할 수 있습니다.", 400);
        }

        var first = charges[0];
        var userId = first.UserId;

        // "Before" (A) must reflect the retailer's WHOLE account minus what's being settled
        // right now, same convention as Payable's GetOrderChargeSummaryAsync (TotalOutstanding
        // = totalOutstanding - saleAmount) - otherwise the ledger would double-count this
        // very transaction into its own "before" balance.
        var allUserReceivables = await _receivableRepository.GetReceivablesForUserAsync(userId);
        var totalOutstandingAll = allUserReceivables.Where(r => r.Type == "CHARGE").Sum(r => r.RemainingAmount);
        var totalOutstandingWeightAll = allUserReceivables.Where(r => r.Type == "CHARGE").Sum(r => r.RemainingWeight);
        var lastPaymentDate = allUserReceivables.Where(r => r.Type == "DEPOSIT" && !r.IsCancelled)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => (DateTime?)r.CreatedAt)
            .FirstOrDefault();

        var saleAmount = charges.Sum(c => c.RemainingAmount);
        var saleWeight = charges.Sum(c => c.RemainingWeight);

        var selectedOrderIds = charges.Where(c => c.OrderId.HasValue).Select(c => c.OrderId!.Value).Distinct().ToList();

        var topLevelItems = await _dbContext.OrderItems
            .Include(oi => oi.Product).ThenInclude(p => p!.ProductPhotos)
            .Include(oi => oi.ProductSet).ThenInclude(ps => ps!.ProductSetPhotos)
            .Where(oi => selectedOrderIds.Contains(oi.OrderId) && oi.ParentId == null)
            .ToListAsync();

        var purityBreakdown = topLevelItems
            .GroupBy(oi => string.IsNullOrEmpty(oi.Purity) ? "기타" : oi.Purity!)
            .Select(g => new PurityBreakdownDto
            {
                Purity = g.Key,
                Weight = g.Sum(oi => GetEffectiveWeight(oi.ConfirmedWeight, oi.ActualWeight, oi.OrderWeight) * GetPureGoldRatio(oi.Purity) * oi.Quantity),
                Amount = g.Sum(oi => ((oi.RetailerConfirmMaterialCost ?? oi.FactoryInputMaterialCost ?? 0) + (oi.RetailerConfirmLaborCost ?? oi.FactoryInputLaborCost ?? 0)) * oi.Quantity)
            })
            .Where(b => b.Weight > 0 || b.Amount > 0)
            .OrderByDescending(b => b.Weight)
            .ToList();

        var itemsByOrder = topLevelItems.GroupBy(oi => oi.OrderId).ToDictionary(g => g.Key, g => g.ToList());

        var summaryItems = charges.Select(c =>
        {
            itemsByOrder.TryGetValue(c.OrderId ?? 0, out var orderItems);
            orderItems ??= new List<OrderItem>();
            var item = orderItems.FirstOrDefault();
            return new ReceivableChargeSummaryItemDto
            {
                ReceivableId = c.Id,
                OrderId = c.OrderId,
                OrderNo = c.Order?.OrderNo,
                ProductName = item?.Product?.Name ?? item?.ProductSet?.Title,
                ProductNo = item?.Product?.ProductNo,
                ProductPhotoUrl = item?.Product?.ProductPhotos.OrderBy(p => p.SortOrder).FirstOrDefault()?.PhotoUrl
                    ?? item?.ProductSet?.ProductSetPhotos.OrderBy(p => p.SortOrder).FirstOrDefault()?.PhotoUrl,
                Purity = item?.Purity,
                Color = item?.Color,
                Size = item?.Size,
                Memo = item?.Memo,
                Quantity = item?.Quantity ?? 0,
                ChargeWeight = c.Weight,
                ActualWeight = item != null ? GetEffectiveWeight(item.ConfirmedWeight, item.ActualWeight, item.OrderWeight) : null,
                RemainingAmount = c.RemainingAmount,
                OrderDate = c.CreatedAt,
                Items = orderItems.Select(oi => new ReceivableOrderItemSummaryDto
                {
                    ProductName = oi.Product?.Name ?? oi.ProductSet?.Title,
                    ProductNo = oi.Product?.ProductNo,
                    PhotoUrl = oi.Product?.ProductPhotos.OrderBy(p => p.SortOrder).FirstOrDefault()?.PhotoUrl
                        ?? oi.ProductSet?.ProductSetPhotos.OrderBy(p => p.SortOrder).FirstOrDefault()?.PhotoUrl,
                    Purity = oi.Purity,
                    Color = oi.Color,
                    Size = oi.Size,
                    Memo = oi.Memo,
                    Quantity = oi.Quantity,
                    ActualWeight = GetEffectiveWeight(oi.ConfirmedWeight, oi.ActualWeight, oi.OrderWeight),
                    MaterialCost = oi.RetailerConfirmMaterialCost ?? oi.FactoryInputMaterialCost ?? 0,
                    LaborCost = oi.RetailerConfirmLaborCost ?? oi.FactoryInputLaborCost ?? 0
                }).ToList()
            };
        }).OrderByDescending(i => i.OrderDate).ToList();

        return ApiResponse<UserReceivableSummaryDto?>.Success(new UserReceivableSummaryDto
        {
            UserId = first.UserId,
            UserName = first.User?.Username,
            UserDisplayName = first.User?.Name,
            CompanyName = first.User?.UserCompanies.FirstOrDefault(uc => uc.Company != null && uc.Company.Category == "RTL")?.Company?.Name
                ?? first.User?.UserCompanies.FirstOrDefault()?.Company?.Name,
            TotalCharge = charges.Sum(c => c.Amount),
            TotalDeposit = charges.Sum(c => c.Amount - c.RemainingAmount),
            TotalReceivable = totalOutstandingAll - saleAmount,
            TotalChargeWeight = charges.Sum(c => c.Weight),
            TotalDepositWeight = charges.Sum(c => c.Weight - c.RemainingWeight),
            TotalReceivableWeight = totalOutstandingWeightAll - saleWeight,
            LastPaymentDate = lastPaymentDate,
            SaleAmount = saleAmount,
            SaleWeight = saleWeight,
            PurityBreakdown = purityBreakdown,
            Items = summaryItems
        });
    }

    public async Task<ApiResponse<PagedResult<ReceivableDto>>> GetReceivablesAsync(ReceivableQueryDto query)
    {
        var userId = GetCurrentUserId();
        List<int>? allowedUserIds = null;
        if (userId != 0)
        {
            var user = await _receivableRepository.GetUserWithCompaniesAsync(userId);

            if (user != null)
            {
                var isRetailer = user.UserCompanies
                    .Any(uc => uc.Company != null && uc.Company.Category == "RTL");

                if (isRetailer)
                {
                    query.UserId = userId;
                }

                var isLogisticsCompany = user.UserCompanies
                    .Any(uc => uc.Company != null && uc.Company.Category == "DCC");

                if (isLogisticsCompany)
                {
                    var logisticsCompanyId = await GetCurrentCompanyIdAsync();
                    var managedRetailers = await _receivableRepository.GetManagedRetailersAsync(logisticsCompanyId);
                    var managedIds = managedRetailers.Select(c => c.Id).ToList();
                    allowedUserIds = await _dbContext.Users
                        .Where(u => u.UserCompanies.Any(uc => managedIds.Contains(uc.CompanyId)))
                        .Select(u => u.Id)
                        .ToListAsync();
                }
            }
        }

        if (query.CompanyId.HasValue)
        {
            var companyUserIds = await _dbContext.Users
                .Where(u => u.UserCompanies.Any(uc => uc.CompanyId == query.CompanyId.Value))
                .Select(u => u.Id)
                .ToListAsync();
            allowedUserIds = allowedUserIds != null ? allowedUserIds.Intersect(companyUserIds).ToList() : companyUserIds;
        }

        var (items, totalCount) = await _receivableRepository.GetReceivablesPagedAsync(query, allowedUserIds);

        return ApiResponse<PagedResult<ReceivableDto>>.Success(new PagedResult<ReceivableDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        });
    }

    public async Task<ApiResponse<PuritySummaryDto>> GetPuritySummaryAsync(int userId)
    {
        var outstandingCharges = await _receivableRepository.GetOutstandingChargesWithOrderItemsAsync(userId);

        decimal p14 = 0, p18 = 0, pure = 0, pt = 0;

        foreach (var charge in outstandingCharges)
        {
            if (charge.Order == null) continue;

            foreach (var item in charge.Order.OrderItems.Where(oi => oi.ParentId == null))
            {
                var weight = GetEffectiveWeight(item.ConfirmedWeight, item.ActualWeight, item.OrderWeight) * item.Quantity;
                var purity = (item.Purity ?? "").ToUpperInvariant();

                if (purity.Contains("24K") || purity.Contains("PURE")) pure += weight;
                else if (purity.Contains("18K")) p18 += weight;
                else if (purity.Contains("14K")) p14 += weight;
                else if (purity.Contains("PT") || purity.Contains("PLATINUM")) pt += weight;
            }
        }

        return ApiResponse<PuritySummaryDto>.Success(new PuritySummaryDto
        {
            Purity14k = p14,
            Purity18k = p18,
            PureGold = pure,
            PurityPt = pt
        });
    }

    // Same reasoning as the Payable side's GetLedgerBeforeAsync: UserReceivableSummaryDto's
    // TotalReceivable is today's running balance, so it's only valid as "balance before" for
    // the single most recent deposit. Anything older needs the true point-in-time balance,
    // computed by walking this user's full chronological ledger up to (not including) the
    // anchor record.
    public async Task<ApiResponse<LedgerBalanceDto>> GetLedgerBeforeAsync(int receivableId)
    {
        var anchor = await _receivableRepository.GetByIdAsync(receivableId);
        if (anchor == null) return ApiResponse<LedgerBalanceDto>.Failure("정산 내역을 찾을 수 없습니다.", 404);

        var allRecords = await _receivableRepository.GetReceivablesForUserAsync(anchor.UserId);

        // Running balance as of right after the most recent deposit (0 if there's never
        // been one), plus whatever charges have arrived since - kept as two separate
        // running totals so a new charge doesn't quietly disappear into "beforeAmount".
        decimal lastPaymentAmount = 0;
        decimal lastPaymentWeight = 0;
        decimal newChargeAmount = 0;
        decimal newChargeWeight = 0;
        decimal runningAmount = 0;
        decimal runningWeight = 0;

        foreach (var r in allRecords.OrderBy(r => r.CreatedAt).ThenBy(r => r.Id))
        {
            if (r.Id == anchor.Id) break;
            if (r.Type == "CHARGE")
            {
                runningAmount += r.Amount;
                runningWeight += r.Weight;
                newChargeAmount += r.Amount;
                newChargeWeight += r.Weight;
            }
            else if (r.Type == "DEPOSIT" && !r.IsCancelled)
            {
                runningAmount -= r.Amount + r.Discount;
                runningWeight -= r.Weight;
                lastPaymentAmount = runningAmount;
                lastPaymentWeight = runningWeight;
                newChargeAmount = 0;
                newChargeWeight = 0;
            }
        }

        return ApiResponse<LedgerBalanceDto>.Success(new LedgerBalanceDto
        {
            BeforeAmount = lastPaymentAmount,
            BeforeWeight = lastPaymentWeight,
            NewChargeAmount = newChargeAmount,
            NewChargeWeight = newChargeWeight
        });
    }

    // Order.Status values reached only AFTER the order already went through the PENDING/
    // PROCESSING "confirm the settlement amount" step in settlement-management.vue
    // (whose SettlementDialog both fixes the final amount and can optionally collect a
    // payment in the same action). Deliberately excludes "Inspected"/"InspectedRequested"
    // (still pre-confirm, no real charge to collect on yet) AND "PENDING"/"PROCESSING"
    // themselves (still belong exclusively to that confirm-first flow) - this worklist is
    // only for orders that already passed through confirmation but still owe a balance
    // (e.g. a partial payment was made), never for bypassing the confirm step.
    private static readonly string[] PostArrivalOrderStatuses =
    {
        "SETTLED",
        "DELIVERY_READY", "DELIVERY_IN_TRANSIT", "DELIVERED", "Completed"
    };

    // DCC's own view of "정산 대상 내역" - which of ITS retailers' orders still have an
    // outstanding Receivable charge. Scoped the same way GetLogisticsSummaryAsync already
    // is (Company.LogisticsCompanyId), not by re-deriving the relationship through Order.
    private async Task<IQueryable<Receivable>> BuildReceivableOrderHistoryQueryAsync(ReceivableOrderHistoryQueryDto query)
    {
        var dbQuery = _dbContext.Receivables
            .Include(r => r.User).ThenInclude(u => u!.UserCompanies).ThenInclude(uc => uc.Company)
            .Include(r => r.Order).ThenInclude(o => o!.OrderItems)
            .Where(r => r.Type == "CHARGE");

        if (!_currentUserService.IsAdmin)
        {
            var logisticsCompanyId = await GetCurrentCompanyIdAsync();
            if (logisticsCompanyId == 0)
            {
                return dbQuery.Where(r => false);
            }

            var managedRetailers = await _receivableRepository.GetManagedRetailersAsync(logisticsCompanyId);
            var managedIds = managedRetailers.Select(c => c.Id).ToList();
            dbQuery = dbQuery.Where(r => r.User != null && r.User.UserCompanies.Any(uc => managedIds.Contains(uc.CompanyId)));
        }

        if (query.UserId.HasValue)
        {
            dbQuery = dbQuery.Where(r => r.UserId == query.UserId.Value);
        }

        if (!string.IsNullOrEmpty(query.ProductName))
        {
            dbQuery = dbQuery.Where(r => r.Order != null && r.Order.OrderItems.Any(oi => oi.ParentId == null &&
                ((oi.Product != null && oi.Product.Name.Contains(query.ProductName)) ||
                 (oi.ProductSet != null && oi.ProductSet.Title.Contains(query.ProductName)))));
        }

        if (!string.IsNullOrEmpty(query.Remarks))
        {
            dbQuery = dbQuery.Where(r => r.Order != null && r.Order.OrderItems.Any(oi => oi.ParentId == null &&
                oi.Memo != null && oi.Memo.Contains(query.Remarks)));
        }

        dbQuery = dbQuery.Where(r => r.Order != null && PostArrivalOrderStatuses.Contains(r.Order.Status));

        return dbQuery;
    }

    public async Task<ApiResponse<PagedResult<ReceivableOrderRowDto>>> GetReceivableOrderHistoryAsync(ReceivableOrderHistoryQueryDto query)
    {
        var dbQuery = await BuildReceivableOrderHistoryQueryAsync(query);

        // Worklist, not a ledger - once a charge is fully paid it drops out (the summary
        // endpoint still covers everything, settled or not, same convention as Payable's).
        dbQuery = dbQuery.Where(r => r.RemainingAmount > 0 || r.RemainingWeight > 0);

        var totalCount = await dbQuery.CountAsync();
        var items = await dbQuery
            .OrderByDescending(r => r.CreatedAt).ThenByDescending(r => r.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(r => new ReceivableOrderRowDto
            {
                ReceivableId = r.Id,
                OrderId = r.OrderId,
                OrderNo = r.Order != null ? r.Order.OrderNo : null,
                ProductName = r.Order != null
                    ? r.Order.OrderItems.Where(oi => oi.ParentId == null)
                        .Select(oi => oi.Product != null ? oi.Product.Name : (oi.ProductSet != null ? oi.ProductSet.Title : null))
                        .FirstOrDefault()
                    : null,
                ProductPhotoUrl = r.Order != null
                    ? r.Order.OrderItems.Where(oi => oi.ParentId == null)
                        .Select(oi => oi.Product != null && oi.Product.ProductPhotos.Any() ? oi.Product.ProductPhotos.OrderBy(x => x.SortOrder).First().PhotoUrl
                            : (oi.ProductSet != null && oi.ProductSet.ProductSetPhotos.Any() ? oi.ProductSet.ProductSetPhotos.OrderBy(x => x.SortOrder).First().PhotoUrl : null))
                        .FirstOrDefault()
                    : null,
                ProductItemCount = r.Order != null ? r.Order.OrderItems.Count(oi => oi.ParentId == null) : 0,
                Items = r.Order != null
                    ? r.Order.OrderItems.Where(oi => oi.ParentId == null)
                        .Select(oi => new ReceivableOrderItemSummaryDto
                        {
                            ProductName = oi.Product != null ? oi.Product.Name : (oi.ProductSet != null ? oi.ProductSet.Title : null),
                            ProductNo = oi.Product != null ? oi.Product.ProductNo : null,
                            PhotoUrl = oi.Product != null && oi.Product.ProductPhotos.Any() ? oi.Product.ProductPhotos.OrderBy(x => x.SortOrder).First().PhotoUrl
                                : (oi.ProductSet != null && oi.ProductSet.ProductSetPhotos.Any() ? oi.ProductSet.ProductSetPhotos.OrderBy(x => x.SortOrder).First().PhotoUrl : null),
                            Purity = oi.Purity,
                            Color = oi.Color,
                            Size = oi.Size ?? (oi.Product != null ? oi.Product.ProductSize : null),
                            Memo = oi.Memo,
                            Quantity = oi.Quantity,
                            ActualWeight = oi.ActualWeight ?? oi.RequestedWeight ?? oi.ApprovedWeight
                        }).ToList()
                    : new List<ReceivableOrderItemSummaryDto>(),
                UserId = r.UserId,
                UserName = r.User != null ? r.User.Username : null,
                UserDisplayName = r.User != null ? r.User.Name : null,
                RetailerCompanyName = r.User != null
                    ? r.User.UserCompanies.Where(uc => uc.Company != null && uc.Company.Category == "RTL").Select(uc => uc.Company!.Name).FirstOrDefault()
                    : null,
                OrderDate = r.CreatedAt,
                ChargeAmount = r.Amount,
                ChargeWeight = r.Weight,
                PaidAmount = r.Amount - r.RemainingAmount,
                PaidWeight = r.Weight - r.RemainingWeight,
                RemainingAmount = r.RemainingAmount,
                RemainingWeight = r.RemainingWeight
            })
            .ToListAsync();

        return ApiResponse<PagedResult<ReceivableOrderRowDto>>.Success(new PagedResult<ReceivableOrderRowDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        });
    }

    // Order-centric history for 정산완료내역 - one row per CHARGE (order) regardless of
    // whether it's fully paid or still partially outstanding, so a deposit split across
    // several 거래번호 over time (e.g. paying off a remaining balance in a second payment)
    // reads as "this order, paid via N transactions" instead of N separate, seemingly
    // unrelated order rows. Deliberately NOT filtered by RemainingAmount - 정산 대상 내역
    // (settlement-management.vue) is the action worklist that filters to only what's still
    // owed; this is the record/history view and should show every charge's status.
    public async Task<ApiResponse<PagedResult<ReceivableOrderRowDto>>> GetCompletedReceivableOrdersAsync(ReceivableOrderHistoryQueryDto query)
    {
        var dbQuery = await BuildReceivableOrderHistoryQueryAsync(query);

        var totalCount = await dbQuery.CountAsync();
        var items = await dbQuery
            // Sort by when the charge was actually PAID (its latest deposit application),
            // not by the order's own creation date - otherwise an order settled just now,
            // but originally placed a while ago, wouldn't surface near the top of 정산완료
            // 내역, which should read as recency of settlement, not recency of the order.
            .Select(r => new
            {
                Receivable = r,
                LastPaymentDate = _dbContext.ReceivableApplications
                    .Where(a => a.ChargeId == r.Id)
                    .Select(a => (DateTime?)a.Deposit!.CreatedAt)
                    .Max()
            })
            .OrderByDescending(x => x.LastPaymentDate ?? x.Receivable.CreatedAt).ThenByDescending(x => x.Receivable.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => x.Receivable)
            .Select(r => new ReceivableOrderRowDto
            {
                ReceivableId = r.Id,
                OrderId = r.OrderId,
                OrderNo = r.Order != null ? r.Order.OrderNo : null,
                ProductName = r.Order != null
                    ? r.Order.OrderItems.Where(oi => oi.ParentId == null)
                        .Select(oi => oi.Product != null ? oi.Product.Name : (oi.ProductSet != null ? oi.ProductSet.Title : null))
                        .FirstOrDefault()
                    : null,
                ProductPhotoUrl = r.Order != null
                    ? r.Order.OrderItems.Where(oi => oi.ParentId == null)
                        .Select(oi => oi.Product != null && oi.Product.ProductPhotos.Any() ? oi.Product.ProductPhotos.OrderBy(x => x.SortOrder).First().PhotoUrl
                            : (oi.ProductSet != null && oi.ProductSet.ProductSetPhotos.Any() ? oi.ProductSet.ProductSetPhotos.OrderBy(x => x.SortOrder).First().PhotoUrl : null))
                        .FirstOrDefault()
                    : null,
                ProductItemCount = r.Order != null ? r.Order.OrderItems.Count(oi => oi.ParentId == null) : 0,
                Items = r.Order != null
                    ? r.Order.OrderItems.Where(oi => oi.ParentId == null)
                        .Select(oi => new ReceivableOrderItemSummaryDto
                        {
                            ProductName = oi.Product != null ? oi.Product.Name : (oi.ProductSet != null ? oi.ProductSet.Title : null),
                            ProductNo = oi.Product != null ? oi.Product.ProductNo : null,
                            PhotoUrl = oi.Product != null && oi.Product.ProductPhotos.Any() ? oi.Product.ProductPhotos.OrderBy(x => x.SortOrder).First().PhotoUrl
                                : (oi.ProductSet != null && oi.ProductSet.ProductSetPhotos.Any() ? oi.ProductSet.ProductSetPhotos.OrderBy(x => x.SortOrder).First().PhotoUrl : null),
                            Purity = oi.Purity,
                            Color = oi.Color,
                            Size = oi.Size ?? (oi.Product != null ? oi.Product.ProductSize : null),
                            Memo = oi.Memo,
                            Quantity = oi.Quantity,
                            ActualWeight = oi.ActualWeight ?? oi.RequestedWeight ?? oi.ApprovedWeight
                        }).ToList()
                    : new List<ReceivableOrderItemSummaryDto>(),
                UserId = r.UserId,
                UserName = r.User != null ? r.User.Username : null,
                UserDisplayName = r.User != null ? r.User.Name : null,
                RetailerCompanyName = r.User != null
                    ? r.User.UserCompanies.Where(uc => uc.Company != null && uc.Company.Category == "RTL").Select(uc => uc.Company!.Name).FirstOrDefault()
                    : null,
                OrderDate = r.CreatedAt,
                ChargeAmount = r.Amount,
                ChargeWeight = r.Weight,
                PaidAmount = r.Amount - r.RemainingAmount,
                PaidWeight = r.Weight - r.RemainingWeight,
                RemainingAmount = r.RemainingAmount,
                RemainingWeight = r.RemainingWeight
            })
            .ToListAsync();

        return ApiResponse<PagedResult<ReceivableOrderRowDto>>.Success(new PagedResult<ReceivableOrderRowDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        });
    }

    // Which DEPOSIT(s) actually paid off one specific CHARGE - lets 주문별 보기's expand
    // show "this order, paid via N transactions" instead of just a combined total.
    public async Task<ApiResponse<List<ReceivableChargeApplicationRowDto>>> GetReceivableChargeApplicationsAsync(int chargeId)
    {
        var charge = await _receivableRepository.GetByIdAsync(chargeId);
        if (charge == null || charge.Type != "CHARGE") return ApiResponse<List<ReceivableChargeApplicationRowDto>>.Failure("청구 내역을 찾을 수 없습니다.", 404);

        if (!_currentUserService.IsAdmin)
        {
            var userId = GetCurrentUserId();
            var user = await _receivableRepository.GetUserWithCompaniesAsync(userId);
            var isLogisticsCompany = user != null && user.UserCompanies.Any(uc => uc.Company != null && uc.Company.Category == "DCC");
            var isOwner = charge.UserId == userId;
            if (!isOwner && !isLogisticsCompany)
            {
                return ApiResponse<List<ReceivableChargeApplicationRowDto>>.Failure("접근 권한이 없습니다.", 403);
            }
        }

        var applications = await _dbContext.ReceivableApplications
            .Where(a => a.ChargeId == chargeId)
            .Select(a => new ReceivableChargeApplicationRowDto
            {
                DepositId = a.DepositId,
                AppliedAmount = a.AppliedAmount,
                AppliedWeight = a.AppliedWeight,
                DepositCreatedAt = a.Deposit!.CreatedAt,
                IsCancelled = a.Deposit!.IsCancelled,
                Memo = a.Deposit!.Memo,
                Amount = a.Deposit!.Amount,
                Weight = a.Deposit!.Weight,
                Discount = a.Deposit!.Discount,
                SourcePaymentId = a.Deposit!.SourcePaymentId
            })
            .OrderBy(a => a.DepositCreatedAt)
            .ToListAsync();

        return ApiResponse<List<ReceivableChargeApplicationRowDto>>.Success(applications);
    }

    public async Task<ApiResponse<ReceivableOrderHistorySummaryDto>> GetReceivableOrderHistorySummaryAsync(ReceivableOrderHistoryQueryDto query)
    {
        var dbQuery = await BuildReceivableOrderHistoryQueryAsync(query);

        var summary = await dbQuery
            .GroupBy(r => 1)
            .Select(g => new ReceivableOrderHistorySummaryDto
            {
                TotalChargeAmount = g.Sum(r => r.Amount),
                TotalChargeWeight = g.Sum(r => r.Weight),
                TotalPaidAmount = g.Sum(r => r.Amount - r.RemainingAmount),
                TotalPaidWeight = g.Sum(r => r.Weight - r.RemainingWeight)
            })
            .FirstOrDefaultAsync();

        return ApiResponse<ReceivableOrderHistorySummaryDto>.Success(summary ?? new ReceivableOrderHistorySummaryDto());
    }

    public async Task<ApiResponse<List<ReceivableOverdueRowDto>>> GetReceivableOverdueSummaryAsync(ReceivableOverdueQueryDto query)
    {
        var userId = GetCurrentUserId();
        List<int>? allowedUserIds = null;

        if (!_currentUserService.IsAdmin && userId != 0)
        {
            var user = await _receivableRepository.GetUserWithCompaniesAsync(userId);
            var isLogisticsCompany = user?.UserCompanies.Any(uc => uc.Company != null && uc.Company.Category == "DCC") ?? false;

            if (isLogisticsCompany)
            {
                var logisticsCompanyId = await GetCurrentCompanyIdAsync();
                var managedRetailers = await _receivableRepository.GetManagedRetailersAsync(logisticsCompanyId);
                var managedIds = managedRetailers.Select(c => c.Id).ToList();
                allowedUserIds = await _dbContext.Users
                    .Where(u => u.UserCompanies.Any(uc => managedIds.Contains(uc.CompanyId)))
                    .Select(u => u.Id)
                    .ToListAsync();
            }
            else
            {
                allowedUserIds = new List<int>();
            }
        }

        var chargesQuery = _dbContext.Receivables
            .Include(r => r.User).ThenInclude(u => u!.UserCompanies).ThenInclude(uc => uc.Company)
            .Where(r => r.Type == "CHARGE" && !r.IsCancelled
                && (r.RemainingAmount < r.Amount || r.RemainingWeight < r.Weight)
                && (r.RemainingAmount > 0 || r.RemainingWeight > 0));

        if (allowedUserIds != null)
        {
            chargesQuery = chargesQuery.Where(r => allowedUserIds.Contains(r.UserId));
        }

        if (query.UserId.HasValue)
        {
            chargesQuery = chargesQuery.Where(r => r.UserId == query.UserId.Value);
        }

        if (query.StartDate.HasValue)
        {
            chargesQuery = chargesQuery.Where(r => r.CreatedAt >= query.StartDate.Value);
        }

        if (query.EndDate.HasValue)
        {
            var endExclusive = query.EndDate.Value.Date.AddDays(1);
            chargesQuery = chargesQuery.Where(r => r.CreatedAt < endExclusive);
        }

        var charges = await chargesQuery.ToListAsync();
        var today = DateTime.UtcNow.Date;

        var rows = charges
            .GroupBy(r => r.UserId)
            .Select(g =>
            {
                var oldest = g.Min(r => r.CreatedAt);
                var first = g.First();
                return new ReceivableOverdueRowDto
                {
                    UserId = g.Key,
                    CompanyName = first.User?.UserCompanies.FirstOrDefault(uc => uc.Company != null && uc.Company.Category == "RTL")?.Company?.Name
                        ?? first.User?.UserCompanies.FirstOrDefault()?.Company?.Name,
                    UserDisplayName = first.User?.Name,
                    SaleDate = oldest,
                    SaleAmount = g.Sum(r => r.Amount),
                    SaleWeight = g.Sum(r => r.Weight),
                    CollectedAmount = g.Sum(r => r.Amount - r.RemainingAmount),
                    CollectedWeight = g.Sum(r => r.Weight - r.RemainingWeight),
                    OutstandingAmount = g.Sum(r => r.RemainingAmount),
                    OutstandingWeight = g.Sum(r => r.RemainingWeight),
                    OverdueDays = (today - oldest.Date).Days,
                    OrderIds = g.Where(r => r.OrderId.HasValue).Select(r => r.OrderId!.Value).Distinct().ToList()
                };
            })
            .OrderByDescending(r => r.OverdueDays)
            .ToList();

        return ApiResponse<List<ReceivableOverdueRowDto>>.Success(rows);
    }

    // Greedily reduces outstanding charges (oldest first) by the given amount/weight,
    // used both when applying a new deposit and when re-applying one during an edit.
    // Cash and gold weight are two views of the same underlying charge, not independent
    // debts - once a payment fully covers either side of a charge, the whole charge is
    // considered settled and the other side is cleared too. Whatever was still outstanding
    // on that other side at that moment is added to forgivenAmount/forgivenWeight so the
    // caller can fold it into the deposit's own Discount/DiscountWeight - without that, the
    // charge's real debt silently disappears from RemainingAmount/RemainingWeight while the
    // deposit's own recorded Amount/Discount stays unchanged, permanently corrupting any
    // later chronological ledger walk with debt that no longer exists anywhere in the live
    // charge data.
    private static List<ReceivableApplication> ApplyToChargeList(List<Receivable> charges, ref decimal remainingAmount, ref decimal remainingWeight, ref decimal forgivenAmount, ref decimal forgivenWeight)
    {
        var applications = new List<ReceivableApplication>();

        foreach (var charge in charges)
        {
            if (remainingAmount <= 0 && remainingWeight <= 0) break;

            bool touchedAmount = false;
            bool touchedWeight = false;
            decimal appliedAmount = 0;
            decimal appliedWeight = 0;

            if (remainingAmount > 0 && charge.RemainingAmount > 0)
            {
                touchedAmount = true;
                appliedAmount = Math.Min(charge.RemainingAmount, remainingAmount);
                if (charge.RemainingAmount <= remainingAmount)
                {
                    remainingAmount -= charge.RemainingAmount;
                    charge.RemainingAmount = 0;
                }
                else
                {
                    charge.RemainingAmount -= remainingAmount;
                    remainingAmount = 0;
                }
            }

            if (remainingWeight > 0 && charge.RemainingWeight > 0)
            {
                touchedWeight = true;
                appliedWeight = Math.Min(charge.RemainingWeight, remainingWeight);
                if (charge.RemainingWeight <= remainingWeight)
                {
                    remainingWeight -= charge.RemainingWeight;
                    charge.RemainingWeight = 0;
                }
                else
                {
                    charge.RemainingWeight -= remainingWeight;
                    remainingWeight = 0;
                }
            }

            if ((touchedAmount && charge.RemainingAmount <= 0) || (touchedWeight && charge.RemainingWeight <= 0))
            {
                if (charge.RemainingAmount > 0) forgivenAmount += charge.RemainingAmount;
                if (charge.RemainingWeight > 0) forgivenWeight += charge.RemainingWeight;
                charge.RemainingAmount = 0;
                charge.RemainingWeight = 0;
            }

            if (touchedAmount || touchedWeight)
            {
                applications.Add(new ReceivableApplication { ChargeId = charge.Id, AppliedAmount = appliedAmount, AppliedWeight = appliedWeight });
            }
        }

        return applications;
    }

    // A deliberately-grouped batch settlement (specific orders checked together and settled
    // as one action, via 미수금 관리's batch-settle dialog) should never leave some of those
    // orders completely untouched just because the deposit wasn't enough to cover the oldest
    // one in full first - every charge in the group gets touched, proportional to its own
    // share of the group's total, so all of them leave the outstanding-charge state together
    // instead of the deposit being fully consumed by the first charge alone.
    private static List<ReceivableApplication> ApplyProportionally(List<Receivable> charges, ref decimal remainingAmount, ref decimal remainingWeight, ref decimal forgivenAmount, ref decimal forgivenWeight)
    {
        var applications = new List<ReceivableApplication>();
        if (charges.Count == 0) return applications;

        var totalChargeAmount = charges.Sum(c => c.RemainingAmount);
        var totalChargeWeight = charges.Sum(c => c.RemainingWeight);
        var poolAmount = Math.Min(remainingAmount, totalChargeAmount);
        var poolWeight = Math.Min(remainingWeight, totalChargeWeight);

        decimal allocatedAmount = 0;
        decimal allocatedWeight = 0;

        for (var i = 0; i < charges.Count; i++)
        {
            var charge = charges[i];
            var isLast = i == charges.Count - 1;

            // Cash and gold weight are two views of the same underlying charge, not
            // independent debts (same rule ApplyToChargeList already applies) - "touched"
            // tracks whether THIS side actually had anything in the pool to begin with, so
            // fully paying off one side (e.g. all cash, zero weight) still closes out the
            // whole charge instead of leaving the untouched side looking perpetually unpaid.
            var touchedAmount = poolAmount > 0 && charge.RemainingAmount > 0;
            var touchedWeight = poolWeight > 0 && charge.RemainingWeight > 0;

            // The last charge absorbs whatever proportional rounding left over, so the sum
            // of applied amounts always exactly equals the pool - never a few won/grams short.
            // Non-last shares are capped at the charge's own remaining so rounding can never
            // overshoot it.
            var shareAmount = isLast
                ? poolAmount - allocatedAmount
                : (totalChargeAmount > 0 ? Math.Min(charge.RemainingAmount, Math.Round(poolAmount * (charge.RemainingAmount / totalChargeAmount), 0, MidpointRounding.AwayFromZero)) : 0);
            var shareWeight = isLast
                ? poolWeight - allocatedWeight
                : (totalChargeWeight > 0 ? Math.Min(charge.RemainingWeight, Math.Round(poolWeight * (charge.RemainingWeight / totalChargeWeight), 2, MidpointRounding.AwayFromZero)) : 0);

            allocatedAmount += shareAmount;
            allocatedWeight += shareWeight;
            charge.RemainingAmount -= shareAmount;
            charge.RemainingWeight -= shareWeight;

            if ((touchedAmount && charge.RemainingAmount <= 0) || (touchedWeight && charge.RemainingWeight <= 0))
            {
                if (charge.RemainingAmount > 0) forgivenAmount += charge.RemainingAmount;
                if (charge.RemainingWeight > 0) forgivenWeight += charge.RemainingWeight;
                charge.RemainingAmount = 0;
                charge.RemainingWeight = 0;
            }

            if (shareAmount != 0 || shareWeight != 0)
            {
                applications.Add(new ReceivableApplication { ChargeId = charge.Id, AppliedAmount = shareAmount, AppliedWeight = shareWeight });
            }
        }

        remainingAmount -= poolAmount;
        remainingWeight -= poolWeight;
        return applications;
    }

    // Restores exactly the charges (and exact amounts) this deposit's own ReceivableApplication
    // records show it paid off - NOT a generic oldest-charge-first/capacity walk. A deposit can
    // target one specific order out of chronological order (see ApplyDepositToChargesAsync), so
    // when a later edit/cancel needs to undo it, guessing which charge to refund by date/capacity
    // can restore the wrong charge (leaving the one this deposit actually paid still marked paid,
    // while an unrelated earlier charge gets incorrectly reopened). Using the real application
    // records removes the guessing entirely.
    private static void ReverseApplications(List<Receivable> charges, List<ReceivableApplication> applications)
    {
        var chargeMap = charges.ToDictionary(c => c.Id);
        foreach (var application in applications)
        {
            if (!chargeMap.TryGetValue(application.ChargeId, out var charge)) continue;
            charge.RemainingAmount = Math.Min(charge.Amount, charge.RemainingAmount + application.AppliedAmount);
            charge.RemainingWeight = Math.Min(charge.Weight, charge.RemainingWeight + application.AppliedWeight);
        }
    }

    // A DCC batch-settling several retail orders at once (정산 대상 내역's checkbox flow) needs
    // to target all of them in one deposit, mirroring PayableService.ApplyToChargeList's
    // OrderIds branch - orderIds takes priority over the single orderId when both are present.
    private async Task<(List<ReceivableApplication> Applications, decimal RemainingAmount, decimal RemainingWeight, decimal ForgivenAmount, decimal ForgivenWeight)> ApplyDepositToChargesAsync(int userId, int? orderId, List<int>? orderIds, decimal amountToApply, decimal weightToApply)
    {
        var applications = new List<ReceivableApplication>();
        decimal forgivenAmount = 0m;
        decimal forgivenWeight = 0m;

        if (orderIds != null && orderIds.Count > 0)
        {
            var targetCharges = await _receivableRepository.GetTargetChargesForOrdersAsync(userId, orderIds);
            applications.AddRange(ApplyProportionally(targetCharges, ref amountToApply, ref weightToApply, ref forgivenAmount, ref forgivenWeight));
        }
        else if (orderId.HasValue)
        {
            var targetCharges = await _receivableRepository.GetTargetChargesAsync(userId, orderId.Value);
            applications.AddRange(ApplyToChargeList(targetCharges, ref amountToApply, ref weightToApply, ref forgivenAmount, ref forgivenWeight));
        }

        if (amountToApply > 0 || weightToApply > 0)
        {
            var outstandingCharges = await _receivableRepository.GetOutstandingChargesAsync(userId);
            applications.AddRange(ApplyToChargeList(outstandingCharges, ref amountToApply, ref weightToApply, ref forgivenAmount, ref forgivenWeight));
        }

        return (applications, amountToApply, weightToApply, forgivenAmount, forgivenWeight);
    }

    public async Task<ApiResponse<bool>> ProcessDepositAsync(CreateDepositDto request)
    {
        var discount = request.Discount ?? 0m;

        if (request.Amount <= 0 && discount <= 0 && (!request.Weight.HasValue || request.Weight.Value <= 0))
        {
            return ApiResponse<bool>.Failure("입금액, 할인액 또는 순금 중량 중 하나는 0보다 커야 합니다.", 400);
        }

        // Discount reduces outstanding charges the same way cash does, it's just
        // tracked separately on the deposit record so the ledger can show how much
        // of the settlement was actual payment vs. a discount write-off.
        var (applications, remainingAmount, remainingWeight, forgivenAmount, forgivenWeight) = await ApplyDepositToChargesAsync(request.UserId, request.OrderId, request.OrderIds, request.Amount + discount, request.Weight ?? 0m);

        // Fold anything the either-side-clears-the-whole-charge rule just forgave into the
        // discount pool below, so it's honestly recorded rather than only ever showing up as
        // a live RemainingAmount change with no paper trail. Receivable has no weight-side
        // discount field (unlike Payable), so a forgiven weight amount still can't be
        // recorded here - a smaller, pre-existing gap this fix doesn't extend to.
        discount += forgivenAmount;
        _ = forgivenWeight;

        // A charge that was already partially settled before (i.e. it's sitting in 미수금
        // 관리, not the fresh worklist) already has its own running deposit record - route
        // any further money toward it into THAT SAME deposit/거래번호 instead of opening a
        // new one, so 정산 완료 내역 keeps exactly one row per order as it accumulates
        // payments over time. Only a charge that has never been touched before gets a brand
        // new deposit record. Cash and discount are attributed to each application using the
        // request's own overall cash:discount ratio, since the two pools were never tracked
        // separately per charge to begin with.
        var chargeIds = applications.Select(a => a.ChargeId).Distinct().ToList();
        var priorApplications = chargeIds.Count > 0
            ? await _dbContext.ReceivableApplications
                .Include(a => a.Deposit)
                .Where(a => chargeIds.Contains(a.ChargeId) && a.Deposit != null && a.Deposit.Type == "DEPOSIT" && !a.Deposit.IsCancelled)
                .ToListAsync()
            : new List<ReceivableApplication>();
        var existingApplicationByCharge = priorApplications
            .GroupBy(a => a.ChargeId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(a => a.Deposit!.CreatedAt).First());

        var totalPool = request.Amount + discount;
        var cashRatio = totalPool > 0 ? request.Amount / totalPool : 0m;

        Receivable? newDeposit = null;
        var newApplications = new List<ReceivableApplication>();

        foreach (var application in applications)
        {
            var cashPortion = application.AppliedAmount * cashRatio;
            var discountPortion = application.AppliedAmount - cashPortion;

            if (existingApplicationByCharge.TryGetValue(application.ChargeId, out var existingApplication))
            {
                var existingDeposit = existingApplication.Deposit!;
                existingDeposit.Amount += cashPortion;
                existingDeposit.Discount += discountPortion;
                existingDeposit.Weight += application.AppliedWeight;
                existingApplication.AppliedAmount += application.AppliedAmount;
                existingApplication.AppliedWeight += application.AppliedWeight;
            }
            else
            {
                newDeposit ??= new Receivable
                {
                    UserId = request.UserId,
                    OrderId = request.OrderId,
                    Type = "DEPOSIT",
                    Memo = request.Memo,
                    SettlementMethod = request.SettlementMethod
                };
                newDeposit.Amount += cashPortion;
                newDeposit.Discount += discountPortion;
                newDeposit.Weight += application.AppliedWeight;
                newApplications.Add(application);
            }
        }

        // Anything left after covering every outstanding charge is an overpayment - keep it
        // on a deposit record instead of discarding it, so it stays visible/traceable rather
        // than silently vanishing, even if every charge this action touched was a top-up.
        if (remainingAmount > 0 || remainingWeight > 0)
        {
            newDeposit ??= new Receivable
            {
                UserId = request.UserId,
                OrderId = request.OrderId,
                Type = "DEPOSIT",
                Memo = request.Memo,
                SettlementMethod = request.SettlementMethod
            };
            var leftoverCash = remainingAmount * cashRatio;
            newDeposit.Amount += leftoverCash;
            newDeposit.Discount += remainingAmount - leftoverCash;
            newDeposit.Weight += remainingWeight;
        }

        if (newDeposit != null)
        {
            newDeposit.RemainingAmount = remainingAmount;
            newDeposit.RemainingWeight = remainingWeight;
            await _receivableRepository.AddAsync(newDeposit);
            foreach (var application in newApplications)
            {
                application.Deposit = newDeposit;
                _dbContext.ReceivableApplications.Add(application);
            }
        }

        await _receivableRepository.SaveChangesAsync();

        return ApiResponse<bool>.Success(true);
    }

    public async Task<ApiResponse<bool>> UpdateDepositAsync(int id, UpdateDepositDto request)
    {
        var deposit = await _receivableRepository.GetByIdAsync(id);
        if (deposit == null || deposit.Type != "DEPOSIT") return ApiResponse<bool>.Failure("입금 내역을 찾을 수 없습니다.", 404);
        if (deposit.IsCancelled) return ApiResponse<bool>.Failure("취소된 거래는 수정할 수 없습니다.", 400);
        if (deposit.SourcePaymentId.HasValue) return ApiResponse<bool>.Failure("이 입금은 정산처리에서 자동 반영되었습니다. 원본 정산처리 내역에서 수정해주세요.", 400);

        // Undo this deposit's original effect before re-applying the new values, so
        // editing behaves like cancel-then-reapply against the current outstanding charges.
        var allCharges = (await _receivableRepository.GetReceivablesForUserAsync(deposit.UserId))
            .Where(r => r.Type == "CHARGE")
            .OrderBy(r => r.CreatedAt)
            .ToList();

        var oldApplications = await _dbContext.ReceivableApplications.Where(a => a.DepositId == deposit.Id).ToListAsync();
        ReverseApplications(allCharges, oldApplications);
        _dbContext.ReceivableApplications.RemoveRange(oldApplications);

        var newDiscount = request.Discount ?? 0m;
        deposit.Amount = request.Amount;
        deposit.Weight = request.Weight ?? 0m;
        deposit.Discount = newDiscount;
        deposit.Memo = request.Memo;
        deposit.SettlementMethod = request.SettlementMethod;

        decimal newAmountToApply = request.Amount + newDiscount;
        decimal newWeightToApply = request.Weight ?? 0m;
        var stillOutstanding = allCharges.Where(c => c.RemainingAmount > 0 || c.RemainingWeight > 0).ToList();
        decimal editForgivenAmount = 0m;
        decimal editForgivenWeight = 0m;
        var newApplications = ApplyToChargeList(stillOutstanding, ref newAmountToApply, ref newWeightToApply, ref editForgivenAmount, ref editForgivenWeight);
        if (editForgivenAmount > 0)
        {
            deposit.Discount += editForgivenAmount;
        }
        foreach (var application in newApplications)
        {
            application.Deposit = deposit;
            _dbContext.ReceivableApplications.Add(application);
        }

        deposit.RemainingAmount = newAmountToApply;
        deposit.RemainingWeight = newWeightToApply;

        await _receivableRepository.SaveChangesAsync();
        return ApiResponse<bool>.Success(true);
    }

    public async Task<ApiResponse<bool>> CancelDepositAsync(int id)
    {
        var deposit = await _receivableRepository.GetByIdAsync(id);
        if (deposit == null || deposit.Type != "DEPOSIT") return ApiResponse<bool>.Failure("입금 내역을 찾을 수 없습니다.", 404);
        if (deposit.IsCancelled) return ApiResponse<bool>.Failure("이미 취소된 거래입니다.", 400);
        if (deposit.SourcePaymentId.HasValue) return ApiResponse<bool>.Failure("이 입금은 정산처리에서 자동 반영되었습니다. 원본 정산처리 내역에서 취소해주세요.", 400);

        var allCharges = (await _receivableRepository.GetReceivablesForUserAsync(deposit.UserId))
            .Where(r => r.Type == "CHARGE")
            .OrderBy(r => r.CreatedAt)
            .ToList();

        var oldApplications = await _dbContext.ReceivableApplications.Where(a => a.DepositId == deposit.Id).ToListAsync();
        ReverseApplications(allCharges, oldApplications);
        _dbContext.ReceivableApplications.RemoveRange(oldApplications);

        deposit.IsCancelled = true;
        deposit.RemainingAmount = 0;
        deposit.RemainingWeight = 0;

        await _receivableRepository.SaveChangesAsync();
        return ApiResponse<bool>.Success(true);
    }
}

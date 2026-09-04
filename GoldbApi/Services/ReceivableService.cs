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
    Task CreateOrderSettlementChargesAsync(int orderId, string triggerStatus);

    // The 물류도착 gate: returns a Korean error message if THIS SAME order's own Payable
    // charge(s) (created when MFG marked 제품출고, one per manufacturer involved) have never
    // been processed through 정산처리 at all (no PayableApplication row against them, not even
    // a 0-value acknowledgement), or null if they've all been at least reviewed/acted on and
    // the transition to PENDING can proceed. Lives here (not in OrderService) so both the
    // normal PENDING transition (OrderService.UpdateOrderStatusAsync) and StockService's two
    // "auto/manual jump straight to PENDING" shortcuts can call the exact same check without a
    // circular OrderService<->StockService dependency.
    Task<string?> GetManufacturerSettlementBacklogBlockAsync(int orderId, int? logisticsCompanyId);
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

    public async Task CreateOrderSettlementChargesAsync(int orderId, string triggerStatus)
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

        // A "물류사 자체 재고용 주문 (본인 구매)" order (주문 수기 등록 with no 소매점 selected) is
        // placed under the logistics company's own user account instead of an actual
        // retailer's - a company can't owe itself money, so this must never generate a
        // Receivable charge against itself.
        var isSelfPurchase = await _dbContext.UserCompanies
            .AnyAsync(uc => uc.UserId == order.UserId && uc.Company != null &&
                (uc.Company.Category == "DCC" || uc.Company.Category == "LOG"));

        // The retailer-facing Receivable charge must not appear in 정산 대상 내역 the moment
        // MFG marks 제품출고 (InspectedRequested) - the goods haven't even arrived at the
        // logistics center yet. It should only ever first appear once DCC actually marks
        // 물류도착 (PENDING), then get corrected again at SETTLED. The Payable charge (what
        // DCC owes the manufacturer) is unaffected by this and still starts at InspectedRequested,
        // since 물류도착's own settlement-backlog gate depends on that charge already existing.
        if (!isSelfPurchase && triggerStatus != "InspectedRequested")
        {
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
        }

        if (order.LogisticsCompanyId.HasValue)
        {
            var byManufacturer = topLevelItems
                .Select(oi => new
                {
                    Item = oi,
                    // CustomManufacturerCompanyId covers 주문 수기 등록's free-text (no
                    // catalog product) items, which have neither Product nor ProductSet.
                    ManufacturerId = oi.Product?.CompanyId ?? oi.ProductSet?.CompanyId ?? oi.CustomManufacturerCompanyId
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

    public async Task<string?> GetManufacturerSettlementBacklogBlockAsync(int orderId, int? logisticsCompanyId)
    {
        if (!logisticsCompanyId.HasValue) return null;

        var untouchedCharge = await _dbContext.Payables
            .Include(p => p.ManufacturerCompany)
            .Where(p => p.Type == "CHARGE" && !p.IsCancelled
                && p.LogisticsCompanyId == logisticsCompanyId.Value
                && p.OrderId == orderId
                && !_dbContext.PayableApplications.Any(a => a.ChargeId == p.Id))
            .OrderBy(p => p.CreatedAt)
            .FirstOrDefaultAsync();

        if (untouchedCharge == null) return null;

        var manufacturerName = untouchedCharge.ManufacturerCompany?.Name ?? "제조사";
        return $"[{manufacturerName}]에 대한 이 주문의 정산처리가 아직 완료되지 않아 물류도착 처리를 할 수 없습니다. 먼저 지급 관리에서 정산처리(0원 처리도 가능)를 완료해 주세요.";
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
            var totalDepositWeight = userReceivables.Where(r => r.Type == "DEPOSIT" && !r.IsCancelled).Sum(r => r.Weight + r.DiscountWeight - r.RemainingWeight);
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
        var totalDepositWeight = userReceivables.Where(r => r.Type == "DEPOSIT" && !r.IsCancelled).Sum(r => r.Weight + r.DiscountWeight - r.RemainingWeight);
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
            .Where(r => r.Type == "CHARGE" && !r.IsCancelled && r.OrderId.HasValue && orderIds.Contains(r.OrderId.Value))
            .ToListAsync();

        if (charges.Count == 0) return ApiResponse<UserReceivableSummaryDto?>.Success(null);

        var distinctUsers = charges.Select(r => r.UserId).Distinct().ToList();
        if (distinctUsers.Count > 1)
        {
            return ApiResponse<UserReceivableSummaryDto?>.Failure("선택한 주문들이 서로 다른 거래처에 속해 있습니다. 같은 거래처의 주문만 함께 정산할 수 있습니다.", 400);
        }

        var first = charges[0];
        var userId = first.UserId;

        var allUserReceivables = await _receivableRepository.GetReceivablesForUserAsync(userId);
        var lastPaymentDate = allUserReceivables.Where(r => r.Type == "DEPOSIT" && !r.IsCancelled)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => (DateTime?)r.CreatedAt)
            .FirstOrDefault();

        // 판매(B) is only ever a charge's own full/remaining amount when the charge has NEVER
        // been touched by 정산처리 before (fresh out of 정산 대상 내역's worklist - that's a
        // genuinely new sale). A charge already sitting in 미수금 관리 has an application on
        // file already (even a 0-value acknowledgement counts, see ProcessDepositAsync) - for
        // that one there's no new sale happening here, just old debt being collected, so its
        // remaining amount belongs folded into 거래 전 미수(A) instead of being double-labeled
        // as a fresh 판매(B) every time more of it gets paid off.
        //
        // Touched-status is checked across EVERY outstanding charge on the retailer's whole
        // account, not just the ones this request selected - 거래 전 미수(A) must only ever
        // reflect genuinely old, already-billed debt. An order sitting untouched in the
        // worklist (never selected, never billed) is neither a sale nor a debt for THIS
        // transaction; it stays invisible here entirely, waiting for its own future 정산처리,
        // instead of silently inflating A just because it belongs to the same retailer.
        var allUserChargeIds = allUserReceivables.Where(r => r.Type == "CHARGE").Select(r => r.Id).ToList();
        var touchedChargeIds = (await _dbContext.ReceivableApplications
            .Where(a => allUserChargeIds.Contains(a.ChargeId))
            .Select(a => a.ChargeId)
            .Distinct()
            .ToListAsync())
            .ToHashSet();

        var totalOutstandingAll = allUserReceivables.Where(r => r.Type == "CHARGE" && touchedChargeIds.Contains(r.Id)).Sum(r => r.RemainingAmount);
        var totalOutstandingWeightAll = allUserReceivables.Where(r => r.Type == "CHARGE" && touchedChargeIds.Contains(r.Id)).Sum(r => r.RemainingWeight);

        var saleAmount = charges.Where(c => !touchedChargeIds.Contains(c.Id)).Sum(c => c.RemainingAmount);
        var saleWeight = charges.Where(c => !touchedChargeIds.Contains(c.Id)).Sum(c => c.RemainingWeight);

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
                ProductName = item?.Product?.Name ?? item?.ProductSet?.Title ?? item?.CustomProductName,
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
                    ProductName = oi.Product?.Name ?? oi.ProductSet?.Title ?? oi.CustomProductName,
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
            TotalReceivable = totalOutstandingAll,
            TotalChargeWeight = charges.Sum(c => c.Weight),
            TotalDepositWeight = charges.Sum(c => c.Weight - c.RemainingWeight),
            TotalReceivableWeight = totalOutstandingWeightAll,
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
            if (r.Type == "CHARGE" && !r.IsCancelled)
            {
                runningAmount += r.Amount;
                runningWeight += r.Weight;
                newChargeAmount += r.Amount;
                newChargeWeight += r.Weight;
            }
            else if (r.Type == "DEPOSIT" && !r.IsCancelled)
            {
                runningAmount -= r.Amount + r.Discount;
                runningWeight -= r.Weight + r.DiscountWeight;
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

    // DCC's own view of "정산 대상 내역" - which of ITS retailers' orders still have an
    // outstanding Receivable charge. Scoped the same way GetLogisticsSummaryAsync already
    // is (Company.LogisticsCompanyId), not by re-deriving the relationship through Order.
    private async Task<IQueryable<Receivable>> BuildReceivableOrderHistoryQueryAsync(ReceivableOrderHistoryQueryDto query)
    {
        var dbQuery = _dbContext.Receivables
            .Include(r => r.User).ThenInclude(u => u!.UserCompanies).ThenInclude(uc => uc.Company)
            .Include(r => r.Order).ThenInclude(o => o!.OrderItems)
            .Where(r => r.Type == "CHARGE" && !r.IsCancelled);

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

        if (query.CompanyId.HasValue)
        {
            var companyUserIds = await _dbContext.Users
                .Where(u => u.UserCompanies.Any(uc => uc.CompanyId == query.CompanyId.Value))
                .Select(u => u.Id)
                .ToListAsync();
            dbQuery = dbQuery.Where(r => companyUserIds.Contains(r.UserId));
        }

        if (!string.IsNullOrEmpty(query.OrderNo))
        {
            dbQuery = dbQuery.Where(r => r.Order != null && r.Order.OrderNo.Contains(query.OrderNo));
        }

        if (query.StartDate.HasValue)
        {
            dbQuery = dbQuery.Where(r => r.CreatedAt >= query.StartDate.Value);
        }

        if (query.EndDate.HasValue)
        {
            var endExclusive = query.EndDate.Value.Date.AddDays(1);
            dbQuery = dbQuery.Where(r => r.CreatedAt < endExclusive);
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

        return dbQuery;
    }

    public async Task<ApiResponse<PagedResult<ReceivableOrderRowDto>>> GetReceivableOrderHistoryAsync(ReceivableOrderHistoryQueryDto query)
    {
        var dbQuery = await BuildReceivableOrderHistoryQueryAsync(query);

        // Worklist, not a ledger - deliberately independent of Order.Status. A charge can be
        // created before an order ever reaches the normal PENDING confirm step (or the order
        // can drift past PENDING through some other flow without ever being touched), and an
        // order-status filter here would make such a charge permanently invisible. Mirrors
        // MFG's GetPayableOrderHistoryAsync exactly: completely untouched charges only - the
        // instant ANY action (even a 0-value acknowledgement, see ProcessDepositAsync) lands
        // on one it drops out of here for good (still fully visible in 미수금 관리 the whole
        // time, since that reads live RemainingAmount, not application history).
        dbQuery = dbQuery.Where(r => r.RemainingAmount == r.Amount && r.RemainingWeight == r.Weight && (r.Amount > 0 || r.Weight > 0)
            && !_dbContext.ReceivableApplications.Any(a => a.ChargeId == r.Id));

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
                        .Select(oi => oi.Product != null ? oi.Product.Name : (oi.ProductSet != null ? oi.ProductSet.Title : oi.CustomProductName))
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
                            ProductName = oi.Product != null ? oi.Product.Name : (oi.ProductSet != null ? oi.ProductSet.Title : oi.CustomProductName),
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
                Remarks = r.Order != null
                    ? r.Order.OrderItems.Where(oi => oi.ParentId == null && oi.Memo != null && oi.Memo != "")
                        .Select(oi => oi.Memo)
                        .FirstOrDefault()
                    : null,
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
                        .Select(oi => oi.Product != null ? oi.Product.Name : (oi.ProductSet != null ? oi.ProductSet.Title : oi.CustomProductName))
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
                            ProductName = oi.Product != null ? oi.Product.Name : (oi.ProductSet != null ? oi.ProductSet.Title : oi.CustomProductName),
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
                Remarks = r.Order != null
                    ? r.Order.OrderItems.Where(oi => oi.ParentId == null && oi.Memo != null && oi.Memo != "")
                        .Select(oi => oi.Memo)
                        .FirstOrDefault()
                    : null,
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

        // A charge nobody has ever run through 정산처리 (not even a 0-value acknowledgement)
        // belongs only to 정산 대상 내역's own worklist - it was never "billed" from this
        // page's point of view, so counting its full Amount into 총 판매 (and therefore into
        // 미수금 = 총 판매 - 총 결제) makes a product nobody has touched yet look like
        // outstanding debt. Same "has an application" gate GetReceivableOverdueSummaryAsync
        // already uses for 미수금 관리 itself.
        dbQuery = dbQuery.Where(r => _dbContext.ReceivableApplications.Any(a => a.ChargeId == r.Id));

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

        // 미수금 관리 is exclusively for charges that have actually been processed through
        // 정산처리 at least once (even a 0-value acknowledgement, see ProcessDepositAsync) -
        // a charge nobody has touched yet belongs only in 정산 대상 내역's worklist. Mirrors
        // that worklist's own "has an application" check so a charge is in exactly one of
        // the two lists at any moment.
        var chargesQuery = _dbContext.Receivables
            .Include(r => r.User).ThenInclude(u => u!.UserCompanies).ThenInclude(uc => uc.Company)
            .Where(r => r.Type == "CHARGE" && !r.IsCancelled
                && (r.RemainingAmount > 0 || r.RemainingWeight > 0)
                && _dbContext.ReceivableApplications.Any(a => a.ChargeId == r.Id));

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

        // 마지막거래일자 needs the most recent activity across this retailer's WHOLE ledger
        // (any Type, not just the still-outstanding charges above), so fetch it separately.
        var involvedUserIds = charges.Select(r => r.UserId).Distinct().ToList();
        var lastTransactionByUser = await _dbContext.Receivables
            .Where(r => !r.IsCancelled && involvedUserIds.Contains(r.UserId))
            .GroupBy(r => r.UserId)
            .Select(g => new { UserId = g.Key, LastDate = g.Max(r => r.CreatedAt) })
            .ToDictionaryAsync(x => x.UserId, x => x.LastDate);

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
                    LastTransactionDate = lastTransactionByUser.TryGetValue(g.Key, out var lastDate) ? lastDate : oldest,
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
    // forgivenAmountByCharge/forgivenWeightByCharge accumulate PER CHARGE (not a single pooled
    // total) - see the long comment at ProcessDepositAsync's cashPortion/discountPortion
    // computation for why a pooled total silently lost money when this method is called
    // against several charges and only one of them triggers the either-side-clears-it rule.
    private static List<ReceivableApplication> ApplyToChargeList(List<Receivable> charges, ref decimal remainingAmount, ref decimal remainingWeight, Dictionary<int, decimal> forgivenAmountByCharge, Dictionary<int, decimal> forgivenWeightByCharge)
    {
        var applications = new List<ReceivableApplication>();

        // See PayableService.ApplyToChargeList's identical guard for the full rationale -
        // either-side-clears-the-whole-charge only makes sense for a single charge (a
        // conversion rounding difference within the SAME order). Across a multi-charge batch,
        // cash and weight are independent pools that can run dry at different charges, and
        // blanket-forgiving whichever side happened to still have a balance would silently
        // write off real, valuable 순금 with no user intent behind it.
        var allowEitherSideForgiveness = charges.Count <= 1;

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

            if (allowEitherSideForgiveness && ((touchedAmount && charge.RemainingAmount <= 0) || (touchedWeight && charge.RemainingWeight <= 0)))
            {
                if (charge.RemainingAmount > 0)
                {
                    forgivenAmountByCharge[charge.Id] = forgivenAmountByCharge.GetValueOrDefault(charge.Id) + charge.RemainingAmount;
                }
                if (charge.RemainingWeight > 0)
                {
                    forgivenWeightByCharge[charge.Id] = forgivenWeightByCharge.GetValueOrDefault(charge.Id) + charge.RemainingWeight;
                }
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
    private async Task<(List<ReceivableApplication> Applications, decimal RemainingAmount, decimal RemainingWeight, Dictionary<int, decimal> ForgivenAmountByCharge, Dictionary<int, decimal> ForgivenWeightByCharge)> ApplyDepositToChargesAsync(int userId, int? orderId, List<int>? orderIds, decimal amountToApply, decimal weightToApply)
    {
        var applications = new List<ReceivableApplication>();
        var forgivenAmountByCharge = new Dictionary<int, decimal>();
        var forgivenWeightByCharge = new Dictionary<int, decimal>();

        if (orderIds != null && orderIds.Count > 0)
        {
            // Pays each selected charge off in full, oldest first (GetTargetChargesForOrdersAsync
            // already orders by CreatedAt), instead of prorating a partial payment thinly across
            // every selected order - a partial batch payment now fully closes as many of the
            // selected orders as it can afford, leaving the rest untouched for next time, rather
            // than leaving every selected order sitting at a small remaining balance.
            var targetCharges = await _receivableRepository.GetTargetChargesForOrdersAsync(userId, orderIds);
            applications.AddRange(ApplyToChargeList(targetCharges, ref amountToApply, ref weightToApply, forgivenAmountByCharge, forgivenWeightByCharge));

            // A charge the payment pool ran out before reaching (ApplyToChargeList pays oldest
            // first) still has to be acknowledged - the user explicitly picked it for THIS batch,
            // so leaving it with no application at all strands it in the untouched 정산 대상
            // 내역 worklist looking like the action never happened to it. A 0-value application
            // (same mechanism as an all-zero 입금처리) moves it into 미수금 관리 instead, still
            // owing its full remaining amount.
            var touchedChargeIds = applications.Select(a => a.ChargeId).ToHashSet();
            foreach (var charge in targetCharges)
            {
                if (!touchedChargeIds.Contains(charge.Id))
                {
                    applications.Add(new ReceivableApplication { ChargeId = charge.Id, AppliedAmount = 0, AppliedWeight = 0 });
                }
            }
        }
        else if (orderId.HasValue)
        {
            var targetCharges = await _receivableRepository.GetTargetChargesAsync(userId, orderId.Value);
            applications.AddRange(ApplyToChargeList(targetCharges, ref amountToApply, ref weightToApply, forgivenAmountByCharge, forgivenWeightByCharge));
        }

        if (amountToApply > 0 || weightToApply > 0)
        {
            var outstandingCharges = await _receivableRepository.GetOutstandingChargesAsync(userId);
            applications.AddRange(ApplyToChargeList(outstandingCharges, ref amountToApply, ref weightToApply, forgivenAmountByCharge, forgivenWeightByCharge));
        }

        return (applications, amountToApply, weightToApply, forgivenAmountByCharge, forgivenWeightByCharge);
    }

    public async Task<ApiResponse<bool>> ProcessDepositAsync(CreateDepositDto request)
    {
        var discount = request.Discount ?? 0m;
        var discountWeight = request.DiscountWeight ?? 0m;
        var isAllZero = request.Amount <= 0 && discount <= 0 && (!request.Weight.HasValue || request.Weight.Value <= 0) && discountWeight <= 0;
        var hasTargetOrders = (request.OrderIds != null && request.OrderIds.Count > 0) || request.OrderId.HasValue;

        if (isAllZero && !hasTargetOrders)
        {
            return ApiResponse<bool>.Failure("입금액, 할인액 또는 순금 중량 중 하나는 0보다 커야 합니다.", 400);
        }

        // A 0/0/0/0 submission against specifically selected orders means nothing was
        // actually collected and nothing is being forgiven - RemainingAmount/RemainingWeight
        // must stay exactly as they were (still fully owed, so 미수금 관리 keeps showing the
        // real balance). But the action still needs a durable trace, or 정산 대상 내역's
        // "never touched" worklist would show this charge forever even after it was reviewed.
        // Record a real (zero-value) deposit + application so GetReceivableOrderHistoryAsync's
        // worklist filter (which excludes any charge with an application on file) drops it.
        if (isAllZero)
        {
            List<Receivable> targetCharges;
            if (request.OrderIds != null && request.OrderIds.Count > 0)
            {
                targetCharges = await _receivableRepository.GetTargetChargesForOrdersAsync(request.UserId, request.OrderIds);
            }
            else
            {
                targetCharges = await _receivableRepository.GetTargetChargesAsync(request.UserId, request.OrderId!.Value);
            }

            if (targetCharges.Count > 0)
            {
                var zeroDeposit = new Receivable
                {
                    UserId = request.UserId,
                    OrderId = request.OrderId,
                    Type = "DEPOSIT",
                    Memo = request.Memo,
                    SettlementMethod = request.SettlementMethod
                };
                await _receivableRepository.AddAsync(zeroDeposit);
                await _receivableRepository.SaveChangesAsync();

                foreach (var charge in targetCharges)
                {
                    _dbContext.ReceivableApplications.Add(new ReceivableApplication
                    {
                        DepositId = zeroDeposit.Id,
                        ChargeId = charge.Id,
                        AppliedAmount = 0,
                        AppliedWeight = 0
                    });
                }
                await _receivableRepository.SaveChangesAsync();
            }

            return ApiResponse<bool>.Success(true);
        }

        // A charge only ever reaches 미수금 관리 once it already has an application on file
        // (even a 0-value acknowledgement counts - see the isAllZero branch above and
        // GetReceivableOrderHistoryAsync's worklist filter, which excludes any charge with
        // one). So if EVERY charge this request targets is already touched, this action can
        // only have come from 미수금 관리's 입금처리 - a pure debt collection, not a new sale -
        // and must open as its own new 거래 row rather than merging into whichever deposit
        // originally introduced that charge.
        List<int> targetChargeIdsForOverdueCheck;
        if (request.OrderIds != null && request.OrderIds.Count > 0)
        {
            targetChargeIdsForOverdueCheck = await _dbContext.Receivables
                .Where(r => r.UserId == request.UserId && r.OrderId.HasValue && request.OrderIds.Contains(r.OrderId.Value)
                    && r.Type == "CHARGE" && (r.RemainingAmount > 0 || r.RemainingWeight > 0))
                .Select(r => r.Id)
                .ToListAsync();
        }
        else if (request.OrderId.HasValue)
        {
            targetChargeIdsForOverdueCheck = await _dbContext.Receivables
                .Where(r => r.UserId == request.UserId && r.OrderId == request.OrderId.Value
                    && r.Type == "CHARGE" && (r.RemainingAmount > 0 || r.RemainingWeight > 0))
                .Select(r => r.Id)
                .ToListAsync();
        }
        else
        {
            targetChargeIdsForOverdueCheck = new List<int>();
        }
        // The caller (미수금 관리's 입금처리 dialog) can state this outright via
        // request.IsOverdueCollection - trust that over the heuristic below when given.
        // Mirrors PayableService.ProcessPaymentAsync's identical override.
        var isOverdueSettlement = request.IsOverdueCollection ?? (targetChargeIdsForOverdueCheck.Count > 0 &&
            (await _dbContext.ReceivableApplications.Where(a => targetChargeIdsForOverdueCheck.Contains(a.ChargeId)).Select(a => a.ChargeId).Distinct().CountAsync())
                == targetChargeIdsForOverdueCheck.Count);

        // Discount reduces outstanding charges the same way cash does, it's just
        // tracked separately on the deposit record so the ledger can show how much
        // of the settlement was actual payment vs. a discount write-off.
        var (applications, remainingAmount, remainingWeight, forgivenAmountByCharge, forgivenWeightByCharge) = await ApplyDepositToChargesAsync(request.UserId, request.OrderId, request.OrderIds, request.Amount + discount, (request.Weight ?? 0m) + discountWeight);

        // ApplyDepositToChargesAsync's two-phase design (targeted charges, then an outstanding-
        // charges fallback if anything remains) can legitimately touch the SAME charge twice in
        // one request - e.g. the targeted phase fully closes a charge's cash side but leaves its
        // weight side positive, so that same charge still qualifies as "outstanding" for the
        // fallback phase too. Left unmerged, this produces two separate ReceivableApplication
        // rows for one charge from a single deposit, so the deposit's own Amount silently
        // disagrees with the sum of what its applications say it paid. Consolidate by ChargeId
        // before anything below treats this list as "one entry per charge touched."
        applications = applications
            .GroupBy(a => a.ChargeId)
            .Select(g => new ReceivableApplication
            {
                ChargeId = g.Key,
                AppliedAmount = g.Sum(a => a.AppliedAmount),
                AppliedWeight = g.Sum(a => a.AppliedWeight)
            })
            .ToList();

        // Fold everything the either-side-clears-the-whole-charge rule forgave into the
        // discount pools below, so it's honestly recorded rather than only ever showing up as
        // a live RemainingAmount/RemainingWeight change with no paper trail.
        var forgivenAmount = forgivenAmountByCharge.Values.Sum();
        var forgivenWeight = forgivenWeightByCharge.Values.Sum();
        var manualDiscount = discount;
        var manualDiscountWeight = discountWeight;
        discount += forgivenAmount;
        discountWeight += forgivenWeight;

        // A charge that was already partially settled before (i.e. it's sitting in 미수금
        // 관리, not the fresh worklist) normally already has its own running deposit record,
        // and further money toward it would route into THAT SAME deposit/거래번호 instead of
        // opening a new one. But per isOverdueSettlement above, when EVERY targeted charge is
        // already-touched debt collection, this request must always become its own fresh row -
        // skip the lookup entirely (empty dictionary) so nothing below can merge into an
        // existing deposit.
        var chargeIds = applications.Select(a => a.ChargeId).Distinct().ToList();
        var priorApplications = (!isOverdueSettlement && chargeIds.Count > 0)
            ? await _dbContext.ReceivableApplications
                .Include(a => a.Deposit)
                .Where(a => chargeIds.Contains(a.ChargeId) && a.Deposit != null && a.Deposit.Type == "DEPOSIT" && !a.Deposit.IsCancelled)
                .ToListAsync()
            : new List<ReceivableApplication>();
        var existingApplicationByCharge = priorApplications
            .GroupBy(a => a.ChargeId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(a => a.Deposit!.CreatedAt).First());

        var totalPool = request.Amount + discount;
        var totalPoolWeight = (request.Weight ?? 0m) + discountWeight;
        var cashRatio = totalPool > 0 ? request.Amount / totalPool : 0m;
        var cashRatioWeight = totalPoolWeight > 0 ? (request.Weight ?? 0m) / totalPoolWeight : 0m;

        // application.AppliedAmount/AppliedWeight is ALREADY pure real cash - the exact amount
        // ApplyToChargeList matched from the payer's own pool, never inflated by forgiveness.
        // Multiplying it by a pool-wide cashRatio (as this used to do) double-counts: it shrinks
        // the recorded cash by the SAME forgiven fraction on every charge, including ones that
        // were never forgiven at all, so Deposit.Amount+Discount ends up smaller than what
        // actually closed the charges - money silently vanished from the running ledger
        // (GetLedgerBeforeAsync) even though every charge's own RemainingAmount was correctly 0.
        // Attribute cash 1:1, spread only the user's OWN manual discount proportionally by cash
        // share, and add each charge's own forgiven amount directly - never redistributed to a
        // charge that wasn't the one actually forgiven.
        var totalAppliedAmount = applications.Sum(a => a.AppliedAmount);
        var totalAppliedWeight = applications.Sum(a => a.AppliedWeight);
        var manualDiscountRatio = totalAppliedAmount > 0 ? manualDiscount / totalAppliedAmount : 0m;
        var manualDiscountRatioWeight = totalAppliedWeight > 0 ? manualDiscountWeight / totalAppliedWeight : 0m;

        Receivable? newDeposit = null;
        var newApplications = new List<ReceivableApplication>();

        foreach (var application in applications)
        {
            var cashPortion = application.AppliedAmount;
            var discountPortion = application.AppliedAmount * manualDiscountRatio + forgivenAmountByCharge.GetValueOrDefault(application.ChargeId);
            var cashPortionWeight = application.AppliedWeight;
            var discountPortionWeight = application.AppliedWeight * manualDiscountRatioWeight + forgivenWeightByCharge.GetValueOrDefault(application.ChargeId);

            if (existingApplicationByCharge.TryGetValue(application.ChargeId, out var existingApplication))
            {
                var existingDeposit = existingApplication.Deposit!;
                existingDeposit.Amount += cashPortion;
                existingDeposit.Discount += discountPortion;
                existingDeposit.Weight += cashPortionWeight;
                existingDeposit.DiscountWeight += discountPortionWeight;
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
                    SettlementMethod = request.SettlementMethod,
                    IsOverdueSettlement = isOverdueSettlement
                };
                newDeposit.Amount += cashPortion;
                newDeposit.Discount += discountPortion;
                newDeposit.Weight += cashPortionWeight;
                newDeposit.DiscountWeight += discountPortionWeight;
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
                SettlementMethod = request.SettlementMethod,
                IsOverdueSettlement = isOverdueSettlement
            };
            var leftoverCash = remainingAmount * cashRatio;
            var leftoverCashWeight = remainingWeight * cashRatioWeight;
            newDeposit.Amount += leftoverCash;
            newDeposit.Discount += remainingAmount - leftoverCash;
            newDeposit.Weight += leftoverCashWeight;
            newDeposit.DiscountWeight += remainingWeight - leftoverCashWeight;
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

        // Only the DCC that collected this deposit (or an admin) may correct it - the
        // retailer it was collected from can view and print it, but never edit its own
        // settlement record.
        if (!_currentUserService.IsAdmin)
        {
            var editorUserId = GetCurrentUserId();
            var editorUser = await _receivableRepository.GetUserWithCompaniesAsync(editorUserId);
            var editorIsLogisticsCompany = editorUser != null && editorUser.UserCompanies.Any(uc => uc.Company != null && uc.Company.Category == "DCC");
            if (!editorIsLogisticsCompany)
            {
                return ApiResponse<bool>.Failure("접근 권한이 없습니다.", 403);
            }
        }

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
        var newDiscountWeight = request.DiscountWeight ?? 0m;
        deposit.Amount = request.Amount;
        deposit.Weight = request.Weight ?? 0m;
        deposit.Discount = newDiscount;
        deposit.DiscountWeight = newDiscountWeight;
        deposit.Memo = request.Memo;
        deposit.SettlementMethod = request.SettlementMethod;

        decimal newAmountToApply = request.Amount + newDiscount;
        decimal newWeightToApply = (request.Weight ?? 0m) + newDiscountWeight;
        var stillOutstanding = allCharges.Where(c => c.RemainingAmount > 0 || c.RemainingWeight > 0).ToList();
        var editForgivenAmountByCharge = new Dictionary<int, decimal>();
        var editForgivenWeightByCharge = new Dictionary<int, decimal>();
        var newApplications = ApplyToChargeList(stillOutstanding, ref newAmountToApply, ref newWeightToApply, editForgivenAmountByCharge, editForgivenWeightByCharge);
        // A single deposit being edited has nowhere else to attribute a forgiven amount to -
        // unlike ProcessDepositAsync's merge-across-multiple-destination-deposits case, there's
        // only this one deposit record, so folding the aggregate straight into its own discount
        // is exact, not an approximation.
        deposit.Discount += editForgivenAmountByCharge.Values.Sum();
        deposit.DiscountWeight += editForgivenWeightByCharge.Values.Sum();
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

        // Only the DCC that collected this deposit (or an admin) may reverse it - mirrors
        // UpdateDepositAsync's identical guard. Without this, any authenticated user
        // (including the retailer it was collected from) could cancel it outright, since
        // this endpoint previously had no ownership check at all.
        if (!_currentUserService.IsAdmin)
        {
            var editorUserId = GetCurrentUserId();
            var editorUser = await _receivableRepository.GetUserWithCompaniesAsync(editorUserId);
            var editorIsLogisticsCompany = editorUser != null && editorUser.UserCompanies.Any(uc => uc.Company != null && uc.Company.Category == "DCC");
            if (!editorIsLogisticsCompany)
            {
                return ApiResponse<bool>.Failure("접근 권한이 없습니다.", 403);
            }
        }

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

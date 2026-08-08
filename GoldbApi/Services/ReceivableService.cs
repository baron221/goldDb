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
        return 0m;
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
            (oi.ConfirmedWeight ?? oi.ActualWeight ?? oi.OrderWeight ?? 0m) * GetPureGoldRatio(oi.Purity) * oi.Quantity);

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
                    (x.Item.ConfirmedWeight ?? x.Item.ActualWeight ?? x.Item.OrderWeight ?? 0m) * GetPureGoldRatio(x.Item.Purity) * x.Item.Quantity);

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
            }
        }

        var userIdsWithReceivables = await _receivableRepository.GetUserIdsWithReceivablesAsync();
        var totalCount = await _receivableRepository.GetUsersWithReceivablesCountAsync(search, userIdsWithReceivables, companyCategoryFilter);
        var users = await _receivableRepository.GetUsersWithReceivablesPagedAsync(page, pageSize, search, userIdsWithReceivables, companyCategoryFilter);

        var summaries = new List<UserReceivableSummaryDto>();
        foreach (var user in users)
        {
            var userReceivables = await _receivableRepository.GetReceivablesForUserAsync(user.Id);

            var totalCharge = userReceivables.Where(r => r.Type == "CHARGE").Sum(r => r.Amount);
            var totalDeposit = userReceivables.Where(r => r.Type == "DEPOSIT" && !r.IsCancelled).Sum(r => r.Amount);
            var totalReceivable = userReceivables.Where(r => r.Type == "CHARGE").Sum(r => r.RemainingAmount);

            var totalChargeWeight = userReceivables.Where(r => r.Type == "CHARGE").Sum(r => r.Weight);
            var totalDepositWeight = userReceivables.Where(r => r.Type == "DEPOSIT" && !r.IsCancelled).Sum(r => r.Weight);
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
        var totalDeposit = userReceivables.Where(r => r.Type == "DEPOSIT" && !r.IsCancelled).Sum(r => r.Amount);
        var totalReceivable = userReceivables.Where(r => r.Type == "CHARGE").Sum(r => r.RemainingAmount);

        var totalChargeWeight = userReceivables.Where(r => r.Type == "CHARGE").Sum(r => r.Weight);
        var totalDepositWeight = userReceivables.Where(r => r.Type == "DEPOSIT" && !r.IsCancelled).Sum(r => r.Weight);
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
            .Where(r => r.Type == "CHARGE" && r.OrderId.HasValue && orderIds.Contains(r.OrderId.Value))
            .ToListAsync();

        if (charges.Count == 0) return ApiResponse<UserReceivableSummaryDto?>.Success(null);

        var distinctUsers = charges.Select(r => r.UserId).Distinct().ToList();
        if (distinctUsers.Count > 1)
        {
            return ApiResponse<UserReceivableSummaryDto?>.Failure("선택한 주문들이 서로 다른 거래처에 속해 있습니다. 같은 거래처의 주문만 함께 정산할 수 있습니다.", 400);
        }

        var first = charges[0];
        var chargeIds = charges.Select(c => c.Id).ToList();
        var lastPaymentDate = await _dbContext.ReceivableApplications
            .Where(a => chargeIds.Contains(a.ChargeId))
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => (DateTime?)a.CreatedAt)
            .FirstOrDefaultAsync();

        return ApiResponse<UserReceivableSummaryDto?>.Success(new UserReceivableSummaryDto
        {
            UserId = first.UserId,
            UserName = first.User?.Username,
            UserDisplayName = first.User?.Name,
            CompanyName = first.User?.UserCompanies.FirstOrDefault()?.Company?.Name,
            TotalCharge = charges.Sum(c => c.Amount),
            TotalDeposit = charges.Sum(c => c.Amount - c.RemainingAmount),
            TotalReceivable = charges.Sum(c => c.RemainingAmount),
            TotalChargeWeight = charges.Sum(c => c.Weight),
            TotalDepositWeight = charges.Sum(c => c.Weight - c.RemainingWeight),
            TotalReceivableWeight = charges.Sum(c => c.RemainingWeight),
            LastPaymentDate = lastPaymentDate
        });
    }

    public async Task<ApiResponse<PagedResult<ReceivableDto>>> GetReceivablesAsync(ReceivableQueryDto query)
    {
        var userId = GetCurrentUserId();
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
            }
        }

        var (items, totalCount) = await _receivableRepository.GetReceivablesPagedAsync(query);

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

        decimal p14 = 0, p18 = 0, pure = 0;

        foreach (var charge in outstandingCharges)
        {
            if (charge.Order == null) continue;

            foreach (var item in charge.Order.OrderItems.Where(oi => oi.ParentId == null))
            {
                var weight = (item.ConfirmedWeight ?? item.ActualWeight ?? item.OrderWeight ?? 0m) * item.Quantity;
                var purity = (item.Purity ?? "").ToUpperInvariant();

                if (purity.Contains("14K")) p14 += weight;
                else if (purity.Contains("18K")) p18 += weight;
                else if (purity.Contains("24K") || purity.Contains("PURE")) pure += weight;
            }
        }

        return ApiResponse<PuritySummaryDto>.Success(new PuritySummaryDto
        {
            Purity14k = p14,
            Purity18k = p18,
            PureGold = pure
        });
    }

    // Greedily reduces outstanding charges (oldest first) by the given amount/weight,
    // used both when applying a new deposit and when re-applying one during an edit.
    // Cash and gold weight are two views of the same underlying charge, not independent
    // debts - once a payment fully covers either side of a charge, the whole charge is
    // considered settled and the other side is cleared too.
    private static List<ReceivableApplication> ApplyToChargeList(List<Receivable> charges, ref decimal remainingAmount, ref decimal remainingWeight)
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

    // Restores previously-reduced charges (oldest first, mirroring the order they were
    // originally reduced in), capped at each charge's original Amount/Weight. Used to
    // undo a deposit's effect when it's edited or cancelled.
    private static void ReverseFromChargeList(List<Receivable> charges, ref decimal remainingAmount, ref decimal remainingWeight)
    {
        foreach (var charge in charges)
        {
            if (remainingAmount <= 0 && remainingWeight <= 0) break;

            if (remainingAmount > 0)
            {
                var capacity = charge.Amount - charge.RemainingAmount;
                if (capacity > 0)
                {
                    var restore = Math.Min(capacity, remainingAmount);
                    charge.RemainingAmount += restore;
                    remainingAmount -= restore;
                }
            }

            if (remainingWeight > 0)
            {
                var capacityWeight = charge.Weight - charge.RemainingWeight;
                if (capacityWeight > 0)
                {
                    var restoreWeight = Math.Min(capacityWeight, remainingWeight);
                    charge.RemainingWeight += restoreWeight;
                    remainingWeight -= restoreWeight;
                }
            }
        }
    }

    private async Task<List<ReceivableApplication>> ApplyDepositToChargesAsync(int userId, int? orderId, decimal amountToApply, decimal weightToApply)
    {
        var applications = new List<ReceivableApplication>();

        if (orderId.HasValue)
        {
            var targetCharges = await _receivableRepository.GetTargetChargesAsync(userId, orderId.Value);
            applications.AddRange(ApplyToChargeList(targetCharges, ref amountToApply, ref weightToApply));
        }

        if (amountToApply > 0 || weightToApply > 0)
        {
            var outstandingCharges = await _receivableRepository.GetOutstandingChargesAsync(userId);
            applications.AddRange(ApplyToChargeList(outstandingCharges, ref amountToApply, ref weightToApply));
        }

        return applications;
    }

    public async Task<ApiResponse<bool>> ProcessDepositAsync(CreateDepositDto request)
    {
        var discount = request.Discount ?? 0m;

        if (request.Amount <= 0 && discount <= 0 && (!request.Weight.HasValue || request.Weight.Value <= 0))
        {
            return ApiResponse<bool>.Failure("입금액, 할인액 또는 순금 중량 중 하나는 0보다 커야 합니다.", 400);
        }

        var deposit = new Receivable
        {
            UserId = request.UserId,
            OrderId = request.OrderId,
            Type = "DEPOSIT",
            Amount = request.Amount,
            RemainingAmount = 0,
            Weight = request.Weight ?? 0m,
            RemainingWeight = 0,
            Memo = request.Memo,
            SettlementMethod = request.SettlementMethod,
            Discount = discount
        };
        await _receivableRepository.AddAsync(deposit);

        // Discount reduces outstanding charges the same way cash does, it's just
        // tracked separately on the deposit record so the ledger can show how much
        // of the settlement was actual payment vs. a discount write-off.
        var applications = await ApplyDepositToChargesAsync(request.UserId, request.OrderId, request.Amount + discount, request.Weight ?? 0m);

        // Record which charge(s) this deposit actually paid off, so the history table
        // can show where a payment went instead of just leaving charges mysteriously
        // cleared with no visible link back to the deposit that paid them.
        foreach (var application in applications)
        {
            application.Deposit = deposit;
            _dbContext.ReceivableApplications.Add(application);
        }

        await _receivableRepository.SaveChangesAsync();

        return ApiResponse<bool>.Success(true);
    }

    public async Task<ApiResponse<bool>> UpdateDepositAsync(int id, UpdateDepositDto request)
    {
        var deposit = await _receivableRepository.GetByIdAsync(id);
        if (deposit == null || deposit.Type != "DEPOSIT") return ApiResponse<bool>.Failure("입금 내역을 찾을 수 없습니다.", 404);
        if (deposit.IsCancelled) return ApiResponse<bool>.Failure("취소된 거래는 수정할 수 없습니다.", 400);

        // Undo this deposit's original effect before re-applying the new values, so
        // editing behaves like cancel-then-reapply against the current outstanding charges.
        var allCharges = (await _receivableRepository.GetReceivablesForUserAsync(deposit.UserId))
            .Where(r => r.Type == "CHARGE")
            .OrderBy(r => r.CreatedAt)
            .ToList();

        decimal oldAmount = deposit.Amount + deposit.Discount;
        decimal oldWeight = deposit.Weight;
        ReverseFromChargeList(allCharges, ref oldAmount, ref oldWeight);

        var oldApplications = await _dbContext.ReceivableApplications.Where(a => a.DepositId == deposit.Id).ToListAsync();
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
        var newApplications = ApplyToChargeList(stillOutstanding, ref newAmountToApply, ref newWeightToApply);
        foreach (var application in newApplications)
        {
            application.Deposit = deposit;
            _dbContext.ReceivableApplications.Add(application);
        }

        await _receivableRepository.SaveChangesAsync();
        return ApiResponse<bool>.Success(true);
    }

    public async Task<ApiResponse<bool>> CancelDepositAsync(int id)
    {
        var deposit = await _receivableRepository.GetByIdAsync(id);
        if (deposit == null || deposit.Type != "DEPOSIT") return ApiResponse<bool>.Failure("입금 내역을 찾을 수 없습니다.", 404);
        if (deposit.IsCancelled) return ApiResponse<bool>.Failure("이미 취소된 거래입니다.", 400);

        var allCharges = (await _receivableRepository.GetReceivablesForUserAsync(deposit.UserId))
            .Where(r => r.Type == "CHARGE")
            .OrderBy(r => r.CreatedAt)
            .ToList();

        decimal amountToRestore = deposit.Amount + deposit.Discount;
        decimal weightToRestore = deposit.Weight;
        ReverseFromChargeList(allCharges, ref amountToRestore, ref weightToRestore);

        var oldApplications = await _dbContext.ReceivableApplications.Where(a => a.DepositId == deposit.Id).ToListAsync();
        _dbContext.ReceivableApplications.RemoveRange(oldApplications);

        deposit.IsCancelled = true;

        await _receivableRepository.SaveChangesAsync();
        return ApiResponse<bool>.Success(true);
    }
}

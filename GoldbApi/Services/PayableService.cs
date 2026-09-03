using GoldbApi.Data;
using GoldbApi.DTOs;
using GoldbApi.Models;
using GoldbApi.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GoldbApi.Services;

public interface IPayableService
{
    Task<ApiResponse<PagedResult<CompanyPayableSummaryDto>>> GetCompanySummariesAsync(int page, int pageSize, string? search);

    Task<ApiResponse<CompanyPayableSummaryDto?>> GetPayableSummaryForOrderAsync(int orderId);

    Task<ApiResponse<PagedResult<PayableOrderRowDto>>> GetPayableOrderHistoryAsync(PayableQueryDto query);

    Task<ApiResponse<PagedResult<PayableOrderRowDto>>> GetCompletedPayableOrdersAsync(PayableQueryDto query);

    Task<ApiResponse<List<ChargeApplicationRowDto>>> GetChargeApplicationsAsync(int chargeId);

    Task<ApiResponse<LedgerBalanceDto>> GetLedgerBeforeAsync(int payableId);

    Task<ApiResponse<PayableOrderHistorySummaryDto>> GetPayableOrderHistorySummaryAsync(PayableQueryDto query);

    Task<ApiResponse<List<PayableOverdueRowDto>>> GetPayableOverdueSummaryAsync(PayableOverdueQueryDto query);

    Task<ApiResponse<PagedResult<PayableDto>>> GetPayablesAsync(PayableQueryDto query);

    Task<ApiResponse<bool>> ProcessPaymentAsync(CreatePaymentDto request);

    Task<ApiResponse<bool>> UpdatePaymentAsync(int id, UpdatePaymentDto request);

    Task<ApiResponse<bool>> CancelPaymentAsync(int id);

    Task<ApiResponse<bool>> DeletePaymentAsync(int id);

    Task<ApiResponse<bool>> MarkMfgProcessedAsync(int payableId, MfgProcessDto request);

    Task<ApiResponse<OrderChargeSummaryDto?>> GetOrderChargeSummaryAsync(List<int> orderIds);

    Task<ApiResponse<List<PaymentApplicationDetailDto>>> GetPaymentApplicationsAsync(int paymentId);

    Task<ApiResponse<bool>> UpdatePaymentApplicationAsync(int applicationId, UpdatePaymentApplicationDto request);
}

public class PayableService : IPayableService
{
    private readonly IRepository<Payable> _payableRepository;
    private readonly IRepository<Company> _companyRepository;
    private readonly IRepository<UserCompany> _userCompanyRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly AppDbContext _dbContext;

    public PayableService(
        IRepository<Payable> payableRepository,
        IRepository<Company> companyRepository,
        IRepository<UserCompany> userCompanyRepository,
        ICurrentUserService currentUserService,
        AppDbContext dbContext)
    {
        _payableRepository = payableRepository;
        _companyRepository = companyRepository;
        _userCompanyRepository = userCompanyRepository;
        _currentUserService = currentUserService;
        _dbContext = dbContext;
    }

    // Mirrors ReceivableService.GetPureGoldRatio - the purity breakdown here must convert
    // to the same fine-gold basis as the Charge.Weight it's reconciled against (which is
    // already stored fine-gold-converted at charge-creation time).
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

    // Prefers the latest confirmed measurement, but a stored 0 doesn't count as "measured" -
    // no physical item actually weighs 0g, so 0 means the factory left the field untouched
    // (e.g. submitted 제품출고 without entering a scale reading), not that it truly weighs
    // nothing. ?? alone would stop at that 0 instead of falling back to the requested weight.
    private static decimal GetEffectiveWeight(decimal? confirmedWeight, decimal? actualWeight, decimal? orderWeight)
    {
        if (confirmedWeight.HasValue && confirmedWeight.Value > 0) return confirmedWeight.Value;
        if (actualWeight.HasValue && actualWeight.Value > 0) return actualWeight.Value;
        return orderWeight ?? 0m;
    }

    // Only DCC (the payer) and MFG (the payee) have a side in this ledger; admins can
    // view but don't have a company to scope by, so they see everything from the
    // manufacturer-owed perspective by default.
    private async Task<(int CompanyId, string Category)?> GetCurrentCompanyAsync()
    {
        var userId = _currentUserService.UserId;
        if (userId == null) return null;

        var uc = await _userCompanyRepository.GetQueryable()
            .Include(x => x.Company)
            .Where(x => x.UserId == userId.Value && !x.IsDeleted && x.Company != null && (x.Company.Category == "DCC" || x.Company.Category == "MFG"))
            .FirstOrDefaultAsync();

        if (uc?.Company == null) return null;
        return (uc.CompanyId, uc.Company.Category!);
    }

    public async Task<ApiResponse<PagedResult<CompanyPayableSummaryDto>>> GetCompanySummariesAsync(int page, int pageSize, string? search)
    {
        var current = await GetCurrentCompanyAsync();

        bool viewingAsLogistics;
        int myCompanyId = 0;

        if (!_currentUserService.IsAdmin)
        {
            if (current == null)
            {
                return ApiResponse<PagedResult<CompanyPayableSummaryDto>>.Success(new PagedResult<CompanyPayableSummaryDto>
                {
                    Items = new List<CompanyPayableSummaryDto>(),
                    TotalCount = 0,
                    Page = page,
                    PageSize = pageSize
                });
            }

            viewingAsLogistics = current.Value.Category == "DCC";
            myCompanyId = current.Value.CompanyId;
        }
        else
        {
            viewingAsLogistics = false;
        }

        var scoped = _payableRepository.GetQueryable();
        if (!_currentUserService.IsAdmin)
        {
            scoped = viewingAsLogistics
                ? scoped.Where(p => p.LogisticsCompanyId == myCompanyId)
                : scoped.Where(p => p.ManufacturerCompanyId == myCompanyId);
        }

        var otherIds = await (viewingAsLogistics
                ? scoped.Select(p => p.ManufacturerCompanyId)
                : scoped.Select(p => p.LogisticsCompanyId))
            .Distinct()
            .ToListAsync();

        var companiesQuery = _companyRepository.GetQueryable().Where(c => otherIds.Contains(c.Id));
        if (!string.IsNullOrEmpty(search))
        {
            companiesQuery = companiesQuery.Where(c => c.Name.Contains(search));
        }

        var totalCount = await companiesQuery.CountAsync();
        var companies = await companiesQuery
            .OrderBy(c => c.Name).ThenBy(c => c.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var summaries = new List<CompanyPayableSummaryDto>();
        foreach (var company in companies)
        {
            var pairPayables = await (viewingAsLogistics
                    ? scoped.Where(p => p.ManufacturerCompanyId == company.Id)
                    : scoped.Where(p => p.LogisticsCompanyId == company.Id))
                .ToListAsync();

            var totalCharge = pairPayables.Where(p => p.Type == "CHARGE").Sum(p => p.Amount);
            // "Paid" = what actually landed on a charge: raw Amount plus any Discount granted,
            // minus whatever part of this payment is still unapplied (see RemainingAmount, set
            // in ProcessPaymentAsync/UpdatePaymentAsync). This keeps TotalCharge - TotalPaid ==
            // TotalOutstanding even when a payment carries a discount or overpays.
            var totalPaid = pairPayables.Where(p => p.Type == "PAYMENT" && !p.IsCancelled).Sum(p => p.Amount + p.Discount - p.RemainingAmount);
            var totalOutstanding = pairPayables.Where(p => p.Type == "CHARGE").Sum(p => p.RemainingAmount);

            var totalChargeWeight = pairPayables.Where(p => p.Type == "CHARGE").Sum(p => p.Weight);
            var totalPaidWeight = pairPayables.Where(p => p.Type == "PAYMENT" && !p.IsCancelled).Sum(p => p.Weight + p.DiscountWeight - p.RemainingWeight);
            var totalOutstandingWeight = pairPayables.Where(p => p.Type == "CHARGE").Sum(p => p.RemainingWeight);

            var lastPaymentDate = pairPayables.Where(p => p.Type == "PAYMENT" && !p.IsCancelled)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => (DateTime?)p.CreatedAt)
                .FirstOrDefault();

            summaries.Add(new CompanyPayableSummaryDto
            {
                CompanyId = company.Id,
                CompanyName = company.Name,
                TotalCharge = totalCharge,
                TotalPaid = totalPaid,
                TotalOutstanding = totalOutstanding,
                TotalChargeWeight = totalChargeWeight,
                TotalPaidWeight = totalPaidWeight,
                TotalOutstandingWeight = totalOutstandingWeight,
                LastPaymentDate = lastPaymentDate
            });
        }

        return ApiResponse<PagedResult<CompanyPayableSummaryDto>>.Success(new PagedResult<CompanyPayableSummaryDto>
        {
            Items = summaries,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    // Every order is produced by a single manufacturer (orders are split by manufacturer
    // at creation time), so an order's settlement-linked "정산 처리" can be scoped to
    // exactly one logistics/manufacturer company pair.
    public async Task<ApiResponse<CompanyPayableSummaryDto?>> GetPayableSummaryForOrderAsync(int orderId)
    {
        var charge = await _payableRepository.GetQueryable()
            .Include(p => p.ManufacturerCompany)
            .Where(p => p.OrderId == orderId && p.Type == "CHARGE")
            .FirstOrDefaultAsync();

        if (charge == null) return ApiResponse<CompanyPayableSummaryDto?>.Success(null);

        var pairPayables = await _payableRepository.GetQueryable()
            .Where(p => p.LogisticsCompanyId == charge.LogisticsCompanyId && p.ManufacturerCompanyId == charge.ManufacturerCompanyId)
            .ToListAsync();

        var totalCharge = pairPayables.Where(p => p.Type == "CHARGE").Sum(p => p.Amount);
        var totalPaid = pairPayables.Where(p => p.Type == "PAYMENT" && !p.IsCancelled).Sum(p => p.Amount + p.Discount - p.RemainingAmount);
        var totalOutstanding = pairPayables.Where(p => p.Type == "CHARGE").Sum(p => p.RemainingAmount);

        var totalChargeWeight = pairPayables.Where(p => p.Type == "CHARGE").Sum(p => p.Weight);
        var totalPaidWeight = pairPayables.Where(p => p.Type == "PAYMENT" && !p.IsCancelled).Sum(p => p.Weight + p.DiscountWeight - p.RemainingWeight);
        var totalOutstandingWeight = pairPayables.Where(p => p.Type == "CHARGE").Sum(p => p.RemainingWeight);

        var lastPaymentDate = pairPayables.Where(p => p.Type == "PAYMENT" && !p.IsCancelled)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => (DateTime?)p.CreatedAt)
            .FirstOrDefault();

        return ApiResponse<CompanyPayableSummaryDto?>.Success(new CompanyPayableSummaryDto
        {
            CompanyId = charge.ManufacturerCompanyId,
            CompanyName = charge.ManufacturerCompany?.Name,
            TotalCharge = totalCharge,
            TotalPaid = totalPaid,
            TotalOutstanding = totalOutstanding,
            TotalChargeWeight = totalChargeWeight,
            TotalPaidWeight = totalPaidWeight,
            TotalOutstandingWeight = totalOutstandingWeight,
            LastPaymentDate = lastPaymentDate
        });
    }

    // Builds the ledger preview for a multi-order batch settlement: A (거래전미수) is the
    // company pair's outstanding balance EXCLUDING the selected orders, B (판매) is the sum
    // of the selected orders' own remaining charge - kept separate from A so the two never
    // double-count the same debt.
    public async Task<ApiResponse<OrderChargeSummaryDto?>> GetOrderChargeSummaryAsync(List<int> orderIds)
    {
        if (orderIds == null || orderIds.Count == 0) return ApiResponse<OrderChargeSummaryDto?>.Success(null);

        var current = await GetCurrentCompanyAsync();
        if (current == null && !_currentUserService.IsAdmin) return ApiResponse<OrderChargeSummaryDto?>.Success(null);

        var chargesQuery = _payableRepository.GetQueryable()
            .Include(p => p.Order)
            .Where(p => p.Type == "CHARGE" && p.OrderId.HasValue && orderIds.Contains(p.OrderId.Value));

        if (!_currentUserService.IsAdmin && current != null)
        {
            chargesQuery = current.Value.Category == "DCC"
                ? chargesQuery.Where(p => p.LogisticsCompanyId == current.Value.CompanyId)
                : chargesQuery.Where(p => p.ManufacturerCompanyId == current.Value.CompanyId);
        }

        var charges = await chargesQuery.ToListAsync();
        if (charges.Count == 0) return ApiResponse<OrderChargeSummaryDto?>.Success(null);

        // A batch settlement ledger only makes sense for a single counterparty - silently
        // narrowing to one pair would drop the other orders from the total without telling
        // the user, so a mixed selection is rejected outright instead.
        var distinctPairs = charges.Select(p => (p.LogisticsCompanyId, p.ManufacturerCompanyId)).Distinct().ToList();
        if (distinctPairs.Count > 1)
        {
            return ApiResponse<OrderChargeSummaryDto?>.Failure("선택한 주문들이 서로 다른 거래처에 속해 있습니다. 같은 거래처의 주문만 함께 정산할 수 있습니다.", 400);
        }

        var logisticsCompanyId = charges[0].LogisticsCompanyId;
        var manufacturerCompanyId = charges[0].ManufacturerCompanyId;

        var isLogisticsViewer = !_currentUserService.IsAdmin && current!.Value.Category == "DCC";
        var counterpartyId = isLogisticsViewer ? manufacturerCompanyId : logisticsCompanyId;
        var counterparty = await _companyRepository.GetQueryable().FirstOrDefaultAsync(c => c.Id == counterpartyId);

        var pairPayables = await _payableRepository.GetQueryable()
            .Where(p => p.LogisticsCompanyId == logisticsCompanyId && p.ManufacturerCompanyId == manufacturerCompanyId)
            .ToListAsync();

        var totalOutstanding = pairPayables.Where(p => p.Type == "CHARGE").Sum(p => p.RemainingAmount);
        var totalOutstandingWeight = pairPayables.Where(p => p.Type == "CHARGE").Sum(p => p.RemainingWeight);
        var lastPaymentDate = pairPayables.Where(p => p.Type == "PAYMENT" && !p.IsCancelled)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => (DateTime?)p.CreatedAt)
            .FirstOrDefault();

        var saleAmount = charges.Sum(p => p.RemainingAmount);
        var saleWeight = charges.Sum(p => p.RemainingWeight);

        var selectedOrderIds = charges.Where(p => p.OrderId.HasValue).Select(p => p.OrderId!.Value).Distinct().ToList();

        var topLevelItems = await _dbContext.OrderItems
            .Include(oi => oi.Product).ThenInclude(p => p!.ProductPhotos)
            .Include(oi => oi.ProductSet).ThenInclude(ps => ps!.ProductSetPhotos)
            .Where(oi => selectedOrderIds.Contains(oi.OrderId) && oi.ParentId == null)
            .ToListAsync();

        var relevantItems = topLevelItems.Where(oi =>
            (oi.Product != null && oi.Product.CompanyId == manufacturerCompanyId) ||
            (oi.ProductSet != null && oi.ProductSet.CompanyId == manufacturerCompanyId));

        var purityBreakdown = relevantItems
            .GroupBy(oi => string.IsNullOrEmpty(oi.Purity) ? "기타" : oi.Purity!)
            .Select(g => new PurityBreakdownDto
            {
                Purity = g.Key,
                Weight = g.Sum(oi => GetEffectiveWeight(oi.ConfirmedWeight, oi.ActualWeight, oi.OrderWeight) * GetPureGoldRatio(oi.Purity) * oi.Quantity),
                Amount = g.Sum(oi => ((oi.FactoryInputMaterialCost ?? 0) + (oi.FactoryInputLaborCost ?? 0)) * oi.Quantity)
            })
            .Where(b => b.Weight > 0 || b.Amount > 0)
            .OrderByDescending(b => b.Weight)
            .ToList();

        var companyNames = await _companyRepository.GetQueryable()
            .Where(c => c.Id == manufacturerCompanyId || c.Id == logisticsCompanyId)
            .ToDictionaryAsync(c => c.Id, c => c.Name);

        var itemsByOrder = topLevelItems
            .GroupBy(oi => oi.OrderId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var summaryItems = charges.Select(c =>
        {
            itemsByOrder.TryGetValue(c.OrderId ?? 0, out var orderItems);
            orderItems ??= new List<OrderItem>();
            var item = orderItems.FirstOrDefault();
            return new OrderChargeSummaryItemDto
            {
                PayableId = c.Id,
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
                ManufacturerName = companyNames.GetValueOrDefault(manufacturerCompanyId),
                LogisticsCompanyName = companyNames.GetValueOrDefault(logisticsCompanyId),
                Items = orderItems.Select(oi => new PayableOrderItemSummaryDto
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
                    MaterialCost = oi.FactoryInputMaterialCost ?? 0,
                    LaborCost = oi.FactoryInputLaborCost ?? 0
                }).ToList()
            };
        }).OrderByDescending(i => i.OrderDate).ToList();

        return ApiResponse<OrderChargeSummaryDto?>.Success(new OrderChargeSummaryDto
        {
            CompanyId = counterpartyId,
            CompanyName = counterparty?.Name,
            TotalOutstanding = totalOutstanding - saleAmount,
            TotalOutstandingWeight = totalOutstandingWeight - saleWeight,
            LastPaymentDate = lastPaymentDate,
            SaleAmount = saleAmount,
            SaleWeight = saleWeight,
            PurityBreakdown = purityBreakdown,
            Items = summaryItems
        });
    }

    // One row per order (each order has exactly one manufacturer, so exactly one CHARGE
    // payable per order) - used for the order-level "정산 받은/처리 내역" listing, as
    // opposed to GetPayablesAsync which lists raw ledger transactions (charges + payments).
    private async Task<IQueryable<Payable>> BuildOrderHistoryQueryAsync(PayableQueryDto query)
    {
        var current = await GetCurrentCompanyAsync();

        var dbQuery = _payableRepository.GetQueryable()
            .Include(p => p.LogisticsCompany)
            .Include(p => p.ManufacturerCompany)
            .Include(p => p.Order).ThenInclude(o => o!.OrderItems)
            .Where(p => p.Type == "CHARGE");

        if (!_currentUserService.IsAdmin)
        {
            if (current == null)
            {
                return dbQuery.Where(p => false);
            }

            dbQuery = current.Value.Category == "DCC"
                ? dbQuery.Where(p => p.LogisticsCompanyId == current.Value.CompanyId)
                : dbQuery.Where(p => p.ManufacturerCompanyId == current.Value.CompanyId);
        }

        if (query.CompanyId.HasValue)
        {
            dbQuery = dbQuery.Where(p => p.ManufacturerCompanyId == query.CompanyId.Value || p.LogisticsCompanyId == query.CompanyId.Value);
        }

        if (query.StartDate.HasValue)
        {
            dbQuery = dbQuery.Where(p => p.CreatedAt >= query.StartDate.Value);
        }

        if (query.EndDate.HasValue)
        {
            var endExclusive = query.EndDate.Value.Date.AddDays(1);
            dbQuery = dbQuery.Where(p => p.CreatedAt < endExclusive);
        }

        if (!string.IsNullOrEmpty(query.ProductName))
        {
            dbQuery = dbQuery.Where(p => p.Order != null && p.Order.OrderItems.Any(oi => oi.ParentId == null &&
                ((oi.Product != null && oi.Product.Name.Contains(query.ProductName)) ||
                 (oi.ProductSet != null && oi.ProductSet.Title.Contains(query.ProductName)))));
        }

        if (!string.IsNullOrEmpty(query.OrderNo))
        {
            dbQuery = dbQuery.Where(p => p.Order != null && p.Order.OrderNo.Contains(query.OrderNo));
        }

        if (!string.IsNullOrEmpty(query.Remarks))
        {
            dbQuery = dbQuery.Where(p => p.Order != null && p.Order.OrderItems.Any(oi => oi.ParentId == null &&
                oi.Memo != null && oi.Memo.Contains(query.Remarks)));
        }

        return dbQuery;
    }

    public async Task<ApiResponse<PayableOrderHistorySummaryDto>> GetPayableOrderHistorySummaryAsync(PayableQueryDto query)
    {
        var dbQuery = await BuildOrderHistoryQueryAsync(query);

        // A charge nobody has ever run through 정산처리 (not even a 0-value acknowledgement)
        // belongs only to 정산 대상 내역's own worklist - it was never "billed" from this
        // page's point of view, so counting its full Amount into 총 판매 (and therefore into
        // 미수금 = 총 판매 - 총 결제) makes a product nobody has touched yet look like
        // outstanding debt. Same "has an application" gate GetPayableOverdueSummaryAsync
        // already uses for 미수금 관리 itself.
        dbQuery = dbQuery.Where(p => _dbContext.PayableApplications.Any(a => a.ChargeId == p.Id));

        var summary = await dbQuery
            .GroupBy(p => 1)
            .Select(g => new PayableOrderHistorySummaryDto
            {
                TotalChargeAmount = g.Sum(p => p.Amount),
                TotalChargeWeight = g.Sum(p => p.Weight),
                TotalPaidAmount = g.Sum(p => p.Amount - p.RemainingAmount),
                TotalPaidWeight = g.Sum(p => p.Weight - p.RemainingWeight)
            })
            .FirstOrDefaultAsync();

        return ApiResponse<PayableOrderHistorySummaryDto>.Success(summary ?? new PayableOrderHistorySummaryDto());
    }

    public async Task<ApiResponse<List<PayableOverdueRowDto>>> GetPayableOverdueSummaryAsync(PayableOverdueQueryDto query)
    {
        var current = await GetCurrentCompanyAsync();
        if (!_currentUserService.IsAdmin && current == null)
        {
            return ApiResponse<List<PayableOverdueRowDto>>.Success(new List<PayableOverdueRowDto>());
        }

        // 미수금 관리 is exclusively for charges that have actually been processed through
        // 정산처리 at least once (even a 0-value acknowledgement, see ProcessPaymentAsync) -
        // a charge nobody has touched yet belongs only in 정산 내역's worklist. Mirrors that
        // worklist's own "has an application" check so a charge is in exactly one of the
        // two lists at any moment.
        var chargesQuery = _payableRepository.GetQueryable()
            .Include(p => p.LogisticsCompany)
            .Where(p => p.Type == "CHARGE" && !p.IsCancelled
                && (p.RemainingAmount > 0 || p.RemainingWeight > 0)
                && _dbContext.PayableApplications.Any(a => a.ChargeId == p.Id));

        if (!_currentUserService.IsAdmin && current != null)
        {
            chargesQuery = chargesQuery.Where(p => p.ManufacturerCompanyId == current.Value.CompanyId);
        }

        if (query.LogisticsCompanyId.HasValue)
        {
            chargesQuery = chargesQuery.Where(p => p.LogisticsCompanyId == query.LogisticsCompanyId.Value);
        }

        if (query.StartDate.HasValue)
        {
            chargesQuery = chargesQuery.Where(p => p.CreatedAt >= query.StartDate.Value);
        }

        if (query.EndDate.HasValue)
        {
            var endExclusive = query.EndDate.Value.Date.AddDays(1);
            chargesQuery = chargesQuery.Where(p => p.CreatedAt < endExclusive);
        }

        var charges = await chargesQuery.ToListAsync();
        var today = DateTime.UtcNow.Date;

        // 마지막거래일자 needs the most recent activity across this DCC's WHOLE ledger with
        // this manufacturer (any Type, not just the still-outstanding charges above).
        var involvedLogisticsCompanyIds = charges.Select(p => p.LogisticsCompanyId).Distinct().ToList();
        var lastTransactionQuery = _payableRepository.GetQueryable()
            .Where(p => !p.IsCancelled && involvedLogisticsCompanyIds.Contains(p.LogisticsCompanyId));
        if (!_currentUserService.IsAdmin && current != null)
        {
            lastTransactionQuery = lastTransactionQuery.Where(p => p.ManufacturerCompanyId == current.Value.CompanyId);
        }
        var lastTransactionByCompany = await lastTransactionQuery
            .GroupBy(p => p.LogisticsCompanyId)
            .Select(g => new { LogisticsCompanyId = g.Key, LastDate = g.Max(p => p.CreatedAt) })
            .ToDictionaryAsync(x => x.LogisticsCompanyId, x => x.LastDate);

        var rows = charges
            .GroupBy(p => p.LogisticsCompanyId)
            .Select(g =>
            {
                var oldest = g.Min(p => p.CreatedAt);
                return new PayableOverdueRowDto
                {
                    LogisticsCompanyId = g.Key,
                    CompanyName = g.First().LogisticsCompany?.Name,
                    SaleDate = oldest,
                    LastTransactionDate = lastTransactionByCompany.TryGetValue(g.Key, out var lastDate) ? lastDate : oldest,
                    SaleAmount = g.Sum(p => p.Amount),
                    SaleWeight = g.Sum(p => p.Weight),
                    CollectedAmount = g.Sum(p => p.Amount - p.RemainingAmount),
                    CollectedWeight = g.Sum(p => p.Weight - p.RemainingWeight),
                    OutstandingAmount = g.Sum(p => p.RemainingAmount),
                    OutstandingWeight = g.Sum(p => p.RemainingWeight),
                    OverdueDays = (today - oldest.Date).Days,
                    OrderIds = g.Where(p => p.OrderId.HasValue).Select(p => p.OrderId!.Value).Distinct().ToList()
                };
            })
            .OrderByDescending(r => r.OverdueDays)
            .ToList();

        return ApiResponse<List<PayableOverdueRowDto>>.Success(rows);
    }

    public async Task<ApiResponse<PagedResult<PayableOrderRowDto>>> GetPayableOrderHistoryAsync(PayableQueryDto query)
    {
        var dbQuery = await BuildOrderHistoryQueryAsync(query);

        // This table is a "new charge, first action needed" worklist, not a running ledger -
        // once a charge has received ANY action at all (even a 0-value acknowledgement, see
        // ProcessPaymentAsync), it drops out here for good, so it doesn't stay re-selectable
        // alongside brand-new untouched charges. A remaining balance surfaces instead in
        // GetPayableOverdueSummaryAsync's dedicated 미수금 관리 tracker (the totals in
        // GetPayableOrderHistorySummaryAsync still cover everything, settled, partially
        // paid, or not touched at all).
        dbQuery = dbQuery.Where(p => p.RemainingAmount == p.Amount && p.RemainingWeight == p.Weight && (p.Amount > 0 || p.Weight > 0)
            && !_dbContext.PayableApplications.Any(a => a.ChargeId == p.Id));

        var totalCount = await dbQuery.CountAsync();
        var items = await dbQuery
            .OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(p => new PayableOrderRowDto
            {
                PayableId = p.Id,
                OrderId = p.OrderId,
                OrderNo = p.Order != null ? p.Order.OrderNo : null,
                ProductName = p.Order != null
                    ? p.Order.OrderItems.Where(oi => oi.ParentId == null)
                        .Select(oi => oi.Product != null ? oi.Product.Name : (oi.ProductSet != null ? oi.ProductSet.Title : oi.CustomProductName))
                        .FirstOrDefault()
                    : null,
                ProductPhotoUrl = p.Order != null
                    ? p.Order.OrderItems.Where(oi => oi.ParentId == null)
                        .Select(oi => oi.Product != null && oi.Product.ProductPhotos.Any() ? oi.Product.ProductPhotos.OrderBy(x => x.SortOrder).First().PhotoUrl
                            : (oi.ProductSet != null && oi.ProductSet.ProductSetPhotos.Any() ? oi.ProductSet.ProductSetPhotos.OrderBy(x => x.SortOrder).First().PhotoUrl : null))
                        .FirstOrDefault()
                    : null,
                ProductItemCount = p.Order != null ? p.Order.OrderItems.Count(oi => oi.ParentId == null) : 0,
                Items = p.Order != null
                    ? p.Order.OrderItems.Where(oi => oi.ParentId == null)
                        .Select(oi => new PayableOrderItemSummaryDto
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
                            ActualWeight = oi.ActualWeight ?? oi.RequestedWeight ?? oi.ApprovedWeight,
                            MaterialCost = oi.FactoryInputMaterialCost ?? 0,
                            LaborCost = oi.FactoryInputLaborCost ?? 0
                        }).ToList()
                    : new List<PayableOrderItemSummaryDto>(),
                Remarks = p.Order != null
                    ? p.Order.OrderItems.Where(oi => oi.ParentId == null && oi.Memo != null && oi.Memo != "")
                        .Select(oi => oi.Memo)
                        .FirstOrDefault()
                    : null,
                LogisticsCompanyId = p.LogisticsCompanyId,
                LogisticsCompanyName = p.LogisticsCompany != null ? p.LogisticsCompany.Name : null,
                ManufacturerCompanyId = p.ManufacturerCompanyId,
                ManufacturerCompanyName = p.ManufacturerCompany != null ? p.ManufacturerCompany.Name : null,
                OrderDate = p.CreatedAt,
                ChargeAmount = p.Amount,
                ChargeWeight = p.Weight,
                PaidAmount = p.Amount - p.RemainingAmount,
                PaidWeight = p.Weight - p.RemainingWeight,
                RemainingAmount = p.RemainingAmount,
                RemainingWeight = p.RemainingWeight
            })
            .ToListAsync();

        return ApiResponse<PagedResult<PayableOrderRowDto>>.Success(new PagedResult<PayableOrderRowDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        });
    }

    // Order-centric view of "정산 완료 내역" - the mirror image of GetPayableOrderHistoryAsync's
    // worklist (which drops a charge once it's fully paid): this one shows ONLY fully-paid
    // charges, one row per order, so a payment that got split across several 거래번호 over
    // time reads as "this order, paid via N transactions" instead of N separate order rows.
    public async Task<ApiResponse<PagedResult<PayableOrderRowDto>>> GetCompletedPayableOrdersAsync(PayableQueryDto query)
    {
        var dbQuery = await BuildOrderHistoryQueryAsync(query);
        dbQuery = dbQuery.Where(p => p.RemainingAmount <= 0 && p.RemainingWeight <= 0);

        var totalCount = await dbQuery.CountAsync();
        var items = await dbQuery
            // Sort by when the charge was actually PAID OFF (its latest payment application),
            // not by the order's own creation date - otherwise an order settled just now, but
            // originally placed a while ago, wouldn't surface near the top of a "정산완료내역"
            // list, which is meant to read as recency of settlement, not recency of the order.
            .Select(p => new
            {
                Payable = p,
                LastPaymentDate = _dbContext.PayableApplications
                    .Where(a => a.ChargeId == p.Id)
                    .Select(a => (DateTime?)a.Payment!.CreatedAt)
                    .Max()
            })
            .OrderByDescending(x => x.LastPaymentDate ?? x.Payable.CreatedAt).ThenByDescending(x => x.Payable.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => x.Payable)
            .Select(p => new PayableOrderRowDto
            {
                PayableId = p.Id,
                OrderId = p.OrderId,
                OrderNo = p.Order != null ? p.Order.OrderNo : null,
                ProductName = p.Order != null
                    ? p.Order.OrderItems.Where(oi => oi.ParentId == null)
                        .Select(oi => oi.Product != null ? oi.Product.Name : (oi.ProductSet != null ? oi.ProductSet.Title : oi.CustomProductName))
                        .FirstOrDefault()
                    : null,
                ProductPhotoUrl = p.Order != null
                    ? p.Order.OrderItems.Where(oi => oi.ParentId == null)
                        .Select(oi => oi.Product != null && oi.Product.ProductPhotos.Any() ? oi.Product.ProductPhotos.OrderBy(x => x.SortOrder).First().PhotoUrl
                            : (oi.ProductSet != null && oi.ProductSet.ProductSetPhotos.Any() ? oi.ProductSet.ProductSetPhotos.OrderBy(x => x.SortOrder).First().PhotoUrl : null))
                        .FirstOrDefault()
                    : null,
                ProductItemCount = p.Order != null ? p.Order.OrderItems.Count(oi => oi.ParentId == null) : 0,
                Items = p.Order != null
                    ? p.Order.OrderItems.Where(oi => oi.ParentId == null)
                        .Select(oi => new PayableOrderItemSummaryDto
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
                            ActualWeight = oi.ActualWeight ?? oi.RequestedWeight ?? oi.ApprovedWeight,
                            MaterialCost = oi.FactoryInputMaterialCost ?? 0,
                            LaborCost = oi.FactoryInputLaborCost ?? 0
                        }).ToList()
                    : new List<PayableOrderItemSummaryDto>(),
                Remarks = p.Order != null
                    ? p.Order.OrderItems.Where(oi => oi.ParentId == null && oi.Memo != null && oi.Memo != "")
                        .Select(oi => oi.Memo)
                        .FirstOrDefault()
                    : null,
                LogisticsCompanyId = p.LogisticsCompanyId,
                LogisticsCompanyName = p.LogisticsCompany != null ? p.LogisticsCompany.Name : null,
                ManufacturerCompanyId = p.ManufacturerCompanyId,
                ManufacturerCompanyName = p.ManufacturerCompany != null ? p.ManufacturerCompany.Name : null,
                OrderDate = p.CreatedAt,
                ChargeAmount = p.Amount,
                ChargeWeight = p.Weight,
                PaidAmount = p.Amount - p.RemainingAmount,
                PaidWeight = p.Weight - p.RemainingWeight,
                RemainingAmount = p.RemainingAmount,
                RemainingWeight = p.RemainingWeight
            })
            .ToListAsync();

        return ApiResponse<PagedResult<PayableOrderRowDto>>.Success(new PagedResult<PayableOrderRowDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        });
    }

    // The inverse of GetPaymentApplicationsAsync: given one order's charge, list every
    // payment (거래번호) that has ever applied against it, so a partially-then-fully-paid
    // order can show its whole payment trail instead of just the latest 거래.
    public async Task<ApiResponse<List<ChargeApplicationRowDto>>> GetChargeApplicationsAsync(int chargeId)
    {
        var charge = await _payableRepository.GetByIdAsync(chargeId);
        if (charge == null || charge.Type != "CHARGE") return ApiResponse<List<ChargeApplicationRowDto>>.Failure("청구 내역을 찾을 수 없습니다.", 404);

        if (!_currentUserService.IsAdmin)
        {
            var current = await GetCurrentCompanyAsync();
            if (current == null || (current.Value.CompanyId != charge.LogisticsCompanyId && current.Value.CompanyId != charge.ManufacturerCompanyId))
            {
                return ApiResponse<List<ChargeApplicationRowDto>>.Failure("접근 권한이 없습니다.", 403);
            }
        }

        var applications = await _dbContext.PayableApplications
            .Where(a => a.ChargeId == chargeId)
            .Select(a => new ChargeApplicationRowDto
            {
                PaymentId = a.PaymentId,
                AppliedAmount = a.AppliedAmount,
                AppliedWeight = a.AppliedWeight,
                PaymentCreatedAt = a.Payment!.CreatedAt,
                IsCancelled = a.Payment!.IsCancelled,
                Memo = a.Payment!.Memo,
                Amount = a.Payment!.Amount,
                Weight = a.Payment!.Weight,
                Discount = a.Payment!.Discount,
                DiscountWeight = a.Payment!.DiscountWeight
            })
            .OrderBy(a => a.PaymentCreatedAt)
            .ToListAsync();

        return ApiResponse<List<ChargeApplicationRowDto>>.Success(applications);
    }

    // Company.TotalOutstanding is always *today's* running balance, so a statement printed
    // for anything other than the single most recent payment can't just add that one
    // record's amount back onto it - every charge/payment that happened AFTER this record
    // but before now would still be baked in, producing a "거래 전 미수" that never actually
    // existed. This walks the full chronological ledger for the company pair and sums
    // everything strictly before this record instead, giving the true point-in-time balance.
    public async Task<ApiResponse<LedgerBalanceDto>> GetLedgerBeforeAsync(int payableId)
    {
        var anchor = await _payableRepository.GetByIdAsync(payableId);
        if (anchor == null) return ApiResponse<LedgerBalanceDto>.Failure("정산 내역을 찾을 수 없습니다.", 404);

        if (!_currentUserService.IsAdmin)
        {
            var current = await GetCurrentCompanyAsync();
            if (current == null || (current.Value.CompanyId != anchor.LogisticsCompanyId && current.Value.CompanyId != anchor.ManufacturerCompanyId))
            {
                return ApiResponse<LedgerBalanceDto>.Failure("접근 권한이 없습니다.", 403);
            }
        }

        var priorRecords = await _payableRepository.GetQueryable()
            .Where(p => p.LogisticsCompanyId == anchor.LogisticsCompanyId && p.ManufacturerCompanyId == anchor.ManufacturerCompanyId)
            .Where(p => p.CreatedAt < anchor.CreatedAt || (p.CreatedAt == anchor.CreatedAt && p.Id < anchor.Id))
            .OrderBy(p => p.CreatedAt).ThenBy(p => p.Id)
            .ToListAsync();

        // Running balance as of right after the most recent payment (0 if there's never
        // been one), plus whatever charges have arrived since - kept as two separate
        // running totals so a new charge doesn't quietly disappear into "beforeAmount".
        decimal lastPaymentAmount = 0;
        decimal lastPaymentWeight = 0;
        decimal newChargeAmount = 0;
        decimal newChargeWeight = 0;
        decimal runningAmount = 0;
        decimal runningWeight = 0;

        foreach (var r in priorRecords)
        {
            if (r.Type == "CHARGE" && !r.IsCancelled)
            {
                runningAmount += r.Amount;
                runningWeight += r.Weight;
                newChargeAmount += r.Amount;
                newChargeWeight += r.Weight;
            }
            else if (r.Type == "PAYMENT" && !r.IsCancelled)
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

    public async Task<ApiResponse<PagedResult<PayableDto>>> GetPayablesAsync(PayableQueryDto query)
    {
        var current = await GetCurrentCompanyAsync();

        var dbQuery = _payableRepository.GetQueryable()
            .Include(p => p.LogisticsCompany)
            .Include(p => p.ManufacturerCompany)
            .Include(p => p.Order)
            .AsQueryable();

        if (!_currentUserService.IsAdmin)
        {
            if (current == null)
            {
                return ApiResponse<PagedResult<PayableDto>>.Success(new PagedResult<PayableDto> { Items = new(), TotalCount = 0, Page = query.Page, PageSize = query.PageSize });
            }

            dbQuery = current.Value.Category == "DCC"
                ? dbQuery.Where(p => p.LogisticsCompanyId == current.Value.CompanyId)
                : dbQuery.Where(p => p.ManufacturerCompanyId == current.Value.CompanyId);
        }

        if (query.CompanyId.HasValue)
        {
            dbQuery = dbQuery.Where(p => p.ManufacturerCompanyId == query.CompanyId.Value || p.LogisticsCompanyId == query.CompanyId.Value);
        }

        if (!string.IsNullOrEmpty(query.Type))
        {
            dbQuery = dbQuery.Where(p => p.Type == query.Type);
        }

        if (query.StartDate.HasValue)
        {
            dbQuery = dbQuery.Where(p => p.CreatedAt >= query.StartDate.Value);
        }

        if (query.EndDate.HasValue)
        {
            var endExclusive = query.EndDate.Value.Date.AddDays(1);
            dbQuery = dbQuery.Where(p => p.CreatedAt < endExclusive);
        }

        if (!string.IsNullOrEmpty(query.OrderNo))
        {
            // CHARGE rows link to their order directly; PAYMENT rows don't (one payment can
            // cover several orders), so those are matched via their applications' charges.
            dbQuery = dbQuery.Where(p =>
                (p.Order != null && p.Order.OrderNo.Contains(query.OrderNo)) ||
                _dbContext.PayableApplications.Any(a => a.PaymentId == p.Id && a.Charge!.Order != null && a.Charge!.Order.OrderNo.Contains(query.OrderNo)));
        }

        if (!string.IsNullOrEmpty(query.ProductName))
        {
            // Same CHARGE-vs-PAYMENT distinction as OrderNo above - a PAYMENT's own product
            // set is whatever its applications' charges cover, not a direct link.
            dbQuery = dbQuery.Where(p =>
                (p.Order != null && p.Order.OrderItems.Any(oi => oi.ParentId == null &&
                    ((oi.Product != null && oi.Product.Name.Contains(query.ProductName)) ||
                     (oi.ProductSet != null && oi.ProductSet.Title.Contains(query.ProductName))))) ||
                _dbContext.PayableApplications.Any(a => a.PaymentId == p.Id && a.Charge!.Order != null &&
                    a.Charge!.Order.OrderItems.Any(oi => oi.ParentId == null &&
                        ((oi.Product != null && oi.Product.Name.Contains(query.ProductName)) ||
                         (oi.ProductSet != null && oi.ProductSet.Title.Contains(query.ProductName))))));
        }

        var totalCount = await dbQuery.CountAsync();
        var items = await dbQuery
            // A PAYMENT sitting in 미수금 관리 that gets more money merged into it (see
            // ProcessPaymentAsync) never gets a new row - its own Amount just grows in place,
            // dated by whenever it was FIRST created. Sorted purely by CreatedAt, a payment
            // touched again today stays buried under its original (possibly old) date instead
            // of surfacing at the top where "I just paid this" is expected. UpdatedAt (already
            // maintained on every save, see AppDbContext's SaveChanges override) reflects the
            // most recent touch, so sort and the displayed 거래일자 below both use it.
            .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt).ThenByDescending(p => p.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(p => new PayableDto
            {
                Id = p.Id,
                LogisticsCompanyId = p.LogisticsCompanyId,
                LogisticsCompanyName = p.LogisticsCompany != null ? p.LogisticsCompany.Name : null,
                ManufacturerCompanyId = p.ManufacturerCompanyId,
                ManufacturerCompanyName = p.ManufacturerCompany != null ? p.ManufacturerCompany.Name : null,
                OrderId = p.OrderId,
                OrderNo = p.Order != null ? p.Order.OrderNo : null,
                Type = p.Type,
                Amount = p.Amount,
                RemainingAmount = p.RemainingAmount,
                Weight = p.Weight,
                RemainingWeight = p.RemainingWeight,
                Memo = p.Memo,
                SettlementMethod = p.SettlementMethod,
                Discount = p.Discount,
                DiscountWeight = p.DiscountWeight,
                IsCancelled = p.IsCancelled,
                // OrderCount/OutstandingChargeAmount/OutstandingChargeWeight are filled in below,
                // after this page is materialized - a correlated subquery here (even a Distinct
                // one) was observed to mis-translate to SQL and silently drop a charge from the
                // aggregate, so this is computed safely in memory instead. See below.
                IsMostRecentPayment = p.Type == "PAYMENT" && !_dbContext.Payables.Any(other =>
                    other.Type == "PAYMENT" && !other.IsCancelled &&
                    other.LogisticsCompanyId == p.LogisticsCompanyId && other.ManufacturerCompanyId == p.ManufacturerCompanyId &&
                    (other.CreatedAt > p.CreatedAt || (other.CreatedAt == p.CreatedAt && other.Id > p.Id))),
                // 거래일자 shows the effective/last-touched date, not necessarily when this
                // row was first inserted - see the sort comment above for why.
                CreatedAt = p.UpdatedAt ?? p.CreatedAt
            })
            .ToListAsync();

        var paymentIds = items.Where(i => i.Type == "PAYMENT").Select(i => i.Id).ToList();
        if (paymentIds.Count > 0)
        {
            var applicationRows = await _dbContext.PayableApplications
                .Where(a => paymentIds.Contains(a.PaymentId))
                .Select(a => new { a.PaymentId, a.ChargeId, a.Charge!.OrderId, a.Charge!.RemainingAmount, a.Charge!.RemainingWeight })
                .ToListAsync();

            var byPayment = applicationRows.GroupBy(a => a.PaymentId);
            foreach (var group in byPayment)
            {
                // A payment's applications can include more than one row against the SAME
                // charge (e.g. an earlier 0-value acknowledgement plus a later real top-up that
                // merged into this same payment) - group by ChargeId first so each charge's
                // OrderId/RemainingAmount/RemainingWeight is only counted once.
                var distinctCharges = group.GroupBy(a => a.ChargeId).Select(g => g.First()).ToList();
                var dto = items.First(i => i.Id == group.Key);
                dto.OrderCount = distinctCharges.Select(c => c.OrderId).Distinct().Count();
                dto.OutstandingChargeAmount = distinctCharges.Sum(c => c.RemainingAmount);
                dto.OutstandingChargeWeight = distinctCharges.Sum(c => c.RemainingWeight);
            }
        }

        return ApiResponse<PagedResult<PayableDto>>.Success(new PagedResult<PayableDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        });
    }

    // Lists which order(s)/charge(s) a single 정산 처리 (PAYMENT) actually paid off, via the
    // PayableApplication trail recorded at the time it was processed - lets the settlement
    // history table show what a batch payment covered instead of just a bare order count.
    public async Task<ApiResponse<List<PaymentApplicationDetailDto>>> GetPaymentApplicationsAsync(int paymentId)
    {
        var payment = await _payableRepository.GetByIdAsync(paymentId);
        if (payment == null || payment.Type != "PAYMENT") return ApiResponse<List<PaymentApplicationDetailDto>>.Failure("정산 내역을 찾을 수 없습니다.", 404);

        if (!_currentUserService.IsAdmin)
        {
            var current = await GetCurrentCompanyAsync();
            if (current == null || (current.Value.CompanyId != payment.LogisticsCompanyId && current.Value.CompanyId != payment.ManufacturerCompanyId))
            {
                return ApiResponse<List<PaymentApplicationDetailDto>>.Failure("접근 권한이 없습니다.", 403);
            }
        }

        // IgnoreQueryFilters: when a charge this payment originally zero-acknowledged is
        // later actually paid off by a DIFFERENT, later payment, that later payment's own
        // application replaces this one - and per the soft-delete + replace pattern, THIS
        // payment's own application row for that charge gets soft-deleted (not this
        // payment itself, and not the charge). Without lifting the filter here, that
        // deleted row silently drops out of this payment's own historical application
        // list, under-counting 판매(B) and inflating 거래 전 미수(A) for this specific
        // 거래번호 - this is a read of "what did this payment touch when it was made", so
        // the historical row belongs here regardless of what later superseded it.
        var items = await _dbContext.PayableApplications
            .IgnoreQueryFilters()
            .Where(a => a.PaymentId == paymentId && !a.Charge!.IsDeleted)
            .Select(a => new PaymentApplicationDetailDto
            {
                Id = a.Id,
                ChargeId = a.ChargeId,
                OrderId = a.Charge!.OrderId,
                OrderNo = a.Charge!.Order != null ? a.Charge!.Order.OrderNo : null,
                ProductName = a.Charge!.Order != null
                    ? a.Charge!.Order.OrderItems.Where(oi => oi.ParentId == null)
                        .Select(oi => oi.Product != null ? oi.Product.Name : (oi.ProductSet != null ? oi.ProductSet.Title : oi.CustomProductName))
                        .FirstOrDefault()
                    : null,
                ProductPhotoUrl = a.Charge!.Order != null
                    ? a.Charge!.Order.OrderItems.Where(oi => oi.ParentId == null)
                        .Select(oi => oi.Product != null && oi.Product.ProductPhotos.Any() ? oi.Product.ProductPhotos.OrderBy(x => x.SortOrder).First().PhotoUrl
                            : (oi.ProductSet != null && oi.ProductSet.ProductSetPhotos.Any() ? oi.ProductSet.ProductSetPhotos.OrderBy(x => x.SortOrder).First().PhotoUrl : null))
                        .FirstOrDefault()
                    : null,
                ProductItemCount = a.Charge!.Order != null ? a.Charge!.Order.OrderItems.Count(oi => oi.ParentId == null) : 0,
                Items = a.Charge!.Order != null
                    ? a.Charge!.Order.OrderItems.Where(oi => oi.ParentId == null)
                        .Select(oi => new PayableOrderItemSummaryDto
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
                            ActualWeight = oi.ActualWeight ?? oi.RequestedWeight ?? oi.ApprovedWeight,
                            MaterialCost = oi.FactoryInputMaterialCost ?? 0,
                            LaborCost = oi.FactoryInputLaborCost ?? 0
                        }).ToList()
                    : new List<PayableOrderItemSummaryDto>(),
                AppliedAmount = a.AppliedAmount,
                AppliedWeight = a.AppliedWeight,
                ChargeAmount = a.Charge!.Amount,
                ChargeWeight = a.Charge!.Weight,
                ChargeRemainingAmount = a.Charge!.RemainingAmount,
                ChargeRemainingWeight = a.Charge!.RemainingWeight
            })
            .OrderBy(a => a.OrderNo)
            .ToListAsync();

        return ApiResponse<List<PaymentApplicationDetailDto>>.Success(items);
    }

    // Corrects how much of a payment applies to ONE specific charge (e.g. a stone-weight
    // correction that only affects one order within a multi-order payment), independent of
    // the auto-distribution ProcessPaymentAsync/UpdatePaymentAsync use. The charge's
    // RemainingAmount/Weight absorbs the delta directly, and the parent payment's own
    // Amount/Weight is kept in sync so it always equals the sum of its applications -
    // otherwise the payment total would silently drift from what it actually paid.
    public async Task<ApiResponse<bool>> UpdatePaymentApplicationAsync(int applicationId, UpdatePaymentApplicationDto request)
    {
        var application = await _dbContext.PayableApplications
            .Include(a => a.Charge)
            .Include(a => a.Payment)
            .FirstOrDefaultAsync(a => a.Id == applicationId);

        if (application == null || application.Charge == null || application.Payment == null)
        {
            return ApiResponse<bool>.Failure("적용 내역을 찾을 수 없습니다.", 404);
        }

        if (application.Payment.IsCancelled)
        {
            return ApiResponse<bool>.Failure("취소된 거래는 수정할 수 없습니다.", 400);
        }

        if (request.AppliedAmount < 0 || request.AppliedWeight < 0)
        {
            return ApiResponse<bool>.Failure("적용 금액과 중량은 0 이상이어야 합니다.", 400);
        }

        var deltaAmount = request.AppliedAmount - application.AppliedAmount;
        var deltaWeight = request.AppliedWeight - application.AppliedWeight;

        var newChargeRemainingAmount = application.Charge.RemainingAmount - deltaAmount;
        var newChargeRemainingWeight = application.Charge.RemainingWeight - deltaWeight;

        if (newChargeRemainingAmount < 0 || newChargeRemainingWeight < 0)
        {
            return ApiResponse<bool>.Failure("적용 금액이 해당 주문의 청구 금액을 초과할 수 없습니다.", 400);
        }

        application.Charge.RemainingAmount = newChargeRemainingAmount;
        application.Charge.RemainingWeight = newChargeRemainingWeight;

        application.Payment.Amount += deltaAmount;
        application.Payment.Weight += deltaWeight;

        application.AppliedAmount = request.AppliedAmount;
        application.AppliedWeight = request.AppliedWeight;

        await _payableRepository.SaveChangesAsync();
        return ApiResponse<bool>.Success(true);
    }

    // Cash and gold weight are two views of the same underlying charge, not independent
    // debts - once a payment fully covers either side of a charge, the whole charge is
    // considered settled and the other side is cleared too. Whatever was still outstanding
    // on that other side at that moment is added to forgivenAmount/forgivenWeight so the
    // caller can fold it into the payment's own Discount/DiscountWeight - without that, the
    // charge's real debt silently disappears from RemainingAmount/RemainingWeight while the
    // payment's own recorded Amount/Discount stays unchanged, permanently corrupting any
    // later chronological ledger walk (거래명세서's A/B totals, which trust Payment.Amount+
    // Discount to know how much debt a payment actually cleared) with debt that no longer
    // exists anywhere in the live charge data.
    // forgivenAmountByCharge/forgivenWeightByCharge accumulate PER CHARGE (not a single pooled
    // total) - see the long comment at ProcessPaymentAsync's cashPortion/discountPortion
    // computation for why a pooled total silently lost money when this method is called
    // against several charges and only one of them triggers the either-side-clears-it rule.
    private static List<PayableApplication> ApplyToChargeList(List<Payable> charges, ref decimal remainingAmount, ref decimal remainingWeight, Dictionary<int, decimal> forgivenAmountByCharge, Dictionary<int, decimal> forgivenWeightByCharge)
    {
        var applications = new List<PayableApplication>();

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
                applications.Add(new PayableApplication { ChargeId = charge.Id, AppliedAmount = appliedAmount, AppliedWeight = appliedWeight });
            }
        }

        return applications;
    }

    // Restores exactly the charges (and exact amounts) this payment's own PayableApplication
    // records show it paid off - NOT a generic oldest-charge-first/capacity walk. A payment can
    // target one or several specific orders out of chronological order (see ProcessPaymentAsync's
    // OrderIds/OrderId branches), so when a later edit/cancel needs to undo it, guessing which
    // charge to refund by date/capacity can restore the wrong charge (leaving the one this
    // payment actually paid still marked paid, while an unrelated earlier charge gets
    // incorrectly reopened). Using the real application records removes the guessing entirely.
    private static void ReverseApplications(List<Payable> charges, List<PayableApplication> applications)
    {
        var chargeMap = charges.ToDictionary(c => c.Id);
        foreach (var application in applications)
        {
            if (!chargeMap.TryGetValue(application.ChargeId, out var charge)) continue;
            charge.RemainingAmount = Math.Min(charge.Amount, charge.RemainingAmount + application.AppliedAmount);
            charge.RemainingWeight = Math.Min(charge.Weight, charge.RemainingWeight + application.AppliedWeight);
        }
    }

    public async Task<ApiResponse<bool>> ProcessPaymentAsync(CreatePaymentDto request)
    {
        var current = await GetCurrentCompanyAsync();
        int logisticsCompanyId;
        int manufacturerCompanyId;

        if (_currentUserService.IsAdmin)
        {
            manufacturerCompanyId = request.CompanyId;
            logisticsCompanyId = await _payableRepository.GetQueryable()
                .Where(p => p.ManufacturerCompanyId == request.CompanyId)
                .Select(p => p.LogisticsCompanyId)
                .FirstOrDefaultAsync();
            if (logisticsCompanyId == 0) logisticsCompanyId = request.CompanyId;
        }
        else if (current != null && current.Value.Category == "MFG")
        {
            manufacturerCompanyId = current.Value.CompanyId;
            logisticsCompanyId = request.CompanyId;
        }
        else if (current != null && current.Value.Category == "DCC")
        {
            logisticsCompanyId = current.Value.CompanyId;
            manufacturerCompanyId = request.CompanyId;
        }
        else
        {
            logisticsCompanyId = current?.CompanyId ?? 0;
            manufacturerCompanyId = request.CompanyId;
        }

        var discount = request.Discount ?? 0m;
        var discountWeight = request.DiscountWeight ?? 0m;

        decimal remainingAmountToApply = request.Amount + discount;
        decimal remainingWeightToApply = (request.Weight ?? 0m) + discountWeight;
        var applications = new List<PayableApplication>();

        var hasTargetOrders = (request.OrderIds != null && request.OrderIds.Count > 0) || request.OrderId.HasValue;
        var isAllZero = remainingAmountToApply <= 0 && remainingWeightToApply <= 0;

        // A 0/0/0/0 submission against specifically selected orders means nothing was
        // actually collected and nothing is being forgiven - RemainingAmount/RemainingWeight
        // must stay exactly as they were (still fully owed, so 미수금 관리 keeps showing the
        // real balance). But the action still needs a durable trace, or 정산 내역's "never
        // touched" worklist would show this charge forever even after it was reviewed.
        // Record a real (zero-value) payment + application so GetPayableOrderHistoryAsync's
        // worklist filter (which excludes any charge with an application on file) drops it.
        // Mirrors ReceivableService.ProcessDepositAsync's identical convention.
        if (isAllZero && hasTargetOrders)
        {
            List<Payable> targetCharges;
            if (request.OrderIds != null && request.OrderIds.Count > 0)
            {
                targetCharges = await _payableRepository.GetQueryable()
                    .Where(p => p.LogisticsCompanyId == logisticsCompanyId && p.ManufacturerCompanyId == manufacturerCompanyId
                        && p.OrderId.HasValue && request.OrderIds.Contains(p.OrderId.Value) && p.Type == "CHARGE" && (p.RemainingAmount > 0 || p.RemainingWeight > 0))
                    .ToListAsync();
            }
            else
            {
                targetCharges = await _payableRepository.GetQueryable()
                    .Where(p => p.LogisticsCompanyId == logisticsCompanyId && p.ManufacturerCompanyId == manufacturerCompanyId
                        && p.OrderId == request.OrderId!.Value && p.Type == "CHARGE" && (p.RemainingAmount > 0 || p.RemainingWeight > 0))
                    .ToListAsync();
            }

            if (targetCharges.Count > 0)
            {
                var zeroPayment = new Payable
                {
                    LogisticsCompanyId = logisticsCompanyId,
                    ManufacturerCompanyId = manufacturerCompanyId,
                    OrderId = request.OrderId,
                    Type = "PAYMENT",
                    Memo = request.Memo,
                    SettlementMethod = request.SettlementMethod
                };
                await _payableRepository.AddAsync(zeroPayment);
                await _payableRepository.SaveChangesAsync();

                foreach (var charge in targetCharges)
                {
                    _dbContext.PayableApplications.Add(new PayableApplication
                    {
                        PaymentId = zeroPayment.Id,
                        ChargeId = charge.Id,
                        AppliedAmount = 0,
                        AppliedWeight = 0
                    });
                }
                await _payableRepository.SaveChangesAsync();
            }

            return ApiResponse<bool>.Success(true);
        }

        var forgivenAmountByCharge = new Dictionary<int, decimal>();
        var forgivenWeightByCharge = new Dictionary<int, decimal>();

        if (request.OrderIds != null && request.OrderIds.Count > 0)
        {
            // Pays each selected charge off in full, oldest first, instead of prorating a
            // partial payment thinly across every selected order - a partial batch payment
            // now fully closes as many of the selected orders as it can afford, leaving the
            // rest untouched for next time, rather than leaving every selected order sitting
            // at a small remaining balance.
            var targetCharges = await _payableRepository.GetQueryable()
                .Where(p => p.LogisticsCompanyId == logisticsCompanyId && p.ManufacturerCompanyId == manufacturerCompanyId
                    && p.OrderId.HasValue && request.OrderIds.Contains(p.OrderId.Value) && p.Type == "CHARGE" && (p.RemainingAmount > 0 || p.RemainingWeight > 0))
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();
            applications.AddRange(ApplyToChargeList(targetCharges, ref remainingAmountToApply, ref remainingWeightToApply, forgivenAmountByCharge, forgivenWeightByCharge));

            // A charge the payment pool ran out before reaching (ApplyToChargeList pays oldest
            // first) still has to be acknowledged - the user explicitly picked it for THIS batch,
            // so leaving it with no application at all strands it in the untouched 정산처리
            // worklist looking like the action never happened to it. A 0-value application (same
            // mechanism as an all-zero 정산처리) moves it into 미수금 관리 instead, still owing
            // its full remaining amount.
            var touchedChargeIds = applications.Select(a => a.ChargeId).ToHashSet();
            foreach (var charge in targetCharges)
            {
                if (!touchedChargeIds.Contains(charge.Id))
                {
                    applications.Add(new PayableApplication { ChargeId = charge.Id, AppliedAmount = 0, AppliedWeight = 0 });
                }
            }
        }
        else if (request.OrderId.HasValue)
        {
            var targetCharges = await _payableRepository.GetQueryable()
                .Where(p => p.LogisticsCompanyId == logisticsCompanyId && p.ManufacturerCompanyId == manufacturerCompanyId
                    && p.OrderId == request.OrderId.Value && p.Type == "CHARGE" && (p.RemainingAmount > 0 || p.RemainingWeight > 0))
                .ToListAsync();
            applications.AddRange(ApplyToChargeList(targetCharges, ref remainingAmountToApply, ref remainingWeightToApply, forgivenAmountByCharge, forgivenWeightByCharge));
        }

        if (remainingAmountToApply > 0 || remainingWeightToApply > 0)
        {
            var outstandingCharges = await _payableRepository.GetQueryable()
                .Where(p => p.LogisticsCompanyId == logisticsCompanyId && p.ManufacturerCompanyId == manufacturerCompanyId
                    && p.Type == "CHARGE" && (p.RemainingAmount > 0 || p.RemainingWeight > 0))
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();
            applications.AddRange(ApplyToChargeList(outstandingCharges, ref remainingAmountToApply, ref remainingWeightToApply, forgivenAmountByCharge, forgivenWeightByCharge));
        }

        // The targeted-charges phase and the outstanding-fallback phase above can legitimately
        // touch the SAME charge twice in one request - e.g. the targeted phase fully closes a
        // charge's cash side but leaves its weight side positive, so that same charge still
        // qualifies as "outstanding" for the fallback phase too. Left unmerged, this produces
        // two separate PayableApplication rows for one charge from a single payment, so the
        // payment's own Amount silently disagrees with the sum of what its applications say it
        // paid. Consolidate by ChargeId before anything below treats this list as "one entry
        // per charge touched."
        applications = applications
            .GroupBy(a => a.ChargeId)
            .Select(g => new PayableApplication
            {
                ChargeId = g.Key,
                AppliedAmount = g.Sum(a => a.AppliedAmount),
                AppliedWeight = g.Sum(a => a.AppliedWeight)
            })
            .ToList();

        // Fold everything the either-side-clears-the-whole-charge rule forgave into the
        // payment's own discount, so it's honestly recorded rather than only ever showing up
        // as a live RemainingAmount/RemainingWeight change with no paper trail.
        var forgivenAmount = forgivenAmountByCharge.Values.Sum();
        var forgivenWeight = forgivenWeightByCharge.Values.Sum();
        var manualDiscount = discount;
        var manualDiscountWeight = discountWeight;
        discount += forgivenAmount;
        discountWeight += forgivenWeight;

        // A charge that was already partially settled before (i.e. it's sitting in 미수금
        // 관리, not the fresh 정산처리 worklist) already has its own running payment record -
        // route any further money toward it into THAT SAME payment/거래번호 instead of opening
        // a new one, so 거래별 보기 keeps exactly one row per order as it accumulates payments
        // over time. Only a charge that has never been touched before gets a brand new
        // payment record.
        var chargeIds = applications.Select(a => a.ChargeId).Distinct().ToList();
        var priorApplications = chargeIds.Count > 0
            ? await _dbContext.PayableApplications
                .Include(a => a.Payment)
                .Where(a => chargeIds.Contains(a.ChargeId) && a.Payment != null && a.Payment.Type == "PAYMENT" && !a.Payment.IsCancelled)
                .ToListAsync()
            : new List<PayableApplication>();
        var existingApplicationByCharge = priorApplications
            .GroupBy(a => a.ChargeId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(a => a.Payment!.CreatedAt).First());

        var totalPool = request.Amount + discount;
        var totalPoolWeight = (request.Weight ?? 0m) + discountWeight;
        var cashRatio = totalPool > 0 ? request.Amount / totalPool : 0m;
        var cashRatioWeight = totalPoolWeight > 0 ? (request.Weight ?? 0m) / totalPoolWeight : 0m;

        // application.AppliedAmount/AppliedWeight is ALREADY pure real cash - the exact amount
        // ApplyToChargeList matched from the payer's own pool, never inflated by forgiveness.
        // Multiplying it by a pool-wide cashRatio (as this used to do) double-counts: it shrinks
        // the recorded cash by the SAME forgiven fraction on every charge, including ones that
        // were never forgiven at all, so Payment.Amount+Discount ends up smaller than what
        // actually closed the charges - money silently vanished from the running ledger
        // (GetLedgerBeforeAsync) even though every charge's own RemainingAmount was correctly 0.
        // Attribute cash 1:1, spread only the user's OWN manual discount proportionally by cash
        // share, and add each charge's own forgiven amount directly - never redistributed to a
        // charge that wasn't the one actually forgiven.
        var totalAppliedAmount = applications.Sum(a => a.AppliedAmount);
        var totalAppliedWeight = applications.Sum(a => a.AppliedWeight);
        var manualDiscountRatio = totalAppliedAmount > 0 ? manualDiscount / totalAppliedAmount : 0m;
        var manualDiscountRatioWeight = totalAppliedWeight > 0 ? manualDiscountWeight / totalAppliedWeight : 0m;

        Payable? newPayment = null;
        var newApplications = new List<PayableApplication>();

        foreach (var application in applications)
        {
            var cashPortion = application.AppliedAmount;
            var discountPortion = application.AppliedAmount * manualDiscountRatio + forgivenAmountByCharge.GetValueOrDefault(application.ChargeId);
            var cashPortionWeight = application.AppliedWeight;
            var discountPortionWeight = application.AppliedWeight * manualDiscountRatioWeight + forgivenWeightByCharge.GetValueOrDefault(application.ChargeId);

            if (existingApplicationByCharge.TryGetValue(application.ChargeId, out var existingApplication))
            {
                var existingPayment = existingApplication.Payment!;
                existingPayment.Amount += cashPortion;
                existingPayment.Discount += discountPortion;
                existingPayment.Weight += cashPortionWeight;
                existingPayment.DiscountWeight += discountPortionWeight;
                existingApplication.AppliedAmount += application.AppliedAmount;
                existingApplication.AppliedWeight += application.AppliedWeight;
            }
            else
            {
                newPayment ??= new Payable
                {
                    LogisticsCompanyId = logisticsCompanyId,
                    ManufacturerCompanyId = manufacturerCompanyId,
                    OrderId = request.OrderId,
                    Type = "PAYMENT",
                    Memo = request.Memo,
                    SettlementMethod = request.SettlementMethod
                };
                newPayment.Amount += cashPortion;
                newPayment.Discount += discountPortion;
                newPayment.Weight += cashPortionWeight;
                newPayment.DiscountWeight += discountPortionWeight;
                newApplications.Add(application);
            }
        }

        // Anything left after covering every outstanding charge is an overpayment - keep it
        // on a payment record instead of discarding it, so it stays visible/traceable rather
        // than silently vanishing, even if every charge this action touched was a top-up (in
        // which case this shell would otherwise carry no Amount/Discount at all, since the
        // loop above only ever credits a payment for the portion of an application it
        // received - the leftover was never part of any application to begin with).
        if (remainingAmountToApply > 0 || remainingWeightToApply > 0)
        {
            newPayment ??= new Payable
            {
                LogisticsCompanyId = logisticsCompanyId,
                ManufacturerCompanyId = manufacturerCompanyId,
                OrderId = request.OrderId,
                Type = "PAYMENT",
                Memo = request.Memo,
                SettlementMethod = request.SettlementMethod
            };
            var leftoverCash = remainingAmountToApply * cashRatio;
            var leftoverCashWeight = remainingWeightToApply * cashRatioWeight;
            newPayment.Amount += leftoverCash;
            newPayment.Discount += remainingAmountToApply - leftoverCash;
            newPayment.Weight += leftoverCashWeight;
            newPayment.DiscountWeight += remainingWeightToApply - leftoverCashWeight;
        }

        if (newPayment != null)
        {
            newPayment.RemainingAmount = remainingAmountToApply;
            newPayment.RemainingWeight = remainingWeightToApply;
            await _payableRepository.AddAsync(newPayment);
            foreach (var application in newApplications)
            {
                application.Payment = newPayment;
                _dbContext.PayableApplications.Add(application);
            }
        }

        await _payableRepository.SaveChangesAsync();

        return ApiResponse<bool>.Success(true);
    }

    public async Task<ApiResponse<bool>> UpdatePaymentAsync(int id, UpdatePaymentDto request)
    {
        var payment = await _payableRepository.GetByIdAsync(id);
        if (payment == null || payment.Type != "PAYMENT") return ApiResponse<bool>.Failure("정산 내역을 찾을 수 없습니다.", 404);
        if (payment.IsCancelled) return ApiResponse<bool>.Failure("취소된 거래는 수정할 수 없습니다.", 400);

        var allCharges = await _payableRepository.GetQueryable()
            .Where(p => p.LogisticsCompanyId == payment.LogisticsCompanyId && p.ManufacturerCompanyId == payment.ManufacturerCompanyId && p.Type == "CHARGE")
            .OrderBy(p => p.CreatedAt)
            .ToListAsync();

        var oldApplications = await _dbContext.PayableApplications.Where(a => a.PaymentId == payment.Id).ToListAsync();
        ReverseApplications(allCharges, oldApplications);
        _dbContext.PayableApplications.RemoveRange(oldApplications);

        var newDiscount = request.Discount ?? 0m;
        var newDiscountWeight = request.DiscountWeight ?? 0m;
        payment.Amount = request.Amount;
        payment.Weight = request.Weight ?? 0m;
        payment.Discount = newDiscount;
        payment.DiscountWeight = newDiscountWeight;
        payment.Memo = request.Memo;
        payment.SettlementMethod = request.SettlementMethod;

        decimal newAmountToApply = request.Amount + newDiscount;
        decimal newWeightToApply = (request.Weight ?? 0m) + newDiscountWeight;
        var stillOutstanding = allCharges.Where(c => c.RemainingAmount > 0 || c.RemainingWeight > 0).ToList();
        var editForgivenAmountByCharge = new Dictionary<int, decimal>();
        var editForgivenWeightByCharge = new Dictionary<int, decimal>();
        var newApplications = ApplyToChargeList(stillOutstanding, ref newAmountToApply, ref newWeightToApply, editForgivenAmountByCharge, editForgivenWeightByCharge);
        // A single payment being edited has nowhere else to attribute a forgiven amount to -
        // unlike ProcessPaymentAsync's merge-across-multiple-destination-payments case, there's
        // only this one payment record, so folding the aggregate straight into its own discount
        // is exact, not an approximation.
        payment.Discount += editForgivenAmountByCharge.Values.Sum();
        payment.DiscountWeight += editForgivenWeightByCharge.Values.Sum();
        foreach (var application in newApplications)
        {
            application.Payment = payment;
            _dbContext.PayableApplications.Add(application);
        }

        payment.RemainingAmount = newAmountToApply;
        payment.RemainingWeight = newWeightToApply;

        await _payableRepository.SaveChangesAsync();

        return ApiResponse<bool>.Success(true);
    }

    public async Task<ApiResponse<bool>> CancelPaymentAsync(int id)
    {
        var payment = await _payableRepository.GetByIdAsync(id);
        if (payment == null || payment.Type != "PAYMENT") return ApiResponse<bool>.Failure("정산 내역을 찾을 수 없습니다.", 404);
        if (payment.IsCancelled) return ApiResponse<bool>.Failure("이미 취소된 거래입니다.", 400);

        var allCharges = await _payableRepository.GetQueryable()
            .Where(p => p.LogisticsCompanyId == payment.LogisticsCompanyId && p.ManufacturerCompanyId == payment.ManufacturerCompanyId && p.Type == "CHARGE")
            .OrderBy(p => p.CreatedAt)
            .ToListAsync();

        var oldApplications = await _dbContext.PayableApplications.Where(a => a.PaymentId == payment.Id).ToListAsync();
        ReverseApplications(allCharges, oldApplications);
        _dbContext.PayableApplications.RemoveRange(oldApplications);

        payment.IsCancelled = true;
        payment.RemainingAmount = 0;
        payment.RemainingWeight = 0;

        await _payableRepository.SaveChangesAsync();
        return ApiResponse<bool>.Success(true);
    }

    // Cancelling a payment already reverses its effect on the charges and clears its
    // applications (CancelPaymentAsync above) - deleting it afterward is purely removing
    // the now-inert record from the list, so it's normally only allowed once a payment is
    // already cancelled, never on a live one still backing real settlement amounts. The one
    // exception is a "hollow" payment that never touched any charge and carries no amount at
    // all (a leftover-overpay shell from before this was fixed to always carry its own
    // Amount/Discount) - there's nothing there to reverse, so it's safe to delete directly.
    public async Task<ApiResponse<bool>> DeletePaymentAsync(int id)
    {
        var payment = await _payableRepository.GetByIdAsync(id);
        if (payment == null || payment.Type != "PAYMENT") return ApiResponse<bool>.Failure("정산 내역을 찾을 수 없습니다.", 404);

        var remainingApplications = await _dbContext.PayableApplications.Where(a => a.PaymentId == id).ToListAsync();
        var isHollow = remainingApplications.Count == 0 && payment.Amount == 0 && payment.Discount == 0 && payment.Weight == 0 && payment.DiscountWeight == 0;
        if (!payment.IsCancelled && !isHollow) return ApiResponse<bool>.Failure("취소된 거래만 삭제할 수 있습니다.", 400);

        if (remainingApplications.Count > 0)
        {
            _dbContext.PayableApplications.RemoveRange(remainingApplications);
        }

        _dbContext.Payables.Remove(payment);
        await _payableRepository.SaveChangesAsync();
        return ApiResponse<bool>.Success(true);
    }

    // MFG-side confirmation that they've reviewed/finalized a charge. This is a
    // bookkeeping marker only - it does not move money or touch RemainingAmount.
    // DCC's actual payment (ProcessPaymentAsync) remains the only thing that
    // reduces the outstanding balance.
    public async Task<ApiResponse<bool>> MarkMfgProcessedAsync(int payableId, MfgProcessDto request)
    {
        var charge = await _payableRepository.GetByIdAsync(payableId);
        if (charge == null || charge.Type != "CHARGE") return ApiResponse<bool>.Failure("정산 내역을 찾을 수 없습니다.", 404);

        if (!_currentUserService.IsAdmin)
        {
            var current = await GetCurrentCompanyAsync();
            if (current == null || current.Value.Category != "MFG" || current.Value.CompanyId != charge.ManufacturerCompanyId)
            {
                return ApiResponse<bool>.Failure("공장 업체만 이 항목을 정산 처리할 수 있습니다.", 403);
            }
        }

        charge.IsMfgProcessed = true;
        charge.MfgProcessedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(request.Memo))
        {
            charge.Memo = request.Memo;
        }

        await _payableRepository.SaveChangesAsync();
        return ApiResponse<bool>.Success(true);
    }
}

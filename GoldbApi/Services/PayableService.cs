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

    Task<ApiResponse<PayableOrderHistorySummaryDto>> GetPayableOrderHistorySummaryAsync(PayableQueryDto query);

    Task<ApiResponse<PagedResult<PayableDto>>> GetPayablesAsync(PayableQueryDto query);

    Task<ApiResponse<bool>> ProcessPaymentAsync(CreatePaymentDto request);

    Task<ApiResponse<bool>> UpdatePaymentAsync(int id, UpdatePaymentDto request);

    Task<ApiResponse<bool>> CancelPaymentAsync(int id);
}

public class PayableService : IPayableService
{
    private readonly IRepository<Payable> _payableRepository;
    private readonly IRepository<Company> _companyRepository;
    private readonly IRepository<UserCompany> _userCompanyRepository;
    private readonly ICurrentUserService _currentUserService;

    public PayableService(
        IRepository<Payable> payableRepository,
        IRepository<Company> companyRepository,
        IRepository<UserCompany> userCompanyRepository,
        ICurrentUserService currentUserService)
    {
        _payableRepository = payableRepository;
        _companyRepository = companyRepository;
        _userCompanyRepository = userCompanyRepository;
        _currentUserService = currentUserService;
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
            .OrderBy(c => c.Name)
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
            var totalPaid = pairPayables.Where(p => p.Type == "PAYMENT" && !p.IsCancelled).Sum(p => p.Amount);
            var totalOutstanding = pairPayables.Where(p => p.Type == "CHARGE").Sum(p => p.RemainingAmount);

            var totalChargeWeight = pairPayables.Where(p => p.Type == "CHARGE").Sum(p => p.Weight);
            var totalPaidWeight = pairPayables.Where(p => p.Type == "PAYMENT" && !p.IsCancelled).Sum(p => p.Weight);
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
        var totalPaid = pairPayables.Where(p => p.Type == "PAYMENT" && !p.IsCancelled).Sum(p => p.Amount);
        var totalOutstanding = pairPayables.Where(p => p.Type == "CHARGE").Sum(p => p.RemainingAmount);

        var totalChargeWeight = pairPayables.Where(p => p.Type == "CHARGE").Sum(p => p.Weight);
        var totalPaidWeight = pairPayables.Where(p => p.Type == "PAYMENT" && !p.IsCancelled).Sum(p => p.Weight);
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

        return dbQuery;
    }

    public async Task<ApiResponse<PayableOrderHistorySummaryDto>> GetPayableOrderHistorySummaryAsync(PayableQueryDto query)
    {
        var dbQuery = await BuildOrderHistoryQueryAsync(query);

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

    public async Task<ApiResponse<PagedResult<PayableOrderRowDto>>> GetPayableOrderHistoryAsync(PayableQueryDto query)
    {
        var dbQuery = await BuildOrderHistoryQueryAsync(query);

        var totalCount = await dbQuery.CountAsync();
        var items = await dbQuery
            .OrderByDescending(p => p.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(p => new PayableOrderRowDto
            {
                PayableId = p.Id,
                OrderId = p.OrderId,
                OrderNo = p.Order != null ? p.Order.OrderNo : null,
                ProductName = p.Order != null
                    ? p.Order.OrderItems.Where(oi => oi.ParentId == null)
                        .Select(oi => oi.Product != null ? oi.Product.Name : (oi.ProductSet != null ? oi.ProductSet.Title : null))
                        .FirstOrDefault()
                    : null,
                ProductPhotoUrl = p.Order != null
                    ? p.Order.OrderItems.Where(oi => oi.ParentId == null)
                        .Select(oi => oi.Product != null && oi.Product.ProductPhotos.Any() ? oi.Product.ProductPhotos.OrderBy(x => x.SortOrder).First().PhotoUrl
                            : (oi.ProductSet != null && oi.ProductSet.ProductSetPhotos.Any() ? oi.ProductSet.ProductSetPhotos.OrderBy(x => x.SortOrder).First().PhotoUrl : null))
                        .FirstOrDefault()
                    : null,
                ProductItemCount = p.Order != null ? p.Order.OrderItems.Count(oi => oi.ParentId == null) : 0,
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

        var totalCount = await dbQuery.CountAsync();
        var items = await dbQuery
            .OrderByDescending(p => p.CreatedAt)
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
                IsCancelled = p.IsCancelled,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<PagedResult<PayableDto>>.Success(new PagedResult<PayableDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        });
    }

    // Cash and gold weight are two views of the same underlying charge, not independent
    // debts - once a payment fully covers either side of a charge, the whole charge is
    // considered settled and the other side is cleared too.
    private static void ApplyToChargeList(List<Payable> charges, ref decimal remainingAmount, ref decimal remainingWeight)
    {
        foreach (var charge in charges)
        {
            if (remainingAmount <= 0 && remainingWeight <= 0) break;

            bool touchedAmount = false;
            bool touchedWeight = false;

            if (remainingAmount > 0 && charge.RemainingAmount > 0)
            {
                touchedAmount = true;
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
        }
    }

    private static void ReverseFromChargeList(List<Payable> charges, ref decimal remainingAmount, ref decimal remainingWeight)
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

        if (request.Amount <= 0 && discount <= 0 && (!request.Weight.HasValue || request.Weight.Value <= 0))
        {
            return ApiResponse<bool>.Failure("결제액, 할인액 또는 순금 중량 중 하나는 0보다 커야 합니다.", 400);
        }

        var payment = new Payable
        {
            LogisticsCompanyId = logisticsCompanyId,
            ManufacturerCompanyId = manufacturerCompanyId,
            OrderId = request.OrderId,
            Type = "PAYMENT",
            Amount = request.Amount,
            RemainingAmount = 0,
            Weight = request.Weight ?? 0m,
            RemainingWeight = 0,
            Memo = request.Memo,
            SettlementMethod = request.SettlementMethod,
            Discount = discount
        };
        await _payableRepository.AddAsync(payment);

        decimal remainingAmountToApply = request.Amount + discount;
        decimal remainingWeightToApply = request.Weight ?? 0m;

        if (request.OrderId.HasValue)
        {
            var targetCharges = await _payableRepository.GetQueryable()
                .Where(p => p.LogisticsCompanyId == logisticsCompanyId && p.ManufacturerCompanyId == manufacturerCompanyId
                    && p.OrderId == request.OrderId.Value && p.Type == "CHARGE" && (p.RemainingAmount > 0 || p.RemainingWeight > 0))
                .ToListAsync();
            ApplyToChargeList(targetCharges, ref remainingAmountToApply, ref remainingWeightToApply);
        }

        if (remainingAmountToApply > 0 || remainingWeightToApply > 0)
        {
            var outstandingCharges = await _payableRepository.GetQueryable()
                .Where(p => p.LogisticsCompanyId == logisticsCompanyId && p.ManufacturerCompanyId == manufacturerCompanyId
                    && p.Type == "CHARGE" && (p.RemainingAmount > 0 || p.RemainingWeight > 0))
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();
            ApplyToChargeList(outstandingCharges, ref remainingAmountToApply, ref remainingWeightToApply);
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

        decimal oldAmount = payment.Amount + payment.Discount;
        decimal oldWeight = payment.Weight;
        ReverseFromChargeList(allCharges, ref oldAmount, ref oldWeight);

        var newDiscount = request.Discount ?? 0m;
        payment.Amount = request.Amount;
        payment.Weight = request.Weight ?? 0m;
        payment.Discount = newDiscount;
        payment.Memo = request.Memo;
        payment.SettlementMethod = request.SettlementMethod;

        decimal newAmountToApply = request.Amount + newDiscount;
        decimal newWeightToApply = request.Weight ?? 0m;
        var stillOutstanding = allCharges.Where(c => c.RemainingAmount > 0 || c.RemainingWeight > 0).ToList();
        ApplyToChargeList(stillOutstanding, ref newAmountToApply, ref newWeightToApply);

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

        decimal amountToRestore = payment.Amount + payment.Discount;
        decimal weightToRestore = payment.Weight;
        ReverseFromChargeList(allCharges, ref amountToRestore, ref weightToRestore);

        payment.IsCancelled = true;

        await _payableRepository.SaveChangesAsync();
        return ApiResponse<bool>.Success(true);
    }
}

using GoldbApi.Data;
using GoldbApi.DTOs;
using GoldbApi.Models;
using GoldbApi.Utils;
using Microsoft.EntityFrameworkCore;

namespace GoldbApi.Repositories;

public class ReceivableRepository : RepositoryBase<Receivable>, IReceivableRepository
{
    public ReceivableRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<User?> GetUserWithCompaniesAsync(int userId)
    {
        return await Context.Users
            .Include(u => u.UserCompanies)
                .ThenInclude(uc => uc.Company)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<List<Company>> GetManagedRetailersAsync(int logisticsCompanyId)
    {
        return await Context.Companies
            .Where(c => c.LogisticsCompanyId == logisticsCompanyId && c.Category == "RTL")
            .ToListAsync();
    }

    public async Task<List<Receivable>> GetReceivablesForLogisticsAsync(List<int> retailerIds)
    {
        return await Context.Receivables
            .Include(r => r.User)
                .ThenInclude(u => u!.UserCompanies)
            .Where(r => r.User != null && r.User.UserCompanies != null && r.User.UserCompanies.Any(uc => retailerIds.Contains(uc.CompanyId)))
            .ToListAsync();
    }

    public async Task<List<int>> GetUserIdsWithReceivablesAsync()
    {
        return await Context.Receivables
            .Select(r => r.UserId)
            .Distinct()
            .ToListAsync();
    }

    public async Task<int> GetUsersWithReceivablesCountAsync(string? search, List<int> userIdsWithReceivables, string? companyCategory = null, List<int>? allowedCompanyIds = null)
    {
        var query = Context.Users
            .Include(u => u.UserCompanies).ThenInclude(uc => uc.Company)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u => u.Username.Contains(search) || u.Name.Contains(search));
        }

        if (!string.IsNullOrEmpty(companyCategory))
        {
            query = query.Where(u => u.UserCompanies.Any(uc => uc.Company != null && uc.Company.Category == companyCategory));
        }

        if (allowedCompanyIds != null)
        {
            query = query.Where(u => u.UserCompanies.Any(uc => allowedCompanyIds.Contains(uc.CompanyId)));
        }

        query = query.Where(u => userIdsWithReceivables.Contains(u.Id));
        return await query.CountAsync();
    }

    public async Task<List<User>> GetUsersWithReceivablesPagedAsync(int page, int pageSize, string? search, List<int> userIdsWithReceivables, string? companyCategory = null, List<int>? allowedCompanyIds = null)
    {
        var query = Context.Users
            .Include(u => u.UserCompanies).ThenInclude(uc => uc.Company)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u => u.Username.Contains(search) || u.Name.Contains(search));
        }

        if (!string.IsNullOrEmpty(companyCategory))
        {
            query = query.Where(u => u.UserCompanies.Any(uc => uc.Company != null && uc.Company.Category == companyCategory));
        }

        if (allowedCompanyIds != null)
        {
            query = query.Where(u => u.UserCompanies.Any(uc => allowedCompanyIds.Contains(uc.CompanyId)));
        }

        query = query.Where(u => userIdsWithReceivables.Contains(u.Id));

        return await query
            .OrderBy(u => u.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<Receivable>> GetReceivablesForUserAsync(int userId)
    {
        return await Context.Receivables
            .Where(r => r.UserId == userId)
            .ToListAsync();
    }

    public async Task<(List<ReceivableDto> Items, int TotalCount)> GetReceivablesPagedAsync(ReceivableQueryDto query, List<int>? allowedUserIds = null)
    {
        var dbQuery = DbSet
            .Include(r => r.User)
            .Include(r => r.Order)
            .AsQueryable();

        if (allowedUserIds != null)
            dbQuery = dbQuery.Where(r => allowedUserIds.Contains(r.UserId));

        if (query.UserId.HasValue)
            dbQuery = dbQuery.Where(r => r.UserId == query.UserId.Value);

        if (!string.IsNullOrEmpty(query.Type))
            dbQuery = dbQuery.Where(r => r.Type == query.Type);

        if (query.StartDate.HasValue)
        {
            var startDateUtc = DateRangeUtils.ToUtcRangeStart(query.StartDate.Value);
            dbQuery = dbQuery.Where(r => r.CreatedAt >= startDateUtc);
        }

        if (query.EndDate.HasValue)
        {
            var nextDayUtc = DateRangeUtils.ToUtcRangeEndExclusive(query.EndDate.Value);
            dbQuery = dbQuery.Where(r => r.CreatedAt < nextDayUtc);
        }

        if (!string.IsNullOrEmpty(query.OrderNo))
        {
            dbQuery = dbQuery.Where(r => r.Order != null && r.Order.OrderNo.Contains(query.OrderNo));
        }

        if (!string.IsNullOrEmpty(query.ProductNo))
        {
            dbQuery = dbQuery.Where(r => r.Order != null && r.Order.OrderItems.Any(oi => oi.Product != null && oi.Product.ProductNo != null && oi.Product.ProductNo.Contains(query.ProductNo)));
        }

        if (!string.IsNullOrEmpty(query.ProductName))
        {
            dbQuery = dbQuery.Where(r => r.Order != null && r.Order.OrderItems.Any(oi => 
                (oi.Product != null && oi.Product.Name.Contains(query.ProductName)) || 
                (oi.ProductSet != null && oi.ProductSet.Title.Contains(query.ProductName))));
        }

        if (query.ManufacturerId.HasValue)
        {
            dbQuery = dbQuery.Where(r => r.Order != null && r.Order.OrderItems.Any(oi => oi.Product != null && oi.Product.CompanyId == query.ManufacturerId.Value));
        }

        if (!string.IsNullOrEmpty(query.CategoryLarge))
        {
            dbQuery = dbQuery.Where(r => r.Order != null && r.Order.OrderItems.Any(oi => oi.Product != null && oi.Product.CategoryLarge == query.CategoryLarge));
        }

        if (!string.IsNullOrEmpty(query.CategoryMedium))
        {
            dbQuery = dbQuery.Where(r => r.Order != null && r.Order.OrderItems.Any(oi => oi.Product != null && oi.Product.CategoryMedium == query.CategoryMedium));
        }

        if (!string.IsNullOrEmpty(query.CategorySmall))
        {
            dbQuery = dbQuery.Where(r => r.Order != null && r.Order.OrderItems.Any(oi => oi.Product != null && oi.Product.CategorySmall == query.CategorySmall));
        }

        var totalCount = await dbQuery.CountAsync();
        var items = await dbQuery
            // A DEPOSIT sitting in 미수금 관리 that gets more money merged into it (see
            // ProcessDepositAsync) never gets a new row - its own Amount just grows in place,
            // dated by whenever it was FIRST created. Sorted purely by CreatedAt, a deposit
            // touched again today stays buried under its original (possibly old) date instead
            // of surfacing at the top where "I just paid this" is expected. UpdatedAt (already
            // maintained on every save, see AppDbContext's SaveChanges override) reflects the
            // most recent touch, so sort and the displayed 거래일자 below both use it.
            .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt).ThenByDescending(r => r.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(r => new ReceivableDto
            {
                Id = r.Id,
                UserId = r.UserId,
                UserName = r.User != null ? r.User.Username : null,
                UserDisplayName = r.User != null ? r.User.Name : null,
                CompanyName = r.User != null
                    ? (r.User.UserCompanies.Where(uc => uc.Company != null && uc.Company.Category == "RTL").Select(uc => uc.Company!.Name).FirstOrDefault()
                        ?? r.User.UserCompanies.Select(uc => uc.Company!.Name).FirstOrDefault())
                    : null,
                OrderId = r.OrderId,
                OrderNo = r.Order != null ? r.Order.OrderNo : null,
                ProductName = r.Order != null
                    ? r.Order.OrderItems.Where(oi => oi.ParentId == null)
                        .Select(oi => oi.Product != null ? oi.Product.Name : (oi.ProductSet != null ? oi.ProductSet.Title : oi.CustomProductName))
                        .FirstOrDefault()
                    : null,
                ProductPhotoUrl = r.Order != null
                    ? r.Order.OrderItems.Where(oi => oi.ParentId == null)
                        .Select(oi => oi.Product != null && oi.Product.ProductPhotos.Any() ? oi.Product.ProductPhotos.OrderBy(p => p.SortOrder).First().PhotoUrl
                            : (oi.ProductSet != null && oi.ProductSet.ProductSetPhotos.Any() ? oi.ProductSet.ProductSetPhotos.OrderBy(p => p.SortOrder).First().PhotoUrl : null))
                        .FirstOrDefault()
                    : null,
                Type = r.Type,
                Amount = r.Amount,
                RemainingAmount = r.RemainingAmount,
                Weight = r.Weight,
                RemainingWeight = r.RemainingWeight,
                Memo = r.Memo,
                SettlementMethod = r.SettlementMethod,
                Discount = r.Discount,
                DiscountWeight = r.DiscountWeight,
                IsCancelled = r.IsCancelled,
                // 거래일자 shows the effective/last-touched date, not necessarily when this
                // row was first inserted - see the sort comment above for why.
                CreatedAt = r.UpdatedAt ?? r.CreatedAt,
                SourcePaymentId = r.SourcePaymentId,
                // IgnoreQueryFilters: when a charge this deposit originally zero-acknowledged
                // is later actually collected by a DIFFERENT, later deposit, that later
                // deposit's own application replaces this one - per the soft-delete + replace
                // pattern, THIS deposit's own application row for that charge gets soft-deleted
                // (not this deposit itself, and not the charge). Without lifting the filter
                // here, that deleted row silently drops out of this deposit's own historical
                // application list, under-counting 판매(B) and inflating 거래 전 미수(A) for
                // this specific 거래번호 in the detail popup - this is "what did this deposit
                // touch when it was made", so the historical row belongs regardless of what
                // later superseded it.
                AppliedCharges = r.Type == "DEPOSIT"
                    ? Context.ReceivableApplications
                        .IgnoreQueryFilters()
                        .Where(a => a.DepositId == r.Id && !a.Charge!.IsDeleted)
                        .Select(a => new AppliedChargeDto
                        {
                            ChargeId = a.ChargeId,
                            OrderId = a.Charge != null ? a.Charge.OrderId : null,
                            OrderNo = a.Charge != null && a.Charge.Order != null ? a.Charge.Order.OrderNo : null,
                            AppliedAmount = a.AppliedAmount,
                            AppliedWeight = a.AppliedWeight,
                            ChargeAmount = a.Charge != null ? a.Charge.Amount : 0,
                            ChargeWeight = a.Charge != null ? a.Charge.Weight : 0,
                            ChargeRemainingAmount = a.Charge != null ? a.Charge.RemainingAmount : 0,
                            ChargeRemainingWeight = a.Charge != null ? a.Charge.RemainingWeight : 0,
                            Items = a.Charge != null && a.Charge.Order != null
                                ? a.Charge.Order.OrderItems.Where(oi => oi.ParentId == null)
                                    .Select(oi => new ReceivableOrderItemSummaryDto
                                    {
                                        ProductName = oi.Product != null ? oi.Product.Name : (oi.ProductSet != null ? oi.ProductSet.Title : oi.CustomProductName),
                                        ProductNo = oi.Product != null ? oi.Product.ProductNo : null,
                                        PhotoUrl = oi.Product != null && oi.Product.ProductPhotos.Any() ? oi.Product.ProductPhotos.OrderBy(p => p.SortOrder).First().PhotoUrl
                                            : (oi.ProductSet != null && oi.ProductSet.ProductSetPhotos.Any() ? oi.ProductSet.ProductSetPhotos.OrderBy(p => p.SortOrder).First().PhotoUrl : null),
                                        Purity = oi.Purity,
                                        Color = oi.Color,
                                        Size = oi.Size,
                                        Memo = oi.Memo,
                                        Quantity = oi.Quantity,
                                        ActualWeight = oi.ActualWeight ?? oi.RequestedWeight ?? oi.ApprovedWeight,
                                        MaterialCost = oi.RetailerConfirmMaterialCost ?? oi.FactoryInputMaterialCost ?? 0,
                                        LaborCost = oi.RetailerConfirmLaborCost ?? oi.FactoryInputLaborCost ?? 0
                                    }).ToList()
                                : new List<ReceivableOrderItemSummaryDto>()
                        })
                        .ToList()
                    : new List<AppliedChargeDto>(),
                // OrderCount/OutstandingChargeAmount/OutstandingChargeWeight are filled in below,
                // after this page is materialized - a correlated subquery here (even a Distinct
                // one) was observed to mis-translate to SQL and silently drop a charge from the
                // aggregate, so this is computed safely in memory instead. See below.
                IsMostRecentPayment = r.Type == "DEPOSIT" && !Context.Receivables.Any(other =>
                    other.Type == "DEPOSIT" && !other.IsCancelled &&
                    other.UserId == r.UserId &&
                    (other.CreatedAt > r.CreatedAt || (other.CreatedAt == r.CreatedAt && other.Id > r.Id))),
                IsOverdueSettlement = r.IsOverdueSettlement
            })
            .ToListAsync();

        var depositIds = items.Where(i => i.Type == "DEPOSIT").Select(i => i.Id).ToList();
        if (depositIds.Count > 0)
        {
            var applicationRows = await Context.ReceivableApplications
                .Where(a => depositIds.Contains(a.DepositId))
                .Select(a => new { a.DepositId, a.ChargeId, a.Charge!.OrderId, a.Charge!.RemainingAmount, a.Charge!.RemainingWeight })
                .ToListAsync();

            var byDeposit = applicationRows.GroupBy(a => a.DepositId);
            foreach (var group in byDeposit)
            {
                // A deposit's applications can include more than one row against the SAME
                // charge (e.g. an earlier 0-value acknowledgement plus a later real top-up that
                // merged into this same deposit) - group by ChargeId first so each charge's
                // OrderId/RemainingAmount/RemainingWeight is only counted once.
                var distinctCharges = group.GroupBy(a => a.ChargeId).Select(g => g.First()).ToList();
                var dto = items.First(i => i.Id == group.Key);
                // An overdue-settlement row (미수금 관리's 입금처리) is a pure debt collection,
                // not tied to any specific order from the user's point of view - showing a real
                // order count here would misleadingly suggest this was a fresh multi-order sale.
                dto.OrderCount = dto.IsOverdueSettlement ? 0 : distinctCharges.Select(c => c.OrderId).Distinct().Count();
                dto.OutstandingChargeAmount = distinctCharges.Sum(c => c.RemainingAmount);
                dto.OutstandingChargeWeight = distinctCharges.Sum(c => c.RemainingWeight);
            }
        }

        return (items, totalCount);
    }

    public async Task<List<Receivable>> GetTargetChargesAsync(int userId, int orderId)
    {
        return await DbSet
            .Where(r => r.UserId == userId && r.OrderId == orderId && r.Type == "CHARGE" && (r.RemainingAmount > 0 || r.RemainingWeight > 0))
            .ToListAsync();
    }

    public async Task<List<Receivable>> GetTargetChargesForOrdersAsync(int userId, List<int> orderIds)
    {
        return await DbSet
            .Where(r => r.UserId == userId && r.OrderId.HasValue && orderIds.Contains(r.OrderId.Value) && r.Type == "CHARGE" && (r.RemainingAmount > 0 || r.RemainingWeight > 0))
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Receivable>> GetOutstandingChargesAsync(int userId)
    {
        return await DbSet
            .Where(r => r.UserId == userId && r.Type == "CHARGE" && (r.RemainingAmount > 0 || r.RemainingWeight > 0))
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Receivable>> GetOutstandingChargesWithOrderItemsAsync(int userId)
    {
        return await DbSet
            .Include(r => r.Order)
                .ThenInclude(o => o!.OrderItems)
            .Where(r => r.UserId == userId && r.Type == "CHARGE" && (r.RemainingAmount > 0 || r.RemainingWeight > 0))
            .ToListAsync();
    }
}

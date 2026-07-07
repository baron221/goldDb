using GoldbApi.Data;
using GoldbApi.DTOs;
using GoldbApi.Models;
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

    public async Task<int> GetUsersWithReceivablesCountAsync(string? search, List<int> userIdsWithReceivables)
    {
        var query = Context.Users.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u => u.Username.Contains(search) || u.Name.Contains(search));
        }

        query = query.Where(u => userIdsWithReceivables.Contains(u.Id));
        return await query.CountAsync();
    }

    public async Task<List<User>> GetUsersWithReceivablesPagedAsync(int page, int pageSize, string? search, List<int> userIdsWithReceivables)
    {
        var query = Context.Users
            .Include(u => u.UserCompanies).ThenInclude(uc => uc.Company)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u => u.Username.Contains(search) || u.Name.Contains(search));
        }

        query = query.Where(u => userIdsWithReceivables.Contains(u.Id));

        return await query
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

    public async Task<(List<ReceivableDto> Items, int TotalCount)> GetReceivablesPagedAsync(ReceivableQueryDto query)
    {
        var dbQuery = DbSet
            .Include(r => r.User)
            .Include(r => r.Order)
            .AsQueryable();

        if (query.UserId.HasValue)
            dbQuery = dbQuery.Where(r => r.UserId == query.UserId.Value);

        if (!string.IsNullOrEmpty(query.Type))
            dbQuery = dbQuery.Where(r => r.Type == query.Type);

        if (query.StartDate.HasValue)
        {
            var startDateUtc = DateTime.SpecifyKind(query.StartDate.Value, DateTimeKind.Utc);
            dbQuery = dbQuery.Where(r => r.CreatedAt >= startDateUtc);
        }

        if (query.EndDate.HasValue)
        {
            var endDateUtc = DateTime.SpecifyKind(query.EndDate.Value, DateTimeKind.Utc).AddDays(1).AddTicks(-1);
            dbQuery = dbQuery.Where(r => r.CreatedAt <= endDateUtc);
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
            .OrderByDescending(r => r.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(r => new ReceivableDto
            {
                Id = r.Id,
                UserId = r.UserId,
                UserName = r.User != null ? r.User.Username : null,
                UserDisplayName = r.User != null ? r.User.Name : null,
                OrderId = r.OrderId,
                OrderNo = r.Order != null ? r.Order.OrderNo : null,
                Type = r.Type,
                Amount = r.Amount,
                RemainingAmount = r.RemainingAmount,
                Memo = r.Memo,
                SettlementMethod = r.SettlementMethod,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<Receivable>> GetTargetChargesAsync(int userId, int orderId)
    {
        return await DbSet
            .Where(r => r.UserId == userId && r.OrderId == orderId && r.Type == "CHARGE" && r.RemainingAmount > 0)
            .ToListAsync();
    }

    public async Task<List<Receivable>> GetOutstandingChargesAsync(int userId)
    {
        return await DbSet
            .Where(r => r.UserId == userId && r.Type == "CHARGE" && r.RemainingAmount > 0)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();
    }
}

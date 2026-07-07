using GoldbApi.Data;
using GoldbApi.DTOs;
using GoldbApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GoldbApi.Repositories;

public class StockRepository : RepositoryBase<Stock>, IStockRepository
{
    public StockRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Stock?> GetByIdWithDetailsAsync(int id)
    {
        return await DbSet
            .Include(s => s.Product)
                .ThenInclude(p => p!.Company)
            .Include(s => s.Product)
                .ThenInclude(p => p!.ProductPhotos)
            .Include(s => s.ProductSet)
                .ThenInclude(ps => ps!.Company)
            .Include(s => s.ProductSet)
                .ThenInclude(ps => ps!.ProductSetPhotos)
            .Include(s => s.SourceOrder)
                .ThenInclude(o => o!.LogisticsCompany)
            .Include(s => s.SourceOrder)
                .ThenInclude(o => o!.Customer)
            .Include(s => s.SourceOrderItem)
            .Include(s => s.Attachments)
            .Include(s => s.Children)
                .ThenInclude(c => c.Product)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Stock?> GetByStockNoAsync(string stockNo)
    {
        return await DbSet.FirstOrDefaultAsync(s => s.StockNo == stockNo);
    }

    public async Task<(List<StockDto> Items, int TotalCount)> GetStocksAsync(StockQueryDto query)
    {
        var dbQuery = DbSet
            .Include(s => s.Product)
                .ThenInclude(p => p!.Company)
            .Include(s => s.Product)
                .ThenInclude(p => p!.ProductPhotos)
            .Include(s => s.ProductSet)
                .ThenInclude(ps => ps!.Company)
            .Include(s => s.ProductSet)
                .ThenInclude(ps => ps!.ProductSetPhotos)
            .Include(s => s.Company)
            .Include(s => s.SourceOrder)
                .ThenInclude(o => o!.LogisticsCompany)
            .Include(s => s.SourceOrder)
                .ThenInclude(o => o!.Customer)
            .Include(s => s.ExhaustionOrder)
            .Include(s => s.Attachments)
            .Include(s => s.Children)
            .AsQueryable();

        dbQuery = ApplyFilters(dbQuery, query);

        var totalCount = await dbQuery.CountAsync();
        var page = query.Page ?? 1;
        var pageSize = query.PageSize ?? 50;

        var items = await dbQuery
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new StockDto
            {
                Id = s.Id,
                ProductId = s.ProductId,
                ProductSetId = s.ProductSetId,
                ParentStockId = s.ParentStockId,
                CompanyId = s.CompanyId,
                ProductName = s.Product != null ? s.Product.Name : string.Empty,
                ProductSetTitle = s.ProductSet != null ? s.ProductSet.Title : string.Empty,
                ProductNo = s.Product != null ? (s.Product.ProductNo ?? string.Empty) : string.Empty,
                ProductPhotoUrl = s.Product != null && s.Product.ProductPhotos.Any() 
                                  ? s.Product.ProductPhotos.OrderBy(p => p.SortOrder).First().PhotoUrl 
                                  : (s.ProductSet != null && s.ProductSet.ProductSetPhotos.Any() 
                                      ? s.ProductSet.ProductSetPhotos.OrderBy(p => p.SortOrder).First().PhotoUrl : string.Empty),
                CategoryLarge = s.Product != null ? s.Product.CategoryLarge : (s.ProductSet != null ? s.ProductSet.CategoryLarge : string.Empty),
                CategoryMedium = s.Product != null ? s.Product.CategoryMedium : (s.ProductSet != null ? s.ProductSet.CategoryMedium : string.Empty),
                CategorySmall = s.Product != null ? s.Product.CategorySmall : (s.ProductSet != null ? s.ProductSet.CategorySmall : string.Empty),
                Category = s.Product != null ? $"{s.Product.CategoryLarge} > {s.Product.CategoryMedium}" : 
                           (s.ProductSet != null ? $"{s.ProductSet.CategoryLarge} > {s.ProductSet.CategoryMedium}" : string.Empty),
                StockNo = s.StockNo,
                Status = s.Status,
                Purity = s.Purity ?? (s.Product != null ? (s.Product.Purity ?? string.Empty) : string.Empty),
                Color = s.Color,
                ActualWeight = s.ActualWeight,
                CompanyName = s.Product != null && s.Product.Company != null ? s.Product.Company.Name : 
                              (s.ProductSet != null && s.ProductSet.Company != null ? s.ProductSet.Company.Name : null),
                OwnerCompanyName = s.Company != null ? s.Company.Name : null,
                LogisticsCompanyName = s.SourceOrder != null && s.SourceOrder.LogisticsCompany != null ? s.SourceOrder.LogisticsCompany.Name : null,
                SourceOrderNo = s.SourceOrder != null ? s.SourceOrder.OrderNo : null,
                SourceOrderDate = s.SourceOrder != null ? s.SourceOrder.CreatedAt : null,
                CustomerName = s.SourceOrder != null && s.SourceOrder.Customer != null ? s.SourceOrder.Customer.Name : null,
                ExhaustionOrderId = s.ExhaustionOrderId,
                ExhaustionOrderNo = s.ExhaustionOrder != null ? s.ExhaustionOrder.OrderNo : null,
                ExhaustionOrderItemId = s.ExhaustionOrderItemId,
                ExhaustionDate = s.ExhaustionDate,
                RenterName = s.RenterName,
                RentDate = s.RentDate,
                ReturnDueDate = s.ReturnDueDate,
                RetailerConfirmMaterialCost = s.RetailerConfirmMaterialCost,
                RetailerConfirmLaborCost = s.RetailerConfirmLaborCost,
                Note = s.Note,
                CreatedAt = s.CreatedAt,
                Attachments = s.Attachments.Select(a => new AttachmentDto
                {
                    Id = a.Id,
                    FileName = a.FileName,
                    OriginalName = a.OriginalName,
                    FilePath = a.FilePath,
                    IsMain = a.IsMain
                }).ToList()
            })
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<Stock>> GetStocksForSummaryAsync(StockQueryDto query)
    {
        var dbQuery = DbSet
            .Include(s => s.Product)
            .Include(s => s.Company)
            .AsQueryable();

        dbQuery = dbQuery.Where(s => s.ParentStockId == null);
        dbQuery = ApplyFilters(dbQuery, query);

        return await dbQuery.ToListAsync();
    }

    public async Task<List<Stock>> GetStocksByIdsAsync(List<int> ids)
    {
        return await DbSet.Where(s => ids.Contains(s.Id)).ToListAsync();
    }

    public async Task<List<OrderStatusHistoryDto>> GetOrderHistoryForStockAsync(int orderId)
    {
        return await Context.OrderStatusHistories
            .Include(h => h.User)
                .ThenInclude(u => u!.UserCompanies)
                    .ThenInclude(uc => uc.Company)
            .Where(h => h.OrderId == orderId)
            .OrderByDescending(h => h.CreatedAt)
            .Select(h => new OrderStatusHistoryDto
            {
                Id = h.Id,
                OrderId = h.OrderId,
                Status = h.Status,
                Remarks = h.Remarks,
                CreatedAt = h.CreatedAt,
                UserId = h.UserId,
                UserName = h.User != null ? h.User.Username : null,
                UserDisplayName = h.User != null ? h.User.Name : null,
                CompanyName = h.User != null && h.User.UserCompanies.Any() ? h.User.UserCompanies.First().Company!.Name : null
            })
            .ToListAsync();
    }

    public async Task<Order?> GetOrderWithItemsAsync(int orderId)
    {
        return await Context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Children)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task<UserCompany?> GetUserCompanyAsync(int userId)
    {
        return await Context.UserCompanies.FirstOrDefaultAsync(uc => uc.UserId == userId);
    }

    public async Task<OrderItem?> GetOrderItemWithExhaustedStocksAsync(int orderItemId)
    {
        return await Context.OrderItems
            .Include(oi => oi.ExhaustedStocks)
            .FirstOrDefaultAsync(oi => oi.Id == orderItemId);
    }

    public async Task<List<Attachment>> GetAttachmentsByIdsAsync(List<int> ids)
    {
        return await Context.Attachments.Where(a => ids.Contains(a.Id)).ToListAsync();
    }

    public async Task<OrderItem?> GetOrderItemWithOrderAndExhaustedStocksAsync(int orderItemId)
    {
        return await Context.OrderItems
            .Include(oi => oi.Order)
            .Include(oi => oi.ExhaustedStocks)
            .FirstOrDefaultAsync(oi => oi.Id == orderItemId);
    }

    public async Task<List<OrderItem>> GetChildOrderItemsAsync(int parentId)
    {
        return await Context.OrderItems
            .Where(oi => oi.ParentId == parentId)
            .ToListAsync();
    }

    public async Task<Order?> GetOrderWithItemsAndExhaustedStocksAsync(int orderId)
    {
        return await Context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ExhaustedStocks)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Children)
                    .ThenInclude(c => c.ExhaustedStocks)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task AddOrderStatusHistoryAsync(OrderStatusHistory history)
    {
        await Context.OrderStatusHistories.AddAsync(history);
    }

    private IQueryable<Stock> ApplyFilters(IQueryable<Stock> dbQuery, StockQueryDto query)
    {
        if (query.CompanyId.HasValue)
        {
            dbQuery = dbQuery.Where(s => s.CompanyId == query.CompanyId.Value);
        }

        if (query.LogisticsCompanyId.HasValue)
        {
            dbQuery = dbQuery.Where(s => s.Company != null && s.Company.LogisticsCompanyId == query.LogisticsCompanyId.Value);
        }

        if (query.IsDirectManagement.HasValue)
        {
            dbQuery = dbQuery.Where(s => s.Company != null && s.Company.IsDirectManagement == query.IsDirectManagement.Value);
        }

        if (query.MinWeight.HasValue)
        {
            dbQuery = dbQuery.Where(s => s.ActualWeight >= query.MinWeight.Value);
        }

        if (query.MaxWeight.HasValue)
        {
            dbQuery = dbQuery.Where(s => s.ActualWeight <= query.MaxWeight.Value);
        }

        if (!string.IsNullOrEmpty(query.CategoryLarge))
        {
            dbQuery = dbQuery.Where(s => s.Product != null && s.Product.CategoryLarge == query.CategoryLarge);
        }

        if (!string.IsNullOrEmpty(query.CategoryMedium))
        {
            dbQuery = dbQuery.Where(s => s.Product != null && s.Product.CategoryMedium == query.CategoryMedium);
        }

        if (!string.IsNullOrEmpty(query.CategorySmall))
        {
            dbQuery = dbQuery.Where(s => s.Product != null && s.Product.CategorySmall == query.CategorySmall);
        }

        if (!string.IsNullOrEmpty(query.SetCategoryLarge))
        {
            dbQuery = dbQuery.Where(s => s.ProductSet != null && s.ProductSet.CategoryLarge == query.SetCategoryLarge);
        }

        if (!string.IsNullOrEmpty(query.SetCategoryMedium))
        {
            dbQuery = dbQuery.Where(s => s.ProductSet != null && s.ProductSet.CategoryMedium == query.SetCategoryMedium);
        }

        if (!string.IsNullOrEmpty(query.SetCategorySmall))
        {
            dbQuery = dbQuery.Where(s => s.ProductSet != null && s.ProductSet.CategorySmall == query.SetCategorySmall);
        }

        if (!string.IsNullOrEmpty(query.OrderNo))
        {
            dbQuery = dbQuery.Where(s => s.SourceOrder != null && s.SourceOrder.OrderNo.Contains(query.OrderNo));
        }

        if (!string.IsNullOrEmpty(query.StockNo))
        {
            dbQuery = dbQuery.Where(s => s.StockNo.Contains(query.StockNo));
        }

        if (!string.IsNullOrEmpty(query.ProductName))
        {
            dbQuery = dbQuery.Where(s => (s.Product != null && s.Product.Name.Contains(query.ProductName)) ||
                                         (s.ProductSet != null && s.ProductSet.Title.Contains(query.ProductName)));
        }

        if (!string.IsNullOrEmpty(query.SearchText))
        {
            dbQuery = dbQuery.Where(s => s.StockNo.Contains(query.SearchText) || 
                                         (s.Product != null && s.Product.Name.Contains(query.SearchText)) ||
                                         (s.ProductSet != null && s.ProductSet.Title.Contains(query.SearchText)));
        }

        if (query.IsExhausted.HasValue)
        {
            dbQuery = dbQuery.Where(s => s.IsExhausted == query.IsExhausted.Value);
        }

        if (query.ExhaustionDateStart.HasValue)
        {
            dbQuery = dbQuery.Where(s => s.ExhaustionDate >= query.ExhaustionDateStart.Value);
        }

        if (query.ExhaustionDateEnd.HasValue)
        {
            dbQuery = dbQuery.Where(s => s.ExhaustionDate <= query.ExhaustionDateEnd.Value);
        }

        if (query.CreatedAtStart.HasValue)
        {
            dbQuery = dbQuery.Where(s => s.CreatedAt >= query.CreatedAtStart.Value);
        }

        if (query.CreatedAtEnd.HasValue)
        {
            dbQuery = dbQuery.Where(s => s.CreatedAt <= query.CreatedAtEnd.Value);
        }

        if (!string.IsNullOrEmpty(query.Status))
        {
            if (query.Status == "ACTIVE")
            {
                dbQuery = dbQuery.Where(s => (s.Status == "IN_STOCK" || s.Status == "ON_DISPLAY" || s.Status == "RENTED") && !s.IsExhausted);
            }
            else
            {
                dbQuery = dbQuery.Where(s => s.Status == query.Status);
            }
        }

        return dbQuery;
    }
}

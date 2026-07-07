using GoldbApi.Data;
using GoldbApi.DTOs;
using GoldbApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GoldbApi.Repositories;

public class OrderRepository : RepositoryBase<Order>, IOrderRepository
{
    public OrderRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Order?> GetByIdWithItemsAsync(int id)
    {
        return await DbSet
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<UserCompany?> GetUserCompanyInfoAsync(int userId)
    {
        return await Context.UserCompanies
            .Include(uc => uc.Company)
            .FirstOrDefaultAsync(uc => uc.UserId == userId && !uc.IsDeleted);
    }

    public async Task<List<CartItem>> GetCartItemsForUserAsync(int userId, List<int> cartItemIds)
    {
        return await Context.CartItems
            .Include(c => c.Product).ThenInclude(p => p!.Company)
            .Include(c => c.Product).ThenInclude(p => p!.OptionWeights)
            .Include(c => c.ProductSet).ThenInclude(ps => ps!.Company)
            .Where(c => c.UserId == userId && cartItemIds.Contains(c.Id))
            .ToListAsync();
    }

    public async Task<int> GetTodayOrderCountAsync(DateTime start, DateTime end)
    {
        return await DbSet
            .CountAsync(o => o.CreatedAt >= start && o.CreatedAt < end);
    }

    public async Task<ProductSet?> GetProductSetWithItemsAsync(int productSetId)
    {
        return await Context.ProductSets
            .Include(ps => ps.SetItems)
            .ThenInclude(si => si.Product)
            .FirstOrDefaultAsync(ps => ps.Id == productSetId);
    }

    public async Task<(List<OrderDto> Items, int TotalCount)> GetMyOrdersAsync(int userId, OrderQueryDto query)
    {
        var dbQuery = DbSet
            .Include(o => o.User).ThenInclude(u => u!.UserCompanies).ThenInclude(uc => uc.Company)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product).ThenInclude(p => p!.Company)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product).ThenInclude(p => p!.ProductPhotos)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductSet).ThenInclude(ps => ps!.ProductSetPhotos)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductSet).ThenInclude(ps => ps!.Company)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Children).ThenInclude(c => c.Product).ThenInclude(p => p!.Company)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Children).ThenInclude(c => c.Product).ThenInclude(p => p!.ProductPhotos)
            .Include(o => o.StatusHistories).ThenInclude(h => h.User).ThenInclude(u => u!.UserCompanies).ThenInclude(uc => uc.Company)
            .Where(o => o.UserId == userId)
            .AsQueryable();

        dbQuery = ApplyFilters(dbQuery, query);
        dbQuery = ApplySorting(dbQuery, query);

        var totalCount = await dbQuery.CountAsync();
        var items = await ProjectToDto(dbQuery)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<(List<OrderDto> Items, int TotalCount)> GetAllOrdersAsync(OrderQueryDto query)
    {
        var dbQuery = DbSet
            .Include(o => o.User).ThenInclude(u => u!.UserCompanies).ThenInclude(uc => uc.Company)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product).ThenInclude(p => p!.Company)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product).ThenInclude(p => p!.ProductPhotos)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductSet).ThenInclude(ps => ps!.ProductSetPhotos)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductSet).ThenInclude(ps => ps!.Company)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Children).ThenInclude(c => c.Product).ThenInclude(p => p!.Company)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Children).ThenInclude(c => c.Product).ThenInclude(p => p!.ProductPhotos)
            .Include(o => o.StatusHistories).ThenInclude(h => h.User).ThenInclude(u => u!.UserCompanies).ThenInclude(uc => uc.Company)
            .AsQueryable();

        dbQuery = ApplyFilters(dbQuery, query);

        if (query.FactoryCompanyId.HasValue && query.FactoryCompanyId.Value > 0)
        {
            dbQuery = dbQuery.Where(o => o.OrderItems.Any(oi => 
                (oi.Product != null && oi.Product.CompanyId == query.FactoryCompanyId.Value) ||
                (oi.ProductSet != null && oi.ProductSet.CompanyId == query.FactoryCompanyId.Value) ||
                (oi.Children.Any(c => c.Product != null && c.Product.CompanyId == query.FactoryCompanyId.Value))
            ));
        }

        if (query.IsAsOnly == true)
        {
            dbQuery = dbQuery.Where(o => o.OrderItems.Any(oi =>
                oi.IsAsOrder == true ||
                oi.Children.Any(c => c.IsAsOrder == true)
            ));
        }

        dbQuery = ApplySorting(dbQuery, query);

        var totalCount = await dbQuery.CountAsync();
        var items = await ProjectToDto(dbQuery)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<OrderItem>> GetOrderItemsByIdsAsync(int orderId, List<int> itemIds)
    {
        return await Context.OrderItems
            .Include(oi => oi.Product)
            .Include(oi => oi.ProductSet)
            .Where(oi => oi.OrderId == orderId && itemIds.Contains(oi.Id))
            .ToListAsync();
    }

    public async Task<List<OrderItem>> GetTopLevelOrderItemsAsync(int orderId)
    {
        return await Context.OrderItems
            .Where(oi => oi.OrderId == orderId && oi.ParentId == null)
            .ToListAsync();
    }

    public async Task<List<OrderStatusHistoryDto>> GetOrderHistoryAsync(int orderId)
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
                UserId = h.UserId,
                UserName = h.User != null ? h.User.Username : null,
                UserDisplayName = h.User != null ? h.User.Name : null,
                CompanyName = h.User != null && h.User.UserCompanies.Any() 
                             ? h.User.UserCompanies.First().Company!.Name : null,
                CompanyType = h.User != null && h.User.UserCompanies.Any() 
                             ? h.User.UserCompanies.First().Company!.Category : null,
                Remarks = h.Remarks,
                CreatedAt = h.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<OrderStatement?> GetOrderStatementAsync(int orderId)
    {
        return await Context.OrderStatements
            .FirstOrDefaultAsync(os => os.OrderId == orderId);
    }

    public async Task<OrderStatementDto?> GetOrderStatementDtoAsync(int orderId)
    {
        return await Context.OrderStatements
            .Where(os => os.OrderId == orderId)
            .Select(os => new OrderStatementDto
            {
                OrderId = os.OrderId,
                SnapshotData = os.SnapshotData,
                CreatedAt = os.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<OrderStatementDto?> GetOrderStatementPdfDtoAsync(int orderId)
    {
        return await Context.OrderStatements
            .Where(os => os.OrderId == orderId)
            .Select(os => new OrderStatementDto
            {
                OrderId = os.OrderId,
                PdfBinary = os.PdfBinary
            })
            .FirstOrDefaultAsync();
    }

    private IQueryable<Order> BuildSettlementQuery(OrderQueryDto query)
    {
        var dbQuery = DbSet.AsQueryable();

        if (!string.IsNullOrEmpty(query.Status))
            dbQuery = dbQuery.Where(o => o.Status == query.Status);
        else
            dbQuery = dbQuery.Where(o => o.Status == "SETTLED");

        if (!string.IsNullOrEmpty(query.OrderNo))
        {
            var searchTerm = query.OrderNo.Trim().ToLower();
            dbQuery = dbQuery.Where(o => 
                o.OrderNo.ToLower().Contains(searchTerm) ||
                (o.GroupOrderNo != null && o.GroupOrderNo.ToLower().Contains(searchTerm)) ||
                o.OrderItems.Any(oi => 
                    (oi.Product != null && (oi.Product.ProductNo!.ToLower().Contains(searchTerm) || oi.Product.Name.ToLower().Contains(searchTerm))) ||
                    (oi.ProductSet != null && (oi.ProductSet.SetNo!.ToLower().Contains(searchTerm) || oi.ProductSet.Title.ToLower().Contains(searchTerm)))
                ));
        }

        if (!string.IsNullOrEmpty(query.UserName))
            dbQuery = dbQuery.Where(o => o.User != null && (o.User.Username.Contains(query.UserName) || o.User.Name.Contains(query.UserName)));

        if (query.CompanyId.HasValue && query.CompanyId.Value > 0)
        {
            dbQuery = dbQuery.Where(o => o.User != null && o.User.UserCompanies.Any(uc => uc.CompanyId == query.CompanyId.Value));
        }

        if (query.LogisticsCompanyId.HasValue && query.LogisticsCompanyId.Value > 0)
        {
            dbQuery = dbQuery.Where(o => o.LogisticsCompanyId == query.LogisticsCompanyId.Value);
        }
        if (query.FactoryCompanyId.HasValue && query.FactoryCompanyId.Value > 0)
        {
            dbQuery = dbQuery.Where(o => o.OrderItems.Any(oi =>
                (oi.Product != null && oi.Product.CompanyId == query.FactoryCompanyId.Value) ||
                (oi.ProductSet != null && oi.ProductSet.CompanyId == query.FactoryCompanyId.Value) ||
                (oi.Children.Any(c => c.Product != null && c.Product.CompanyId == query.FactoryCompanyId.Value))
            ));
        }

        if (!string.IsNullOrEmpty(query.CategoryLarge))
        {
            dbQuery = dbQuery.Where(o => o.OrderItems.Any(oi =>
                (oi.Product != null && oi.Product.CategoryLarge == query.CategoryLarge) ||
                (oi.ProductSet != null && oi.ProductSet.CategoryLarge == query.CategoryLarge) ||
                (oi.Children.Any(c => c.Product != null && c.Product.CategoryLarge == query.CategoryLarge))
            ));
        }

        if (!string.IsNullOrEmpty(query.CategoryMedium))
        {
            dbQuery = dbQuery.Where(o => o.OrderItems.Any(oi =>
                (oi.Product != null && oi.Product.CategoryMedium == query.CategoryMedium) ||
                (oi.ProductSet != null && oi.ProductSet.CategoryMedium == query.CategoryMedium) ||
                (oi.Children.Any(c => c.Product != null && c.Product.CategoryMedium == query.CategoryMedium))
            ));
        }

        if (!string.IsNullOrEmpty(query.CategorySmall))
        {
            dbQuery = dbQuery.Where(o => o.OrderItems.Any(oi =>
                (oi.Product != null && oi.Product.CategorySmall == query.CategorySmall) ||
                (oi.ProductSet != null && oi.ProductSet.CategorySmall == query.CategorySmall) ||
                (oi.Children.Any(c => c.Product != null && c.Product.CategorySmall == query.CategorySmall))
            ));
        }

        if (query.StartDate.HasValue)
        {
            var startDateUtc = DateTime.SpecifyKind(query.StartDate.Value, DateTimeKind.Utc);
            dbQuery = dbQuery.Where(o => o.CreatedAt >= startDateUtc);
        }

        if (query.EndDate.HasValue)
        {
            var endDateUtc = DateTime.SpecifyKind(query.EndDate.Value, DateTimeKind.Utc);
            dbQuery = dbQuery.Where(o => o.CreatedAt <= endDateUtc);
        }

        if (!string.IsNullOrEmpty(query.SetCategoryLarge))
        {
            dbQuery = dbQuery.Where(o => o.OrderItems.Any(oi => oi.ProductSet != null && oi.ProductSet.CategoryLarge == query.SetCategoryLarge));
        }
        if (!string.IsNullOrEmpty(query.SetCategoryMedium))
        {
            dbQuery = dbQuery.Where(o => o.OrderItems.Any(oi => oi.ProductSet != null && oi.ProductSet.CategoryMedium == query.SetCategoryMedium));
        }
        if (!string.IsNullOrEmpty(query.SetCategorySmall))
        {
            dbQuery = dbQuery.Where(o => o.OrderItems.Any(oi => oi.ProductSet != null && oi.ProductSet.CategorySmall == query.SetCategorySmall));
        }

        return dbQuery;
    }

    public async Task<List<Order>> GetSettlementOrdersAsync(OrderQueryDto query)
    {
        var dbQuery = BuildSettlementQuery(query)
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Children);

        return await dbQuery.ToListAsync();
    }

    public async Task<SettlementHistorySummaryDto> GetSettlementSummaryAsync(OrderQueryDto query)
    {
        var baseQuery = BuildSettlementQuery(query);

        var parentItems = baseQuery.SelectMany(o => o.OrderItems);

        var childItems = baseQuery.SelectMany(o => o.OrderItems).SelectMany(oi => oi.Children);

        var parentTotalWeight = await parentItems.SumAsync(oi => oi.ActualWeight ?? 0);
        var childTotalWeight = await childItems.SumAsync(c => c.ActualWeight ?? 0);

        var parentTotalAmount = await parentItems.SumAsync(oi => oi.SettlementAmount ?? 0);
        var childTotalAmount = await childItems.SumAsync(c => c.SettlementAmount ?? 0);

        var parentPurityGroups = await parentItems
            .Where(oi => oi.Purity != null)
            .GroupBy(oi => oi.Purity)
            .Select(g => new { Purity = g.Key, Weight = g.Sum(oi => oi.ActualWeight ?? 0) })
            .ToListAsync();

        var childPurityGroups = await childItems
            .Where(c => c.Purity != null)
            .GroupBy(c => c.Purity)
            .Select(g => new { Purity = g.Key, Weight = g.Sum(c => c.ActualWeight ?? 0) })
            .ToListAsync();

        decimal totalFineGold = 0;
        var allGroups = parentPurityGroups.Concat(childPurityGroups);

        foreach(var group in allGroups)
        {
            decimal ratio = group.Purity switch
            {
                "14K" => 0.585m,
                "18K" => 0.75m,
                "24K" or "PURE_GOLD" => 1.0m,
                "PT" or "PLATINUM" => 0.95m,
                _ => 0
            };
            totalFineGold += group.Weight * ratio;
        }

        return new SettlementHistorySummaryDto
        {
            TotalActualWeight = parentTotalWeight + childTotalWeight,
            TotalSettlementAmount = parentTotalAmount + childTotalAmount,
            TotalFineGold = totalFineGold
        };
    }

    public async Task AddStatusHistoryAsync(OrderStatusHistory history)
    {
        await Context.OrderStatusHistories.AddAsync(history);
    }

    public async Task AddOrderStatementAsync(OrderStatement statement)
    {
        await Context.OrderStatements.AddAsync(statement);
    }

    public async Task AddReceivableAsync(Receivable receivable)
    {
        await Context.Receivables.AddAsync(receivable);
    }

    public void RemoveCartItems(List<CartItem> cartItems)
    {
        Context.CartItems.RemoveRange(cartItems);
    }

    private IQueryable<Order> ApplyFilters(IQueryable<Order> dbQuery, OrderQueryDto query)
    {
        if (string.IsNullOrWhiteSpace(query.Status))
        {
            if (query.ExcludeCancelled == true)
            {
                dbQuery = dbQuery.Where(o => o.Status.ToUpper() != "CANCELLED" && o.Status.ToUpper() != "SETTLED_CANCELLED");
            }

            if (query.ExcludeCompleted == true)
            {
                dbQuery = dbQuery.Where(o => o.Status.ToUpper() != "COMPLETED");
            }
        }
        else
        {
            if (query.Status.Contains(","))
            {
                var statusList = query.Status.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
                dbQuery = dbQuery.Where(o => statusList.Contains(o.Status));
            }
            else
            {
                switch (query.Status)
                {
                    case "Order_Group":
                        dbQuery = dbQuery.Where(o => o.Status == "ORDERED" || o.Status == "LogisticsApproved");
                        break;
                    case "Production":
                        dbQuery = dbQuery.Where(o => o.Status == "FactoryRequested" || o.Status == "WorkOrderCreated" || o.Status == "InspectedRequested");
                        break;
                    case "Inspection":
                        dbQuery = dbQuery.Where(o => o.Status == "Inspected" || o.Status == "PENDING");
                        break;
                    case "Settlement":
                        dbQuery = dbQuery.Where(o => o.Status == "PROCESSING" || o.Status == "SETTLED");
                        break;
                    case "Shipping_Group":
                        dbQuery = dbQuery.Where(o => o.Status == "DELIVERY_READY" || o.Status == "DELIVERY_IN_TRANSIT" || o.Status == "DELIVERED");
                        break;
                    case "Completed_Group":
                        dbQuery = dbQuery.Where(o => o.Status == "Completed");
                        break;
                    case "Cancelled_Group":
                        dbQuery = dbQuery.Where(o => o.Status == "Cancelled" || o.Status == "SETTLED_CANCELLED");
                        break;
                    case "Factory_Group":
                        dbQuery = dbQuery.Where(o => o.Status != "ORDERED" && o.Status != "LogisticsApproved" && o.Status != "Cancelled");
                        break;
                    case "Post_Inspected_Group":
                        var postInspectedStatuses = new[] { "PENDING", "PROCESSING", "SETTLED", "DELIVERY_READY", "DELIVERY_IN_TRANSIT", "DELIVERED", "Completed" };
                        dbQuery = dbQuery.Where(o => postInspectedStatuses.Contains(o.Status));
                        break;
                    default:
                        dbQuery = dbQuery.Where(o => o.Status == query.Status);
                        break;
                }
            }
        }

        if (!string.IsNullOrEmpty(query.OrderNo))
        {
            var searchTerm = query.OrderNo.Trim().ToLower();
            dbQuery = dbQuery.Where(o => 
                o.OrderNo.ToLower().Contains(searchTerm) ||
                (o.GroupOrderNo != null && o.GroupOrderNo.ToLower().Contains(searchTerm)) ||
                (o.User != null && (o.User.Username.ToLower().Contains(searchTerm) || o.User.Name.ToLower().Contains(searchTerm))) ||
                (o.Customer != null && o.Customer.Name.ToLower().Contains(searchTerm)) ||
                o.OrderItems.Any(oi => 
                    (oi.Product != null && (
                        oi.Product.ProductNo!.ToLower().Contains(searchTerm) || 
                        oi.Product.Name.ToLower().Contains(searchTerm) ||
                        (oi.Product.NormalizedProductNo != null && oi.Product.NormalizedProductNo.Contains(searchTerm)) ||
                        oi.Product.NormalizedName.Contains(searchTerm)
                    )) ||
                    (oi.ProductSet != null && (
                        oi.ProductSet.SetNo!.ToLower().Contains(searchTerm) || 
                        oi.ProductSet.Title.ToLower().Contains(searchTerm)
                    )) ||
                    oi.Children.Any(c => c.Product != null && (
                        c.Product.ProductNo!.ToLower().Contains(searchTerm) || 
                        c.Product.Name.ToLower().Contains(searchTerm) ||
                        (c.Product.NormalizedProductNo != null && c.Product.NormalizedProductNo.Contains(searchTerm)) ||
                        c.Product.NormalizedName.Contains(searchTerm)
                    ))
                ));
        }

        if (!string.IsNullOrEmpty(query.UserName))
        {
            dbQuery = dbQuery.Where(o => (o.User != null && o.User.Username.Contains(query.UserName)) || (o.User != null && o.User.Name.Contains(query.UserName)));
        }

        if (query.CompanyId.HasValue && query.CompanyId.Value > 0)
        {
            dbQuery = dbQuery.Where(o => o.User != null && o.User.UserCompanies.Any(uc => uc.CompanyId == query.CompanyId.Value));
        }

        if (query.CustomerId.HasValue)
        {
            dbQuery = dbQuery.Where(o => o.CustomerId == query.CustomerId.Value);
        }

        if (query.LogisticsCompanyId.HasValue && query.LogisticsCompanyId.Value > 0)
        {
            dbQuery = dbQuery.Where(o => o.LogisticsCompanyId == query.LogisticsCompanyId.Value);
        }

        if (query.StartDate.HasValue)
        {
            var startDateUtc = DateTime.SpecifyKind(query.StartDate.Value, DateTimeKind.Utc);
            dbQuery = dbQuery.Where(o => o.CreatedAt >= startDateUtc);
        }

        if (query.EndDate.HasValue)
        {
            var nextDayUtc = DateTime.SpecifyKind(query.EndDate.Value.AddDays(1), DateTimeKind.Utc);
            dbQuery = dbQuery.Where(o => o.CreatedAt < nextDayUtc);
        }

        if (!string.IsNullOrEmpty(query.CategoryLarge))
        {
            dbQuery = dbQuery.Where(o => o.OrderItems.Any(oi => oi.Product != null && oi.Product.CategoryLarge == query.CategoryLarge));
        }
        if (!string.IsNullOrEmpty(query.CategoryMedium))
        {
            dbQuery = dbQuery.Where(o => o.OrderItems.Any(oi => oi.Product != null && oi.Product.CategoryMedium == query.CategoryMedium));
        }
        if (!string.IsNullOrEmpty(query.CategorySmall))
        {
            dbQuery = dbQuery.Where(o => o.OrderItems.Any(oi => oi.Product != null && oi.Product.CategorySmall == query.CategorySmall));
        }

        if (!string.IsNullOrEmpty(query.SetCategoryLarge))
        {
            dbQuery = dbQuery.Where(o => o.OrderItems.Any(oi => oi.ProductSet != null && oi.ProductSet.CategoryLarge == query.SetCategoryLarge));
        }
        if (!string.IsNullOrEmpty(query.SetCategoryMedium))
        {
            dbQuery = dbQuery.Where(o => o.OrderItems.Any(oi => oi.ProductSet != null && oi.ProductSet.CategoryMedium == query.SetCategoryMedium));
        }
        if (!string.IsNullOrEmpty(query.SetCategorySmall))
        {
            dbQuery = dbQuery.Where(o => o.OrderItems.Any(oi => oi.ProductSet != null && oi.ProductSet.CategorySmall == query.SetCategorySmall));
        }

        return dbQuery;
    }

    private IQueryable<Order> ApplySorting(IQueryable<Order> dbQuery, OrderQueryDto query)
    {
        return query.SortBy?.ToLower() switch
        {
            "id" => (query.IsDescending ?? true) ? dbQuery.OrderByDescending(o => o.Id) : dbQuery.OrderBy(o => o.Id),
            "orderno" => (query.IsDescending ?? true) ? dbQuery.OrderByDescending(o => o.OrderNo) : dbQuery.OrderBy(o => o.OrderNo),
            "status" => (query.IsDescending ?? true) ? dbQuery.OrderByDescending(o => o.Status) : dbQuery.OrderBy(o => o.Status),
            _ => (query.IsDescending ?? true) ? dbQuery.OrderByDescending(o => o.CreatedAt) : dbQuery.OrderBy(o => o.CreatedAt)
        };
    }

    private IQueryable<OrderDto> ProjectToDto(IQueryable<Order> dbQuery)
    {
        return dbQuery.Select(o => new OrderDto
        {
            Id = o.Id,
            OrderNo = o.OrderNo,
            GroupOrderNo = o.GroupOrderNo,
            UserId = o.UserId,
            CustomerId = o.CustomerId,
            CustomerName = o.Customer != null ? o.Customer.Name : null,
            UserName = o.User != null ? o.User.Username : null,
            UserDisplayName = o.User != null ? o.User.Name : null,
            CompanyName = o.User != null && o.User.UserCompanies.Any() ? o.User.UserCompanies.First().Company!.Name : null,
            TotalAmount = o.TotalAmount,
            Status = o.Status,
            OrderMemo = o.OrderMemo,
            FactoryRemarks = o.FactoryRemarks,
            LogisticsRemarks = o.LogisticsRemarks,
            InspectionRemarks = o.InspectionRemarks,
            WorkOrderRemarks = o.WorkOrderRemarks,
            CancellationReason = o.CancellationReason,
            SettlementMethod = o.SettlementMethod,
            SettlementRemarks = o.SettlementRemarks,
            SettlementStartMemo = o.SettlementStartMemo,
            SettlementAmount = o.SettlementAmount,
            ModificationMemo = o.ModificationMemo,
            CloseMemo = o.CloseMemo,
            DeliveryDate = o.DeliveryDate,
            LogisticsCompanyId = o.LogisticsCompanyId,
            LogisticsCompanyName = o.LogisticsCompany != null ? o.LogisticsCompany.Name : null,
            LogisticsCompanyBusinessNo = o.LogisticsCompany != null ? o.LogisticsCompany.BusinessNumber : null,
            LogisticsCompanyAddress = o.LogisticsCompany != null ? $"{o.LogisticsCompany.AddressBase} {o.LogisticsCompany.AddressDetail}".Trim() : null,
            LogisticsCompanyCEO = o.LogisticsCompany != null ? o.LogisticsCompany.CEO : null,
            LogisticsCompanyPhone = o.LogisticsCompany != null ? o.LogisticsCompany.Phone : null,
            CompanyPhone = o.User != null && o.User.UserCompanies.Any() ? o.User.UserCompanies.First().Company!.Phone : null,
            CompanyBusinessNo = o.User != null && o.User.UserCompanies.Any() ? o.User.UserCompanies.First().Company!.BusinessNumber : null,
            CompanyAddress = o.User != null && o.User.UserCompanies.Any() ? $"{o.User.UserCompanies.First().Company!.AddressBase} {o.User.UserCompanies.First().Company!.AddressDetail}".Trim() : null,
            CompanyCEO = o.User != null && o.User.UserCompanies.Any() ? o.User.UserCompanies.First().Company!.CEO : null,
            CompanyBusinessType = o.User != null && o.User.UserCompanies.Any() ? o.User.UserCompanies.First().Company!.BusinessType : null,
            CompanyBusinessCategory = o.User != null && o.User.UserCompanies.Any() ? o.User.UserCompanies.First().Company!.BusinessCategory : null,
            HasStatement = Context.OrderStatements.Any(os => os.OrderId == o.Id),
            CreatedAt = o.CreatedAt,
            UpdatedAt = o.UpdatedAt,
            OrderItems = o.OrderItems.Where(oi => oi.ParentId == null).Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                ProductId = oi.ProductId,
                ProductName = oi.Product != null ? oi.Product.Name : null,
                ProductNo = oi.Product != null ? oi.Product.ProductNo : null,
                Size = oi.Product != null ? oi.Product.ProductSize : null,
                PhotoUrl = oi.Product != null && oi.Product.ProductPhotos.Any() 
                           ? oi.Product.ProductPhotos.OrderBy(p => p.SortOrder).First().PhotoUrl 
                           : (oi.ProductSet != null 
                               ? (oi.ProductSet.ProductSetPhotos.Any() 
                                   ? oi.ProductSet.ProductSetPhotos.OrderBy(p => p.SortOrder).First().PhotoUrl 
                                   : oi.ProductSet.SetItems.SelectMany(si => si.Product!.ProductPhotos).OrderByDescending(ph => ph.IsMain).ThenBy(ph => ph.SortOrder).Select(ph => ph.PhotoUrl).FirstOrDefault()) 
                               : null),
                ProductSetId = oi.ProductSetId,
                ProductSetTitle = oi.ProductSet != null ? oi.ProductSet.Title : null,
                Quantity = oi.Quantity,
                Price = oi.Price,
                FactoryPrice = oi.FactoryPrice,
                LaborCost = oi.LaborCost,
                FactoryInputMaterialCost = oi.FactoryInputMaterialCost,
                FactoryInputLaborCost = oi.FactoryInputLaborCost,
                RetailerConfirmMaterialCost = oi.RetailerConfirmMaterialCost,
                RetailerConfirmLaborCost = oi.RetailerConfirmLaborCost,
                Weight = oi.Product != null ? oi.Product.Weight : (oi.ProductSet != null ? 0 : 0),
                OrderWeight = oi.OrderWeight,
                ActualWeight = oi.ActualWeight,
                InspectionMemo = oi.InspectionMemo,
                Purity = oi.Purity,
                Color = oi.Color,
                IsAsOrder = oi.IsAsOrder,
                ConfirmedWeight = oi.ConfirmedWeight,
                LogisticsMemo = oi.LogisticsMemo,
                ApprovedWeight = oi.ApprovedWeight,
                ApprovedMemo = oi.ApprovedMemo,
                RequestedWeight = oi.RequestedWeight,
                RequestedMemo = oi.RequestedMemo,
                SettlementRatio = oi.SettlementRatio,
                SettlementAmount = oi.SettlementAmount,
                SettlementMemo = oi.SettlementMemo,
                ManufacturerName = oi.Product != null && oi.Product.Company != null 
                                   ? oi.Product.Company.Name 
                                   : (oi.ProductSet != null && oi.ProductSet.Company != null ? oi.ProductSet.Company.Name : null),
                IsStockExhausted = oi.ExhaustedStocks.Any(),
                StockNo = oi.ExhaustedStocks.Select(s => s.StockNo).FirstOrDefault(),
                CategoryLarge = oi.Product != null ? oi.Product.CategoryLarge : (oi.ProductSet != null ? oi.ProductSet.CategoryLarge : null),
                CategoryMedium = oi.Product != null ? oi.Product.CategoryMedium : (oi.ProductSet != null ? oi.ProductSet.CategoryMedium : null),
                CategorySmall = oi.Product != null ? oi.Product.CategorySmall : (oi.ProductSet != null ? oi.ProductSet.CategorySmall : null),
                Children = oi.Children.Select(c => new OrderItemDto
                {
                    Id = c.Id,
                    ProductId = c.ProductId,
                    ProductName = c.Product != null ? c.Product.Name : null,
                    ProductNo = c.Product != null ? c.Product.ProductNo : null,
                    Size = c.Product != null ? c.Product.ProductSize : null,
                    PhotoUrl = c.Product != null && c.Product.ProductPhotos.Any() 
                               ? c.Product.ProductPhotos.OrderBy(p => p.SortOrder).First().PhotoUrl : null,
                    Quantity = c.Quantity,
                    Price = c.Price,
                    FactoryPrice = c.FactoryPrice,
                    LaborCost = c.LaborCost,
                    FactoryInputMaterialCost = c.FactoryInputMaterialCost,
                    FactoryInputLaborCost = c.FactoryInputLaborCost,
                    RetailerConfirmMaterialCost = c.RetailerConfirmMaterialCost,
                    RetailerConfirmLaborCost = c.RetailerConfirmLaborCost,
                    Weight = c.Product != null ? c.Product.Weight : 0,
                    OrderWeight = c.OrderWeight,
                    ActualWeight = c.ActualWeight,
                    InspectionMemo = c.InspectionMemo,
                    Purity = c.Purity,
                    Color = c.Color,
                    IsAsOrder = c.IsAsOrder,
                    ConfirmedWeight = c.ConfirmedWeight,
                    LogisticsMemo = c.LogisticsMemo,
                    ApprovedWeight = c.ApprovedWeight,
                    ApprovedMemo = c.ApprovedMemo,
                    RequestedWeight = c.RequestedWeight,
                    RequestedMemo = c.RequestedMemo,
                    SettlementRatio = c.SettlementRatio,
                    SettlementAmount = c.SettlementAmount,
                    SettlementMemo = c.SettlementMemo,
                    ManufacturerName = c.Product != null && c.Product.Company != null ? c.Product.Company.Name : null,
                    IsStockExhausted = c.ExhaustedStocks.Any(),
                    StockNo = c.ExhaustedStocks.Select(s => s.StockNo).FirstOrDefault(),
                    CategoryLarge = c.Product != null ? c.Product.CategoryLarge : null,
                    CategoryMedium = c.Product != null ? c.Product.CategoryMedium : null,
                    CategorySmall = c.Product != null ? c.Product.CategorySmall : null
                }).ToList()
            }).ToList(),
            StatusHistory = o.StatusHistories.Select(h => new OrderStatusHistoryDto
            {
                Id = h.Id,
                OrderId = h.OrderId,
                Status = h.Status,
                UserId = h.UserId,
                UserName = h.User != null ? h.User.Username : null,
                UserDisplayName = h.User != null ? h.User.Name : null,
                CompanyName = h.User != null && h.User.UserCompanies.Any() ? h.User.UserCompanies.First().Company!.Name : null,
                CompanyType = h.User != null && h.User.UserCompanies.Any() ? h.User.UserCompanies.First().Company!.Category : null,
                Remarks = h.Remarks,
                CreatedAt = h.CreatedAt
            }).OrderByDescending(h => h.CreatedAt).ToList()
        });
    }
}

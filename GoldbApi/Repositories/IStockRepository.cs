using GoldbApi.DTOs;
using GoldbApi.Models;

namespace GoldbApi.Repositories;

public interface IStockRepository : IRepository<Stock>
{
    Task<Stock?> GetByIdWithDetailsAsync(int id);
    Task<Stock?> GetByStockNoAsync(string stockNo);
    Task<(List<StockDto> Items, int TotalCount)> GetStocksAsync(StockQueryDto query);
    Task<List<Stock>> GetStocksForSummaryAsync(StockQueryDto query);
    Task<List<Stock>> GetStocksByIdsAsync(List<int> ids);
    Task<List<OrderStatusHistoryDto>> GetOrderHistoryForStockAsync(int orderId);
    Task<Order?> GetOrderWithItemsAsync(int orderId);
    Task<UserCompany?> GetUserCompanyAsync(int userId);
    Task<OrderItem?> GetOrderItemWithOrderAndExhaustedStocksAsync(int orderItemId);
    Task<List<OrderItem>> GetChildOrderItemsAsync(int parentId);
    Task<Order?> GetOrderWithItemsAndExhaustedStocksAsync(int orderId);
    Task<OrderItem?> GetOrderItemWithExhaustedStocksAsync(int orderItemId);
    Task<List<Attachment>> GetAttachmentsByIdsAsync(List<int> ids);

    Task AddOrderStatusHistoryAsync(OrderStatusHistory history);
}

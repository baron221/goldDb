using GoldbApi.DTOs;
using GoldbApi.Models;

namespace GoldbApi.Repositories;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByIdWithItemsAsync(int id);
    Task<UserCompany?> GetUserCompanyInfoAsync(int userId);
    Task<List<CartItem>> GetCartItemsForUserAsync(int userId, List<int> cartItemIds);
    Task<int> GetTodayOrderCountAsync(DateTime start, DateTime end);
    Task<ProductSet?> GetProductSetWithItemsAsync(int productSetId);
    Task<(List<OrderDto> Items, int TotalCount)> GetMyOrdersAsync(int userId, OrderQueryDto query);
    Task<(List<OrderDto> Items, int TotalCount)> GetAllOrdersAsync(OrderQueryDto query);
    Task<List<OrderItem>> GetOrderItemsByIdsAsync(int orderId, List<int> itemIds);
    Task<List<OrderItem>> GetTopLevelOrderItemsAsync(int orderId);
    Task<List<OrderStatusHistoryDto>> GetOrderHistoryAsync(int orderId);
    Task<OrderStatement?> GetOrderStatementAsync(int orderId);
    Task<OrderStatementDto?> GetOrderStatementDtoAsync(int orderId);
    Task<OrderStatementDto?> GetOrderStatementPdfDtoAsync(int orderId);
    Task<List<Order>> GetSettlementOrdersAsync(OrderQueryDto query);
    Task<SettlementHistorySummaryDto> GetSettlementSummaryAsync(OrderQueryDto query);
    Task AddStatusHistoryAsync(OrderStatusHistory history);
    Task AddOrderStatementAsync(OrderStatement statement);
    Task AddReceivableAsync(Receivable receivable);
    void RemoveCartItems(List<CartItem> cartItems);
}

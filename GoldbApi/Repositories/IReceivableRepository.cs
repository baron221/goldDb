using GoldbApi.DTOs;
using GoldbApi.Models;

namespace GoldbApi.Repositories;

public interface IReceivableRepository : IRepository<Receivable>
{
    Task<User?> GetUserWithCompaniesAsync(int userId);
    Task<List<Company>> GetManagedRetailersAsync(int logisticsCompanyId);
    Task<List<Receivable>> GetReceivablesForLogisticsAsync(List<int> retailerIds);
    Task<List<int>> GetUserIdsWithReceivablesAsync();
    Task<int> GetUsersWithReceivablesCountAsync(string? search, List<int> userIdsWithReceivables, string? companyCategory = null, List<int>? allowedCompanyIds = null);
    Task<List<User>> GetUsersWithReceivablesPagedAsync(int page, int pageSize, string? search, List<int> userIdsWithReceivables, string? companyCategory = null, List<int>? allowedCompanyIds = null);
    Task<List<Receivable>> GetReceivablesForUserAsync(int userId);
    Task<(List<ReceivableDto> Items, int TotalCount)> GetReceivablesPagedAsync(ReceivableQueryDto query, List<int>? allowedUserIds = null);
    Task<List<Receivable>> GetTargetChargesAsync(int userId, int orderId);
    Task<List<Receivable>> GetTargetChargesForOrdersAsync(int userId, List<int> orderIds);
    Task<List<Receivable>> GetOutstandingChargesAsync(int userId);
    Task<List<Receivable>> GetOutstandingChargesWithOrderItemsAsync(int userId);
}

using GoldbApi.DTOs;
using GoldbApi.Models;

namespace GoldbApi.Repositories;

public interface IReceivableRepository : IRepository<Receivable>
{
    Task<User?> GetUserWithCompaniesAsync(int userId);
    Task<List<Company>> GetManagedRetailersAsync(int logisticsCompanyId);
    Task<List<Receivable>> GetReceivablesForLogisticsAsync(List<int> retailerIds);
    Task<List<int>> GetUserIdsWithReceivablesAsync();
    Task<int> GetUsersWithReceivablesCountAsync(string? search, List<int> userIdsWithReceivables);
    Task<List<User>> GetUsersWithReceivablesPagedAsync(int page, int pageSize, string? search, List<int> userIdsWithReceivables);
    Task<List<Receivable>> GetReceivablesForUserAsync(int userId);
    Task<(List<ReceivableDto> Items, int TotalCount)> GetReceivablesPagedAsync(ReceivableQueryDto query);
    Task<List<Receivable>> GetTargetChargesAsync(int userId, int orderId);
    Task<List<Receivable>> GetOutstandingChargesAsync(int userId);
}

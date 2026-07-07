using GoldbApi.Data;
using GoldbApi.DTOs;
using GoldbApi.Models;
using GoldbApi.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GoldbApi.Services;

public interface IMockDataService
{
    Task<ApiResponse<List<RoleResponse>>> GetRolesAsync();
    Task<ApiResponse<TransactionListResponse>> GetTransactionsAsync();
    Task<ApiResponse<UserSearchResponse>> SearchUsersAsync(string? name);
}

public class MockDataService : IMockDataService
{
    private readonly IRepository<Role> _roleRepository;
    private readonly IRepository<Transaction> _transactionRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IAuthService _authService;

    public MockDataService(
        IRepository<Role> roleRepository,
        IRepository<Transaction> transactionRepository,
        IRepository<User> userRepository,
        IAuthService authService)
    {
        _roleRepository = roleRepository;
        _transactionRepository = transactionRepository;
        _userRepository = userRepository;
        _authService = authService;
    }

    public async Task<ApiResponse<List<RoleResponse>>> GetRolesAsync()
    {
        var roles = await _roleRepository.GetAllAsync();
        var menusResponse = await _authService.GetMenusAsync();
        var allMenus = menusResponse.Data ?? new List<MenuResponse>();

        var result = roles.Select(r => new RoleResponse
        {
            Key = r.Key,
            Name = r.Name,
            Description = r.Description ?? "",
            Routes = FilterRoutesForRole(r.Key, allMenus)
        }).ToList();

        return ApiResponse<List<RoleResponse>>.Success(result);
    }

    private List<MenuResponse> FilterRoutesForRole(string roleKey, List<MenuResponse> routes)
    {
        return routes;
    }

    public async Task<ApiResponse<TransactionListResponse>> GetTransactionsAsync()
    {
        var items = await _transactionRepository.GetQueryable().OrderByDescending(t => t.Id).ToListAsync();
        return ApiResponse<TransactionListResponse>.Success(new TransactionListResponse
        {
            Total = items.Count,
            Items = items
        });
    }

    public async Task<ApiResponse<UserSearchResponse>> SearchUsersAsync(string? name)
    {
        var users = await _userRepository.GetQueryable()
            .Where(u => string.IsNullOrEmpty(name) || u.Name.Contains(name))
            .Select(u => new UserSearchItem { Name = u.Name })
            .ToListAsync();

        return ApiResponse<UserSearchResponse>.Success(new UserSearchResponse { Items = users });
    }
}

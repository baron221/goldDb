using GoldbApi.Data;
using GoldbApi.DTOs;
using GoldbApi.Models;
using GoldbApi.Repositories;
using Microsoft.EntityFrameworkCore;
using Mapster;

namespace GoldbApi.Services;

public class UserMenuSettingService : IUserMenuSettingService
{
    private readonly IRepository<UserMenuSetting> _userMenuSettingRepository;
    private readonly ICurrentUserService _currentUserService;

    public UserMenuSettingService(IRepository<UserMenuSetting> userMenuSettingRepository, ICurrentUserService currentUserService)
    {
        _userMenuSettingRepository = userMenuSettingRepository;
        _currentUserService = currentUserService;
    }

    private int? GetCurrentUserId()
    {
        return _currentUserService.UserId;
    }

    public async Task<ApiResponse<List<UserMenuSettingDto>>> GetUserMenuSettingsAsync()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return ApiResponse<List<UserMenuSettingDto>>.Failure("Unauthorized", 401);

        var settings = await _userMenuSettingRepository.GetQueryable()
            .Where(s => s.UserId == userId.Value)
            .OrderBy(s => s.SortOrder)
            .ProjectToType<UserMenuSettingDto>()
            .ToListAsync();

        return ApiResponse<List<UserMenuSettingDto>>.Success(settings);
    }

    public async Task<ApiResponse<bool>> UpdateUserMenuSettingAsync(UserMenuSettingUpdateRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return ApiResponse<bool>.Failure("Unauthorized", 401);

        var setting = await _userMenuSettingRepository.GetQueryable()
            .FirstOrDefaultAsync(s => s.UserId == userId.Value && s.MenuId == request.MenuId);

        if (setting == null)
        {
            var maxSort = await _userMenuSettingRepository.GetQueryable()
                .Where(s => s.UserId == userId.Value)
                .Select(s => (int?)s.SortOrder)
                .MaxAsync() ?? 0;

            setting = new UserMenuSetting
            {
                UserId = userId.Value,
                MenuId = request.MenuId,
                Affix = request.Affix,
                SortOrder = request.SortOrder ?? (maxSort + 1)
            };
            await _userMenuSettingRepository.AddAsync(setting);
        }
        else
        {
            if (request.Affix.HasValue) setting.Affix = request.Affix;
            if (request.SortOrder.HasValue) setting.SortOrder = request.SortOrder.Value;
        }

        await _userMenuSettingRepository.SaveChangesAsync();
        return ApiResponse<bool>.Success(true);
    }

    public async Task<ApiResponse<bool>> UpdateSortOrderAsync(UserMenuSettingSortRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return ApiResponse<bool>.Failure("Unauthorized", 401);

        var settings = await _userMenuSettingRepository.GetQueryable()
            .Where(s => s.UserId == userId.Value)
            .ToListAsync();

        foreach (var item in request.Items)
        {
            var setting = settings.FirstOrDefault(s => s.MenuId == item.MenuId);
            if (setting != null)
            {
                setting.SortOrder = item.SortOrder;
            }
        }

        await _userMenuSettingRepository.SaveChangesAsync();
        return ApiResponse<bool>.Success(true);
    }
}

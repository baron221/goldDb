using GoldbApi.DTOs;

namespace GoldbApi.Services;

public interface IUserMenuSettingService
{
    Task<ApiResponse<List<UserMenuSettingDto>>> GetUserMenuSettingsAsync();
    Task<ApiResponse<bool>> UpdateUserMenuSettingAsync(UserMenuSettingUpdateRequest request);
    Task<ApiResponse<bool>> UpdateSortOrderAsync(UserMenuSettingSortRequest request);
}

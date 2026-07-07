using GoldbApi.DTOs;

namespace GoldbApi.Services;

public interface IUserPersonalSettingService
{
    Task<ApiResponse<UserPersonalSettingDto>> GetUserPersonalSettingAsync();
    Task<ApiResponse<bool>> SaveUserPersonalSettingAsync(UserPersonalSettingDto request);
}

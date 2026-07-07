using GoldbApi.Data;
using GoldbApi.DTOs;
using GoldbApi.Models;
using GoldbApi.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GoldbApi.Services;

public class UserPersonalSettingService : IUserPersonalSettingService
{
    private readonly IRepository<UserPersonalSetting> _userPersonalSettingRepository;
    private readonly ICurrentUserService _currentUserService;

    public UserPersonalSettingService(IRepository<UserPersonalSetting> userPersonalSettingRepository, ICurrentUserService currentUserService)
    {
        _userPersonalSettingRepository = userPersonalSettingRepository;
        _currentUserService = currentUserService;
    }

    private int? GetCurrentUserId()
    {
        return _currentUserService.UserId;
    }

    public async Task<ApiResponse<UserPersonalSettingDto>> GetUserPersonalSettingAsync()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return ApiResponse<UserPersonalSettingDto>.Failure("Unauthorized", 401);

        var setting = await _userPersonalSettingRepository.GetQueryable()
            .FirstOrDefaultAsync(s => s.UserId == userId.Value);

        if (setting == null)
        {
            return ApiResponse<UserPersonalSettingDto>.Success(null!);
        }

        var dto = new UserPersonalSettingDto
        {
            Size = setting.Size,
            TagsView = setting.TagsView,
            FixedHeader = setting.FixedHeader,
            SidebarLogo = setting.SidebarLogo,
            SecondMenuPopup = setting.SecondMenuPopup
        };

        return ApiResponse<UserPersonalSettingDto>.Success(dto);
    }

    public async Task<ApiResponse<bool>> SaveUserPersonalSettingAsync(UserPersonalSettingDto request)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return ApiResponse<bool>.Failure("Unauthorized", 401);

        var setting = await _userPersonalSettingRepository.GetQueryable()
            .FirstOrDefaultAsync(s => s.UserId == userId.Value);

        if (setting == null)
        {
            setting = new UserPersonalSetting
            {
                UserId = userId.Value,
                Size = request.Size,
                TagsView = request.TagsView,
                FixedHeader = request.FixedHeader,
                SidebarLogo = request.SidebarLogo,
                SecondMenuPopup = request.SecondMenuPopup
            };
            await _userPersonalSettingRepository.AddAsync(setting);
        }
        else
        {
            setting.Size = request.Size;
            setting.TagsView = request.TagsView;
            setting.FixedHeader = request.FixedHeader;
            setting.SidebarLogo = request.SidebarLogo;
            setting.SecondMenuPopup = request.SecondMenuPopup;
            _userPersonalSettingRepository.Update(setting);
        }

        await _userPersonalSettingRepository.SaveChangesAsync();
        return ApiResponse<bool>.Success(true);
    }
}

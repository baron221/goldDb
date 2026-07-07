using GoldbApi.Data;
using GoldbApi.DTOs;
using GoldbApi.Models;
using GoldbApi.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GoldbApi.Services;

public interface IRoleService
{

    Task<ApiResponse<List<RoleResponse>>> GetRolesAsync();

    Task<ApiResponse<RoleResponse>> CreateRoleAsync(RoleCreateRequest request);

    Task<ApiResponse<RoleResponse>> UpdateRoleAsync(int id, RoleUpdateRequest request);

    Task<ApiResponse<string>> DeleteRoleAsync(int id);

    Task<ApiResponse<List<MenuRoleResponse>>> GetRoleMenusAsync(string roleKey);

    Task<ApiResponse<string>> AssignMenusToRoleAsync(AssignMenusRequest request);

    Task<ApiResponse<List<UserRoleResponse>>> GetRoleUsersAsync(string roleKey);

    Task<ApiResponse<string>> AssignUsersToRoleAsync(AssignUsersRequest request);
}

public class RoleService : IRoleService
{
    private readonly IRepository<Role> _roleRepository;
    private readonly IRepository<Menu> _menuRepository;
    private readonly IRepository<MenuPermission> _menuPermissionRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<UserRole> _userRoleRepository;

    public RoleService(
        IRepository<Role> roleRepository,
        IRepository<Menu> menuRepository,
        IRepository<MenuPermission> menuPermissionRepository,
        IRepository<User> userRepository,
        IRepository<UserRole> userRoleRepository)
    {
        _roleRepository = roleRepository;
        _menuRepository = menuRepository;
        _menuPermissionRepository = menuPermissionRepository;
        _userRepository = userRepository;
        _userRoleRepository = userRoleRepository;
    }

    public async Task<ApiResponse<List<RoleResponse>>> GetRolesAsync()
    {
        var roles = await _roleRepository.GetQueryable()
            .Select(r => new RoleResponse
            {
                Id = r.Id,
                Key = r.Key,
                Name = r.Name,
                Description = r.Description
            })
            .ToListAsync();

        return ApiResponse<List<RoleResponse>>.Success(roles);
    }

    public async Task<ApiResponse<RoleResponse>> CreateRoleAsync(RoleCreateRequest request)
    {
        if (await _roleRepository.GetQueryable().AnyAsync(r => r.Key == request.Key))
        {
            return ApiResponse<RoleResponse>.Failure("Role key already exists.");
        }

        var role = new Role
        {
            Key = request.Key,
            Name = request.Name,
            Description = request.Description
        };

        await _roleRepository.AddAsync(role);
        await _roleRepository.SaveChangesAsync();

        return ApiResponse<RoleResponse>.Success(new RoleResponse
        {
            Id = role.Id,
            Key = role.Key,
            Name = role.Name,
            Description = role.Description
        });
    }

    public async Task<ApiResponse<RoleResponse>> UpdateRoleAsync(int id, RoleUpdateRequest request)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null) return ApiResponse<RoleResponse>.Failure("Role not found.", 404);

        role.Name = request.Name;
        role.Description = request.Description;

        await _roleRepository.SaveChangesAsync();

        return ApiResponse<RoleResponse>.Success(new RoleResponse
        {
            Id = role.Id,
            Key = role.Key,
            Name = role.Name,
            Description = role.Description
        });
    }

    public async Task<ApiResponse<string>> DeleteRoleAsync(int id)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null) return ApiResponse<string>.Failure("Role not found.", 404);

        _roleRepository.Delete(role);
        await _roleRepository.SaveChangesAsync();

        return ApiResponse<string>.Success("success");
    }

    public async Task<ApiResponse<List<MenuRoleResponse>>> GetRoleMenusAsync(string roleKey)
    {
        var menus = await _menuRepository.GetAllAsync();
        var permissions = await _menuPermissionRepository.GetQueryable()
            .Where(p => p.RoleKey == roleKey)
            .ToListAsync();

        var response = menus.Select(m =>
        {
            var p = permissions.FirstOrDefault(perm => perm.MenuId == m.Id);
            return new MenuRoleResponse
            {
                Id = m.Id,
                Title = m.Title,
                Path = m.Path,
                ParentId = m.ParentId,
                IsAssigned = p != null,
                CanSearch = p?.CanSearch ?? false,
                CanCreate = p?.CanCreate ?? false,
                CanDelete = p?.CanDelete ?? false,
                CanSave = p?.CanSave ?? false,
                CanPrint = p?.CanPrint ?? false,
                Custom1 = p?.Custom1 ?? false,
                Custom2 = p?.Custom2 ?? false,
                Custom3 = p?.Custom3 ?? false,
                Custom4 = p?.Custom4 ?? false,
                Custom5 = p?.Custom5 ?? false,
                Custom6 = p?.Custom6 ?? false,
                Custom7 = p?.Custom7 ?? false,
                Custom8 = p?.Custom8 ?? false
            };
        }).ToList();

        return ApiResponse<List<MenuRoleResponse>>.Success(response);
    }

    public async Task<ApiResponse<string>> AssignMenusToRoleAsync(AssignMenusRequest request)
    {
        using var transaction = await _roleRepository.BeginTransactionAsync();
        try
        {

            await _menuPermissionRepository.GetQueryable()
                .Where(p => p.RoleKey == request.RoleKey)
                .ExecuteDeleteAsync();

            var newPermissions = request.Permissions.Select(p => new MenuPermission
            {
                MenuId = p.MenuId,
                RoleKey = request.RoleKey,
                CanSearch = p.CanSearch,
                CanCreate = p.CanCreate,
                CanDelete = p.CanDelete,
                CanSave = p.CanSave,
                CanPrint = p.CanPrint,
                Custom1 = p.Custom1,
                Custom2 = p.Custom2,
                Custom3 = p.Custom3,
                Custom4 = p.Custom4,
                Custom5 = p.Custom5,
                Custom6 = p.Custom6,
                Custom7 = p.Custom7,
                Custom8 = p.Custom8
            }).ToList();

            foreach (var permission in newPermissions)
            {
                await _menuPermissionRepository.AddAsync(permission);
            }
            await _menuPermissionRepository.SaveChangesAsync();
            await transaction.CommitAsync();

            return ApiResponse<string>.Success("success");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return ApiResponse<string>.Failure($"Failed to assign menus: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<UserRoleResponse>>> GetRoleUsersAsync(string roleKey)
    {
        var role = await _roleRepository.GetQueryable().FirstOrDefaultAsync(r => r.Key == roleKey);
        if (role == null) return ApiResponse<List<UserRoleResponse>>.Failure("Role not found.", 404);

        var users = await _userRepository.GetQueryable()
            .Select(u => new UserRoleResponse
            {
                Id = u.Id,
                Username = u.Username,
                Name = u.Name,
                IsAssigned = _userRoleRepository.GetQueryable().Any(ur => ur.UserId == u.Id && ur.RoleId == role.Id)
            })
            .ToListAsync();

        return ApiResponse<List<UserRoleResponse>>.Success(users);
    }

    public async Task<ApiResponse<string>> AssignUsersToRoleAsync(AssignUsersRequest request)
    {
        var role = await _roleRepository.GetQueryable().FirstOrDefaultAsync(r => r.Key == request.RoleKey);
        if (role == null) return ApiResponse<string>.Failure("Role not found.", 404);

        using var transaction = await _roleRepository.BeginTransactionAsync();
        try
        {

            await _userRoleRepository.GetQueryable()
                .Where(ur => ur.RoleId == role.Id)
                .ExecuteDeleteAsync();

            var newAssignments = request.UserIds.Select(userId => new UserRole
            {
                UserId = userId,
                RoleId = role.Id
            }).ToList();

            foreach (var assignment in newAssignments)
            {
                await _userRoleRepository.AddAsync(assignment);
            }
            await _userRoleRepository.SaveChangesAsync();
            await transaction.CommitAsync();

            return ApiResponse<string>.Success("success");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return ApiResponse<string>.Failure($"Failed to assign users: {ex.Message}");
        }
    }
}

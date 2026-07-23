using GoldbApi.Data;
using GoldbApi.DTOs;
using GoldbApi.Models;
using GoldbApi.Repositories;
using GoldbApi.Utils;
using Microsoft.EntityFrameworkCore;
using Mapster;

namespace GoldbApi.Services;

public interface IUserService
{

    Task<ApiResponse<List<UserListItemResponse>>> GetUsersAsync(string? companyType = null, string? searchText = null, bool isUnassignedOnly = false, bool isLogisticsUnassigned = false, bool isPendingApprovalOnly = false);

    Task<ApiResponse<UserDetailResponse>> GetUserDetailAsync(int id);

    Task<ApiResponse<UserListItemResponse>> CreateUserAsync(UserCreateRequest request);

    Task<ApiResponse<string>> UpdateUserAsync(int id, UserUpdateRequest request);

    Task<ApiResponse<string>> DeleteUserAsync(int id);

    Task<ApiResponse<string>> ApproveUserAsync(int id);
    Task<ApiResponse<string>> UnapproveUserAsync(int id);
}

public class UserService : IUserService
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Role> _roleRepository;
    private readonly IRepository<UserRole> _userRoleRepository;
    private readonly IRepository<UserCompany> _userCompanyRepository;
    private readonly IRepository<UserEmail> _userEmailRepository;
    private readonly IRepository<UserPhone> _userPhoneRepository;
    private readonly IRepository<UserPhoto> _userPhotoRepository;
    private readonly IRepository<Company> _companyRepository;

    private readonly ICurrentUserService _currentUserService;

    public UserService(
        IRepository<User> userRepository,
        IRepository<Role> roleRepository,
        IRepository<UserRole> userRoleRepository,
        IRepository<UserCompany> userCompanyRepository,
        IRepository<UserEmail> userEmailRepository,
        IRepository<UserPhone> userPhoneRepository,
        IRepository<UserPhoto> userPhotoRepository,
        IRepository<Company> companyRepository,
        ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _userRoleRepository = userRoleRepository;
        _userCompanyRepository = userCompanyRepository;
        _userEmailRepository = userEmailRepository;
        _userPhoneRepository = userPhoneRepository;
        _companyRepository = companyRepository;
        _userPhotoRepository = userPhotoRepository;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<List<UserListItemResponse>>> GetUsersAsync(string? companyType = null, string? searchText = null, bool isUnassignedOnly = false, bool isLogisticsUnassigned = false, bool isPendingApprovalOnly = false)
    {
        var isAdmin = _currentUserService.IsAdmin;
        var currentUserId = _currentUserService.UserId;
        int? currentUserCompanyId = null;

        if (!isAdmin)
        {
            if (currentUserId == null) return ApiResponse<List<UserListItemResponse>>.Failure("Unauthorized", 401);
            var currentUserCompany = await _userCompanyRepository.GetQueryable().FirstOrDefaultAsync(uc => uc.UserId == currentUserId.Value && !uc.IsDeleted);
            if (currentUserCompany == null) return ApiResponse<List<UserListItemResponse>>.Failure("User has no company assigned", 403);
            currentUserCompanyId = currentUserCompany.CompanyId;
        }
        var query = _userRepository.GetQueryable()
            .Include(u => u.UserCompanies)
            .ThenInclude(uc => uc.Company)
                .ThenInclude(c => c!.LogisticsCompany)
            .AsQueryable();

        if (!isAdmin)
        {
            query = query.Where(u => u.UserCompanies.Any(uc => uc.CompanyId == currentUserCompanyId));
        }

        if (isPendingApprovalOnly)
        {
            query = query.Where(u => !u.IsApproved);
        }
        else if (isUnassignedOnly)
        {
            query = query.Where(u => u.UserType == "COMPANY" && !u.UserCompanies.Any());
        }
        else if (isLogisticsUnassigned)
        {
            query = query.Where(u => u.UserCompanies.Any(uc => uc.Company != null && uc.Company.Category == "RTL" && uc.Company.LogisticsCompanyId == null));
        }
        else if (!string.IsNullOrEmpty(companyType))
        {
            query = query.Where(u => u.UserCompanies.Any(uc => uc.Company != null && uc.Company.Category == companyType));
        }

        if (!string.IsNullOrEmpty(searchText))
        {
            query = query.Where(u => u.Username.Contains(searchText) || u.Name.Contains(searchText));
        }

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .ProjectToType<UserListItemResponse>()
            .ToListAsync();

        return ApiResponse<List<UserListItemResponse>>.Success(users);
    }

    public async Task<ApiResponse<UserDetailResponse>> GetUserDetailAsync(int id)
    {
        var isAdmin = _currentUserService.IsAdmin;
        var currentUserId = _currentUserService.UserId;
        int? currentUserCompanyId = null;

        if (!isAdmin)
        {
            if (currentUserId == null) return ApiResponse<UserDetailResponse>.Failure("Unauthorized", 401);
            var currentUserCompany = await _userCompanyRepository.GetQueryable().FirstOrDefaultAsync(uc => uc.UserId == currentUserId.Value && !uc.IsDeleted);
            if (currentUserCompany == null) return ApiResponse<UserDetailResponse>.Failure("User has no company assigned", 403);
            currentUserCompanyId = currentUserCompany.CompanyId;
        }

        var user = await _userRepository.GetQueryable()
            .Include(u => u.UserEmails)
            .Include(u => u.UserPhones)
            .Include(u => u.UserPhotos)
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Include(u => u.UserCompanies)
            .ThenInclude(uc => uc.Company)
                .ThenInclude(c => c!.LogisticsCompany)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) return ApiResponse<UserDetailResponse>.Failure("User not found", 404);

        if (!isAdmin)
        {
            if (!user.UserCompanies.Any(uc => uc.CompanyId == currentUserCompanyId))
                return ApiResponse<UserDetailResponse>.Failure("Forbidden", 403);
        }

        var response = user.Adapt<UserDetailResponse>();
        response.Ssn = string.IsNullOrEmpty(user.Ssn) ? user.Ssn : EncryptionUtils.Decrypt(user.Ssn);

        return ApiResponse<UserDetailResponse>.Success(response);
    }

    public async Task<ApiResponse<UserListItemResponse>> CreateUserAsync(UserCreateRequest request)
    {
        var isAdmin = _currentUserService.IsAdmin;
        var currentUserId = _currentUserService.UserId;

        if (!isAdmin)
        {
            if (currentUserId == null) return ApiResponse<UserListItemResponse>.Failure("Unauthorized", 401);
            var currentUserCompany = await _userCompanyRepository.GetQueryable().FirstOrDefaultAsync(uc => uc.UserId == currentUserId.Value && !uc.IsDeleted);
            if (currentUserCompany == null) return ApiResponse<UserListItemResponse>.Failure("User has no company assigned", 403);
            
            request.UserType = "COMPANY";
            request.CompanyId = currentUserCompany.CompanyId;
        }

        if (await _userRepository.GetQueryable().AnyAsync(u => u.Username == request.Username))
        {
            return ApiResponse<UserListItemResponse>.Failure("Username already exists");
        }

        var user = new User
        {
            Username = request.Username,
            Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Name = request.Name,
            UserType = request.UserType
        };

        if (request.UserType == "COMPANY" && request.CompanyId.HasValue)
        {
            user.UserCompanies.Add(new UserCompany { CompanyId = request.CompanyId.Value });

            // Give the new employee the role matching their company type so they
            // get the same menu access as the rest of their company. Without a
            // role the user has no menus and the app errors out on login.
            var company = await _companyRepository.GetByIdAsync(request.CompanyId.Value);
            var roleKey = company?.Category switch
            {
                "MFG" => "mfg",
                "DCC" => "dc",
                "RTL" => "editor",
                "ADMIN" => "admin",
                _ => null
            };
            if (roleKey != null)
            {
                var role = await _roleRepository.GetQueryable().FirstOrDefaultAsync(r => r.Key == roleKey);
                if (role != null) user.UserRoles.Add(new UserRole { RoleId = role.Id });
            }
        }

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return ApiResponse<UserListItemResponse>.Success(new UserListItemResponse
        {
            Id = user.Id,
            Username = user.Username,
            Name = user.Name,
            UserType = user.UserType,
            CreatedAt = user.CreatedAt
        });
    }

    public async Task<ApiResponse<string>> UpdateUserAsync(int id, UserUpdateRequest request)
    {
        var isAdmin = _currentUserService.IsAdmin;
        var currentUserId = _currentUserService.UserId;
        int? currentUserCompanyId = null;

        if (!isAdmin)
        {
            if (currentUserId == null) return ApiResponse<string>.Failure("Unauthorized", 401);
            var currentUserCompany = await _userCompanyRepository.GetQueryable().FirstOrDefaultAsync(uc => uc.UserId == currentUserId.Value && !uc.IsDeleted);
            if (currentUserCompany == null) return ApiResponse<string>.Failure("User has no company assigned", 403);
            currentUserCompanyId = currentUserCompany.CompanyId;
        }

        var user = await _userRepository.GetQueryable()
            .Include(u => u.UserCompanies)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) return ApiResponse<string>.Failure("User not found", 404);

        if (!isAdmin)
        {
            if (!user.UserCompanies.Any(uc => uc.CompanyId == currentUserCompanyId))
                return ApiResponse<string>.Failure("Forbidden", 403);
                
            request.UserType = "COMPANY";
            request.CompanyId = currentUserCompanyId;
        }

        using var transaction = await _userRepository.BeginTransactionAsync();
        try
        {

            user.Name = request.Name;
            user.Ssn = string.IsNullOrEmpty(request.Ssn) ? request.Ssn : EncryptionUtils.Encrypt(request.Ssn);
            user.ZipCode = request.ZipCode;
            user.AddressBase = request.AddressBase;
            user.AddressDetail = request.AddressDetail;
            user.Avatar = request.Avatar ?? "";
            user.AvatarId = request.AvatarId;
            user.Introduction = request.Introduction ?? "";
            user.SmsAllowed = request.SmsAllowed;
            user.KakaoAllowed = request.KakaoAllowed;
            user.EmailAllowed = request.EmailAllowed;
            user.UserType = request.UserType;

            _userRepository.Update(user);

            await _userCompanyRepository.GetQueryable().Where(uc => uc.UserId == user.Id).ExecuteDeleteAsync();
            if (request.CompanyId.HasValue)
            {
                user.UserCompanies.Add(new UserCompany { UserId = user.Id, CompanyId = request.CompanyId.Value });
            }

            await _userEmailRepository.GetQueryable().Where(e => e.UserId == user.Id).ExecuteDeleteAsync();
            user.UserEmails = request.Emails.Select(e => new UserEmail { Email = e.Email, IsPrimary = e.IsPrimary }).ToList();

            await _userPhoneRepository.GetQueryable().Where(p => p.UserId == user.Id).ExecuteDeleteAsync();
            user.UserPhones = request.Phones.Select(p => new UserPhone { PhoneNumber = p.PhoneNumber, IsPrimary = p.IsPrimary }).ToList();

            await _userPhotoRepository.GetQueryable().Where(p => p.UserId == user.Id).ExecuteDeleteAsync();
            user.UserPhotos = request.Photos.Select(p => new UserPhoto 
            { 
                PhotoUrl = p.PhotoUrl ?? "", 
                AttachmentId = p.AttachmentId,
                SortOrder = p.SortOrder 
            }).ToList();

            await _userRoleRepository.GetQueryable().Where(ur => ur.UserId == user.Id).ExecuteDeleteAsync();
            var roleEntities = await _roleRepository.GetQueryable().Where(r => request.Roles.Contains(r.Key)).ToListAsync();
            foreach (var role in roleEntities)
            {
                user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
            }

            await _userRepository.SaveChangesAsync();
            await transaction.CommitAsync();
            return ApiResponse<string>.Success("success");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return ApiResponse<string>.Failure($"Update failed: {ex.Message}");
        }
    }

    public async Task<ApiResponse<string>> DeleteUserAsync(int id)
    {
        var isAdmin = _currentUserService.IsAdmin;
        var currentUserId = _currentUserService.UserId;
        int? currentUserCompanyId = null;

        if (!isAdmin)
        {
            if (currentUserId == null) return ApiResponse<string>.Failure("Unauthorized", 401);
            var currentUserCompany = await _userCompanyRepository.GetQueryable().FirstOrDefaultAsync(uc => uc.UserId == currentUserId.Value && !uc.IsDeleted);
            if (currentUserCompany == null) return ApiResponse<string>.Failure("User has no company assigned", 403);
            currentUserCompanyId = currentUserCompany.CompanyId;
        }

        var user = await _userRepository.GetQueryable()
            .Include(u => u.UserCompanies)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) return ApiResponse<string>.Failure("User not found", 404);

        if (!isAdmin)
        {
            if (!user.UserCompanies.Any(uc => uc.CompanyId == currentUserCompanyId))
                return ApiResponse<string>.Failure("Forbidden", 403);
        }

        _userRepository.Delete(user);
        await _userRepository.SaveChangesAsync();
        return ApiResponse<string>.Success("success");
    }

    public async Task<ApiResponse<string>> ApproveUserAsync(int id)
    {
        var isAdmin = _currentUserService.IsAdmin;
        var currentUserId = _currentUserService.UserId;
        int? currentUserCompanyId = null;

        if (!isAdmin)
        {
            if (currentUserId == null) return ApiResponse<string>.Failure("Unauthorized", 401);
            var currentUserCompany = await _userCompanyRepository.GetQueryable().FirstOrDefaultAsync(uc => uc.UserId == currentUserId.Value && !uc.IsDeleted);
            if (currentUserCompany == null) return ApiResponse<string>.Failure("User has no company assigned", 403);
            currentUserCompanyId = currentUserCompany.CompanyId;
        }

        var user = await _userRepository.GetQueryable()
            .Include(u => u.UserCompanies)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) return ApiResponse<string>.Failure("User not found", 404);

        if (!isAdmin)
        {
            if (!user.UserCompanies.Any(uc => uc.CompanyId == currentUserCompanyId))
                return ApiResponse<string>.Failure("Forbidden", 403);
        }

        user.IsApproved = true;
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();
        return ApiResponse<string>.Success("success");
    }

    public async Task<ApiResponse<string>> UnapproveUserAsync(int id)
    {
        var isAdmin = _currentUserService.IsAdmin;
        var currentUserId = _currentUserService.UserId;
        int? currentUserCompanyId = null;

        if (!isAdmin)
        {
            if (currentUserId == null) return ApiResponse<string>.Failure("Unauthorized", 401);
            var currentUserCompany = await _userCompanyRepository.GetQueryable().FirstOrDefaultAsync(uc => uc.UserId == currentUserId.Value && !uc.IsDeleted);
            if (currentUserCompany == null) return ApiResponse<string>.Failure("User has no company assigned", 403);
            currentUserCompanyId = currentUserCompany.CompanyId;
        }

        var user = await _userRepository.GetQueryable()
            .Include(u => u.UserCompanies)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) return ApiResponse<string>.Failure("User not found", 404);

        if (!isAdmin)
        {
            if (!user.UserCompanies.Any(uc => uc.CompanyId == currentUserCompanyId))
                return ApiResponse<string>.Failure("Forbidden", 403);
        }

        user.IsApproved = false;
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();
        return ApiResponse<string>.Success("success");
    }
}

using GoldbApi.Data;
using GoldbApi.DTOs;
using GoldbApi.Models;
using GoldbApi.Repositories;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace GoldbApi.Services;

public interface ICompanyService
{

    Task<ApiResponse<CompanyListResponse>> GetCompaniesAsync(CompanyListRequest request);

    Task<ApiResponse<CompanyDto>> GetCompanyByIdAsync(int id);

    Task<ApiResponse<CompanyDto>> CreateCompanyAsync(CompanyCreateRequest request);

    Task<ApiResponse<string>> UpdateCompanyAsync(int id, CompanyUpdateRequest request);

    Task<ApiResponse<string>> DeleteCompanyAsync(int id);

    Task<ApiResponse<List<UserListItemResponse>>> GetCompanyUsersAsync(int companyId);

    Task<ApiResponse<string>> AddUserToCompanyAsync(int companyId, int userId);

    Task<ApiResponse<string>> RemoveUserFromCompanyAsync(int companyId, int userId);

    Task<ApiResponse<List<UserListItemResponse>>> GetAvailableUsersAsync();

    Task<ApiResponse<List<CompanyDto>>> GetRetailersByCenterAsync(int centerId);

    Task<ApiResponse<List<CompanyDto>>> GetUnassignedRetailersAsync();

    Task<ApiResponse<string>> AssignRetailersToCenterAsync(int centerId, List<int> retailerIds);

    Task<ApiResponse<List<CompanyDto>>> GetManufacturersByCenterAsync(int centerId);

    Task<ApiResponse<List<CompanyDto>>> GetUnassignedManufacturersAsync(int centerId);

    Task<ApiResponse<string>> AssignManufacturersToCenterAsync(int centerId, List<int> manufacturerIds);
}

public class CompanyService : ICompanyService
{
    private readonly IRepository<Company> _companyRepository;
    private readonly IRepository<UserCompany> _userCompanyRepository;
    private readonly IRepository<User> _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<ManufacturerLogistics> _manufacturerLogisticsRepository;

    public CompanyService(
        IRepository<Company> companyRepository,
        IRepository<UserCompany> userCompanyRepository,
        IRepository<User> userRepository,
        ICurrentUserService currentUserService,
        IRepository<ManufacturerLogistics> manufacturerLogisticsRepository)
    {
        _companyRepository = companyRepository;
        _userCompanyRepository = userCompanyRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _manufacturerLogisticsRepository = manufacturerLogisticsRepository;
    }

    public async Task<ApiResponse<CompanyListResponse>> GetCompaniesAsync(CompanyListRequest request)
    {
        var query = _companyRepository.GetQueryable()
            .Include(c => c.LogisticsCompany)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.Name))
            query = query.Where(c => c.Name.Contains(request.Name));

        if (!string.IsNullOrEmpty(request.CEO))
            query = query.Where(c => c.CEO.Contains(request.CEO));

        if (!string.IsNullOrEmpty(request.Phone))
            query = query.Where(c => c.Phone != null && c.Phone.Contains(request.Phone));

        if (!string.IsNullOrEmpty(request.Category))
            query = query.Where(c => c.Category == request.Category);

        if (!string.IsNullOrEmpty(request.Region))
            query = query.Where(c => c.Region == request.Region);

        if (request.IsDirectManagement.HasValue)
            query = query.Where(c => c.IsDirectManagement == request.IsDirectManagement.Value);

        var total = await query.CountAsync();
        int page = request.Page ?? 1;
        int pageSize = request.PageSize ?? 20;

        var items = await query
            .OrderByDescending(c => c.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectToType<CompanyDto>()
            .ToListAsync();

        return ApiResponse<CompanyListResponse>.Success(new CompanyListResponse
        {
            Total = total,
            Items = items
        });
    }

    public async Task<ApiResponse<CompanyDto>> GetCompanyByIdAsync(int id)
    {
        var c = await _companyRepository.GetQueryable()
            .Include(c => c.LogisticsCompany)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (c == null) return ApiResponse<CompanyDto>.Failure("Company not found", 404);

        return ApiResponse<CompanyDto>.Success(c.Adapt<CompanyDto>());
    }

    public async Task<ApiResponse<CompanyDto>> CreateCompanyAsync(CompanyCreateRequest request)
    {
        var company = request.Adapt<Company>();

        await _companyRepository.AddAsync(company);
        await _companyRepository.SaveChangesAsync();

        return ApiResponse<CompanyDto>.Success(company.Adapt<CompanyDto>());
    }

    public async Task<ApiResponse<string>> UpdateCompanyAsync(int id, CompanyUpdateRequest request)
    {
        var c = await _companyRepository.GetByIdAsync(id);
        if (c == null) return ApiResponse<string>.Failure("Company not found", 404);

        bool isAdmin = true;

        request.Adapt(c);

        if (!isAdmin)
        {

        }

        await _companyRepository.SaveChangesAsync();
        return ApiResponse<string>.Success("success");
    }

    public async Task<ApiResponse<List<UserListItemResponse>>> GetCompanyUsersAsync(int companyId)
    {
        var users = await _userCompanyRepository.GetQueryable()
            .Where(uc => uc.CompanyId == companyId)
            .Include(uc => uc.User)
            .Select(uc => uc.User)
            .ProjectToType<UserListItemResponse>()
            .ToListAsync();

        return ApiResponse<List<UserListItemResponse>>.Success(users);
    }

    public async Task<ApiResponse<string>> AddUserToCompanyAsync(int companyId, int userId)
    {
        if (await _userCompanyRepository.GetQueryable().AnyAsync(uc => uc.UserId == userId && uc.CompanyId == companyId))
        {
            return ApiResponse<string>.Failure("Already associated");
        }

        var uc = new UserCompany { UserId = userId, CompanyId = companyId };
        await _userCompanyRepository.AddAsync(uc);
        await _userCompanyRepository.SaveChangesAsync();

        return ApiResponse<string>.Success("success");
    }

    public async Task<ApiResponse<string>> RemoveUserFromCompanyAsync(int companyId, int userId)
    {
        await _userCompanyRepository.GetQueryable()
            .Where(uc => uc.UserId == userId && uc.CompanyId == companyId)
            .ExecuteDeleteAsync();

        return ApiResponse<string>.Success("success");
    }

    public async Task<ApiResponse<List<UserListItemResponse>>> GetAvailableUsersAsync()
    {
        var users = await _userRepository.GetQueryable()
            .Where(u => u.UserType == "COMPANY")
            .OrderBy(u => u.Name)
            .ProjectToType<UserListItemResponse>()
            .ToListAsync();

        return ApiResponse<List<UserListItemResponse>>.Success(users);
    }

    public async Task<ApiResponse<string>> DeleteCompanyAsync(int id)
    {
        var c = await _companyRepository.GetByIdAsync(id);
        if (c == null) return ApiResponse<string>.Failure("Company not found", 404);

        _companyRepository.Delete(c);
        await _companyRepository.SaveChangesAsync();
        return ApiResponse<string>.Success("success");
    }

    public async Task<ApiResponse<List<CompanyDto>>> GetRetailersByCenterAsync(int centerId)
    {
        var retailers = await _companyRepository.GetQueryable()
            .Where(c => c.Category == "RTL" && c.LogisticsCompanyId == centerId)
            .ProjectToType<CompanyDto>()
            .ToListAsync();

        return ApiResponse<List<CompanyDto>>.Success(retailers);
    }

    public async Task<ApiResponse<List<CompanyDto>>> GetUnassignedRetailersAsync()
    {
        var retailers = await _companyRepository.GetQueryable()
            .Where(c => c.Category == "RTL" && c.LogisticsCompanyId == null)
            .ProjectToType<CompanyDto>()
            .ToListAsync();

        return ApiResponse<List<CompanyDto>>.Success(retailers);
    }

    public async Task<ApiResponse<string>> AssignRetailersToCenterAsync(int centerId, List<int> retailerIds)
    {
        var retailersToAssign = await _companyRepository.GetQueryable()
            .Where(c => retailerIds.Contains(c.Id))
            .ToListAsync();

        foreach (var r in retailersToAssign)
        {
            r.LogisticsCompanyId = centerId;
        }

        var retailersToUnassign = await _companyRepository.GetQueryable()
            .Where(c => c.LogisticsCompanyId == centerId && !retailerIds.Contains(c.Id))
            .ToListAsync();

        foreach (var r in retailersToUnassign)
        {
            r.LogisticsCompanyId = null;
        }

        await _companyRepository.SaveChangesAsync();
        return ApiResponse<string>.Success("success");
    }

    public async Task<ApiResponse<List<CompanyDto>>> GetManufacturersByCenterAsync(int centerId)
    {
        var manufacturerIds = await _manufacturerLogisticsRepository.GetQueryable()
            .Where(ml => ml.LogisticsId == centerId)
            .Select(ml => ml.ManufacturerId)
            .ToListAsync();

        var manufacturers = await _companyRepository.GetQueryable()
            .Where(c => c.Category == "MFG" && manufacturerIds.Contains(c.Id))
            .ProjectToType<CompanyDto>()
            .ToListAsync();

        return ApiResponse<List<CompanyDto>>.Success(manufacturers);
    }

    public async Task<ApiResponse<List<CompanyDto>>> GetUnassignedManufacturersAsync(int centerId)
    {
        var manufacturerIds = await _manufacturerLogisticsRepository.GetQueryable()
            .Where(ml => ml.LogisticsId == centerId)
            .Select(ml => ml.ManufacturerId)
            .ToListAsync();

        var unassignedManufacturers = await _companyRepository.GetQueryable()
            .Where(c => c.Category == "MFG" && !manufacturerIds.Contains(c.Id))
            .ProjectToType<CompanyDto>()
            .ToListAsync();

        return ApiResponse<List<CompanyDto>>.Success(unassignedManufacturers);
    }

    public async Task<ApiResponse<string>> AssignManufacturersToCenterAsync(int centerId, List<int> manufacturerIds)
    {
        // Add new associations
        var existingAssignedIds = await _manufacturerLogisticsRepository.GetQueryable()
            .Where(ml => ml.LogisticsId == centerId)
            .Select(ml => ml.ManufacturerId)
            .ToListAsync();

        var toAdd = manufacturerIds.Except(existingAssignedIds).ToList();
        foreach (var mfgId in toAdd)
        {
            await _manufacturerLogisticsRepository.AddAsync(new ManufacturerLogistics
            {
                ManufacturerId = mfgId,
                LogisticsId = centerId
            });
        }

        var toRemove = existingAssignedIds.Except(manufacturerIds).ToList();
        if (toRemove.Any())
        {
            var removeEntities = await _manufacturerLogisticsRepository.GetQueryable()
                .Where(ml => ml.LogisticsId == centerId && toRemove.Contains(ml.ManufacturerId))
                .ToListAsync();

            foreach (var entity in removeEntities)
            {
                _manufacturerLogisticsRepository.Delete(entity);
            }
        }

        await _manufacturerLogisticsRepository.SaveChangesAsync();
        return ApiResponse<string>.Success("success");
    }
}

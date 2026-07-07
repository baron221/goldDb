using GoldbApi.DTOs;
using GoldbApi.Models;
using GoldbApi.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GoldbApi.Services;

public interface ICommonCodeService
{

    Task<ApiResponse<List<CommonCodeResponse>>> GetCodesAsync();

    Task<ApiResponse<string>> SaveCodeAsync(CommonCodeSaveRequest request);

    Task<ApiResponse<string>> DeleteCodeAsync(int id);

    Task<ApiResponse<List<CommonCodeResponse>>> GetCodesByGroupAsync(string groupCode);

    Task<ApiResponse<List<CommonCodeResponse>>> GetCodesByParentIdAsync(int parentId);
}

public class CommonCodeService : ICommonCodeService
{
    private readonly IRepository<CommonCode> _commonCodeRepository;

    public CommonCodeService(IRepository<CommonCode> commonCodeRepository)
    {
        _commonCodeRepository = commonCodeRepository;
    }

    public async Task<ApiResponse<List<CommonCodeResponse>>> GetCodesAsync()
    {
        var allCodes = await _commonCodeRepository.GetQueryable()
            .OrderBy(c => c.SortOrder)
            .ToListAsync();

        var rootCodes = allCodes.Where(c => c.ParentId == null).ToList();
        var result = rootCodes.Select(c => MapToResponse(c, allCodes)).ToList();

        return ApiResponse<List<CommonCodeResponse>>.Success(result);
    }

    public async Task<ApiResponse<string>> SaveCodeAsync(CommonCodeSaveRequest request)
    {
        CommonCode? code;
        if (request.Id.HasValue && request.Id > 0)
        {
            code = await _commonCodeRepository.GetByIdAsync(request.Id.Value);
            if (code == null) return ApiResponse<string>.Failure("Code not found", 404);
        }
        else
        {
            code = new CommonCode();
            await _commonCodeRepository.AddAsync(code);
        }

        code.ParentId = request.ParentId;
        code.Code = request.Code;
        code.Name = request.Name;
        code.Description = request.Description;
        code.SortOrder = request.SortOrder;
        code.IsActive = request.IsActive;
        code.Custom1 = request.Custom1;
        code.Custom2 = request.Custom2;
        code.Custom3 = request.Custom3;
        code.Custom4 = request.Custom4;

        await _commonCodeRepository.SaveChangesAsync();
        return ApiResponse<string>.Success("success");
    }

    public async Task<ApiResponse<string>> DeleteCodeAsync(int id)
    {
        var code = await _commonCodeRepository.GetByIdAsync(id);
        if (code == null) return ApiResponse<string>.Failure("Code not found", 404);

        var hasChildren = await _commonCodeRepository.GetQueryable().AnyAsync(c => c.ParentId == id);
        if (hasChildren) return ApiResponse<string>.Failure("Cannot delete code with children", 400);

        _commonCodeRepository.Delete(code);
        await _commonCodeRepository.SaveChangesAsync();
        return ApiResponse<string>.Success("success");
    }

    public async Task<ApiResponse<List<CommonCodeResponse>>> GetCodesByGroupAsync(string groupCode)
    {
        var group = await _commonCodeRepository.GetQueryable().FirstOrDefaultAsync(c => c.Code == groupCode && c.ParentId == null);
        if (group == null) return ApiResponse<List<CommonCodeResponse>>.Success(new List<CommonCodeResponse>());

        var children = await _commonCodeRepository.GetQueryable()
            .Where(c => c.ParentId == group.Id && c.IsActive)
            .OrderBy(c => c.SortOrder)
            .Select(c => new CommonCodeResponse
            {
                Id = c.Id,
                ParentId = c.ParentId,
                Code = c.Code,
                Name = c.Name,
                Description = c.Description,
                SortOrder = c.SortOrder,
                IsActive = c.IsActive,
                Custom1 = c.Custom1,
                Custom2 = c.Custom2,
                Custom3 = c.Custom3,
                Custom4 = c.Custom4
            })
            .ToListAsync();

        return ApiResponse<List<CommonCodeResponse>>.Success(children);
    }

    public async Task<ApiResponse<List<CommonCodeResponse>>> GetCodesByParentIdAsync(int parentId)
    {
        var children = await _commonCodeRepository.GetQueryable()
            .Where(c => c.ParentId == parentId && c.IsActive)
            .OrderBy(c => c.SortOrder)
            .Select(c => new CommonCodeResponse
            {
                Id = c.Id,
                ParentId = c.ParentId,
                Code = c.Code,
                Name = c.Name,
                Description = c.Description,
                SortOrder = c.SortOrder,
                IsActive = c.IsActive,
                Custom1 = c.Custom1,
                Custom2 = c.Custom2,
                Custom3 = c.Custom3,
                Custom4 = c.Custom4
            })
            .ToListAsync();

        return ApiResponse<List<CommonCodeResponse>>.Success(children);
    }

    private CommonCodeResponse MapToResponse(CommonCode code, List<CommonCode> allCodes)
    {
        var response = new CommonCodeResponse
        {
            Id = code.Id,
            ParentId = code.ParentId,
            Code = code.Code,
            Name = code.Name,
            Description = code.Description,
            SortOrder = code.SortOrder,
            IsActive = code.IsActive,
            Custom1 = code.Custom1,
            Custom2 = code.Custom2,
            Custom3 = code.Custom3,
            Custom4 = code.Custom4
        };

        var children = allCodes.Where(c => c.ParentId == code.Id).ToList();
        if (children.Any())
        {
            response.Children = children.Select(child => MapToResponse(child, allCodes)).ToList();
        }

        return response;
    }
}

using GoldbApi.Data;
using GoldbApi.DTOs;
using GoldbApi.Models;
using GoldbApi.Repositories;
using Microsoft.EntityFrameworkCore;
using Mapster;

namespace GoldbApi.Services;

public interface IProductSetService
{
    Task<ApiResponse<PagedResult<ProductSetDto>>> GetProductSetsAsync(ProductSetQueryDto query);
    Task<ApiResponse<ProductSetDto>> GetProductSetAsync(int id);
    Task<ApiResponse<ProductSetDto>> CreateProductSetAsync(CreateProductSetDto request);
    Task<ApiResponse<string>> UpdateProductSetAsync(int id, CreateProductSetDto request);
    Task<ApiResponse<string>> DeleteProductSetAsync(int id);
}

public class ProductSetService : IProductSetService
{
    private readonly IRepository<ProductSet> _productSetRepository;
    private readonly IRepository<ProductSetPhoto> _productSetPhotoRepository;
    private readonly IRepository<ProductSetItem> _productSetItemRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<UserCompany> _userCompanyRepository;
    private readonly IRepository<ManufacturerLogistics> _manufacturerLogisticsRepository;

    public ProductSetService(
        IRepository<ProductSet> productSetRepository,
        IRepository<ProductSetPhoto> productSetPhotoRepository,
        IRepository<ProductSetItem> productSetItemRepository,
        ICurrentUserService currentUserService,
        IRepository<UserCompany> userCompanyRepository,
        IRepository<ManufacturerLogistics> manufacturerLogisticsRepository)
    {
        _productSetRepository = productSetRepository;
        _productSetPhotoRepository = productSetPhotoRepository;
        _productSetItemRepository = productSetItemRepository;
        _currentUserService = currentUserService;
        _userCompanyRepository = userCompanyRepository;
        _manufacturerLogisticsRepository = manufacturerLogisticsRepository;
    }

    private async Task<List<int>> GetCurrentUserCompanyIdsAsync()
    {
        if (_currentUserService.IsAdmin) return new List<int>();

        var userId = _currentUserService.UserId;
        if (userId == null) throw new UnauthorizedAccessException();

        return await _userCompanyRepository.GetQueryable()
            .Where(uc => uc.UserId == userId.Value && !uc.IsDeleted)
            .Select(uc => uc.CompanyId)
            .ToListAsync();
    }

    public async Task<ApiResponse<PagedResult<ProductSetDto>>> GetProductSetsAsync(ProductSetQueryDto query)
    {
        var dbQuery = _productSetRepository.GetQueryable();

        var userId = _currentUserService.UserId;
        if (!_currentUserService.IsAdmin && userId.HasValue)
        {
            var userCompany = await _userCompanyRepository.GetQueryable()
                .Include(uc => uc.Company)
                .FirstOrDefaultAsync(uc => uc.UserId == userId.Value);

            if (userCompany?.Company != null)
            {
                if (userCompany.Company.Category == "DCC" || userCompany.Company.Category == "RTL")
                {
                    // A logistics center IS the center (use its own id); a retailer
                    // reaches its center through LogisticsCompanyId.
                    var logisticsId = userCompany.Company.Category == "DCC"
                        ? (int?)userCompany.Company.Id
                        : userCompany.Company.LogisticsCompanyId;
                    if (logisticsId.HasValue)
                    {
                        var allowedManufacturerIds = await _manufacturerLogisticsRepository.GetQueryable()
                            .Where(ml => ml.LogisticsId == logisticsId.Value)
                            .Select(ml => ml.ManufacturerId)
                            .ToListAsync();

                        dbQuery = dbQuery.Where(p => p.CompanyId.HasValue && allowedManufacturerIds.Contains(p.CompanyId.Value));
                    }
                    else
                    {
                        dbQuery = dbQuery.Where(p => false);
                    }
                }
                else if (userCompany.Company.Category == "MFG")
                {
                    var myCompanyIds = await GetCurrentUserCompanyIdsAsync();
                    dbQuery = dbQuery.Where(p => p.CompanyId.HasValue && myCompanyIds.Contains(p.CompanyId.Value));
                }
            }
        }

        if (query.IsPublic.HasValue)
            dbQuery = dbQuery.Where(s => s.IsPublic == query.IsPublic.Value);

        if (query.CompanyId.HasValue)
            dbQuery = dbQuery.Where(s => s.CompanyId == query.CompanyId.Value);

        if (!string.IsNullOrEmpty(query.Title))
        {
            var normalizedTitle = query.Title.Replace(" ", "").ToLower();
            dbQuery = dbQuery.Where(s =>
                s.Title.Replace(" ", "").ToLower().Contains(normalizedTitle));
        }

        if (!string.IsNullOrWhiteSpace(query.CategoryLarge))
            dbQuery = dbQuery.Where(s => s.CategoryLarge == query.CategoryLarge.Trim());

        if (!string.IsNullOrWhiteSpace(query.CategoryMedium))
            dbQuery = dbQuery.Where(s => s.CategoryMedium == query.CategoryMedium.Trim());

        if (!string.IsNullOrWhiteSpace(query.CategorySmall))
            dbQuery = dbQuery.Where(s => s.CategorySmall == query.CategorySmall.Trim());

        var totalCount = await dbQuery.CountAsync();
        var items = await dbQuery
            .OrderByDescending(s => s.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ProjectToType<ProductSetDto>()
            .ToListAsync();

        return ApiResponse<PagedResult<ProductSetDto>>.Success(new PagedResult<ProductSetDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        });
    }

    public async Task<ApiResponse<ProductSetDto>> GetProductSetAsync(int id)
    {
        var set = await _productSetRepository.GetQueryable()
            .Include(s => s.ProductSetPhotos)
            .Include(s => s.Company)
            .Include(s => s.SetItems)
            .ThenInclude(si => si.Product).ThenInclude(p => p!.ProductPhotos)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (set == null) return ApiResponse<ProductSetDto>.Failure("Product set not found", 404);

        if (!_currentUserService.IsAdmin && set.CompanyId.HasValue)
        {
            var userId = _currentUserService.UserId;
            if (userId.HasValue)
            {
                var userCompany = await _userCompanyRepository.GetQueryable()
                    .Include(uc => uc.Company)
                    .FirstOrDefaultAsync(uc => uc.UserId == userId.Value);

                if (userCompany?.Company != null)
                {
                    if (userCompany.Company.Category == "MFG")
                    {
                        var myCompanyIds = await GetCurrentUserCompanyIdsAsync();
                        if (!myCompanyIds.Contains(set.CompanyId.Value))
                            return ApiResponse<ProductSetDto>.Failure("Forbidden", 403);
                    }
                    else if (userCompany.Company.Category == "DCC" || userCompany.Company.Category == "RTL")
                    {
                        var logisticsId = userCompany.Company.Category == "DCC"
                            ? (int?)userCompany.Company.Id
                            : userCompany.Company.LogisticsCompanyId;
                        if (logisticsId.HasValue)
                        {
                            var isAllowed = await _manufacturerLogisticsRepository.GetQueryable()
                                .AnyAsync(ml => ml.LogisticsId == logisticsId.Value && ml.ManufacturerId == set.CompanyId.Value);
                            if (!isAllowed)
                                return ApiResponse<ProductSetDto>.Failure("Forbidden", 403);
                        }
                        else
                        {
                            return ApiResponse<ProductSetDto>.Failure("Forbidden", 403);
                        }
                    }
                }
            }
        }

        return ApiResponse<ProductSetDto>.Success(set.Adapt<ProductSetDto>());
    }

    public async Task<ApiResponse<ProductSetDto>> CreateProductSetAsync(CreateProductSetDto request)
    {
        if (!_currentUserService.IsAdmin)
        {
            var myCompanyIds = await GetCurrentUserCompanyIdsAsync();
            if (request.CompanyId.HasValue && !myCompanyIds.Contains(request.CompanyId.Value))
                return ApiResponse<ProductSetDto>.Failure("Forbidden", 403);
            if (!request.CompanyId.HasValue)
            {
                if (myCompanyIds.Count > 0) request.CompanyId = myCompanyIds[0];
                else return ApiResponse<ProductSetDto>.Failure("No company assigned", 403);
            }
        }

        var set = new ProductSet
        {
            Title = request.Title,
            SetNo = request.SetNo,
            Description = request.Description,
            CompanyId = request.CompanyId,
            BasicComponentCount = request.BasicComponentCount,
            PhotoUrl = string.Empty,
            IsPublic = request.IsPublic,
            FactoryPrice = request.FactoryPrice,
            LaborCost = request.LaborCost,
            CategoryLarge = request.CategoryLarge,
            CategoryMedium = request.CategoryMedium,
            CategorySmall = request.CategorySmall
        };

        foreach (var photoReq in request.Photos)
        {
            set.ProductSetPhotos.Add(new ProductSetPhoto
            {
                PhotoUrl = photoReq.PhotoUrl,
                AttachmentId = photoReq.AttachmentId,
                IsMain = photoReq.IsMain,
                SortOrder = photoReq.SortOrder
            });
        }

        foreach (var productId in request.ProductIds)
        {
            set.SetItems.Add(new ProductSetItem { ProductId = productId });
        }

        await _productSetRepository.AddAsync(set);
        await _productSetRepository.SaveChangesAsync();

        return await GetProductSetAsync(set.Id);
    }

    public async Task<ApiResponse<string>> UpdateProductSetAsync(int id, CreateProductSetDto request)
    {
        var set = await _productSetRepository.GetQueryable()
            .Include(s => s.ProductSetPhotos).Include(s => s.SetItems)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (set == null) return ApiResponse<string>.Failure("Product set not found", 404);

        if (!_currentUserService.IsAdmin)
        {
            var myCompanyIds = await GetCurrentUserCompanyIdsAsync();
            if (set.CompanyId.HasValue && !myCompanyIds.Contains(set.CompanyId.Value))
                return ApiResponse<string>.Failure("Forbidden", 403);
            if (request.CompanyId.HasValue && !myCompanyIds.Contains(request.CompanyId.Value))
                return ApiResponse<string>.Failure("Forbidden", 403);
        }

        set.Title = request.Title;
        set.SetNo = request.SetNo;
        set.Description = request.Description;
        set.CompanyId = request.CompanyId;
        set.BasicComponentCount = request.BasicComponentCount;
        set.IsPublic = request.IsPublic;
        set.FactoryPrice = request.FactoryPrice;
        set.LaborCost = request.LaborCost;
        set.CategoryLarge = request.CategoryLarge;
        set.CategoryMedium = request.CategoryMedium;
        set.CategorySmall = request.CategorySmall;

        foreach (var photo in set.ProductSetPhotos.ToList())
        {
            _productSetPhotoRepository.Delete(photo);
        }
        set.ProductSetPhotos.Clear();

        foreach (var photoReq in request.Photos)
        {
            set.ProductSetPhotos.Add(new ProductSetPhoto
            {
                PhotoUrl = photoReq.PhotoUrl,
                AttachmentId = photoReq.AttachmentId,
                IsMain = photoReq.IsMain,
                SortOrder = photoReq.SortOrder
            });
        }

        var existingProductIds = set.SetItems.Select(si => si.ProductId).ToList();
        var targetProductIds = request.ProductIds.Distinct().ToList();

        var itemsToRemove = set.SetItems.Where(si => !targetProductIds.Contains(si.ProductId)).ToList();
        foreach (var item in itemsToRemove)
        {
            set.SetItems.Remove(item);
        }

        var productIdsToAdd = targetProductIds.Where(pid => !existingProductIds.Contains(pid)).ToList();
        foreach (var productId in productIdsToAdd)
        {
            set.SetItems.Add(new ProductSetItem { ProductSetId = set.Id, ProductId = productId });
        }

        await _productSetRepository.SaveChangesAsync();
        return ApiResponse<string>.Success("success");
    }

    public async Task<ApiResponse<string>> DeleteProductSetAsync(int id)
    {
        var set = await _productSetRepository.GetByIdAsync(id);
        if (set == null) return ApiResponse<string>.Failure("Product set not found", 404);

        if (!_currentUserService.IsAdmin)
        {
            var myCompanyIds = await GetCurrentUserCompanyIdsAsync();
            if (set.CompanyId.HasValue && !myCompanyIds.Contains(set.CompanyId.Value))
                return ApiResponse<string>.Failure("Forbidden", 403);
        }

        _productSetRepository.Delete(set);
        await _productSetRepository.SaveChangesAsync();
        return ApiResponse<string>.Success("success");
    }
}

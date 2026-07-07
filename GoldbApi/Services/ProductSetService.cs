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

    public ProductSetService(
        IRepository<ProductSet> productSetRepository,
        IRepository<ProductSetPhoto> productSetPhotoRepository,
        IRepository<ProductSetItem> productSetItemRepository)
    {
        _productSetRepository = productSetRepository;
        _productSetPhotoRepository = productSetPhotoRepository;
        _productSetItemRepository = productSetItemRepository;
    }

    public async Task<ApiResponse<PagedResult<ProductSetDto>>> GetProductSetsAsync(ProductSetQueryDto query)
    {
        var dbQuery = _productSetRepository.GetQueryable();

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

        return ApiResponse<ProductSetDto>.Success(set.Adapt<ProductSetDto>());
    }

    public async Task<ApiResponse<ProductSetDto>> CreateProductSetAsync(CreateProductSetDto request)
    {
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

        _productSetRepository.Delete(set);
        await _productSetRepository.SaveChangesAsync();
        return ApiResponse<string>.Success("success");
    }
}

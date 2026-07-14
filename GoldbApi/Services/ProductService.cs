using GoldbApi.Data;
using GoldbApi.DTOs;
using GoldbApi.Models;
using GoldbApi.Repositories;
using Microsoft.EntityFrameworkCore;
using Mapster;

namespace GoldbApi.Services;

public interface IProductService
{

    Task<ApiResponse<PagedResult<ProductDto>>> GetProductsAsync(ProductQueryDto query);

    Task<ApiResponse<ProductDto>> GetProductAsync(int id);

    Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductDto request);

    Task<ApiResponse<string>> UpdateProductAsync(int id, CreateProductDto request);

    Task<ApiResponse<string>> DeleteProductAsync(int id);
}

public class ProductService : IProductService
{
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<ProductPhoto> _productPhotoRepository;
    private readonly IRepository<ProductOptionWeight> _productOptionWeightRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<UserCompany> _userCompanyRepository;
    private readonly IRepository<ManufacturerLogistics> _manufacturerLogisticsRepository;
    private readonly IRepository<Company> _companyRepository;

    public ProductService(
        IRepository<Product> productRepository,
        IRepository<ProductPhoto> productPhotoRepository,
        IRepository<ProductOptionWeight> productOptionWeightRepository,
        ICurrentUserService currentUserService,
        IRepository<UserCompany> userCompanyRepository,
        IRepository<ManufacturerLogistics> manufacturerLogisticsRepository,
        IRepository<Company> companyRepository)
    {
        _productRepository = productRepository;
        _productPhotoRepository = productPhotoRepository;
        _productOptionWeightRepository = productOptionWeightRepository;
        _currentUserService = currentUserService;
        _userCompanyRepository = userCompanyRepository;
        _manufacturerLogisticsRepository = manufacturerLogisticsRepository;
        _companyRepository = companyRepository;
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

    public async Task<ApiResponse<PagedResult<ProductDto>>> GetProductsAsync(ProductQueryDto query)
    {
        var dbQuery = _productRepository.GetQueryable();

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

        if (!string.IsNullOrEmpty(query.Name))
        {
            var normalizedName = query.Name.Replace(" ", "").ToLower();
            dbQuery = dbQuery.Where(p => p.NormalizedName.Contains(normalizedName));
        }

        if (!string.IsNullOrEmpty(query.ProductNo))
        {
            var normalizedNo = query.ProductNo.Replace(" ", "").ToLower();
            dbQuery = dbQuery.Where(p => p.NormalizedProductNo != null && p.NormalizedProductNo.Contains(normalizedNo));
        }

        if (query.MinWeight.HasValue)
            dbQuery = dbQuery.Where(p => p.Weight >= query.MinWeight.Value);

        if (query.MaxWeight.HasValue)
            dbQuery = dbQuery.Where(p => p.Weight <= query.MaxWeight.Value);

        if (!string.IsNullOrWhiteSpace(query.CategoryLarge))
        {
            var categoryLarge = query.CategoryLarge.Trim();
            dbQuery = dbQuery.Where(p => p.CategoryLarge == categoryLarge);
        }

        if (!string.IsNullOrWhiteSpace(query.CategoryMedium))
        {
            var categoryMedium = query.CategoryMedium.Trim();
            dbQuery = dbQuery.Where(p => p.CategoryMedium == categoryMedium);
        }

        if (!string.IsNullOrWhiteSpace(query.CategorySmall))
        {
            var categorySmall = query.CategorySmall.Trim();
            dbQuery = dbQuery.Where(p => p.CategorySmall == categorySmall);
        }

        if (query.IsPublic.HasValue)
            dbQuery = dbQuery.Where(p => p.IsPublic == query.IsPublic.Value);

        if (query.CompanyId.HasValue)
            dbQuery = dbQuery.Where(p => p.CompanyId == query.CompanyId.Value);

        if (!string.IsNullOrWhiteSpace(query.Purity))
        {
            var purity = query.Purity.Trim();
            dbQuery = dbQuery.Where(p => p.Purity == purity);
        }

        if (query.MinSize.HasValue || query.MaxSize.HasValue)
        {
            int min = query.MinSize ?? 1;
            int max = query.MaxSize ?? 30;

            if (min <= max)
            {
                var possibleCodes = Enumerable.Range(min, max - min + 1)
                    .Select(num => $"SIZE_{num}")
                    .ToList();

                dbQuery = dbQuery.Where(p => p.Sizes != null && possibleCodes.Any(code => p.Sizes.Contains(code)));
            }
        }

        var totalCount = await dbQuery.CountAsync();
        var items = await dbQuery
            .OrderByDescending(p => p.CreatedAt)
            .Skip(((query.Page ?? 1) - 1) * (query.PageSize ?? 20))
            .Take(query.PageSize ?? 20)
            .ProjectToType<ProductDto>()
            .ToListAsync();

        return ApiResponse<PagedResult<ProductDto>>.Success(new PagedResult<ProductDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page ?? 1,
            PageSize = query.PageSize ?? 20
        });
    }

    public async Task<ApiResponse<ProductDto>> GetProductAsync(int id)
    {
        var p = await _productRepository.GetQueryable()
            .Include(p => p.Company)
            .Include(p => p.ProductPhotos)
            .Include(p => p.OptionWeights)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (p == null) return ApiResponse<ProductDto>.Failure("Product not found", 404);

        if (!_currentUserService.IsAdmin && p.CompanyId.HasValue)
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
                        if (!myCompanyIds.Contains(p.CompanyId.Value))
                            return ApiResponse<ProductDto>.Failure("Forbidden", 403);
                    }
                    else if (userCompany.Company.Category == "DCC" || userCompany.Company.Category == "RTL")
                    {
                        var logisticsId = userCompany.Company.Category == "DCC"
                            ? (int?)userCompany.Company.Id
                            : userCompany.Company.LogisticsCompanyId;
                        if (logisticsId.HasValue)
                        {
                            var isAllowed = await _manufacturerLogisticsRepository.GetQueryable()
                                .AnyAsync(ml => ml.LogisticsId == logisticsId.Value && ml.ManufacturerId == p.CompanyId.Value);
                            if (!isAllowed)
                                return ApiResponse<ProductDto>.Failure("Forbidden", 403);
                        }
                        else
                        {
                            return ApiResponse<ProductDto>.Failure("Forbidden", 403);
                        }
                    }
                }
            }
        }

        return ApiResponse<ProductDto>.Success(p.Adapt<ProductDto>());
    }

    public async Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductDto request)
    {
        if (!_currentUserService.IsAdmin)
        {
            var myCompanyIds = await GetCurrentUserCompanyIdsAsync();
            if (request.CompanyId.HasValue && !myCompanyIds.Contains(request.CompanyId.Value))
                return ApiResponse<ProductDto>.Failure("Forbidden", 403);
            if (!request.CompanyId.HasValue)
            {
                if (myCompanyIds.Count > 0) request.CompanyId = myCompanyIds[0];
                else return ApiResponse<ProductDto>.Failure("No company assigned", 403);
            }
        }
        var product = new Product
        {
            Name = request.Name,
            NormalizedName = request.Name.Replace(" ", "").ToLower(),
            ProductNo = request.ProductNo,
            NormalizedProductNo = request.ProductNo?.Replace(" ", "").ToLower(),
            Purity = request.Purity,
            Weight = request.Weight,
            CompanyId = request.CompanyId,
            CategoryLarge = request.CategoryLarge,
            CategoryMedium = request.CategoryMedium,
            CategorySmall = request.CategorySmall,
            FactoryPrice = request.FactoryPrice,
            ProductSize = request.ProductSize,
            BasicLoss = request.BasicLoss,
            CenterStone = request.CenterStone,
            CenterStoneShape = request.CenterStoneShape,
            CenterStoneSize = request.CenterStoneSize,
            CenterStoneCount = request.CenterStoneCount,
            SideStone = request.SideStone,
            SideStoneShape = request.SideStoneShape,
            SideStoneSize = request.SideStoneSize,
            SideStoneCount = request.SideStoneCount,
            Description = request.Description,
            SpecialNote = request.SpecialNote,
            IsPublic = request.IsPublic,
            LaborCost = request.LaborCost,
            Colors = request.Colors,
            Sizes = request.Sizes
        };

        foreach (var photoReq in request.Photos)
        {
            product.ProductPhotos.Add(new ProductPhoto
            {
                PhotoUrl = photoReq.PhotoUrl ?? string.Empty,
                AttachmentId = photoReq.AttachmentId,
                IsMain = photoReq.IsMain,
                SortOrder = photoReq.SortOrder
            });
        }

        if (request.OptionWeights != null)
        {
            foreach (var owReq in request.OptionWeights)
            {
                product.OptionWeights.Add(new ProductOptionWeight
                {
                    Purity = owReq.Purity,
                    Color = owReq.Color,
                    Weight = owReq.Weight
                });
            }
        }

        await _productRepository.AddAsync(product);
        await _productRepository.SaveChangesAsync();

        return await GetProductAsync(product.Id);
    }

    public async Task<ApiResponse<string>> UpdateProductAsync(int id, CreateProductDto request)
    {
        var product = await _productRepository.GetQueryable()
            .Include(p => p.ProductPhotos)
            .Include(p => p.OptionWeights)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) return ApiResponse<string>.Failure("Product not found", 404);

        if (!_currentUserService.IsAdmin)
        {
            var myCompanyIds = await GetCurrentUserCompanyIdsAsync();
            if (product.CompanyId.HasValue && !myCompanyIds.Contains(product.CompanyId.Value))
                return ApiResponse<string>.Failure("Forbidden", 403);
            if (request.CompanyId.HasValue && !myCompanyIds.Contains(request.CompanyId.Value))
                return ApiResponse<string>.Failure("Forbidden", 403);
        }

        product.Name = request.Name;
        product.NormalizedName = request.Name.Replace(" ", "").ToLower();
        product.ProductNo = request.ProductNo;
        product.NormalizedProductNo = request.ProductNo?.Replace(" ", "").ToLower();
        product.Purity = request.Purity;
        product.Weight = request.Weight;
        product.CompanyId = request.CompanyId;
        product.CategoryLarge = request.CategoryLarge;
        product.CategoryMedium = request.CategoryMedium;
        product.CategorySmall = request.CategorySmall;
        product.FactoryPrice = request.FactoryPrice;

        product.ProductSize = request.ProductSize;
        product.BasicLoss = request.BasicLoss;
        product.CenterStone = request.CenterStone;
        product.CenterStoneShape = request.CenterStoneShape;
        product.CenterStoneSize = request.CenterStoneSize;
        product.CenterStoneCount = request.CenterStoneCount;
        product.SideStone = request.SideStone;
        product.SideStoneShape = request.SideStoneShape;
        product.SideStoneSize = request.SideStoneSize;
        product.SideStoneCount = request.SideStoneCount;
        product.Description = request.Description;
        product.SpecialNote = request.SpecialNote;
        product.IsPublic = request.IsPublic;
        product.LaborCost = request.LaborCost;
        product.Colors = request.Colors;
        product.Sizes = request.Sizes;

        var photoReqIds = request.Photos
            .Where(r => r.Id.HasValue && r.Id.Value > 0)
            .Select(r => r.Id!.Value)
            .ToList();

        var photosToDelete = product.ProductPhotos
            .Where(p => !photoReqIds.Contains(p.Id))
            .ToList();

        foreach (var photo in photosToDelete)
        {
            _productPhotoRepository.Delete(photo);
        }

        foreach (var photoReq in request.Photos)
        {
            if (photoReq.Id.HasValue && photoReq.Id.Value > 0)
            {
                var existingPhoto = product.ProductPhotos.FirstOrDefault(p => p.Id == photoReq.Id.Value);
                if (existingPhoto != null)
                {
                    existingPhoto.PhotoUrl = photoReq.PhotoUrl ?? string.Empty;
                    existingPhoto.AttachmentId = photoReq.AttachmentId;
                    existingPhoto.IsMain = photoReq.IsMain;
                    existingPhoto.SortOrder = photoReq.SortOrder;
                }
            }
            else
            {
                product.ProductPhotos.Add(new ProductPhoto
                {
                    PhotoUrl = photoReq.PhotoUrl ?? string.Empty,
                    AttachmentId = photoReq.AttachmentId,
                    IsMain = photoReq.IsMain,
                    SortOrder = photoReq.SortOrder
                });
            }
        }

        if (request.OptionWeights != null)
        {
            var weightReqIds = request.OptionWeights
                .Where(r => r.Id > 0)
                .Select(r => r.Id)
                .ToList();

            var weightsToDelete = product.OptionWeights
                .Where(w => !weightReqIds.Contains(w.Id))
                .ToList();

            foreach (var weight in weightsToDelete)
            {
                _productOptionWeightRepository.Delete(weight);
            }

            foreach (var owReq in request.OptionWeights)
            {
                if (owReq.Id > 0)
                {
                    var existingWeight = product.OptionWeights.FirstOrDefault(w => w.Id == owReq.Id);
                    if (existingWeight != null)
                    {
                        existingWeight.Purity = owReq.Purity;
                        existingWeight.Color = owReq.Color;
                        existingWeight.Weight = owReq.Weight;
                    }
                }
                else
                {
                    product.OptionWeights.Add(new ProductOptionWeight
                    {
                        Purity = owReq.Purity,
                        Color = owReq.Color,
                        Weight = owReq.Weight
                    });
                }
            }
        }
        else
        {
            foreach (var weight in product.OptionWeights.ToList())
            {
                _productOptionWeightRepository.Delete(weight);
            }
        }

        await _productRepository.SaveChangesAsync();
        return ApiResponse<string>.Success("success");
    }

    public async Task<ApiResponse<string>> DeleteProductAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null) return ApiResponse<string>.Failure("Product not found", 404);

        if (!_currentUserService.IsAdmin)
        {
            var myCompanyIds = await GetCurrentUserCompanyIdsAsync();
            if (product.CompanyId.HasValue && !myCompanyIds.Contains(product.CompanyId.Value))
                return ApiResponse<string>.Failure("Forbidden", 403);
        }

        _productRepository.Delete(product);
        await _productRepository.SaveChangesAsync();
        return ApiResponse<string>.Success("success");
    }
}

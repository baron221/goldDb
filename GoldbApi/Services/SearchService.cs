using GoldbApi.Data;
using GoldbApi.DTOs;
using GoldbApi.Models;
using GoldbApi.Repositories;
using GoldbApi.Utils;
using Microsoft.EntityFrameworkCore;
using Mapster;

namespace GoldbApi.Services;

public interface ISearchService
{

    Task<ApiResponse<IntegratedSearchResultDto>> IntegratedSearchAsync(SearchQueryDto query);
}

public class SearchService : ISearchService
{
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<ProductSet> _productSetRepository;
    private readonly IRepository<Stock> _stockRepository;
    private readonly IRepository<Order> _orderRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<UserCompany> _userCompanyRepository;
    private readonly IRepository<ManufacturerLogistics> _manufacturerLogisticsRepository;

    public SearchService(
        IRepository<Product> productRepository,
        IRepository<ProductSet> productSetRepository,
        IRepository<Stock> stockRepository,
        IRepository<Order> orderRepository,
        ICurrentUserService currentUserService,
        IRepository<UserCompany> userCompanyRepository,
        IRepository<ManufacturerLogistics> manufacturerLogisticsRepository)
    {
        _productRepository = productRepository;
        _productSetRepository = productSetRepository;
        _stockRepository = stockRepository;
        _orderRepository = orderRepository;
        _currentUserService = currentUserService;
        _userCompanyRepository = userCompanyRepository;
        _manufacturerLogisticsRepository = manufacturerLogisticsRepository;
    }

    // Returns the manufacturer company ids whose products the current user is
    // allowed to see. null means "no restriction" (admin). An empty list means
    // the user may see nothing.
    private async Task<List<int>?> GetVisibleManufacturerCompanyIdsAsync()
    {
        if (_currentUserService.IsAdmin) return null;

        var userId = _currentUserService.UserId;
        if (userId == null) return new List<int>();

        var userCompany = await _userCompanyRepository.GetQueryable()
            .Include(uc => uc.Company)
            .FirstOrDefaultAsync(uc => uc.UserId == userId.Value && !uc.IsDeleted);

        if (userCompany?.Company == null) return new List<int>();

        var company = userCompany.Company;

        if (company.Category == "MFG")
        {
            // A manufacturer only sees its own products.
            return await _userCompanyRepository.GetQueryable()
                .Where(uc => uc.UserId == userId.Value && !uc.IsDeleted)
                .Select(uc => uc.CompanyId)
                .ToListAsync();
        }

        if (company.Category == "DCC" || company.Category == "RTL")
        {
            // A logistics center IS the center (its own id); a retailer reaches
            // its center through LogisticsCompanyId. Either way, only products
            // from manufacturers mapped to that center are visible.
            var logisticsId = company.Category == "DCC" ? (int?)company.Id : company.LogisticsCompanyId;
            if (logisticsId == null) return new List<int>();

            return await _manufacturerLogisticsRepository.GetQueryable()
                .Where(ml => ml.LogisticsId == logisticsId.Value)
                .Select(ml => ml.ManufacturerId)
                .ToListAsync();
        }

        return new List<int>();
    }

    public async Task<ApiResponse<IntegratedSearchResultDto>> IntegratedSearchAsync(SearchQueryDto queryDto)
    {
        var searchTerm = queryDto.Search ?? queryDto.Q;
        var normalizedQuery = searchTerm?.Replace(" ", "").ToLower();
        var result = new IntegratedSearchResultDto();

        // null = admin (no restriction); otherwise only products from these
        // manufacturer companies are visible to the current user.
        var visibleCompanyIds = await GetVisibleManufacturerCompanyIdsAsync();

        if (!string.IsNullOrEmpty(normalizedQuery) && string.IsNullOrEmpty(queryDto.Type))
        {

            var productPreview = _productRepository.GetQueryable()
                .Where(p => p.Name.Replace(" ", "").ToLower().Contains(normalizedQuery) ||
                            (p.ProductNo != null && p.ProductNo.Replace(" ", "").ToLower().Contains(normalizedQuery)));
            if (visibleCompanyIds != null)
                productPreview = productPreview.Where(p => p.CompanyId.HasValue && visibleCompanyIds.Contains(p.CompanyId.Value));
            result.Products = await productPreview
                .OrderByDescending(p => p.CreatedAt)
                .Take(20)
                .ProjectToType<ProductSearchResultDto>()
                .ToListAsync();

            var setPreview = _productSetRepository.GetQueryable()
                .Where(ps => ps.Title.Replace(" ", "").ToLower().Contains(normalizedQuery) ||
                             (ps.SetNo != null && ps.SetNo.Replace(" ", "").ToLower().Contains(normalizedQuery)));
            if (visibleCompanyIds != null)
                setPreview = setPreview.Where(ps => ps.CompanyId.HasValue && visibleCompanyIds.Contains(ps.CompanyId.Value));
            result.ProductSets = await setPreview
                .OrderByDescending(ps => ps.CreatedAt)
                .Take(20)
                .ProjectToType<ProductSetSearchResultDto>()
                .ToListAsync();

            result.Stocks = await _stockRepository.GetQueryable()
                .Where(s => s.StockNo.Replace(" ", "").ToLower().Contains(normalizedQuery) ||
                            (s.Product != null && s.Product.Name.Replace(" ", "").ToLower().Contains(normalizedQuery)))
                .OrderByDescending(s => s.CreatedAt)
                .Take(20)
                .ProjectToType<StockSearchResultDto>()
                .ToListAsync();

            result.Orders = await _orderRepository.GetQueryable()
                .Where(o => o.OrderNo.Replace(" ", "").ToLower().Contains(normalizedQuery) ||
                            (o.User != null && o.User.Name.Replace(" ", "").ToLower().Contains(normalizedQuery)))
                .OrderByDescending(o => o.CreatedAt)
                .Take(20)
                .ProjectToType<OrderSearchResultDto>()
                .ToListAsync();
        }

        var productQuery = _productRepository.GetQueryable();
        var setQuery = _productSetRepository.GetQueryable();

        if (visibleCompanyIds != null)
        {
            productQuery = productQuery.Where(p => p.CompanyId.HasValue && visibleCompanyIds.Contains(p.CompanyId.Value));
            setQuery = setQuery.Where(ps => ps.CompanyId.HasValue && visibleCompanyIds.Contains(ps.CompanyId.Value));
        }

        if (!string.IsNullOrEmpty(normalizedQuery))
        {
            productQuery = productQuery.Where(p => p.Name.Replace(" ", "").ToLower().Contains(normalizedQuery) || 
                                                 (p.ProductNo != null && p.ProductNo.Replace(" ", "").ToLower().Contains(normalizedQuery)));
            setQuery = setQuery.Where(ps => ps.Title.Replace(" ", "").ToLower().Contains(normalizedQuery) || 
                                           (ps.SetNo != null && ps.SetNo.Replace(" ", "").ToLower().Contains(normalizedQuery)));
        }

        if (!string.IsNullOrEmpty(queryDto.CategoryLarge))
        {
            productQuery = productQuery.Where(p => p.CategoryLarge == queryDto.CategoryLarge);
            setQuery = setQuery.Where(ps => ps.CategoryLarge == queryDto.CategoryLarge);
        }

        if (!string.IsNullOrEmpty(queryDto.CategoryMedium))
        {
            productQuery = productQuery.Where(p => p.CategoryMedium == queryDto.CategoryMedium);
            setQuery = setQuery.Where(ps => ps.CategoryMedium == queryDto.CategoryMedium);
        }

        if (!string.IsNullOrEmpty(queryDto.CategorySmall))
        {
            productQuery = productQuery.Where(p => p.CategorySmall == queryDto.CategorySmall);
            setQuery = setQuery.Where(ps => ps.CategorySmall == queryDto.CategorySmall);
        }

        if (queryDto.CompanyId.HasValue)
        {
            productQuery = productQuery.Where(p => p.CompanyId == queryDto.CompanyId);
        }

        var marketItems = new List<MarketItemDto>();

        if (queryDto.Type == "product")
        {
            result.Total = await productQuery.CountAsync();
            marketItems = await productQuery
                .OrderByDescending(p => p.CreatedAt)
                .Skip((queryDto.Page - 1) * queryDto.PageSize)
                .Take(queryDto.PageSize)
                .Select(p => new MarketItemDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    No = p.ProductNo,
                    IsSet = false,
                    CategoryName = p.CategoryLarge,
                    PhotoUrl = p.ProductPhotos.OrderByDescending(ph => ph.IsMain).ThenBy(ph => ph.SortOrder).Select(ph => ph.PhotoUrl).FirstOrDefault(),
                    Price = p.FactoryPrice
                }).ToListAsync();
        }
        else if (queryDto.Type == "set")
        {
            result.Total = await setQuery.CountAsync();
            marketItems = await setQuery
                .OrderByDescending(ps => ps.CreatedAt)
                .Skip((queryDto.Page - 1) * queryDto.PageSize)
                .Take(queryDto.PageSize)
                .Select(ps => new MarketItemDto
                {
                    Id = ps.Id,
                    Name = ps.Title,
                    No = ps.SetNo,
                    IsSet = true,
                    CategoryName = ps.CategoryLarge,
                    PhotoUrl = ps.ProductSetPhotos.OrderByDescending(ph => ph.IsMain).ThenBy(ph => ph.SortOrder).Select(ph => ph.PhotoUrl).FirstOrDefault()
                        ?? ps.SetItems.SelectMany(si => si.Product!.ProductPhotos).OrderByDescending(ph => ph.IsMain).ThenBy(ph => ph.SortOrder).Select(ph => ph.PhotoUrl).FirstOrDefault(),
                    Price = 0 
                }).ToListAsync();
        }
        else 
        {

            var prods = await productQuery.Select(p => new MarketItemDto { Id = p.Id, Name = p.Name, No = p.ProductNo, IsSet = false, CategoryName = p.CategoryLarge, PhotoUrl = p.ProductPhotos.OrderByDescending(ph => ph.IsMain).ThenBy(ph => ph.SortOrder).Select(ph => ph.PhotoUrl).FirstOrDefault(), Price = p.FactoryPrice }).ToListAsync();
            var sets = await setQuery.Select(ps => new MarketItemDto 
            { 
                Id = ps.Id, 
                Name = ps.Title, 
                No = ps.SetNo, 
                IsSet = true, 
                CategoryName = ps.CategoryLarge, 
                PhotoUrl = ps.ProductSetPhotos.OrderByDescending(ph => ph.IsMain).ThenBy(ph => ph.SortOrder).Select(ph => ph.PhotoUrl).FirstOrDefault()
                    ?? ps.SetItems.SelectMany(si => si.Product!.ProductPhotos).OrderByDescending(ph => ph.IsMain).ThenBy(ph => ph.SortOrder).Select(ph => ph.PhotoUrl).FirstOrDefault(), 
                Price = 0 
            }).ToListAsync();

            var combined = prods.Concat(sets).OrderByDescending(x => x.Id).ToList();
            result.Total = combined.Count();
            marketItems = combined.Skip((queryDto.Page - 1) * queryDto.PageSize).Take(queryDto.PageSize).ToList();
        }

        result.Items = marketItems;
        return ApiResponse<IntegratedSearchResultDto>.Success(result);
    }
}

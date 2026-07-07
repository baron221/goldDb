using GoldbApi.Data;
using GoldbApi.DTOs;
using GoldbApi.Models;
using GoldbApi.Repositories;
using Microsoft.EntityFrameworkCore;
using Mapster;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoldbApi.Services;

public interface ICatalogService
{

    Task<ApiResponse<PagedResult<CatalogDto>>> GetCatalogsAsync(CatalogQueryDto query);

    Task<ApiResponse<CatalogDto>> GetCatalogAsync(int id);

    Task<ApiResponse<CatalogDto>> CreateCatalogAsync(CreateCatalogDto request);

    Task<ApiResponse<string>> UpdateCatalogAsync(int id, CreateCatalogDto request);

    Task<ApiResponse<string>> DeleteCatalogAsync(int id);
}

public class CatalogService : ICatalogService
{
    private readonly IRepository<Catalog> _catalogRepository;
    private readonly IRepository<CatalogPage> _catalogPageRepository;

    public CatalogService(IRepository<Catalog> catalogRepository, IRepository<CatalogPage> catalogPageRepository)
    {
        _catalogRepository = catalogRepository;
        _catalogPageRepository = catalogPageRepository;
    }

    public async Task<ApiResponse<PagedResult<CatalogDto>>> GetCatalogsAsync(CatalogQueryDto query)
    {
        var dbQuery = _catalogRepository.GetQueryable();

        if (!string.IsNullOrEmpty(query.Title))
            dbQuery = dbQuery.Where(c => c.Title.Contains(query.Title));

        if (!string.IsNullOrEmpty(query.SecurityClass))
            dbQuery = dbQuery.Where(c => c.SecurityClass == query.SecurityClass);

        var totalCount = await dbQuery.CountAsync();
        var items = await dbQuery
            .OrderByDescending(c => c.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ProjectToType<CatalogDto>()
            .ToListAsync();

        return ApiResponse<PagedResult<CatalogDto>>.Success(new PagedResult<CatalogDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        });
    }

    public async Task<ApiResponse<CatalogDto>> GetCatalogAsync(int id)
    {
        var catalog = await _catalogRepository.GetQueryable()
            .Include(c => c.CatalogPages)
                .ThenInclude(p => p.Company)
            .Include(c => c.CatalogPages)
                .ThenInclude(p => p.CatalogPageProducts)
                    .ThenInclude(cpp => cpp.Product)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (catalog == null) return ApiResponse<CatalogDto>.Failure("Catalog not found", 404);

        return ApiResponse<CatalogDto>.Success(catalog.Adapt<CatalogDto>());
    }

    public async Task<ApiResponse<CatalogDto>> CreateCatalogAsync(CreateCatalogDto request)
    {
        var catalog = request.Adapt<Catalog>();

        catalog.CatalogPages = new List<CatalogPage>();
        if (request.Pages != null)
        {
            foreach (var pageReq in request.Pages)
            {
                var catalogPage = pageReq.Adapt<CatalogPage>();
                if (pageReq.ProductIds != null)
                {
                    foreach (var productId in pageReq.ProductIds)
                    {
                        catalogPage.CatalogPageProducts.Add(new CatalogPageProduct { ProductId = productId });
                    }
                }
                catalog.CatalogPages.Add(catalogPage);
            }
        }

        await _catalogRepository.AddAsync(catalog);
        await _catalogRepository.SaveChangesAsync();

        return await GetCatalogAsync(catalog.Id);
    }

    public async Task<ApiResponse<string>> UpdateCatalogAsync(int id, CreateCatalogDto request)
    {
        var catalog = await _catalogRepository.GetQueryable()
            .Include(c => c.CatalogPages)
                .ThenInclude(p => p.CatalogPageProducts)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (catalog == null) return ApiResponse<string>.Failure("Catalog not found", 404);

        request.Adapt(catalog);

        if (catalog.CatalogPages.Any())
        {
            foreach (var page in catalog.CatalogPages.ToList())
            {
                _catalogPageRepository.Delete(page);
            }
            catalog.CatalogPages.Clear();
        }

        if (request.Pages != null)
        {
            foreach (var pageReq in request.Pages)
            {
                var catalogPage = pageReq.Adapt<CatalogPage>();
                catalogPage.CatalogId = catalog.Id;

                if (pageReq.ProductIds != null)
                {
                    foreach (var productId in pageReq.ProductIds)
                    {
                        catalogPage.CatalogPageProducts.Add(new CatalogPageProduct { ProductId = productId });
                    }
                }

                catalog.CatalogPages.Add(catalogPage);
            }
        }

        await _catalogRepository.SaveChangesAsync();
        return ApiResponse<string>.Success("success");
    }

    public async Task<ApiResponse<string>> DeleteCatalogAsync(int id)
    {
        var catalog = await _catalogRepository.GetByIdAsync(id);
        if (catalog == null) return ApiResponse<string>.Failure("Catalog not found", 404);

        _catalogRepository.Delete(catalog);
        await _catalogRepository.SaveChangesAsync();
        return ApiResponse<string>.Success("success");
    }
}

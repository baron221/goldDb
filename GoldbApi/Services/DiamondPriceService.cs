using GoldbApi.DTOs;
using GoldbApi.Models;
using GoldbApi.Repositories;
using Microsoft.EntityFrameworkCore;
using Mapster;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoldbApi.Services;

public interface IDiamondPriceService
{
    Task<ApiResponse<List<DiamondPriceDto>>> GetDiamondPricesAsync(string? priceType = null);
    Task<ApiResponse<List<DiamondPriceDto>>> GetLatestDiamondPricesAsync(string priceType);
    Task<ApiResponse<DiamondPriceDto>> CreateDiamondPriceAsync(CreateDiamondPriceDto request);
    Task<ApiResponse<DiamondPriceDto>> UpdateDiamondPriceAsync(int id, UpdateDiamondPriceDto request);
    Task<ApiResponse<string>> DeleteDiamondPriceAsync(int id);
}

public class DiamondPriceService : IDiamondPriceService
{
    private readonly IRepository<DiamondPrice> _diamondPriceRepository;

    public DiamondPriceService(IRepository<DiamondPrice> diamondPriceRepository)
    {
        _diamondPriceRepository = diamondPriceRepository;
    }

    public async Task<ApiResponse<List<DiamondPriceDto>>> GetDiamondPricesAsync(string? priceType = null)
    {
        var query = _diamondPriceRepository.GetQueryable();

        if (!string.IsNullOrEmpty(priceType))
        {
            query = query.Where(p => p.PriceType == priceType);
        }

        var prices = await query
            .OrderByDescending(p => p.AnnouncedAt)
            .ThenBy(p => p.Size)
            .ProjectToType<DiamondPriceDto>()
            .ToListAsync();

        return ApiResponse<List<DiamondPriceDto>>.Success(prices);
    }

    public async Task<ApiResponse<List<DiamondPriceDto>>> GetLatestDiamondPricesAsync(string priceType)
    {
        var sizes = new[] { "1부", "2부", "3부", "4부", "5부", "6부", "7부", "8부", "9부" };
        var latestPrices = new List<DiamondPriceDto>();

        foreach (var size in sizes)
        {
            var price = await _diamondPriceRepository.GetQueryable()
                .Where(p => p.PriceType == priceType && p.Size == size)
                .OrderByDescending(p => p.AnnouncedAt)
                .ProjectToType<DiamondPriceDto>()
                .FirstOrDefaultAsync();

            if (price != null)
            {
                latestPrices.Add(price);
            }
        }

        return ApiResponse<List<DiamondPriceDto>>.Success(latestPrices);
    }

    public async Task<ApiResponse<DiamondPriceDto>> CreateDiamondPriceAsync(CreateDiamondPriceDto request)
    {
        var diamondPrice = request.Adapt<DiamondPrice>();

        await _diamondPriceRepository.AddAsync(diamondPrice);
        await _diamondPriceRepository.SaveChangesAsync();

        return ApiResponse<DiamondPriceDto>.Success(diamondPrice.Adapt<DiamondPriceDto>());
    }

    public async Task<ApiResponse<DiamondPriceDto>> UpdateDiamondPriceAsync(int id, UpdateDiamondPriceDto request)
    {
        var diamondPrice = await _diamondPriceRepository.GetByIdAsync(id);
        if (diamondPrice == null) return ApiResponse<DiamondPriceDto>.Failure("Price not found", 404);

        request.Adapt(diamondPrice);

        await _diamondPriceRepository.SaveChangesAsync();

        return ApiResponse<DiamondPriceDto>.Success(diamondPrice.Adapt<DiamondPriceDto>());
    }

    public async Task<ApiResponse<string>> DeleteDiamondPriceAsync(int id)
    {
        var price = await _diamondPriceRepository.GetByIdAsync(id);
        if (price == null) return ApiResponse<string>.Failure("Price not found", 404);

        _diamondPriceRepository.Delete(price);
        await _diamondPriceRepository.SaveChangesAsync();
        return ApiResponse<string>.Success("success");
    }
}

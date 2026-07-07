using GoldbApi.DTOs;
using GoldbApi.Models;
using GoldbApi.Repositories;
using Microsoft.EntityFrameworkCore;
using Mapster;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoldbApi.Services;

public interface IGoldPriceService
{
    Task<ApiResponse<List<GoldPriceDto>>> GetGoldPricesAsync();
    Task<ApiResponse<GoldPriceDto>> GetLatestGoldPriceAsync();
    Task<ApiResponse<GoldPriceDto>> CreateGoldPriceAsync(CreateGoldPriceDto request);
    Task<ApiResponse<GoldPriceDto>> UpdateGoldPriceAsync(int id, UpdateGoldPriceDto request);
    Task<ApiResponse<string>> DeleteGoldPriceAsync(int id);
}

public class GoldPriceService : IGoldPriceService
{
    private readonly IRepository<GoldPrice> _goldPriceRepository;

    public GoldPriceService(IRepository<GoldPrice> goldPriceRepository)
    {
        _goldPriceRepository = goldPriceRepository;
    }

    public async Task<ApiResponse<List<GoldPriceDto>>> GetGoldPricesAsync()
    {
        var prices = await _goldPriceRepository.GetQueryable()
            .OrderByDescending(p => p.AnnouncedAt)
            .ProjectToType<GoldPriceDto>()
            .ToListAsync();

        return ApiResponse<List<GoldPriceDto>>.Success(prices);
    }

    public async Task<ApiResponse<GoldPriceDto>> GetLatestGoldPriceAsync()
    {
        var price = await _goldPriceRepository.GetQueryable()
            .OrderByDescending(p => p.AnnouncedAt)
            .ProjectToType<GoldPriceDto>()
            .FirstOrDefaultAsync();

        if (price == null) return ApiResponse<GoldPriceDto>.Failure("No price data found", 404);

        return ApiResponse<GoldPriceDto>.Success(price);
    }

    public async Task<ApiResponse<GoldPriceDto>> CreateGoldPriceAsync(CreateGoldPriceDto request)
    {
        var goldPrice = request.Adapt<GoldPrice>();

        await _goldPriceRepository.AddAsync(goldPrice);
        await _goldPriceRepository.SaveChangesAsync();

        return ApiResponse<GoldPriceDto>.Success(goldPrice.Adapt<GoldPriceDto>());
    }

    public async Task<ApiResponse<GoldPriceDto>> UpdateGoldPriceAsync(int id, UpdateGoldPriceDto request)
    {
        var goldPrice = await _goldPriceRepository.GetByIdAsync(id);
        if (goldPrice == null) return ApiResponse<GoldPriceDto>.Failure("Price not found", 404);

        request.Adapt(goldPrice);

        await _goldPriceRepository.SaveChangesAsync();

        return ApiResponse<GoldPriceDto>.Success(goldPrice.Adapt<GoldPriceDto>());
    }

    public async Task<ApiResponse<string>> DeleteGoldPriceAsync(int id)
    {
        var price = await _goldPriceRepository.GetByIdAsync(id);
        if (price == null) return ApiResponse<string>.Failure("Price not found", 404);

        _goldPriceRepository.Delete(price);
        await _goldPriceRepository.SaveChangesAsync();

        return ApiResponse<string>.Success("success");
    }
}

using GoldbApi.DTOs;
using GoldbApi.Models;
using GoldbApi.Repositories;
using Microsoft.EntityFrameworkCore;
using Mapster;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GoldbApi.Services;

public interface IFavoriteService
{
    Task<ApiResponse<List<FavoriteDto>>> GetMyFavoritesAsync();
    Task<ApiResponse<FavoriteDto>> AddFavoriteAsync(CreateFavoriteDto request);
    Task<ApiResponse<string>> RemoveFavoriteAsync(int id);
    Task<ApiResponse<string>> RemoveFavoriteByProductAsync(int? productId, int? productSetId);
}

public class FavoriteService : IFavoriteService
{
    private readonly IRepository<Favorite> _favoriteRepository;
    private readonly ICurrentUserService _currentUserService;

    public FavoriteService(IRepository<Favorite> favoriteRepository, ICurrentUserService currentUserService)
    {
        _favoriteRepository = favoriteRepository;
        _currentUserService = currentUserService;
    }

    private int GetCurrentUserId()
    {
        return _currentUserService.UserId ?? throw new UnauthorizedAccessException("User is not authenticated");
    }

    public async Task<ApiResponse<List<FavoriteDto>>> GetMyFavoritesAsync()
    {
        var userId = GetCurrentUserId();
        var items = await _favoriteRepository.GetQueryable()
            .Include(f => f.Product).ThenInclude(p => p!.ProductPhotos)
            .Include(f => f.Product).ThenInclude(p => p!.Company)
            .Include(f => f.ProductSet).ThenInclude(ps => ps!.ProductSetPhotos)
            .Include(f => f.ProductSet).ThenInclude(ps => ps!.Company)
            .Include(f => f.ProductSet).ThenInclude(ps => ps!.SetItems).ThenInclude(psi => psi.Product).ThenInclude(p => p!.ProductPhotos)
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .ProjectToType<FavoriteDto>()
            .ToListAsync();

        return ApiResponse<List<FavoriteDto>>.Success(items);
    }

    public async Task<ApiResponse<FavoriteDto>> AddFavoriteAsync(CreateFavoriteDto request)
    {
        var userId = GetCurrentUserId();
        var existing = await _favoriteRepository.GetQueryable()
            .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == request.ProductId && f.ProductSetId == request.ProductSetId);

        if (existing != null) return ApiResponse<FavoriteDto>.Failure("Already added to favorites", 400);

        var favorite = request.Adapt<Favorite>();
        favorite.UserId = userId;

        await _favoriteRepository.AddAsync(favorite);
        await _favoriteRepository.SaveChangesAsync();

        return ApiResponse<FavoriteDto>.Success(favorite.Adapt<FavoriteDto>());
    }

    public async Task<ApiResponse<string>> RemoveFavoriteAsync(int id)
    {
        var userId = GetCurrentUserId();
        var favorite = await _favoriteRepository.GetQueryable()
            .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);
        if (favorite == null) return ApiResponse<string>.Failure("Favorite not found", 404);

        _favoriteRepository.Delete(favorite);
        await _favoriteRepository.SaveChangesAsync();

        return ApiResponse<string>.Success("success");
    }

    public async Task<ApiResponse<string>> RemoveFavoriteByProductAsync(int? productId, int? productSetId)
    {
        var userId = GetCurrentUserId();
        var favorite = await _favoriteRepository.GetQueryable()
            .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId && f.ProductSetId == productSetId);
        if (favorite == null) return ApiResponse<string>.Failure("Favorite not found", 404);

        _favoriteRepository.Delete(favorite);
        await _favoriteRepository.SaveChangesAsync();

        return ApiResponse<string>.Success("success");
    }
}

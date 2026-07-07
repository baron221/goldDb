using GoldbApi.Data;
using GoldbApi.DTOs;
using GoldbApi.Models;
using GoldbApi.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GoldbApi.Services;

public class MenuFavoriteService : IMenuFavoriteService
{
    private readonly IRepository<MenuFavorite> _menuFavoriteRepository;
    private readonly ICurrentUserService _currentUserService;

    public MenuFavoriteService(IRepository<MenuFavorite> menuFavoriteRepository, ICurrentUserService currentUserService)
    {
        _menuFavoriteRepository = menuFavoriteRepository;
        _currentUserService = currentUserService;
    }

    private int? GetCurrentUserId()
    {
        return _currentUserService.UserId;
    }

    public async Task<ApiResponse<List<int>>> GetFavoriteMenuIdsAsync()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return ApiResponse<List<int>>.Failure("Unauthorized", 401);

        var favoriteIds = await _menuFavoriteRepository.GetQueryable()
            .Where(f => f.UserId == userId.Value)
            .OrderBy(f => f.SortOrder)
            .Select(f => f.MenuId)
            .ToListAsync();

        return ApiResponse<List<int>>.Success(favoriteIds);
    }

    public async Task<ApiResponse<List<MenuFavoriteResponse>>> GetFavoritesAsync()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return ApiResponse<List<MenuFavoriteResponse>>.Failure("Unauthorized", 401);

        var favorites = await _menuFavoriteRepository.GetQueryable()
            .Include(f => f.Menu)
            .Where(f => f.UserId == userId.Value)
            .OrderBy(f => f.SortOrder)
            .Select(f => new MenuFavoriteResponse
            {
                MenuId = f.MenuId,
                Title = f.Menu != null ? f.Menu.Title : null,
                SortOrder = f.SortOrder
            })
            .ToListAsync();

        return ApiResponse<List<MenuFavoriteResponse>>.Success(favorites);
    }

    public async Task<ApiResponse<bool>> ToggleFavoriteAsync(int menuId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return ApiResponse<bool>.Failure("Unauthorized", 401);

        var existing = await _menuFavoriteRepository.GetQueryable()
            .FirstOrDefaultAsync(f => f.UserId == userId.Value && f.MenuId == menuId);

        if (existing != null)
        {
            _menuFavoriteRepository.Delete(existing);
            await _menuFavoriteRepository.SaveChangesAsync();
            return ApiResponse<bool>.Success(false);
        }
        else
        {
            var maxSort = await _menuFavoriteRepository.GetQueryable()
                .Where(f => f.UserId == userId.Value)
                .Select(f => (int?)f.SortOrder)
                .MaxAsync() ?? 0;

            await _menuFavoriteRepository.AddAsync(new MenuFavorite
            {
                UserId = userId.Value,
                MenuId = menuId,
                SortOrder = maxSort + 1
            });
            await _menuFavoriteRepository.SaveChangesAsync();
            return ApiResponse<bool>.Success(true);
        }
    }

    public async Task<ApiResponse<bool>> AddFavoriteAsync(int menuId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return ApiResponse<bool>.Failure("Unauthorized", 401);

        var exists = await _menuFavoriteRepository.GetQueryable()
            .AnyAsync(f => f.UserId == userId.Value && f.MenuId == menuId);

        if (!exists)
        {
            var maxSort = await _menuFavoriteRepository.GetQueryable()
                .Where(f => f.UserId == userId.Value)
                .Select(f => (int?)f.SortOrder)
                .MaxAsync() ?? 0;

            await _menuFavoriteRepository.AddAsync(new MenuFavorite
            {
                UserId = userId.Value,
                MenuId = menuId,
                SortOrder = maxSort + 1
            });
            await _menuFavoriteRepository.SaveChangesAsync();
        }

        return ApiResponse<bool>.Success(true);
    }

    public async Task<ApiResponse<bool>> UpdateSortOrderAsync(MenuFavoriteSortRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return ApiResponse<bool>.Failure("Unauthorized", 401);

        var favorites = await _menuFavoriteRepository.GetQueryable()
            .Where(f => f.UserId == userId.Value)
            .ToListAsync();

        foreach (var item in request.Items)
        {
            var favorite = favorites.FirstOrDefault(f => f.MenuId == item.MenuId);
            if (favorite != null)
            {
                favorite.SortOrder = item.SortOrder;
            }
        }

        await _menuFavoriteRepository.SaveChangesAsync();
        return ApiResponse<bool>.Success(true);
    }

    public async Task<ApiResponse<bool>> RemoveFavoriteAsync(int menuId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return ApiResponse<bool>.Failure("Unauthorized", 401);

        var favorite = await _menuFavoriteRepository.GetQueryable()
            .FirstOrDefaultAsync(f => f.UserId == userId.Value && f.MenuId == menuId);

        if (favorite != null)
        {
            _menuFavoriteRepository.Delete(favorite);
            await _menuFavoriteRepository.SaveChangesAsync();
        }

        return ApiResponse<bool>.Success(true);
    }
}

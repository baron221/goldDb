using GoldbApi.DTOs;

namespace GoldbApi.Services;

public interface IMenuFavoriteService
{
    Task<ApiResponse<List<int>>> GetFavoriteMenuIdsAsync();
    Task<ApiResponse<List<MenuFavoriteResponse>>> GetFavoritesAsync();
    Task<ApiResponse<bool>> ToggleFavoriteAsync(int menuId);
    Task<ApiResponse<bool>> AddFavoriteAsync(int menuId);
    Task<ApiResponse<bool>> RemoveFavoriteAsync(int menuId);
    Task<ApiResponse<bool>> UpdateSortOrderAsync(MenuFavoriteSortRequest request);
}

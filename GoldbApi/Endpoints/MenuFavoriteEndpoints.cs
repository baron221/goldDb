using GoldbApi.Services;
using GoldbApi.DTOs;

namespace GoldbApi.Endpoints;

public static class MenuFavoriteEndpoints
{

    public static void MapMenuFavoriteEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/user/menu-favorites").RequireAuthorization();

        group.MapGet("/", async (IMenuFavoriteService favoriteService) =>
        {
            var result = await favoriteService.GetFavoriteMenuIdsAsync();
            return Results.Ok(result);
        });

        group.MapGet("/detail", async (IMenuFavoriteService favoriteService) =>
        {
            var result = await favoriteService.GetFavoritesAsync();
            return Results.Ok(result);
        });

        group.MapPost("/sort", async (MenuFavoriteSortRequest request, IMenuFavoriteService favoriteService) =>
        {
            var result = await favoriteService.UpdateSortOrderAsync(request);
            return Results.Ok(result);
        });

        group.MapPost("/toggle/{menuId:int}", async (int menuId, IMenuFavoriteService favoriteService) =>
        {
            var result = await favoriteService.ToggleFavoriteAsync(menuId);
            return Results.Ok(result);
        });

        group.MapPost("/{menuId:int}", async (int menuId, IMenuFavoriteService favoriteService) =>
        {
            var result = await favoriteService.AddFavoriteAsync(menuId);
            return Results.Ok(result);
        });

        group.MapDelete("/{menuId:int}", async (int menuId, IMenuFavoriteService favoriteService) =>
        {
            var result = await favoriteService.RemoveFavoriteAsync(menuId);
            return Results.Ok(result);
        });
    }
}

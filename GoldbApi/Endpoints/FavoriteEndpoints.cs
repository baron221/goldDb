using GoldbApi.DTOs;
using GoldbApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace GoldbApi.Endpoints;

public static class FavoriteEndpoints
{

    public static void MapFavoriteEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/favorite").WithTags("Favorite").RequireAuthorization();

        group.MapGet("/", async (IFavoriteService service) =>
        {
            return Results.Ok(await service.GetMyFavoritesAsync());
        });

        group.MapPost("/", async (IFavoriteService service, CreateFavoriteDto request) =>
        {
            return Results.Ok(await service.AddFavoriteAsync(request));
        });

        group.MapDelete("/{id}", async (IFavoriteService service, int id) =>
        {
            return Results.Ok(await service.RemoveFavoriteAsync(id));
        });

        group.MapDelete("/product", async (IFavoriteService service, [FromQuery] int? productId, [FromQuery] int? productSetId) =>
        {
            return Results.Ok(await service.RemoveFavoriteByProductAsync(productId, productSetId));
        });
    }
}

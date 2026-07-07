using GoldbApi.DTOs;
using GoldbApi.Services;
using Microsoft.AspNetCore.Mvc;
using GoldbApi.Filters;

namespace GoldbApi.Endpoints;

public static class CartEndpoints
{

    public static void MapCartEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/cart").WithTags("Cart").RequireAuthorization();

        group.MapGet("/", async (ICartService service) =>
        {
            return Results.Ok(await service.GetMyCartAsync());
        });

        group.MapPost("/", async (ICartService service, AddToCartDto request) =>
        {
            return Results.Ok(await service.AddToCartAsync(request));
        }).WithValidation<AddToCartDto>();

        group.MapPut("/{id}/quantity", async (ICartService service, int id, UpdateCartQuantityDto request) =>
        {
            return Results.Ok(await service.UpdateQuantityAsync(id, request.Quantity));
        });

        group.MapPut("/{id}/price", async (ICartService service, int id, UpdateCartPriceDto request) =>
        {
            return Results.Ok(await service.UpdatePriceAsync(id, request.FactoryPrice, request.LaborCost));
        });

        group.MapDelete("/{id}", async (ICartService service, int id) =>
        {
            return Results.Ok(await service.RemoveFromCartAsync(id));
        });

        group.MapDelete("/", async (ICartService service) =>
        {
            return Results.Ok(await service.ClearCartAsync());
        });
    }
}

using GoldbApi.DTOs;
using GoldbApi.Services;

namespace GoldbApi.Endpoints;

public static class DiamondPriceEndpoints
{

    public static void MapDiamondPriceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/diamond-prices").WithTags("DiamondPrice");

        group.MapGet("/", async (IDiamondPriceService service, string? priceType) =>
        {
            return Results.Ok(await service.GetDiamondPricesAsync(priceType));
        });

        group.MapGet("/latest/{priceType}", async (IDiamondPriceService service, string priceType) =>
        {
            return Results.Ok(await service.GetLatestDiamondPricesAsync(priceType));
        });

        group.MapPost("/", async (IDiamondPriceService service, CreateDiamondPriceDto request) =>
        {
            return Results.Ok(await service.CreateDiamondPriceAsync(request));
        });

        group.MapPut("/{id:int}", async (int id, UpdateDiamondPriceDto request, IDiamondPriceService service) =>
        {
            return Results.Ok(await service.UpdateDiamondPriceAsync(id, request));
        });

        group.MapDelete("/{id}", async (IDiamondPriceService service, int id) =>
        {
            return Results.Ok(await service.DeleteDiamondPriceAsync(id));
        });
    }
}

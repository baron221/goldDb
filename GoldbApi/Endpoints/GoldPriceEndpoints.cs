using GoldbApi.DTOs;
using GoldbApi.Services;

namespace GoldbApi.Endpoints;

public static class GoldPriceEndpoints
{

    public static void MapGoldPriceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/gold-prices").WithTags("GoldPrice");

        group.MapGet("/", async (IGoldPriceService service) =>
        {
            return Results.Ok(await service.GetGoldPricesAsync());
        });

        group.MapGet("/latest", async (IGoldPriceService service) =>
        {
            return Results.Ok(await service.GetLatestGoldPriceAsync());
        });

        group.MapPost("/", async (IGoldPriceService service, CreateGoldPriceDto request) =>
        {
            return Results.Ok(await service.CreateGoldPriceAsync(request));
        });

        group.MapPut("/{id:int}", async (int id, UpdateGoldPriceDto request, IGoldPriceService service) =>
        {
            return Results.Ok(await service.UpdateGoldPriceAsync(id, request));
        });

        group.MapDelete("/{id}", async (IGoldPriceService service, int id) =>
        {
            return Results.Ok(await service.DeleteGoldPriceAsync(id));
        });
    }
}

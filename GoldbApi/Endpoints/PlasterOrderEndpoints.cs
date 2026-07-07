using GoldbApi.DTOs;
using GoldbApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace GoldbApi.Endpoints;

public static class PlasterOrderEndpoints
{

    public static void MapPlasterOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/plaster-orders").WithTags("PlasterOrder");

        group.MapGet("/", async (IPlasterOrderService service, [AsParameters] PlasterOrderQueryDto query) =>
        {
            return Results.Ok(await service.GetPlasterOrdersAsync(query));
        });

        group.MapGet("/{id}", async (IPlasterOrderService service, int id) =>
        {
            return Results.Ok(await service.GetPlasterOrderAsync(id));
        });

        group.MapPost("/", async (IPlasterOrderService service, CreatePlasterOrderDto request) =>
        {
            return Results.Ok(await service.CreatePlasterOrderAsync(request));
        });

        group.MapPut("/{id}", async (IPlasterOrderService service, int id, CreatePlasterOrderDto request) =>
        {
            return Results.Ok(await service.UpdatePlasterOrderAsync(id, request));
        });

        group.MapDelete("/{id}", async (IPlasterOrderService service, int id) =>
        {
            return Results.Ok(await service.DeletePlasterOrderAsync(id));
        });
    }
}

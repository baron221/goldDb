using GoldbApi.DTOs;
using GoldbApi.Services;
using Microsoft.AspNetCore.Mvc;
using GoldbApi.Filters;

namespace GoldbApi.Endpoints;

public static class OrderEndpoints
{

    public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/order").WithTags("Order").RequireAuthorization();

        group.MapGet("/", async (IOrderService service, [AsParameters] OrderQueryDto query) =>
        {
            return Results.Ok(await service.GetMyOrdersAsync(query));
        });

        group.MapPost("/", async (IOrderService service, CreateOrderDto request) =>
        {
            return Results.Ok(await service.CreateOrderAsync(request));
        }).WithValidation<CreateOrderDto>();

        group.MapGet("/all", async (IOrderService service, [AsParameters] OrderQueryDto query) =>
        {
            return Results.Ok(await service.GetAllOrdersAsync(query));
        });

        group.MapPut("/{id:int}/status", async (int id, UpdateOrderStatusDto request, IOrderService service) =>
        {
            return Results.Ok(await service.UpdateOrderStatusAsync(id, request));
        }).WithValidation<UpdateOrderStatusDto>();

        group.MapDelete("/{id:int}", async (int id, IOrderService service) =>
        {
            return Results.Ok(await service.DeleteOrderAsync(id));
        });

        group.MapGet("/settlement-summary", async (IOrderService service, [AsParameters] OrderQueryDto query) =>
        {
            return Results.Ok(await service.GetSettlementSummaryAsync(query));
        });

        group.MapGet("/{id:int}/history", async (int id, IOrderService service) =>
        {
            return Results.Ok(await service.GetOrderHistoryAsync(id));
        });

        group.MapPost("/{id:int}/statement", async (int id, SaveOrderStatementDto request, IOrderService service) =>
        {
            request.OrderId = id;
            return Results.Ok(await service.SaveOrderStatementAsync(request));
        }).WithValidation<SaveOrderStatementDto>();

        group.MapGet("/{id:int}/statement", async (int id, IOrderService service) =>
        {
            return Results.Ok(await service.GetOrderStatementAsync(id));
        });

        group.MapGet("/{id:int}/statement/pdf", async (int id, IOrderService service) =>
        {
            return Results.Ok(await service.GetOrderStatementPdfAsync(id));
        });
    }
}

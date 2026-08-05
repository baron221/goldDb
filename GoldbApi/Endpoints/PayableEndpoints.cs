using GoldbApi.DTOs;
using GoldbApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace GoldbApi.Endpoints;

public static class PayableEndpoints
{

    public static void MapPayableEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/payables").WithTags("Payable").RequireAuthorization();

        group.MapGet("/summaries", async (IPayableService service, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null) =>
        {
            return Results.Ok(await service.GetCompanySummariesAsync(page, pageSize, search));
        });

        group.MapGet("/", async (IPayableService service, [AsParameters] PayableQueryDto query) =>
        {
            return Results.Ok(await service.GetPayablesAsync(query));
        });

        group.MapGet("/summary-by-order/{orderId:int}", async (IPayableService service, int orderId) =>
        {
            return Results.Ok(await service.GetPayableSummaryForOrderAsync(orderId));
        });

        group.MapGet("/order-history", async (IPayableService service, [AsParameters] PayableQueryDto query) =>
        {
            return Results.Ok(await service.GetPayableOrderHistoryAsync(query));
        });

        group.MapGet("/order-history/summary", async (IPayableService service, [AsParameters] PayableQueryDto query) =>
        {
            return Results.Ok(await service.GetPayableOrderHistorySummaryAsync(query));
        });

        group.MapPost("/payment", async (IPayableService service, CreatePaymentDto request) =>
        {
            return Results.Ok(await service.ProcessPaymentAsync(request));
        });

        group.MapPut("/{id:int}", async (IPayableService service, int id, UpdatePaymentDto request) =>
        {
            return Results.Ok(await service.UpdatePaymentAsync(id, request));
        });

        group.MapPost("/{id:int}/cancel", async (IPayableService service, int id) =>
        {
            return Results.Ok(await service.CancelPaymentAsync(id));
        });
    }
}

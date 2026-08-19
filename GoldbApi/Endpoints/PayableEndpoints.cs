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

        group.MapGet("/order-history/completed", async (IPayableService service, [AsParameters] PayableQueryDto query) =>
        {
            return Results.Ok(await service.GetCompletedPayableOrdersAsync(query));
        });

        group.MapGet("/charges/{chargeId:int}/applications", async (IPayableService service, int chargeId) =>
        {
            return Results.Ok(await service.GetChargeApplicationsAsync(chargeId));
        });

        group.MapPost("/payment", async (IPayableService service, CreatePaymentDto request) =>
        {
            return Results.Ok(await service.ProcessPaymentAsync(request));
        });

        group.MapPost("/order-charge-summary", async (IPayableService service, OrderIdsQueryDto request) =>
        {
            return Results.Ok(await service.GetOrderChargeSummaryAsync(request.OrderIds));
        });

        group.MapGet("/{id:int}/applications", async (IPayableService service, int id) =>
        {
            return Results.Ok(await service.GetPaymentApplicationsAsync(id));
        });

        group.MapPut("/applications/{applicationId:int}", async (IPayableService service, int applicationId, UpdatePaymentApplicationDto request) =>
        {
            return Results.Ok(await service.UpdatePaymentApplicationAsync(applicationId, request));
        });

        group.MapPut("/{id:int}", async (IPayableService service, int id, UpdatePaymentDto request) =>
        {
            return Results.Ok(await service.UpdatePaymentAsync(id, request));
        });

        group.MapPost("/{id:int}/cancel", async (IPayableService service, int id) =>
        {
            return Results.Ok(await service.CancelPaymentAsync(id));
        });

        group.MapPost("/{id:int}/mfg-process", async (IPayableService service, int id, MfgProcessDto request) =>
        {
            return Results.Ok(await service.MarkMfgProcessedAsync(id, request));
        });

        group.MapDelete("/{id:int}", async (IPayableService service, int id) =>
        {
            return Results.Ok(await service.DeletePaymentAsync(id));
        });
    }
}

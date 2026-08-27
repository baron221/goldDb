using GoldbApi.DTOs;
using GoldbApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace GoldbApi.Endpoints;

public static class ReceivableEndpoints
{

    public static void MapReceivableEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/receivables").WithTags("Receivable").RequireAuthorization();

        group.MapGet("/summaries", async (IReceivableService service, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null) =>
        {
            return Results.Ok(await service.GetUserSummariesAsync(page, pageSize, search));
        });

        group.MapGet("/logistics-summary", async (IReceivableService service) =>
        {
            return Results.Ok(await service.GetLogisticsSummaryAsync());
        });

        group.MapGet("/order-history", async (IReceivableService service, [AsParameters] ReceivableOrderHistoryQueryDto query) =>
        {
            return Results.Ok(await service.GetReceivableOrderHistoryAsync(query));
        });

        group.MapGet("/order-history/summary", async (IReceivableService service, [AsParameters] ReceivableOrderHistoryQueryDto query) =>
        {
            return Results.Ok(await service.GetReceivableOrderHistorySummaryAsync(query));
        });

        group.MapGet("/order-history/completed", async (IReceivableService service, [AsParameters] ReceivableOrderHistoryQueryDto query) =>
        {
            return Results.Ok(await service.GetCompletedReceivableOrdersAsync(query));
        });

        group.MapGet("/overdue-summary", async (IReceivableService service, [AsParameters] ReceivableOverdueQueryDto query) =>
        {
            return Results.Ok(await service.GetReceivableOverdueSummaryAsync(query));
        });

        group.MapGet("/{chargeId:int}/charge-applications", async (IReceivableService service, int chargeId) =>
        {
            return Results.Ok(await service.GetReceivableChargeApplicationsAsync(chargeId));
        });

        group.MapGet("/purity-summary", async (IReceivableService service, [FromQuery] int userId) =>
        {
            return Results.Ok(await service.GetPuritySummaryAsync(userId));
        });

        group.MapGet("/{id:int}/ledger-before", async (IReceivableService service, int id) =>
        {
            return Results.Ok(await service.GetLedgerBeforeAsync(id));
        });

        group.MapGet("/user-summary/{userId:int}", async (IReceivableService service, int userId) =>
        {
            return Results.Ok(await service.GetUserSummaryByIdAsync(userId));
        });

        group.MapPost("/order-charge-summary", async (IReceivableService service, OrderIdsQueryDto request) =>
        {
            return Results.Ok(await service.GetReceivableChargeSummaryAsync(request.OrderIds));
        });

        group.MapGet("/", async (IReceivableService service, [AsParameters] ReceivableQueryDto query) =>
        {
            return Results.Ok(await service.GetReceivablesAsync(query));
        });

        group.MapPost("/deposit", async (IReceivableService service, CreateDepositDto request) =>
        {
            return Results.Ok(await service.ProcessDepositAsync(request));
        });

        group.MapPut("/{id:int}", async (IReceivableService service, int id, UpdateDepositDto request) =>
        {
            return Results.Ok(await service.UpdateDepositAsync(id, request));
        });

        group.MapPost("/{id:int}/cancel", async (IReceivableService service, int id) =>
        {
            return Results.Ok(await service.CancelDepositAsync(id));
        });
    }
}

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

        group.MapGet("/purity-summary", async (IReceivableService service, [FromQuery] int userId) =>
        {
            return Results.Ok(await service.GetPuritySummaryAsync(userId));
        });

        group.MapGet("/user-summary/{userId:int}", async (IReceivableService service, int userId) =>
        {
            return Results.Ok(await service.GetUserSummaryByIdAsync(userId));
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

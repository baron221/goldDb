using GoldbApi.DTOs;
using GoldbApi.Services;
using Microsoft.AspNetCore.Mvc;
using GoldbApi.Filters;

namespace GoldbApi.Endpoints;

public static class StockEndpoints
{

    public static void MapStockEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/stocks").WithTags("Stock");

        group.MapGet("/", async (IStockService service, [AsParameters] StockQueryDto query) =>
        {
            return Results.Ok(await service.GetStocksAsync(query));
        });

        group.MapGet("/summary", async (IStockService service, [AsParameters] StockQueryDto query) =>
        {
            return Results.Ok(await service.GetStockSummaryAsync(query));
        });

        group.MapGet("/{id:int}", async (IStockService service, int id) =>
        {
            return Results.Ok(await service.GetStockDetailAsync(id));
        });

        group.MapPost("/", async (IStockService service, CreateStockDto request) =>
        {
            return Results.Ok(await service.CreateStockAsync(request));
        }).WithValidation<CreateStockDto>();

        group.MapPut("/{id}", async (IStockService service, int id, UpdateStockDto request) =>
        {
            return Results.Ok(await service.UpdateStockAsync(id, request));
        }).WithValidation<UpdateStockDto>();

        group.MapDelete("/{id}", async (IStockService service, int id) =>
        {
            return Results.Ok(await service.DeleteStockAsync(id));
        });

        group.MapPost("/{id}/photos", async (IStockService service, int id, StockPhotoUpdateDto request) =>
        {
            return Results.Ok(await service.UpdateStockPhotosAsync(id, request.Attachments.Select(a => a.AttachmentId).ToList(), request.MainAttachmentId));
        });

        group.MapPost("/delete-bulk", async (IStockService service, StockDeleteDto request) =>
        {
            return Results.Ok(await service.DeleteStocksAsync(request));
        });

        group.MapPost("/rent", async (IStockService service, StockRentDto request) =>
        {
            return Results.Ok(await service.RentStocksAsync(request));
        });

        group.MapPost("/return", async (IStockService service, List<int> ids) =>
        {
            return Results.Ok(await service.ReturnStocksAsync(ids));
        });

        group.MapPost("/rent-barcode", async (IStockService service, StockBarcodeActionDto request) =>
        {
            return Results.Ok(await service.RentByBarcodeAsync(request));
        });

        group.MapPost("/return-barcode", async (IStockService service, StockBarcodeActionDto request) =>
        {
            return Results.Ok(await service.ReturnByBarcodeAsync(request));
        });

        group.MapPost("/bulk-update", async (IStockService service, List<StockDto> stocks) =>
        {
            return Results.Ok(await service.BulkUpdateStockAsync(stocks));
        });

        group.MapPost("/exhaust-item", async (IStockService service, ExhaustStockOrderItemDto request) =>
        {
            return Results.Ok(await service.ExhaustStockForOrderItemAsync(request));
        });
    }
}

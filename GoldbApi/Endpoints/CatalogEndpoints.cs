using GoldbApi.DTOs;
using GoldbApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace GoldbApi.Endpoints;

public static class CatalogEndpoints
{

    public static void MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/catalogs").WithTags("Catalog").RequireAuthorization();

        group.MapGet("/", async (ICatalogService service, [AsParameters] CatalogQueryDto query) =>
        {
            query.SecurityClass = "일반";
            return Results.Ok(await service.GetCatalogsAsync(query));
        }).AllowAnonymous();

        group.MapGet("/{id}", async (ICatalogService service, int id) =>
        {
            return Results.Ok(await service.GetCatalogAsync(id));
        }).AllowAnonymous();

        group.MapPost("/", async (ICatalogService service, CreateCatalogDto request) =>
        {
            return Results.Ok(await service.CreateCatalogAsync(request));
        });

        group.MapPut("/{id}", async (ICatalogService service, int id, CreateCatalogDto request) =>
        {
            return Results.Ok(await service.UpdateCatalogAsync(id, request));
        });

        group.MapDelete("/{id}", async (ICatalogService service, int id) =>
        {
            return Results.Ok(await service.DeleteCatalogAsync(id));
        });
    }
}

using GoldbApi.DTOs;
using GoldbApi.Services;
using Microsoft.AspNetCore.Mvc;
using GoldbApi.Filters;

namespace GoldbApi.Endpoints;

public static class ProductEndpoints
{

    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {

        var group = app.MapGroup("/products").WithTags("Product").RequireAuthorization();

        group.MapGet("/", async (IProductService service, [AsParameters] ProductQueryDto query) =>
        {

            query.IsPublic = true;
            return Results.Ok(await service.GetProductsAsync(query));
        }).AllowAnonymous();

        group.MapGet("/admin", async (IProductService service, [AsParameters] ProductQueryDto query) =>
        {
            return Results.Ok(await service.GetProductsAsync(query));
        });

        group.MapGet("/{id}", async (IProductService service, int id, System.Security.Claims.ClaimsPrincipal user) =>
        {
            var response = await service.GetProductAsync(id);

            if (response.Code == 20000 && response.Data != null && !response.Data.IsPublic)
            {
                if (user.Identity == null || !user.Identity.IsAuthenticated)
                {

                    return Results.Challenge();
                }
            }
            return Results.Ok(response);
        }).AllowAnonymous();

        group.MapPost("/", async (IProductService service, CreateProductDto request) =>
        {
            return Results.Ok(await service.CreateProductAsync(request));
        }).WithValidation<CreateProductDto>();

        group.MapPut("/{id}", async (IProductService service, int id, CreateProductDto request) =>
        {
            return Results.Ok(await service.UpdateProductAsync(id, request));
        }).WithValidation<CreateProductDto>();

        group.MapDelete("/{id}", async (IProductService service, int id) =>
        {
            return Results.Ok(await service.DeleteProductAsync(id));
        });
    }
}

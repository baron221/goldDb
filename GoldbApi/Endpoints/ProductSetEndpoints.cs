using GoldbApi.DTOs;
using GoldbApi.Services;

namespace GoldbApi.Endpoints;

public static class ProductSetEndpoints
{

    public static void MapProductSetEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/product-sets").WithTags("ProductSet").RequireAuthorization();

        group.MapGet("/", async (IProductSetService service, [AsParameters] ProductSetQueryDto query) =>
        {
            query.IsPublic = true;
            return Results.Ok(await service.GetProductSetsAsync(query));
        }).AllowAnonymous();

        group.MapGet("/admin", async (IProductSetService service, [AsParameters] ProductSetQueryDto query) =>
        {
            return Results.Ok(await service.GetProductSetsAsync(query));
        });

        group.MapGet("/{id}", async (IProductSetService service, int id, System.Security.Claims.ClaimsPrincipal user) =>
        {
            var response = await service.GetProductSetAsync(id);
            if (response.Code == 20000 && !response.Data!.IsPublic)
            {
                if (user.Identity == null || !user.Identity.IsAuthenticated)
                {
                    return Results.Challenge();
                }
            }
            return Results.Ok(response);
        }).AllowAnonymous();

        group.MapPost("/", async (IProductSetService service, CreateProductSetDto request) =>
        {
            return Results.Ok(await service.CreateProductSetAsync(request));
        });

        group.MapPut("/{id}", async (IProductSetService service, int id, CreateProductSetDto request) =>
        {
            return Results.Ok(await service.UpdateProductSetAsync(id, request));
        });

        group.MapDelete("/{id}", async (IProductSetService service, int id) =>
        {
            return Results.Ok(await service.DeleteProductSetAsync(id));
        });
    }
}

using GoldbApi.DTOs;
using GoldbApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace GoldbApi.Endpoints;

public static class SearchEndpoints
{

    public static void MapSearchEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/search").RequireAuthorization();

        group.MapGet("", async (ISearchService service, [AsParameters] SearchQueryDto query) =>
        {
            return Results.Ok(await service.IntegratedSearchAsync(query));
        });
    }
}

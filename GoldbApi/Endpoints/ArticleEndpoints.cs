using GoldbApi.DTOs;
using GoldbApi.Models;
using GoldbApi.Services;
using Microsoft.AspNetCore.Mvc;
using GoldbApi.Filters;

namespace GoldbApi.Endpoints;

public static class ArticleEndpoints
{

    public static void MapArticleEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/article/list", async ([AsParameters] ArticleListRequest request, IArticleService articleService) =>
        {
            var result = await articleService.GetArticlesAsync(request);
            return Results.Ok(result);
        }).RequireAuthorization();

        app.MapGet("/article/detail", async (int id, IArticleService articleService) =>
        {
            var result = await articleService.GetArticleDetailAsync(id);
            return Results.Ok(result);
        }).RequireAuthorization();

        app.MapGet("/article/pv", () =>
        {
            var pvData = new[]
            {
                new { key = "PC", pv = 1024 },
                new { key = "mobile", pv = 1024 },
                new { key = "ios", pv = 1024 },
                new { key = "android", pv = 1024 }
            };
            return Results.Ok(ApiResponse<object>.Success(new { pvData }));
        }).RequireAuthorization();

        app.MapPost("/article/create", async (Article article, IArticleService articleService) =>
        {
            var result = await articleService.CreateArticleAsync(article);
            return Results.Ok(result);
        }).RequireAuthorization().WithValidation<Article>();

        app.MapPost("/article/update", async (Article article, IArticleService articleService) =>
        {
            var result = await articleService.UpdateArticleAsync(article);
            return Results.Ok(result);
        }).RequireAuthorization().WithValidation<Article>();
    }
}

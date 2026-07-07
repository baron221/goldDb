using GoldbApi.DTOs;
using GoldbApi.Services;
using GoldbApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace GoldbApi.Endpoints;

public static class FileEndpoints
{

    public static void MapFileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/file").RequireAuthorization();

        group.MapPost("/upload", async (IFormFile file, [FromQuery] string? subDir, IFileService fileService) =>
        {
            if (file == null || file.Length == 0) return Results.BadRequest("No file uploaded");

            var url = await fileService.SaveFileAsync(file, subDir ?? "general");
            return Results.Ok(ApiResponse<string>.Success(url));
        })
        .DisableAntiforgery();

        group.MapPost("/upload-image", async (IFormFile file, [FromQuery] string? subDir, IFileService fileService) =>
        {
            if (file == null || file.Length == 0) return Results.BadRequest("No file uploaded");

            var attachment = await fileService.UploadImageAsync(file, subDir ?? "general");
            return Results.Ok(ApiResponse<Attachment>.Success(attachment));
        })
        .DisableAntiforgery();
    }
}

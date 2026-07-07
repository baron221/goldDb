using GoldbApi.DTOs;
using GoldbApi.Services;
using GoldbApi.Filters;

namespace GoldbApi.Endpoints;

public static class CommonCodeEndpoints
{

    public static void MapCommonCodeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/code").RequireAuthorization();

        group.MapGet("/", async (ICommonCodeService codeService) =>
        {
            var result = await codeService.GetCodesAsync();
            return Results.Ok(result);
        });

        group.MapGet("/group/{groupCode}", async (string groupCode, ICommonCodeService codeService) =>
        {
            var result = await codeService.GetCodesByGroupAsync(groupCode);
            return Results.Ok(result);
        });

        group.MapGet("/parent/{parentId:int}", async (int parentId, ICommonCodeService codeService) =>
        {
            var result = await codeService.GetCodesByParentIdAsync(parentId);
            return Results.Ok(result);
        });

        group.MapPost("/", async (CommonCodeSaveRequest request, ICommonCodeService codeService) =>
        {
            var result = await codeService.SaveCodeAsync(request);
            return Results.Ok(result);
        }).WithValidation<CommonCodeSaveRequest>();

        group.MapDelete("/{id:int}", async (int id, ICommonCodeService codeService) =>
        {
            var result = await codeService.DeleteCodeAsync(id);
            return Results.Ok(result);
        });
    }
}

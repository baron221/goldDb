using GoldbApi.DTOs;
using GoldbApi.Services;
using Microsoft.AspNetCore.Mvc;
using GoldbApi.Filters;

namespace GoldbApi.Endpoints;

public static class NoticeEndpoints
{

    public static void MapNoticeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/notices").WithTags("Notices");

        group.MapGet("/", async (INoticeService service, [FromQuery] bool? isLoginRequired) =>
        {
            var notices = await service.GetAllNoticesAsync(isLoginRequired);
            return Results.Ok(ApiResponse<IEnumerable<NoticeDto>>.Success(notices));
        }).AllowAnonymous(); 

        group.MapGet("/{id:int}", async (int id, INoticeService service) =>
        {
            var notice = await service.GetNoticeByIdAsync(id);
            if (notice == null) return Results.NotFound(ApiResponse<object>.Failure("Notice not found", 404));

            await service.IncrementViewCountAsync(id);
            return Results.Ok(ApiResponse<NoticeDto>.Success(notice));
        }).AllowAnonymous();

        group.MapPost("/", async (CreateNoticeDto createDto, INoticeService service) =>
        {
            var notice = await service.CreateNoticeAsync(createDto);
            return Results.Ok(ApiResponse<NoticeDto>.Success(notice));
        }).RequireAuthorization().WithValidation<CreateNoticeDto>();

        group.MapPut("/", async (UpdateNoticeDto updateDto, INoticeService service) =>
        {
            var success = await service.UpdateNoticeAsync(updateDto);
            return success ? Results.Ok(ApiResponse<string>.Success("success")) : Results.NotFound(ApiResponse<object>.Failure("Notice not found", 404));
        }).RequireAuthorization().WithValidation<UpdateNoticeDto>();

        group.MapDelete("/{id:int}", async (int id, INoticeService service) =>
        {
            var success = await service.DeleteNoticeAsync(id);
            return success ? Results.Ok(ApiResponse<string>.Success("success")) : Results.NotFound(ApiResponse<object>.Failure("Notice not found", 404));
        }).RequireAuthorization();
    }
}

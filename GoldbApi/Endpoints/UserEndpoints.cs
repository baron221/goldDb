using GoldbApi.DTOs;
using GoldbApi.Services;
using Microsoft.AspNetCore.Mvc;
using GoldbApi.Filters;

namespace GoldbApi.Endpoints;

public static class UserEndpoints
{

    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/user").RequireAuthorization();

        group.MapGet("/", async ([FromQuery] string? companyType, [FromQuery] string? searchText, [FromQuery] bool? isUnassignedOnly, [FromQuery] bool? isLogisticsUnassigned, IUserService userService) =>
        {
            var result = await userService.GetUsersAsync(companyType, searchText, isUnassignedOnly ?? false, isLogisticsUnassigned ?? false);
            return Results.Ok(result);
        });

        group.MapGet("/{id:int}", async (int id, IUserService userService) =>
        {
            var result = await userService.GetUserDetailAsync(id);
            return Results.Ok(result);
        });

        group.MapPost("/", async (UserCreateRequest request, IUserService userService) =>
        {
            var result = await userService.CreateUserAsync(request);
            return Results.Ok(result);
        }).WithValidation<UserCreateRequest>();

        group.MapPut("/{id:int}", async (int id, UserUpdateRequest request, IUserService userService) =>
        {
            var result = await userService.UpdateUserAsync(id, request);
            return Results.Ok(result);
        }).WithValidation<UserUpdateRequest>();

        group.MapDelete("/{id:int}", async (int id, IUserService userService) =>
        {
            var result = await userService.DeleteUserAsync(id);
            return Results.Ok(result);
        });

        group.MapGet("/personal-settings", async (IUserPersonalSettingService personalSettingService) =>
        {
            var result = await personalSettingService.GetUserPersonalSettingAsync();
            return Results.Ok(result);
        });

        group.MapPost("/personal-settings", async (UserPersonalSettingDto request, IUserPersonalSettingService personalSettingService) =>
        {
            var result = await personalSettingService.SaveUserPersonalSettingAsync(request);
            return Results.Ok(result);
        }).WithValidation<UserPersonalSettingDto>();
    }
}
